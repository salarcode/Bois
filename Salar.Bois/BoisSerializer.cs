using Salar.BinaryBuffers;
using Salar.BinaryBuffers.Compatibility;
using Salar.Bois.Serializers;
using Salar.Bois.Types;
using System;
using System.IO;
using System.Reflection;
using System.Text;

/* 
 * Salar BOIS (Binary Object Indexed Serialization)
 * by Salar Khalilzadeh
 * 
 * https://github.com/salarcode/Bois
 * Mozilla Public License v2
 */
namespace Salar.Bois
{
	/// <summary>
	/// Salar.Bois serializer.
	/// Which provides binary serialization and deserialization for .NET objects.
	/// BOIS stands for 'Binary Object Indexed Serialization'.
	/// </summary>
	/// <Author>
	/// Salar Khalilzadeh
	/// </Author>
	public class BoisSerializer
	{
		static BoisTypeCompiler.GetBufferReaderFromMemoryStream _getBufferReaderFromMemoryStream;

		/// <summary>
		/// Character encoding for strings.
		/// </summary>
		public Encoding Encoding { get; set; }

        public static Encoding DefaultEncoding
        {
            get;
            set => field = value ?? throw new ArgumentNullException(nameof(value));
        } = Encoding.UTF8;

        /// <summary>
        /// Arrays and collections declaring up to this many items are read without checking the data left, so a forged
        /// count can't make the reader allocate more than this many items. Declaring more must fit in the data left,
        /// which only readers that know their length (not non-seekable streams) can tell, otherwise reading fails.
        /// Raise it to read bigger arrays from such streams. Default is 1,000,000.
        /// </summary>
        public static int MaxUnverifiedCount
        {
            get => PrimitiveReader.MaxUnverifiedCount;
            set => PrimitiveReader.MaxUnverifiedCount = value;
        }

        /// <summary>
        /// Same as <see cref="MaxUnverifiedCount"/> for the byte length of strings and byte arrays. Default is 16 MB.
        /// </summary>
        public static int MaxUnverifiedByteLength
        {
            get => PrimitiveReader.MaxUnverifiedByteLength;
            set => PrimitiveReader.MaxUnverifiedByteLength = value;
        }

        /// <summary>
        /// The largest byte size a versioned object (<c>[BoisContract(Versioned = true)]</c>) may declare in the data.
        /// The size must also fit in the data left when the reader knows it. Default is <see cref="int.MaxValue"/>,
        /// because the size is only used to jump over data, never to allocate memory.
        /// </summary>
        public static int MaxVersionedObjectLength
        {
            get => PrimitiveReader.MaxVersionedObjectLength;
            set => PrimitiveReader.MaxVersionedObjectLength = value;
        }

        /// <summary>
        /// Initializing a new instance of Bois serializer.
        /// </summary>
        public BoisSerializer()
		{
			Encoding = DefaultEncoding;
		}

		static BoisSerializer()
		{
			_getBufferReaderFromMemoryStream = BoisTypeCompiler.ComputeBufferReaderFromMemoryStream();
		}

		public static void Initialize<T>()
		{
			BoisTypeCache.GetRootTypeComputed(typeof(T), true, true);
		}

		public static void Initialize(params Type[] types)
		{
			if (types == null)
				return;
			foreach (var type in types)
			{
				if (type != null)
					BoisTypeCache.GetRootTypeComputed(type, true, true
#if EmitAssemblyOut && !NETCOREAPP
						, outputAssembly: false
#endif
					);
			}

#if EmitAssemblyOut && !NETCOREAPP
			BoisTypeCompiler.SaveAssemblyOutput_Writer();
			BoisTypeCompiler.SaveAssemblyOutput_Reader();
#endif
		}

		/// <summary>
		/// Use with caution, all the calculations will be lost then might be calculated again once needed
		/// </summary>
		public static void ClearCache()
		{
			BoisTypeCache.ClearCache();
		}

