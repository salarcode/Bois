using System;
using System.IO;
using K4os.Compression.LZ4;

namespace Salar.Bois.LZ4
{
	public class BoisLz4Serializer
	{
		private readonly BoisSerializer _serializer;

		public BoisLz4Serializer()
		{
			_serializer = new BoisSerializer();
		}

		public static void Initialize(params Type[] types)
		{
			BoisSerializer.Initialize(types);
		}

		public static void Initialize<T>()
		{
			BoisSerializer.Initialize<T>();
		}

		public static void ClearCache()
		{
			BoisSerializer.ClearCache();
		}

		public void Pickle<T>(T obj, Stream output)
		{
			Pickle(obj, output, LZ4Level.L00_FAST);
		}

		public void Pickle<T>(T obj, Stream output, LZ4Level lz4Level)
		{
			using (var mem = new MemoryStream())
			{
				_serializer.Serialize(obj, mem);

				var serializedBuff = mem.GetBuffer();
				var length = (int)mem.Length;

				var compressedBuff = LZ4Pickler.Pickle(serializedBuff, 0, length, lz4Level);
				output.Write(compressedBuff, 0, compressedBuff.Length);
			}
		}

		public T Unpickle<T>(Stream objectData)
		{
			int length;
			int offset = 0;
			byte[] compressedBuff = null;
			MemoryStream mem;
			using (mem = new MemoryStream())
			{
				if (objectData is MemoryStream outMem)
				{
#if NETCOREAPP || NETSTANDARD2_1
					if (outMem.TryGetBuffer(out var arraySegment))
					{
						compressedBuff = arraySegment.Array;
						length = arraySegment.Count;
						offset = arraySegment.Offset;
					}
#else
					try
					{
						compressedBuff = outMem.GetBuffer();
					}
					catch (UnauthorizedAccessException)
					{
						// eat the error
					}
#endif
				}

				if (compressedBuff == null)
				{
					objectData.CopyTo(mem);
					compressedBuff = mem.GetBuffer();
					length = (int)mem.Length;
					offset = (int)mem.Position;
				}
				else
				{
					length = compressedBuff.Length;
				}

				var serializedBuff = LZ4Pickler.Unpickle(compressedBuff, offset, length);

				mem.Dispose();
				mem = new MemoryStream(serializedBuff);

				return _serializer.Deserialize<T>(mem);
			}
		}
	}
}
