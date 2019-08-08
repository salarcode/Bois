using System;
using System.IO;
using System.Runtime.CompilerServices;
using K4os.Compression.LZ4;

namespace Salar.Bois.LZ4
{
	/// <summary>
	/// LZ4 compression wrapper for Salar BOIS
	/// </summary>
	public class BoisLz4Serializer
	{
		private readonly BoisSerializer _serializer;

		public BoisLz4Serializer()
		{
			_serializer = new BoisSerializer();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Initialize(params Type[] types)
		{
			BoisSerializer.Initialize(types);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Initialize<T>()
		{
			BoisSerializer.Initialize<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ClearCache()
		{
			BoisSerializer.ClearCache();
		}

		/// <summary>
		/// Serializing an object to binary bois format, then compresses it using LZ4 pickle self-contained format. Compression level is set to FAST.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Pickle<T>(T obj, Stream output)
		{
			Pickle(obj, output, LZ4Level.L00_FAST);
		}

		/// <summary>
		/// Serializing an object to binary bois format, then compresses it using LZ4 pickle self-contained format.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <param name="output"></param>
		/// <param name="lz4Level">Compression level</param>
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

		/// <summary>
		/// Deserializing binary data to a new instance. Decompression is using LZ4 pickle self-contained format.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="objectData">Compressed data expected</param>
		/// <returns></returns>
		public T Unpickle<T>(Stream objectData)
		{
			int length = 0;
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
						length = (int)outMem.Length;
						offset = (int)outMem.Position;
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

				var serializedBuff = LZ4Pickler.Unpickle(compressedBuff, offset, length);

				mem.Dispose();
				mem = new MemoryStream(serializedBuff);

				return _serializer.Deserialize<T>(mem);
			}
		}
	}
}