		/// <summary>
		/// Serializing an object to binary bois format.
		/// </summary>
		/// <param name="obj">The object to be serialized.</param>
		/// <param name="output">The binary data.</param>
		/// <param name="position"></param>
		/// <param name="length"></param>
		/// <typeparam name="T">The object type.</typeparam>
		public void Serialize<T>(T obj, byte[] output, int position, int length)
		{
#if NET9_0_OR_GREATER
			var writer = new BinarySpanBufferWriter(new Span<byte>(output, position, length));
			Serialize<T, BinarySpanBufferWriter>(obj, ref writer);
#else
			var writer = new BinaryBufferWriter(output, position, length);
			Serialize<T, BinaryBufferWriter>(obj, ref writer);
#endif
		}
		/// <summary>
		/// Serializing an object to binary bois format.
		/// </summary>
		/// <param name="obj">The object to be serialized.</param>
		/// <param name="output">The output of the serialization in binary.</param>
		/// <typeparam name="T">The object type.</typeparam>
		public void Serialize<T>(T obj, Stream output)
		{
			if (VersionedRoot<T>.Contains)
			{
				SerializeVersioned(obj, output);
				return;
			}

			var writer = new StreamBufferWriter(output);

			Serialize<T, StreamBufferWriter>(obj, ref writer);
		}

		/// <summary>
		/// Versioned types write their size after their members, so the bytes are held in the writer's buffer until
		/// the size is filled in.
		/// </summary>
		private void SerializeVersioned<T>(T obj, Stream output)
		{
			var writer = BoisBufferWriter.Rent(output);

			Serialize<T, BoisBufferWriter>(obj, ref writer);
			writer.Return();
		}

		/// <summary>
		/// Serializing an object to binary bois format.
		/// </summary>
		/// <param name="obj">The object to be serialized.</param>
		/// <param name="bufferWriter"></param>
		/// <typeparam name="T">The object type.</typeparam>
		/// <typeparam name="TWriter">The buffer writer type of BufferWriterBase or IBufferWriter.</typeparam>
		public void Serialize<T, TWriter>(T obj, ref TWriter bufferWriter)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
		{
			if (obj == null)
				throw new ArgumentNullException(nameof(obj), "Object cannot be null.");

			var type = typeof(T);
			var typeInfo = BoisTypeCache.GetBasicType(type);
			if (typeInfo.AsRootNeedsCompute)
			{
				var computedType = BoisTypeCache.GetRootTypeComputed(type, false, true, typeof(TWriter));

				computedType.InvokeWriter(ref bufferWriter, obj, Encoding);
			}
			else
			{
				PrimitiveWriter.WriteRootBasicType(ref bufferWriter, obj, type, typeInfo, Encoding);
			}
		}
		/// <summary>
		/// Serializing an object to binary bois format.
		/// Prefer to use `byte[]` overload for better performance.
		/// </summary>
		/// <param name="obj">The object to be serialized.</param>
		/// <param name="type">The object type.</param>
		/// <param name="output">The output of the serialization in binary.</param>
		public void Serialize(object obj, Type type, Stream output)
		{
			if (BoisTypeCache.ContainsVersioned(type))
			{
				var versionedWriter = BoisBufferWriter.Rent(output);
				Serialize(obj, type, versionedWriter);
				versionedWriter.Return();
				return;
			}

            var writer = new StreamBufferWriter(output);

            Serialize(obj, type, writer);
		}

		/// <summary>
		/// Serializing an object to binary bois format.
		/// </summary>
		/// <param name="obj">The object to be serialized.</param>
		/// <param name="type">The object type.</param>
		/// <param name="bufferWriter">The writer to output of the serialization</param>
		public void Serialize(object obj, Type type, BufferWriterBase bufferWriter)
		{
			if (obj == null)
				throw new ArgumentNullException(nameof(obj), "Object cannot be null.");

			var typeInfo = BoisTypeCache.GetBasicType(type);
			if (typeInfo.AsRootNeedsCompute)
			{
				var computedType = BoisTypeCache.GetRootTypeComputed(type, false, true, typeof(BufferWriterBase));

				// ReSharper disable once PossibleNullReferenceException
				var invokeMethod = typeof(BoisComputedTypeInfo).GetMethod(nameof(BoisComputedTypeInfo.InvokeWriter),
						BindingFlags.Instance | BindingFlags.NonPublic)
					.MakeGenericMethod(type, typeof(BufferWriterBase));

				invokeMethod.Invoke(computedType, new object[] { bufferWriter, obj, Encoding });
			}
			else
			{
				PrimitiveWriter.WriteRootBasicType(ref bufferWriter, obj, type, typeInfo, Encoding);
			}
		}

