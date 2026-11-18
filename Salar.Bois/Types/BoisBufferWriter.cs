using Salar.BinaryBuffers;
using Salar.Bois.Serializers;
using System;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.CompilerServices;

#nullable disable

// This file is compiled into both packages. Each copy gets its own namespace, so users who reference both
// packages don't get two types with the same full name.
#if BOIS_CODEGEN
namespace Salar.Bois.CodeGen;
#else
namespace Salar.Bois.Types;
#endif

public sealed class BoisBufferWriter : BufferWriterBase
{
	public const int InternalCacheSize = 1024 * 8;

	/// <summary>
	/// For stream output. It grows while a versioned object is open, because its size is filled in after its
	/// members are written, so its bytes can't be sent to the stream before that.
	/// </summary>
	private byte[] _cache;
	private int _openVersioned;

	// not readonly, so the serializer can reuse one writer per thread, see Rent
	private byte[] _buffer;
	private Stream _stream;
	private int _offset;
	private int _length;
	private bool _useStream;
	private bool _canSeek;

	private int _position;
	private int _cachePosition;
	private int _cacheStartPosition;
	private int _writtenLength;

	/// <summary>
	/// A writer reused by the serializer on this thread, so writing versioned types allocates nothing once warmed
	/// up. It is taken while in use, so two writers never share it.
	/// </summary>
	[ThreadStatic]
	private static BoisBufferWriter _free;

	private const int MaxKeptCacheSize = 64 * 1024;

	public BoisBufferWriter(byte[] buffer)
		: this(buffer, 0, buffer?.Length ?? throw new ArgumentNullException(nameof(buffer)))
	{
	}

	public BoisBufferWriter(byte[] buffer, int position, int length)
	{
		Init(buffer, position, length);
	}

	public BoisBufferWriter(Stream output)
	{
		Init(output);
	}

	private void Init(byte[] buffer, int position, int length)
	{
		if (buffer == null)
			throw new ArgumentNullException(nameof(buffer));
		if (position < 0 || position > buffer.Length)
			throw new ArgumentOutOfRangeException(nameof(position));
		if (length < 0 || position + length > buffer.Length)
			throw new ArgumentOutOfRangeException(nameof(length));

		_buffer = buffer;
		_offset = position;
		_length = length;
	}

	private void Init(Stream output)
	{
		_stream = output ?? throw new ArgumentNullException(nameof(output));
		_useStream = true;
		_canSeek = output.CanSeek;
		_position = _canSeek ? checked((int)output.Position) : 0;
		_writtenLength = _position;
		_cache ??= new byte[InternalCacheSize];
	}

	/// <summary>
	/// This thread's reused writer, or a new one. Give it back with <see cref="Return"/>. If the call throws it is
	/// not given back, and the next call creates a new one.
	/// </summary>
	internal static BoisBufferWriter Rent(byte[] buffer, int position, int length)
	{
		var writer = _free ?? new BoisBufferWriter();
		_free = null;
		writer.Init(buffer, position, length);
		return writer;
	}

	/// <inheritdoc cref="Rent(byte[], int, int)"/>
	internal static BoisBufferWriter Rent(Stream output)
	{
		var writer = _free ?? new BoisBufferWriter();
		_free = null;
		writer.Init(output);
		return writer;
	}

	private BoisBufferWriter()
	{
	}

	/// <summary>
	/// Flushes, then keeps this writer for the next call on this thread. Buffers that grew too big are dropped.
	/// </summary>
	internal void Return()
	{
		FlushCore();
		if (_cache != null && _cache.Length > MaxKeptCacheSize)
			_cache = null;

		_buffer = null;
		_stream = null;
		_offset = _length = _position = _cachePosition = _cacheStartPosition = _writtenLength = 0;
		_useStream = _canSeek = false;
		_free = this;
	}

	public override int Offset => _offset;

	public override int Length
	{
		get
		{
			if (!_useStream)
				return _length;

			if (!_canSeek)
				return 0;

			var length = _stream.Length;
			return length > int.MaxValue ? int.MaxValue : unchecked((int)length);
		}
	}

