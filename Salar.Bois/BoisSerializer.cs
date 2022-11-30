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

		/// <summary>
		/// Character encoding for strings.
		/// </summary>
		public Encoding Encoding { get; set; }

		/// <summary>
		/// Initializing a new instance of Bois serializer.
		/// </summary>
		public BoisSerializer()
		{
			Encoding = Encoding.UTF8;
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
		/// <param name="output">The output of the serialization in binary.</param>
		/// <typeparam name="T">The object type.</typeparam>
		public void Serialize<T>(T obj, Stream output)
		{
			var writer = new StreamBufferWriter(output);

			Serialize(obj, writer);
		}

		/// <summary>
		/// Serializing an object to binary bois format.
		/// </summary>
		/// <param name="obj">The object to be serialized.</param>
		/// <param name="bufferWriter"></param>
		/// <typeparam name="T">The object type.</typeparam>
		public void Serialize<T>(T obj, IBufferWriter bufferWriter)
		{
			if (obj == null)
				throw new ArgumentNullException(nameof(obj), "Object cannot be null.");

			var type = typeof(T);
			var typeInfo = BoisTypeCache.GetBasicType(type);
			if (typeInfo.AsRootNeedsCompute)
			{
				var computedType = BoisTypeCache.GetRootTypeComputed(type, false, true);

				computedType.InvokeWriter(bufferWriter, obj, Encoding);
			}
			else
			{
				PrimitiveWriter.WriteRootBasicType(bufferWriter, obj, type, typeInfo, Encoding);
			}
		}
		/// <summary>
		/// Serializing an object to binary bois format.
		/// </summary>
		/// <param name="obj">The object to be serialized.</param>
		/// <param name="type">The object type.</param>
		/// <param name="output">The output of the serialization in binary.</param>
		public void Serialize(object obj, Type type, Stream output)
		{
			var writer = new StreamBufferWriter(output);

			Serialize(obj, type, writer);
		}

		/// <summary>
		/// Serializing an object to binary bois format.
		/// </summary>
		/// <param name="obj">The object to be serialized.</param>
		/// <param name="type">The object type.</param>
		/// <param name="bufferWriter">The writer to output of the serialization</param>
		public void Serialize(object obj, Type type, IBufferWriter bufferWriter)
		{
			if (obj == null)
				throw new ArgumentNullException(nameof(obj), "Object cannot be null.");

			var typeInfo = BoisTypeCache.GetBasicType(type);
			if (typeInfo.AsRootNeedsCompute)
			{
				var computedType = BoisTypeCache.GetRootTypeComputed(type, false, true);

				// ReSharper disable once PossibleNullReferenceException
				var invokeMethod = typeof(BoisComputedTypeInfo).GetMethod(nameof(BoisComputedTypeInfo.InvokeWriter),
						BindingFlags.Instance | BindingFlags.NonPublic)
					.MakeGenericMethod(type);

				invokeMethod.Invoke(computedType, new object[] { bufferWriter, obj, Encoding });
			}
			else
			{
				PrimitiveWriter.WriteRootBasicType(bufferWriter, obj, type, typeInfo, Encoding);
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
			var reader = new BinaryBufferReader(buffer, position, length);

			return Deserialize<T>(reader);
		}

		/// <summary>
		/// Deserializing binary data to a new instance.
		/// </summary>
		/// <param name="objectData">The binary data.</param>
		/// <typeparam name="T">The object type.</typeparam>
		/// <returns>New instance of the deserialized data.</returns>
		public T Deserialize<T>(Stream objectData)
		{
			BufferReaderBase reader = null;
			if (objectData is MemoryStream memoryStream)
			{
				if (memoryStream.TryGetBuffer(out var buffer))
					reader = new BinaryBufferReader(buffer);
			}
			reader ??= new StreamBufferReader(objectData);

			return Deserialize<T>(reader);
		}

		/// <summary>
		/// Deserializing binary data to a new instance.
		/// </summary>
		/// <param name="bufferReader"></param>
		/// <typeparam name="T">The object type.</typeparam>
		/// <returns>New instance of the deserialized data.</returns>
		public T Deserialize<T>(BufferReaderBase bufferReader)
		{
			var type = typeof(T);
			var typeInfo = BoisTypeCache.GetBasicType(type);

			if (typeInfo.AsRootNeedsCompute)
			{
				var computedType = BoisTypeCache.GetRootTypeComputed(type, true, false);

				return computedType.InvokeReader<T>(bufferReader, Encoding);
			}
			else
			{
				return (T)PrimitiveReader.ReadRootBasicType(bufferReader, type, typeInfo, Encoding);
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
				if (memoryStream.TryGetBuffer(out var buffer))
					reader = new BinaryBufferReader(buffer);
			}
			reader ??= new StreamBufferReader(objectData);

			return Deserialize(reader, type);
		}

		/// <summary>
		/// Deserializing binary data to a new instance.
		/// </summary>
		/// <param name="objectData">The binary data.</param>
		/// <param name="type">The object type.</param>
		/// <returns>New instance of the deserialized data.</returns>
		public object Deserialize(BufferReaderBase bufferReader, Type type)
		{
			var typeInfo = BoisTypeCache.GetBasicType(type);

			if (typeInfo.AsRootNeedsCompute)
			{
				var computedType = BoisTypeCache.GetRootTypeComputed(type, true, false);

				// ReSharper disable once PossibleNullReferenceException
				var invokeMethod = typeof(BoisComputedTypeInfo).GetMethod(nameof(BoisComputedTypeInfo.InvokeReader),
					BindingFlags.Instance | BindingFlags.NonPublic)
					.MakeGenericMethod(type);

				return invokeMethod.Invoke(computedType, new object[] { bufferReader, Encoding });
			}
			else
			{
				return PrimitiveReader.ReadRootBasicType(bufferReader, type, typeInfo, Encoding);
			}
		}
	}
}