		/// <summary>
		/// Deserializing binary data to a new instance.
		/// </summary>
		/// <param name="buffer">The binary data.</param>
		/// <param name="position"></param>
		/// <param name="length"></param>
		/// <typeparam name="T">The object type.</typeparam>
		/// <returns>New instance of the deserialized data.</returns>
		public T Deserialize<T>(byte[] buffer, int position, int length)
		{
#if NET9_0_OR_GREATER
			var reader = new BinarySpanBufferReader(new ReadOnlySpan<byte>(buffer, position, length));
			return Deserialize<T, BinarySpanBufferReader>(ref reader);
#else
			var reader = new BinaryBufferReader(buffer, position, length);
			return Deserialize<T, BinaryBufferReader>(ref reader);
#endif
		}

		/// <summary>
		/// Deserializing binary data to a new instance.
		/// Prefer to use `byte[]` overload for better performance.
		/// </summary>
		/// <param name="objectData">The binary data.</param>
		/// <typeparam name="T">The object type.</typeparam>
		/// <returns>New instance of the deserialized data.</returns>
		public T Deserialize<T>(Stream objectData)
		{
			BufferReaderBase reader = null;
			if (objectData is MemoryStream memoryStream)
			{
				reader = _getBufferReaderFromMemoryStream(memoryStream);
			}
			else if (VersionedRoot<T>.Contains)
			{
				// versioned types need a reader that counts its position and skips without allocating
				reader = new BoisStreamReader(objectData);
			}
			else
			{
				reader ??= new StreamBufferReader(objectData);
			}

			return Deserialize<T, BufferReaderBase>(ref reader);
		}

		/// <summary>
		/// Deserializing binary data to a new instance.
		/// </summary>
		/// <param name="bufferReader"></param>
		/// <typeparam name="T">The object type.</typeparam>
		/// <typeparam name="TReader">The buffer reader type of BufferReaderBase or IBufferReader.</typeparam>
		/// <returns>New instance of the deserialized data.</returns>
		public T Deserialize<T, TReader>(ref TReader bufferReader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
		{
			var type = typeof(T);
			var typeInfo = BoisTypeCache.GetBasicType(type);

			if (typeInfo.AsRootNeedsCompute)
			{
				var computedType = BoisTypeCache.GetRootTypeComputed(type, true, false, typeof(TReader));

				return computedType.InvokeReader<T, TReader>(ref bufferReader, Encoding);
			}
			else
			{
				return (T)PrimitiveReader.ReadRootBasicType(ref bufferReader, type, typeInfo, Encoding);
			}
		}

		/// <summary>
		/// Deserializing binary data to a new instance.
		/// </summary>
		/// <param name="objectData">The binary data.</param>
		/// <param name="type">The object type.</param>
		/// <returns>New instance of the deserialized data.</returns>
		public object Deserialize(Stream objectData, Type type)
		{
			BufferReaderBase reader = null;
			if (objectData is MemoryStream memoryStream)
			{
				reader = _getBufferReaderFromMemoryStream(memoryStream);
			}
			else if (BoisTypeCache.ContainsVersioned(type))
			{
				reader = new BoisStreamReader(objectData);
			}
			else
			{
				reader ??= new StreamBufferReader(objectData);
			}
			return Deserialize(reader, type);
		}

		/// <summary>
		/// Deserializing binary data to a new instance.
		/// </summary>
		/// <param name="bufferReader">The buffer reader.</param>
		/// <param name="type">The object type.</param>
		/// <returns>New instance of the deserialized data.</returns>
		public object Deserialize(BufferReaderBase bufferReader, Type type)
		{
			var typeInfo = BoisTypeCache.GetBasicType(type);

			if (typeInfo.AsRootNeedsCompute)
			{
				var computedType = BoisTypeCache.GetRootTypeComputed(type, true, false, typeof(BufferReaderBase));

				// ReSharper disable once PossibleNullReferenceException
				var invokeMethod = typeof(BoisComputedTypeInfo).GetMethod(nameof(BoisComputedTypeInfo.InvokeReader),
					BindingFlags.Instance | BindingFlags.NonPublic)
					.MakeGenericMethod(type, typeof(BufferReaderBase));

				return invokeMethod.Invoke(computedType, [bufferReader, Encoding]);
			}
			else
			{
				return PrimitiveReader.ReadRootBasicType(ref bufferReader, type, typeInfo, Encoding);
			}
		}

		/// <summary>
		/// Whether <typeparamref name="T"/> contains a versioned type, worked out once per type.
		/// </summary>
		private static class VersionedRoot<T>
		{
			internal static readonly bool Contains = BoisTypeCache.ContainsVersioned(typeof(T));
		}
	}
}