	public override int Position
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => _position;
		set => SetPosition(value);
	}

	public int WrittenLength => _writtenLength;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Flush()
	{
		FlushCore();
	}

	public override void ResetBuffer()
	{
		_cachePosition = 0;
		_openVersioned = 0;

		if (_useStream)
		{
			if (!_canSeek)
				throw new NotSupportedException("The underlying stream must support seeking to reset the writer.");

			_stream.SetLength(0);
			_stream.Position = 0;
			_position = 0;
			_cacheStartPosition = 0;
			_writtenLength = 0;
			return;
		}

		_position = 0;
		_cacheStartPosition = 0;
		_writtenLength = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override void Write(bool value)
	{
		Write(value ? (byte)1 : (byte)0);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override void Write(byte value)
	{
		EnsureWritable(1);

		if (!_useStream)
		{
			_buffer[_offset + _position] = value;
			Advance(1);
			return;
		}

		if (_cachePosition == 0)
			_cacheStartPosition = _position;

		_cache[_cachePosition++] = value;
		Advance(1);

		if (_cachePosition == _cache.Length)
			CacheFull();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override void Write(sbyte value)
	{
		Write(unchecked((byte)value));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override void Write(byte[] buffer)
	{
		if (buffer == null)
			throw new ArgumentNullException(nameof(buffer));

		Write(new ReadOnlySpan<byte>(buffer));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override void Write(byte[] buffer, int offset, int length)
	{
		if (buffer == null)
			throw new ArgumentNullException(nameof(buffer));
		if ((uint)offset > (uint)buffer.Length)
			throw new ArgumentOutOfRangeException(nameof(offset));
		if ((uint)length > (uint)(buffer.Length - offset))
			throw new ArgumentOutOfRangeException(nameof(length));

		Write(new ReadOnlySpan<byte>(buffer, offset, length));
	}

	public override void Write(decimal value)
	{
		var span = GetFixedWriteSpan(16);
		var bits = decimal.GetBits(value);
		BinaryPrimitives.WriteInt32LittleEndian(span, bits[0]);
		BinaryPrimitives.WriteInt32LittleEndian(span.Slice(4), bits[1]);
		BinaryPrimitives.WriteInt32LittleEndian(span.Slice(8), bits[2]);
		BinaryPrimitives.WriteInt32LittleEndian(span.Slice(12), bits[3]);
		CompleteFixedWrite(16);
	}

	public override void Write(double value)
	{
		Write(DoubleToUInt64Bits(value));
	}

	public override void Write(float value)
	{
		Write(SingleToUInt32Bits(value));
	}

	public override void Write(short value)
	{
		var span = GetFixedWriteSpan(2);
		BinaryPrimitives.WriteInt16LittleEndian(span, value);
		CompleteFixedWrite(2);
	}

	public override void Write(ushort value)
	{
		var span = GetFixedWriteSpan(2);
		BinaryPrimitives.WriteUInt16LittleEndian(span, value);
		CompleteFixedWrite(2);
	}

	public override void Write(int value)
	{
		var span = GetFixedWriteSpan(4);
		BinaryPrimitives.WriteInt32LittleEndian(span, value);
		CompleteFixedWrite(4);
	}

	public override void Write(uint value)
	{
		var span = GetFixedWriteSpan(4);
		BinaryPrimitives.WriteUInt32LittleEndian(span, value);
		CompleteFixedWrite(4);
	}

	public override void Write(long value)
	{
		var span = GetFixedWriteSpan(8);
		BinaryPrimitives.WriteInt64LittleEndian(span, value);
		CompleteFixedWrite(8);
	}

	public override void Write(ulong value)
	{
		var span = GetFixedWriteSpan(8);
		BinaryPrimitives.WriteUInt64LittleEndian(span, value);
		CompleteFixedWrite(8);
	}

	public override void Write(ReadOnlySpan<byte> buffer)
	{
		var length = buffer.Length;
		if (length == 0)
			return;

		EnsureWritable(length);

		if (!_useStream)
		{
			buffer.CopyTo(new Span<byte>(_buffer, _offset + _position, length));
			Advance(length);
			return;
		}

		if (length >= InternalCacheSize && _openVersioned == 0)
		{
			FlushCore();
			CommitDirect(_position, buffer);
			Advance(length);
			return;
		}

		ReserveCache(length);

		buffer.CopyTo(new Span<byte>(_cache, _cachePosition, length));
		_cachePosition += length;
		Advance(length);

		if (_cachePosition == _cache.Length)
			CacheFull();
	}

	public byte[] ToArray()
	{
		FlushCore();

		if (_useStream)
		{
			if (_stream is MemoryStream memoryStream)
				return memoryStream.ToArray();

			throw new NotSupportedException("ToArray is only supported for byte-array output or MemoryStream output.");
		}

		var result = new byte[_writtenLength];
		if (_writtenLength > 0)
			Buffer.BlockCopy(_buffer, _offset, result, 0, _writtenLength);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void Advance(int count)
	{
		unchecked
		{
			_position += count;
		}

		if (_position > _writtenLength)
			_writtenLength = _position;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void EnsureWritable(int count)
	{
		if (_useStream)
			return;

		if ((uint)count > (uint)(_length - _position))
                throw new EndOfStreamException("Reached end of the underlying buffer.");
        }

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private Span<byte> GetFixedWriteSpan(int size)
	{
		EnsureWritable(size);

		if (!_useStream)
			return new Span<byte>(_buffer, _offset + _position, size);

		ReserveCache(size);
		return new Span<byte>(_cache, _cachePosition, size);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void CompleteFixedWrite(int size)
	{
		if (!_useStream)
		{
			Advance(size);
			return;
		}

		_cachePosition += size;
		Advance(size);

		if (_cachePosition == _cache.Length)
			CacheFull();
	}

	/// <summary>
	/// Makes room in the cache for <paramref name="size"/> bytes at <see cref="_cachePosition"/>.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ReserveCache(int size)
	{
		if (_cachePosition == 0)
		{
			_cacheStartPosition = _position;
		}
		else if (_cachePosition + size > _cache.Length)
		{
			if (_openVersioned > 0)
			{
				GrowCache(size);
			}
			else
			{
				FlushCore();
				_cacheStartPosition = _position;
			}
		}

		if (size > _cache.Length)
			GrowCache(size);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void CacheFull()
	{
		if (_openVersioned > 0)
			GrowCache(1);
		else
			FlushCore();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void GrowCache(int needed)
	{
		var size = Math.Max(_cache.Length * 2, _cachePosition + needed);
		Array.Resize(ref _cache, size);
	}

	/// <summary>
	/// Starts a versioned object: writes a placeholder for its size and returns where it starts.
	/// </summary>
	internal int BeginVersioned()
	{
		var start = _position;
		_openVersioned++;
		Write((byte)0);
		return start;
	}

	/// <summary>
	/// Ends a versioned object: fills in its size. Sizes above 63 bytes need more than the placeholder byte, so
	/// the object's bytes are moved forward to make room, in one copy.
	/// </summary>
	internal void EndVersioned(int start)
	{
		var length = _position - start - 1;
		_openVersioned--;

		if (length <= VersionedFormat.MaxShortLength)
		{
			// the common case: the size fits in the placeholder, nothing moves
			if (_useStream)
				_cache[start - _cacheStartPosition] = (byte)(VersionedFormat.ShortLengthFlag | length);
			else
				_buffer[_offset + start] = (byte)(VersionedFormat.ShortLengthFlag | length);
		}
		else
		{
			var extra = VersionedFormat.LongLengthExtraBytes(length);
			var buffer = MakeRoom(extra);
			var index = BufferIndex(start);
			Buffer.BlockCopy(buffer, index + 1, buffer, index + 1 + extra, length);
			VersionedFormat.WriteLongLength(new Span<byte>(buffer, index, 1 + extra), length);
			Advance(extra);
		}

		if (_useStream && _openVersioned == 0 && _cachePosition >= InternalCacheSize)
			FlushCore();
	}

	/// <summary>
	/// Makes room for <paramref name="extra"/> more bytes after the written ones and returns the buffer that holds them.
	/// </summary>
	private byte[] MakeRoom(int extra)
	{
		if (!_useStream)
		{
			EnsureWritable(extra);
			return _buffer;
		}

		if (_cachePosition + extra > _cache.Length)
			GrowCache(extra);
		_cachePosition += extra;
		return _cache;
	}

	private int BufferIndex(int position) => _useStream ? position - _cacheStartPosition : _offset + position;

	private void FlushCore()
	{
		if (_cachePosition == 0)
			return;

		CommitBuffered(_cacheStartPosition, _cachePosition);
		_cachePosition = 0;
	}

	private void CommitBuffered(int position, int count)
	{
		if (_useStream)
		{
			if (_canSeek && _stream.Position != position)
				_stream.Position = position;

			_stream.Write(_cache, 0, count);
			return;
		}

		Buffer.BlockCopy(_cache, 0, _buffer, _offset + position, count);
	}

	private void CommitDirect(int position, ReadOnlySpan<byte> buffer)
	{
		if (_useStream)
		{
			if (_canSeek && _stream.Position != position)
				_stream.Position = position;

#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
			_stream.Write(buffer);
			return;
#else

			var remaining = buffer;
			while (!remaining.IsEmpty)
			{
				var chunkLength = remaining.Length > InternalCacheSize ? InternalCacheSize : remaining.Length;
				remaining.Slice(0, chunkLength).CopyTo(_cache);
				_stream.Write(_cache, 0, chunkLength);
				remaining = remaining.Slice(chunkLength);
			}

			return;
#endif
		}

		buffer.CopyTo(new Span<byte>(_buffer, _offset + position, buffer.Length));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static unsafe uint SingleToUInt32Bits(float value)
	{
		return *(uint*)&value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static unsafe ulong DoubleToUInt64Bits(double value)
	{
		return *(ulong*)&value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void SetPosition(int value)
	{
		if (value < 0)
			throw new ArgumentOutOfRangeException(nameof(value));

		if (_useStream)
		{
			if (!_canSeek)
				throw new NotSupportedException("The underlying stream must support seeking to change the writer position.");

			FlushCore();
			_stream.Position = value;
			_position = value;
			if (_position > _writtenLength)
				_writtenLength = _position;
			return;
		}

		if (value > _length)
			throw new ArgumentOutOfRangeException(nameof(value),
				"Position (zero-based) must be equal to or less than the size of the underlying byte array.");

		FlushCore();
		_position = value;
	}
}
