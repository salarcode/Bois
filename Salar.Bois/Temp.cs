using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salar.Bois;
internal class Online
{
	internal class MemStream
	{
		internal byte[] _buffer;    // Either allocated internally or externally.
		internal readonly int _origin;       // For user-provided arrays, start at this origin
		internal int _position;     // read/write head.
		internal int _length;       // Number of bytes within the memory stream

	}

	private BinaryBufferReader GetBinaryBufferReader(MemStream mem)
	{
		return new BinaryBufferReader(mem._buffer, mem._origin, mem._length - mem._origin);
	}

	public sealed class BinaryBufferReader
	{
		public BinaryBufferReader(byte[] data, int offset, int length)
		{

		}
	}
}

