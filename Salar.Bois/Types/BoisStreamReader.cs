using Salar.BinaryBuffers;
using System;
using System.IO;

#nullable disable

/*
 * Salar BOIS (Binary Object Indexed Serialization)
 * by Salar Khalilzadeh
 *
 * https://github.com/salarcode/Bois
 * Mozilla Public License v2
 */
// This file is compiled into both packages, see BoisBufferWriter.
#if BOIS_CODEGEN
namespace Salar.Bois.CodeGen;
#else
namespace Salar.Bois.Types;
#endif

/// <summary>
/// Reads from a stream that may not be seekable, and counts the bytes it reads, so <see cref="Position"/> works.
/// Versioned types need the position to find the end of each object.
/// <para>
/// It reads only the bytes it is asked for, never ahead, so the stream is left right after the data.
/// </para>
/// </summary>
internal sealed class BoisStreamReader : BufferReaderBase
{
	private const int SharedBufferSize = 4 * 1024;

	/// <summary>
	/// A small fixed buffer per thread, for skipping and for small spans, so reading allocates nothing.
	/// </summary>
	[ThreadStatic]
	private static byte[] _sharedBuffer;

	private readonly Stream _stream;
	private readonly bool _canSeek;
	private readonly int _startPosition;
	private int _position;

	/// <summary>
	/// For spans bigger than the shared buffer. It lives as long as this reader.
	/// </summary>
	private byte[] _largeBuffer;

	public BoisStreamReader(Stream stream)
	{
		_stream = stream ?? throw new ArgumentNullException(nameof(stream));
		_canSeek = stream.CanSeek;
		_startPosition = _canSeek ? checked((int)stream.Position) : 0;
	}

	public override int Offset => 0;

	public override int Length
		=> _canSeek ? checked((int)_stream.Length) - _startPosition : throw new NotSupportedException("The stream is not seekable, so its length is unknown.");

	/// <summary>
	/// The number of bytes read. Setting it can only move forward on a non-seekable stream.
	/// </summary>
	public override int Position
	{
		get => _position;
		set
		{
			if (value < _position && !_canSeek)
				throw new NotSupportedException("The stream is not seekable, so the position can only move forward.");
			if (value >= _position)
				Skip(value - _position);
			else
			{
				_stream.Position = _startPosition + value;
				_position = value;
			}
		}
	}

	public override int Remaining
		=> _canSeek ? Length - _position : throw new NotSupportedException("The stream is not seekable, so the data left is unknown.");

	/// <summary>
	/// Jumps over <paramref name="count"/> bytes. On a non-seekable stream they are read into a small reused buffer.
	/// </summary>
	internal void Skip(int count)
	{
		if (count < 0)
			throw new ArgumentOutOfRangeException(nameof(count));

		if (_canSeek)
		{
			if (count > Remaining)
				throw new EndOfStreamException("Reached the end of the stream.");
			_stream.Seek(count, SeekOrigin.Current);
			_position += count;
			return;
		}

		var buffer = SharedBuffer();
		while (count > 0)
		{
			var read = _stream.Read(buffer, 0, Math.Min(count, buffer.Length));
			if (read <= 0)
				throw new EndOfStreamException("Reached the end of the stream.");
			_position += read;
			count -= read;
		}
	}

	protected override byte InternalReadByte()
	{
		var value = _stream.ReadByte();
		if (value < 0)
			throw new EndOfStreamException("Reached the end of the stream.");
		_position++;
		return (byte)value;
	}

	protected override ReadOnlySpan<byte> InternalReadSpan(int count)
	{
		byte[] buffer;
		if (count <= SharedBufferSize)
			buffer = SharedBuffer();
		else if (_largeBuffer != null && _largeBuffer.Length >= count)
			buffer = _largeBuffer;
		else
			buffer = _largeBuffer = new byte[count];

		ReadExactly(buffer, 0, count);
		return new ReadOnlySpan<byte>(buffer, 0, count);
	}

	public override ReadOnlySpan<byte> ReadSpan(int count) => InternalReadSpan(count);

	public override ReadOnlyMemory<byte> ReadMemory(int count) => ReadBytes(count);

	public override byte[] ReadBytes(int count)
	{
		var result = new byte[count];
		ReadExactly(result, 0, count);
		return result;
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		var read = _stream.Read(buffer, offset, count);
		_position += read;
		return read;
	}

	private void ReadExactly(byte[] buffer, int offset, int count)
	{
		while (count > 0)
		{
			var read = _stream.Read(buffer, offset, count);
			if (read <= 0)
				throw new EndOfStreamException("Reached the end of the stream.");
			_position += read;
			offset += read;
			count -= read;
		}
	}

	private static byte[] SharedBuffer() => _sharedBuffer ??= new byte[SharedBufferSize];
}
