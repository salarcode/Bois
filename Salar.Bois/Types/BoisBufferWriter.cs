using Salar.BinaryBuffers;
using System;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.CompilerServices;

namespace Salar.Bois.Types
{
	public sealed class BoisBufferWriter : BufferWriterBase
	{
		public const int InternalCacheSize = 1024 * 8;

		private readonly byte[] _cache;
		private readonly byte[] _buffer;
		private readonly Stream _stream;
		private readonly int _offset;
		private readonly int _length;
		private readonly bool _useStream;

		private int _position;
		private int _cachePosition;
		private int _cacheStartPosition;
		private int _writtenLength;

		public BoisBufferWriter(byte[] buffer)
			: this(buffer, 0, buffer?.Length ?? throw new ArgumentNullException(nameof(buffer)))
		{
		}

		public BoisBufferWriter(byte[] buffer, int position, int length)
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

		public BoisBufferWriter(Stream output)
		{
			_stream = output ?? throw new ArgumentNullException(nameof(output));
			_useStream = true;
			_position = output.CanSeek ? checked((int)output.Position) : 0;
			_writtenLength = _position;
			_cache = new byte[InternalCacheSize];
		}

		public override int Offset => _offset;

		public override int Length
		{
			get
			{
				if (!_useStream)
					return _length;

				if (!_stream.CanSeek)
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

			if (_useStream)
			{
				if (!_stream.CanSeek)
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

			if (_cachePosition == InternalCacheSize)
				FlushCore();
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

			if (length >= InternalCacheSize)
			{
				FlushCore();
				CommitDirect(_position, buffer);
				Advance(length);
				return;
			}

			if (_cachePosition == 0)
			{
				_cacheStartPosition = _position;
			}
			else if (_cachePosition + length > InternalCacheSize)
			{
				FlushCore();
				_cacheStartPosition = _position;
			}

			buffer.CopyTo(new Span<byte>(_cache, _cachePosition, length));
			_cachePosition += length;
			Advance(length);

			if (_cachePosition == InternalCacheSize)
				FlushCore();
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
				throw new EndOfStreamException("Reached to end of data");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Span<byte> GetFixedWriteSpan(int size)
		{
			EnsureWritable(size);

			if (!_useStream)
				return new Span<byte>(_buffer, _offset + _position, size);

			if (_cachePosition == 0)
			{
				_cacheStartPosition = _position;
			}
			else if (_cachePosition + size > InternalCacheSize)
			{
				FlushCore();
				_cacheStartPosition = _position;
			}

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

			if (_cachePosition == InternalCacheSize)
				FlushCore();
		}

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
				if (_stream.CanSeek && _stream.Position != position)
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
				if (_stream.CanSeek && _stream.Position != position)
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
				if (!_stream.CanSeek)
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
}
