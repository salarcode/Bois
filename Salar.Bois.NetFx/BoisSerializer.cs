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
	/// Which provides binary serialization and deserialzation for .NET objects.
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
		/// Initializing a new instance of Bois serializar.
		/// </summary>
		public BoisSerializer()
		{
			Encoding = Encoding.UTF8;
		}

		public static void Initialize<T>()
		{
			BoisTypeCache.GetRootTypeComputed(typeof(T), true, true);
		}

		public void Initialize(params Type[] types)
		{
			if (types == null)
				return;
			foreach (var type in types)
			{
				if (type != null)
					BoisTypeCache.GetRootTypeComputed(type, true, true
#if EmitAssemblyOut
						, outputAssembly: false
#endif
					);
			}

#if EmitAssemblyOut
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
			if (obj == null)
				throw new ArgumentNullException(nameof(obj), "Object cannot be null.");
			//_serializeDepth = 0;
			var writer = new BinaryWriter(output, Encoding);

			var type = typeof(T);
			var typeInfo = BoisTypeCache.GetBasicType(type);
			if (typeInfo.AsRootNeedsCompute)
			{
				var computedType = BoisTypeCache.GetRootTypeComputed(type, false, true);

				computedType.InvokeWriter(writer, obj, Encoding);
			}
			else
			{
				PrimitiveWriter.WriteRootBasicType(writer, obj, type, typeInfo, Encoding);
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
			if (obj == null)
				throw new ArgumentNullException(nameof(obj), "Object cannot be null.");
			//_serializeDepth = 0;
			var writer = new BinaryWriter(output, Encoding);

			var typeInfo = BoisTypeCache.GetBasicType(type);
			if (typeInfo.AsRootNeedsCompute)
			{
				var computedType = BoisTypeCache.GetRootTypeComputed(type, false, true);

				computedType.InvokeWriter(writer, obj, Encoding);
			}
			else
			{
				PrimitiveWriter.WriteRootBasicType(writer, obj, type, typeInfo, Encoding);
			}
		}


		/// <summary>
		/// Deserilizing binary data to a new instance.
		/// </summary>
		/// <param name="objectData">The binary data.</param>
		/// <typeparam name="T">The object type.</typeparam>
		/// <returns>New instance of the deserialized data.</returns>
		public T Deserialize<T>(Stream objectData)
		{
			var reader = new BinaryReader(objectData, Encoding);

			var type = typeof(T);
			var typeInfo = BoisTypeCache.GetBasicType(type);

			if (typeInfo.AsRootNeedsCompute)
			{
				var computedType = BoisTypeCache.GetRootTypeComputed(type, true, false);

				return computedType.InvokeReader<T>(reader, Encoding);
			}
			else
			{
				return (T)PrimitiveReader.ReadRootBasicType(reader, type, typeInfo, Encoding);
			}
		}

		/// <summary>
		/// Deserilizing binary data to a new instance.
		/// </summary>
		/// <param name="objectData">The binary data.</param>
		/// <param name="type">The object type.</param>
		/// <returns>New instance of the deserialized data.</returns>
		internal object Deserialize(Stream objectData, Type type)
		{
			var reader = new BinaryReader(objectData, Encoding);

			var typeInfo = BoisTypeCache.GetBasicType(type);

			if (typeInfo.AsRootNeedsCompute)
			{
				var computedType = BoisTypeCache.GetRootTypeComputed(type, true, false);

				// ReSharper disable once PossibleNullReferenceException
				var invokeMethod = typeof(BoisComputedTypeInfo).GetMethod(nameof(BoisComputedTypeInfo.InvokeReader),
					BindingFlags.Instance | BindingFlags.NonPublic)
					.MakeGenericMethod(type);

				return invokeMethod.Invoke(computedType, new object[] { reader, Encoding });
			}
			else
			{
				return PrimitiveReader.ReadRootBasicType(reader, type, typeInfo, Encoding);
			}
		}
	}
}
