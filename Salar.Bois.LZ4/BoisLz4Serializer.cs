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

				var compressedBuff = LZ4Pickler.Pickle(serializedBuff, 0, length, lz4Level);
				output.Write(compressedBuff, 0, length);

				//var outputBuff = new byte[LZ4Codec.MaximumOutputSize(length)];
				//var finalLength = LZ4Codec.Encode(serializedBuff, 0, length, outputBuff, 0, outputBuff.Length, lz4Level);

				//if (finalLength + 1 > length)
				//{
				//	// TODO: update the very first index byte and flag compressed as true
				//	WriteCompressedFlag(compressed: true, output);
				//	// compressed size is smaller
				//	output.Write(outputBuff, 0, finalLength);
				//}
				//else
				//{
				//	WriteCompressedFlag(compressed: true, output);

				//	// compressed size is not small enough
				//	output.Write(serializedBuff, 0, length);
				//}
			}
		}

		public T Deserialize<T>(Stream objectData)
		{
			using (var mem = new MemoryStream())
			{
				objectData.CopyTo(mem);

				var compressedBuff = mem.GetBuffer();
				var length = (int)mem.Length;

				var serializedBuff = LZ4Pickler.Unpickle(compressedBuff, 0, length);

				mem.Position = 0;
				mem.SetLength(serializedBuff.Length);
				mem.Write(serializedBuff, 0, serializedBuff.Length);
				mem.Position = 0;

				return _serializer.Deserialize<T>(mem);
			}
		}
		public T Deserialize_Temp<T>(Stream objectData)
		{
			// TODO: read the very first index byte and check the flag if the buffer is compressed

			var isCompressed = ReadCompressedFlag(objectData);
			if (isCompressed)
			{
				using (var mem = new MemoryStream())
				{
					objectData.CopyTo(mem);

					var compressedBuff = mem.GetBuffer();
					var length = (int)mem.Length;



					//LZ4Codec.Decode(compressedBuff, 0, compressedBuff.Length);
				}
			}
			else
			{
				return _serializer.Deserialize<T>(objectData);
			}
			throw new NotImplementedException();
		}

		/// <summary>
		/// 0010 0000 = 32 -- data is compressed
		/// </summary>
		internal const byte FlagCompressed = 0b0_0_1_0_0_0_0_0;

		/// <summary>
		/// 0000 0000 = 0 -- data is not compressed
		/// </summary>
		internal const byte FlagUncompressed = 0b0_0_0_0_0_0_0_0;

		private void WriteCompressedFlag(bool compressed, Stream stream)
		{
			if (compressed)
				stream.WriteByte(FlagCompressed);
			else
				stream.WriteByte(FlagUncompressed);
		}

		private bool ReadCompressedFlag(Stream stream)
		{
			return stream.ReadByte() == FlagCompressed;
		}

	}
}
