using System;
using System.IO;
using K4os.Compression.LZ4;

namespace Salar.Bois.LZ4
{
	public class Lz4BoisSerializer
	{
		private readonly BoisSerializer _serializer;

		public Lz4BoisSerializer()
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

		public void Serialize<T>(T obj, Stream output)
		{
			Serialize(obj, output, LZ4Level.L00_FAST);
		}

		public void Serialize<T>(T obj, Stream output, LZ4Level lz4Level)
		{
			using (var mem = new MemoryStream())
			{
				_serializer.Serialize(obj, mem/*, TODO: store compression flag*/);

				var serializedBuff = mem.GetBuffer();
				var length = (int)mem.Length;

				var outputBuff = new byte[LZ4Codec.MaximumOutputSize(length)];
				var finalLength = LZ4Codec.Encode(serializedBuff, 0, length, outputBuff, 0, outputBuff.Length, lz4Level);

				if (finalLength + 1 > length)
				{
					// TODO: update the very first index byte and flag compressed as true

					// compressed size is smaller
					output.Write(outputBuff, 0, finalLength);
				}
				else
				{
					// compressed size is not small enough
					output.Write(serializedBuff, 0, length);
				}
			}
		}

		public T Deserialize<T>(Stream objectData)
		{
			// TODO: read the very first index byte and check the flag if the buffer is compressed

			throw new NotImplementedException();
		}
	}
}
