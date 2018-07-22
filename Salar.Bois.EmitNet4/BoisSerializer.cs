using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using Salar.Bois.Serializers;
using Salar.Bois.Types;

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
		private BoisTypeCache _typeCache;

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
			_typeCache = new BoisTypeCache();
		}

		public void Initialize<T>()
		{
			_typeCache.GetRootTypeComputed(typeof(T), true, true);
		}

		public void Initialize(params Type[] types)
		{
			foreach (var type in types)
			{
				_typeCache.GetRootTypeComputed(type, true, true);
			}
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
			if (_typeCache.IsPrimitveType(type))
			{
				WriteRootPrimtiveValue(obj, type, writer);
			}
			else
			{
				var computedType = _typeCache.GetRootTypeComputed(type, false, true);

				computedType.InvokeWriter(writer, obj, Encoding);
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
			if (_typeCache.IsPrimitveType(type))
			{
				return (T)ReadRootPrimtiveValue(type, reader);
			}
			var computedType = _typeCache.GetRootTypeComputed(type, true, false);

			return computedType.InvokeReader<T>(reader, Encoding);
		}

		private object ReadRootPrimtiveValue(Type memType, BinaryReader reader)
		{
			if (memType == typeof(string))
			{
				return PrimitiveReader.ReadString(reader, Encoding);
			}
			Type memActualType = memType;
			Type underlyingTypeNullable;
			bool isNullable = ReflectionHelper.IsNullable(memType, out underlyingTypeNullable);

			// check the underling type
			if (isNullable && underlyingTypeNullable != null)
			{
				memActualType = underlyingTypeNullable;
			}
			else
			{
				underlyingTypeNullable = null;
			}

			if (memActualType == typeof(char))
			{
				if (isNullable)
					return PrimitiveReader.ReadCharNullable(reader);
				return PrimitiveReader.ReadChar(reader);
			}
			if (memActualType == typeof(bool))
			{
				if (isNullable)
					return PrimitiveReader.ReadBooleanNullable(reader);
				return PrimitiveReader.ReadBoolean(reader);
			}
			if (memActualType == typeof(DateTime))
			{
				if (isNullable)
					return PrimitiveReader.ReadDateTimeNullable(reader);
				return PrimitiveReader.ReadDateTime(reader);
			}
			if (memActualType == typeof(DateTimeOffset))
			{
				if (isNullable)
					return PrimitiveReader.ReadDateTimeOffsetNullable(reader);
				return PrimitiveReader.ReadDateTimeOffset(reader);
			}
			if (memActualType == typeof(byte[]))
			{
				return PrimitiveReader.ReadByteArray(reader);
			}
			if (ReflectionHelper.CompareSubType(memActualType, typeof(Enum)))
			{
				return PrimitiveReader.ReadEnum(reader);
			}

			object readNumber;
			var isANumber = ReadRootPrimtiveNumberValue(reader, memActualType, isNullable, out readNumber);
			if (isANumber)
				return readNumber;

			if (memActualType == typeof(TimeSpan))
			{
				if (isNullable)
					return PrimitiveReader.ReadTimeSpanNullable(reader);
				return PrimitiveReader.ReadTimeSpan(reader);
			}

			if (memActualType == typeof(Version))
			{
				return PrimitiveReader.ReadVersion(reader);
			}
			if (memActualType == typeof(Guid))
			{
				if (isNullable)
					return PrimitiveReader.ReadGuidNullable(reader);
				return PrimitiveReader.ReadGuid(reader);
			}
#if DotNet || DotNetCore || DotNetStandard
			if (memActualType == typeof(DBNull))
			{
				return PrimitiveReader.ReadDBNull(reader);
			}
#endif
			if (ReflectionHelper.CompareSubType(memActualType, typeof(Array)))
			{
				var arrayItemType = memActualType.GetElementType();

				// only supported if the item type of array is primitive type, not a complex one
				if (_typeCache.IsPrimitveType(arrayItemType))
				{
					return ReadRootPrimtiveArrayValue(reader, arrayItemType);
				}
			}

			// this should never run, it should be prevented in IsPrimitveType method
			throw new InvalidOperationException($"'{memActualType}' is not supported primitive type");
		}

		private object ReadRootPrimtiveArrayValue(BinaryReader reader, Type arrayItemType)
		{
			var length = PrimitivesConvertion.ReadVarUInt32Nullable(reader);

			if (length == null)
			{
				return null;
			}

			var result = Array.CreateInstance(arrayItemType, (int)length.Value);

			for (int i = 0; i < length; i++)
			{
				var item = ReadRootPrimtiveValue(arrayItemType, reader);
				result.SetValue(item, i);
			}
			return result;
		}

		private bool ReadRootPrimtiveNumberValue(BinaryReader reader, Type memType, bool isNullable, out object outNumber)
		{
			outNumber = null;
			if (memType.IsClass)
			{
				return false;
			}
			if (memType == typeof(int))
			{
				if (isNullable)
					outNumber = PrimitivesConvertion.ReadVarInt32Nullable(reader);
				else
					outNumber = PrimitivesConvertion.ReadVarInt32(reader);

				return true;
			}
			else if (memType == typeof(long))
			{
				if (isNullable)
					outNumber = PrimitivesConvertion.ReadVarInt64Nullable(reader);
				else
					outNumber = PrimitivesConvertion.ReadVarInt64(reader);

				return true;
			}
			else if (memType == typeof(short))
			{
				if (isNullable)
					outNumber = PrimitivesConvertion.ReadVarInt16Nullable(reader);
				else
					outNumber = PrimitivesConvertion.ReadVarInt16(reader);

				return true;
			}
			else if (memType == typeof(double))
			{
				if (isNullable)
					outNumber = PrimitivesConvertion.ReadVarDoubleNullable(reader);
				else
					outNumber = PrimitivesConvertion.ReadVarDouble(reader);

				return true;
			}
			else if (memType == typeof(decimal))
			{
				if (isNullable)
					outNumber = PrimitivesConvertion.ReadVarDecimalNullable(reader);
				else
					outNumber = PrimitivesConvertion.ReadVarDecimal(reader);
				return true;
			}
			else if (memType == typeof(float))
			{
				if (isNullable)
					outNumber = PrimitivesConvertion.ReadVarSingleNullable(reader);
				else
					outNumber = PrimitivesConvertion.ReadVarSingle(reader);
				return true;
			}
			else if (memType == typeof(byte))
			{
				if (isNullable)
					outNumber = PrimitivesConvertion.ReadVarByteNullable(reader);
				else
					outNumber = reader.ReadByte();
				return true;
			}
			else if (memType == typeof(sbyte))
			{
				if (isNullable)
					outNumber = (sbyte?)PrimitivesConvertion.ReadVarByteNullable(reader);
				else
					outNumber = reader.ReadSByte();
				return true;
			}
			else if (memType == typeof(ushort))
			{
				if (isNullable)
					outNumber = PrimitivesConvertion.ReadVarUInt16Nullable(reader);
				else
					outNumber = PrimitivesConvertion.ReadVarUInt16(reader);
				return true;
			}
			else if (memType == typeof(uint))
			{
				if (isNullable)
					outNumber = PrimitivesConvertion.ReadVarUInt32Nullable(reader);
				else
					outNumber = PrimitivesConvertion.ReadVarUInt32(reader);
				return true;
			}
			else if (memType == typeof(ulong))
			{
				if (isNullable)
					outNumber = PrimitivesConvertion.ReadVarUInt64Nullable(reader);
				else
					outNumber = PrimitivesConvertion.ReadVarUInt64(reader);
				return true;
			}

			return false;
		}


		private void WriteRootPrimtiveValue(object obj, Type memType, BinaryWriter writer)
		{
			if (memType == typeof(string))
			{
				PrimitiveWriter.WriteValue(writer, obj as string, Encoding);
				return;
			}
			Type memActualType = memType;
			Type underlyingTypeNullable;
			bool isNullable = ReflectionHelper.IsNullable(memType, out underlyingTypeNullable);

			// check the underling type
			if (isNullable && underlyingTypeNullable != null)
			{
				memActualType = underlyingTypeNullable;
			}
			else
			{
				underlyingTypeNullable = null;
			}


			if (memActualType == typeof(char))
			{
				if (isNullable)
					PrimitiveWriter.WriteValue(writer, (char?)obj);
				else
					PrimitiveWriter.WriteValue(writer, (char)obj);
				return;
			}
			if (memActualType == typeof(bool))
			{
				if (isNullable)
					PrimitiveWriter.WriteValue(writer, (bool?)obj);
				else
					PrimitiveWriter.WriteValue(writer, (bool)obj);
				return;
			}
			if (memActualType == typeof(DateTime))
			{
				if (isNullable)
					PrimitiveWriter.WriteValue(writer, (DateTime?)obj);
				else
					PrimitiveWriter.WriteValue(writer, (DateTime)obj);
				return;
			}
			if (memActualType == typeof(DateTimeOffset))
			{
				if (isNullable)
					PrimitiveWriter.WriteValue(writer, (DateTimeOffset?)obj);
				else
					PrimitiveWriter.WriteValue(writer, (DateTimeOffset)obj);
				return;
			}
			if (memActualType == typeof(byte[]))
			{
				PrimitiveWriter.WriteValue(writer, (byte[])obj);
				return;
			}
			if (ReflectionHelper.CompareSubType(memActualType, typeof(Enum)))
			{
				PrimitiveWriter.WriteValue(writer, (Enum)obj);
				return;
			}

			var isANumber = WriteRootPrimtiveNumberValue(writer, obj, memActualType, isNullable);
			if (isANumber)
				return;

			if (memActualType == typeof(TimeSpan))
			{
				if (isNullable)
					PrimitiveWriter.WriteValue(writer, (TimeSpan?)obj);
				else
					PrimitiveWriter.WriteValue(writer, (TimeSpan)obj);
				return;
			}

			if (memActualType == typeof(Version))
			{
				PrimitiveWriter.WriteValue(writer, (Version)obj);
				return;
			}
			if (memActualType == typeof(Guid))
			{
				if (isNullable)
					PrimitiveWriter.WriteValue(writer, (Guid?)obj);
				else
					PrimitiveWriter.WriteValue(writer, (Guid)obj);
				return;
			}
#if DotNet || DotNetCore || DotNetStandard
			if (memActualType == typeof(DBNull))
			{
				PrimitiveWriter.WriteValue(writer, (DBNull)obj);
				return;
			}
#endif
			if (ReflectionHelper.CompareSubType(memActualType, typeof(Array)))
			{
				var arrayItemType = memActualType.GetElementType();

				// only supported if the item type of array is primitive type, not a complex one
				if (_typeCache.IsPrimitveType(arrayItemType))
				{
					WriteRootPrimtiveArrayValue(writer, arrayItemType, (Array)obj);
					return;
				}
			}

			// this should never run, it should be prevented in IsPrimitveType method
			throw new InvalidOperationException($"'{memActualType}' is not supported primitive type");
		}

		/// <summary>
		/// Array - Format: (Length:Embedded-Nullable-0-0-0-0-0-0) [if length not embedded?0-0-0-0-0-0-0-0] (the data of array)
		/// Embeddable length range: 0..63
		/// </summary>
		private void WriteRootPrimtiveArrayValue(BinaryWriter writer, Type arrayItemType, Array array)
		{
			if (array == null)
			{
				PrimitiveWriter.WriteNullableType(writer, true);
				return;
			}

			// Int32
			PrimitivesConvertion.WriteVarInt(writer, (uint?)array.Length);

			for (int i = 0; i < array.Length; i++)
			{
				WriteRootPrimtiveValue(array.GetValue(i), arrayItemType, writer);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns>true if the value type is a number</returns>
		private bool WriteRootPrimtiveNumberValue(BinaryWriter writer, object obj, Type memType, bool isNullable)
		{
			if (memType.IsClass)
			{
				return false;
			}
			if (memType == typeof(int))
			{
				if (isNullable)
					PrimitivesConvertion.WriteVarInt(writer, (int?)obj);
				else
					PrimitivesConvertion.WriteVarInt(writer, (int)obj);
			}
			else if (memType == typeof(long))
			{
				if (isNullable)
					PrimitivesConvertion.WriteVarInt(writer, (long?)obj);
				else
					PrimitivesConvertion.WriteVarInt(writer, (long)obj);
			}
			else if (memType == typeof(short))
			{
				if (isNullable)
					PrimitivesConvertion.WriteVarInt(writer, (short?)obj);
				else
					PrimitivesConvertion.WriteVarInt(writer, (short)obj);
			}
			else if (memType == typeof(double))
			{
				if (isNullable)
					PrimitivesConvertion.WriteVarDecimal(writer, (double?)obj);
				else
					PrimitivesConvertion.WriteVarDecimal(writer, (double)obj);
			}
			else if (memType == typeof(decimal))
			{
				if (isNullable)
					PrimitivesConvertion.WriteVarDecimal(writer, (decimal?)obj);
				else
					PrimitivesConvertion.WriteVarDecimal(writer, (decimal)obj);
			}
			else if (memType == typeof(float))
			{
				if (isNullable)
					PrimitivesConvertion.WriteVarDecimal(writer, (float?)obj);
				else
					PrimitivesConvertion.WriteVarDecimal(writer, (float)obj);
			}
			else if (memType == typeof(byte))
			{
				if (isNullable)
					PrimitivesConvertion.WriteVarInt(writer, (byte?)obj);
				else
					writer.Write((byte)obj);
			}
			else if (memType == typeof(sbyte))
			{
				if (isNullable)
					PrimitivesConvertion.WriteVarInt(writer, (sbyte?)obj);
				else
					writer.Write((sbyte)obj);
			}
			else if (memType == typeof(ushort))
			{
				if (isNullable)
					PrimitivesConvertion.WriteVarInt(writer, (ushort?)obj);
				else
					PrimitivesConvertion.WriteVarInt(writer, (ushort)obj);
			}
			else if (memType == typeof(uint))
			{
				if (isNullable)
					PrimitivesConvertion.WriteVarInt(writer, (uint?)obj);
				else
					PrimitivesConvertion.WriteVarInt(writer, (uint)obj);
			}
			else if (memType == typeof(ulong))
			{
				if (isNullable)
					PrimitivesConvertion.WriteVarInt(writer, (ulong?)obj);
				else
					PrimitivesConvertion.WriteVarInt(writer, (ulong)obj);
			}

			return false;
		}


	}
}
