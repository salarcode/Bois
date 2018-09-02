using Salar.Bois.Types;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;

namespace Salar.Bois.Serializers
{
	internal static class PrimitiveReader
	{
		internal static string ReadString(BinaryReader reader, Encoding encoding)
		{
			int? length = NumericSerializers.ReadVarInt32Nullable(reader);
			if (length == null)
			{
				return null;
			}
			else if (length == 0)
			{
				return String.Empty;
			}
			else
			{
				var strBuff = reader.ReadBytes(length.Value);
				return encoding.GetString(strBuff, 0, strBuff.Length);
			}
		}

		internal static char ReadChar(BinaryReader reader)
		{
			var charByte = reader.ReadUInt16();
			return (char)charByte;
		}

		internal static char? ReadCharNullable(BinaryReader reader)
		{
			var charByte = NumericSerializers.ReadVarUInt16Nullable(reader);
			if (charByte == null)
				return null;
			return (char)charByte.Value;
		}

		internal static bool? ReadBooleanNullable(BinaryReader reader)
		{
			var value = NumericSerializers.ReadVarByteNullable(reader);
			if (value == null)
				return null;
			return value.Value != 0;
		}

		internal static bool ReadBoolean(BinaryReader reader)
		{
			return reader.ReadByte() != 0;
		}

		internal static DateTime? ReadDateTimeNullable(BinaryReader reader)
		{
			var kind = NumericSerializers.ReadVarByteNullable(reader);
			if (kind == null)
				return null;

			var ticks = NumericSerializers.ReadVarInt64(reader);
			if (ticks == 0L)
			{
				return DateTime.MinValue;
			}
			if (ticks == 1L)
			{
				return DateTime.MaxValue;
			}

			return new DateTime(ticks, (DateTimeKind)kind.Value);
		}

		internal static DateTime ReadDateTime(BinaryReader reader)
		{
			var kind = reader.ReadByte();
			var ticks = NumericSerializers.ReadVarInt64(reader);
			if (ticks == 0L)
			{
				return DateTime.MinValue;
			}
			if (ticks == 1L)
			{
				return DateTime.MaxValue;
			}

			return new DateTime(ticks, (DateTimeKind)kind);
		}

		internal static DateTimeOffset? ReadDateTimeOffsetNullable(BinaryReader reader)
		{
			var offsetMinutes = NumericSerializers.ReadVarInt16Nullable(reader);
			if (offsetMinutes == null)
			{
				return null;
			}

			var ticks = NumericSerializers.ReadVarInt64(reader);

			return new DateTimeOffset(ticks, TimeSpan.FromMinutes(offsetMinutes.Value));
		}

		internal static DateTimeOffset ReadDateTimeOffset(BinaryReader reader)
		{
			var offsetMinutes = NumericSerializers.ReadVarInt16(reader);

			var ticks = NumericSerializers.ReadVarInt64(reader);

			return new DateTimeOffset(ticks, TimeSpan.FromMinutes(offsetMinutes));
		}

		internal static byte[] ReadByteArray(BinaryReader reader)
		{
			var length = NumericSerializers.ReadVarUInt32Nullable(reader);
			if (length == null)
			{
				return null;
			}
			return reader.ReadBytes((int)length.Value);
		}

		internal static Enum ReadEnum(BinaryReader reader, Type type)
		{
			var val = NumericSerializers.ReadVarInt32Nullable(reader);
			if (val == null)
				return null;

			return (Enum)Enum.ToObject(type, val);
		}

		internal static T ReadEnumGeneric<T>(BinaryReader reader)
		{
			var val = NumericSerializers.ReadVarInt32Nullable(reader);
			if (val == null)
				return default(T);

			return (T)Enum.ToObject(typeof(T), val);
		}

		internal static TimeSpan? ReadTimeSpanNullable(BinaryReader reader)
		{
			var ticks = NumericSerializers.ReadVarInt64Nullable(reader);
			if (ticks == null)
				return null;

			return new TimeSpan(ticks.Value);
		}

		internal static TimeSpan ReadTimeSpan(BinaryReader reader)
		{
			var ticks = NumericSerializers.ReadVarInt64(reader);
			return new TimeSpan(ticks);
		}

		internal static Version ReadVersion(BinaryReader reader)
		{
			var version = ReadString(reader, Encoding.ASCII);
			if (version == null)
				return null;
			return new Version(version);
		}

		internal static Guid? ReadGuidNullable(BinaryReader reader)
		{
			var len = NumericSerializers.ReadVarUInt32Nullable(reader);

			if (len == null)
				return null;

			if (len == 0)
				return Guid.Empty;

			var gbuff = reader.ReadBytes((int)len.Value);
			return new Guid(gbuff);
		}

		internal static Guid ReadGuid(BinaryReader reader)
		{
			var len = NumericSerializers.ReadVarUInt32(reader);
			if (len == 0)
				return Guid.Empty;

			var gbuff = reader.ReadBytes((int)len);
			return new Guid(gbuff);
		}

		internal static DBNull ReadDbNull(BinaryReader reader)
		{
			if (reader.ReadByte() == NumericSerializers.FlagNullable)
				return null;
			return DBNull.Value;
		}

		internal static Color? ReadColorNullable(BinaryReader reader)
		{
			var argb = NumericSerializers.ReadVarInt32Nullable(reader);
			if (argb == null)
				return null;
			return Color.FromArgb(argb.Value);
		}

		internal static Color ReadColor(BinaryReader reader)
		{
			return Color.FromArgb(NumericSerializers.ReadVarInt32(reader));
		}

		internal static Uri ReadUri(BinaryReader reader)
		{
			var uri = ReadString(reader, Encoding.UTF8);
			if (uri == null)
				return null;
			return new Uri(uri, UriKind.RelativeOrAbsolute);
		}

		internal static DataTable ReadDataTable(BinaryReader reader, Encoding encoding)
		{
			var columnCount = NumericSerializers.ReadVarInt32Nullable(reader);
			if (columnCount == null)
				return null;

			// table name
			var tableName = ReadString(reader, encoding);

			var dt = new DataTable(tableName);

			// columns
			for (int index = 0; index < columnCount.Value; index++)
			{
				var caption = ReadString(reader, encoding);
				var columnName = ReadString(reader, encoding);
				var dataTypeStr = ReadString(reader, encoding);

				if (dataTypeStr.StartsWith("0."))
				{
					dataTypeStr = "System." + dataTypeStr.Remove(0, "0.".Length);
				}
				var dataType = Type.GetType(dataTypeStr, false) ?? typeof(string);

				dt.Columns.Add(new DataColumn(columnName, dataType)
				{
					Caption = caption
				});
			}

			var rowsCount = NumericSerializers.ReadVarInt32(reader);
			for (int index = 0; index < rowsCount; index++)
			{
				var itemArray = new object[columnCount.Value];

				for (int colIndex = 0; colIndex < columnCount.Value; colIndex++)
				{
					var itemType = dt.Columns[colIndex].DataType;

					var basicTypeInfo = BoisTypeCache.GetBasicType(itemType);

					if (basicTypeInfo.KnownType == EnBasicKnownType.Unknown)
						throw new Exception($"Deserialization of DataTable with item type of '{itemType}' is not supported.");

					var item = ReadRootBasicType(reader, itemType, basicTypeInfo, encoding);

					itemArray[colIndex] = item;
				}

				dt.Rows.Add(itemArray);
			}


			return dt;
		}

		internal static DataSet ReadDataSet(BinaryReader reader, Encoding encoding)
		{
			var tablesCount = NumericSerializers.ReadVarInt32Nullable(reader);
			if (tablesCount == null)
				return null;

			// set name
			var setName = ReadString(reader, encoding);

			var ds = new DataSet(setName);

			for (int index = 0; index < tablesCount.Value; index++)
			{
				var dt = ReadDataTable(reader, encoding);
				ds.Tables.Add(dt);
			}
			return ds;
		}



		internal static object ReadRootBasicType(BinaryReader reader, Type type, BoisBasicTypeInfo typeInfo, Encoding encoding)
		{
			switch (typeInfo.KnownType)
			{
				case EnBasicKnownType.String:
					return PrimitiveReader.ReadString(reader, encoding);

				case EnBasicKnownType.Char:
					if (typeInfo.IsNullable)
						return PrimitiveReader.ReadCharNullable(reader);
					return PrimitiveReader.ReadChar(reader);

				case EnBasicKnownType.Guid:
					if (typeInfo.IsNullable)
						return PrimitiveReader.ReadGuidNullable(reader);
					return PrimitiveReader.ReadGuid(reader);

				case EnBasicKnownType.Bool:
					if (typeInfo.IsNullable)
						return PrimitiveReader.ReadBooleanNullable(reader);
					return PrimitiveReader.ReadBoolean(reader);

				case EnBasicKnownType.Enum:
					return PrimitiveReader.ReadEnum(reader, type);

				case EnBasicKnownType.DateTime:
					if (typeInfo.IsNullable)
						return PrimitiveReader.ReadDateTimeNullable(reader);
					return PrimitiveReader.ReadDateTime(reader);

				case EnBasicKnownType.DateTimeOffset:
					if (typeInfo.IsNullable)
						return PrimitiveReader.ReadDateTimeOffsetNullable(reader);
					return PrimitiveReader.ReadDateTimeOffset(reader);

				case EnBasicKnownType.TimeSpan:
					if (typeInfo.IsNullable)
						return PrimitiveReader.ReadTimeSpanNullable(reader);
					return PrimitiveReader.ReadTimeSpan(reader);

				case EnBasicKnownType.ByteArray:
					return PrimitiveReader.ReadByteArray(reader);

				case EnBasicKnownType.KnownTypeArray:
					return ReadRootBasicTypedArray(reader, typeInfo, encoding);

				case EnBasicKnownType.Color:
					if (typeInfo.IsNullable)
						return PrimitiveReader.ReadColorNullable(reader);
					return PrimitiveReader.ReadColor(reader);

				case EnBasicKnownType.Version:
					return PrimitiveReader.ReadVersion(reader);

				case EnBasicKnownType.DbNull:
					return PrimitiveReader.ReadDbNull(reader);

				case EnBasicKnownType.DataTable:
					return PrimitiveReader.ReadDataTable(reader, encoding);

				case EnBasicKnownType.DataSet:
					return PrimitiveReader.ReadDataSet(reader, encoding);

				case EnBasicKnownType.Uri:
					return PrimitiveReader.ReadUri(reader);

				case EnBasicKnownType.Int16:
					if (typeInfo.IsNullable)
						return NumericSerializers.ReadVarInt16Nullable(reader);
					return NumericSerializers.ReadVarInt16(reader);

				case EnBasicKnownType.Int32:
					if (typeInfo.IsNullable)
						return NumericSerializers.ReadVarInt32Nullable(reader);
					return NumericSerializers.ReadVarInt32(reader);

				case EnBasicKnownType.Int64:
					if (typeInfo.IsNullable)
						return NumericSerializers.ReadVarInt64Nullable(reader);
					return NumericSerializers.ReadVarInt64(reader);

				case EnBasicKnownType.UInt16:
					if (typeInfo.IsNullable)
						return NumericSerializers.ReadVarUInt16Nullable(reader);
					return NumericSerializers.ReadVarUInt16(reader);

				case EnBasicKnownType.UInt32:
					if (typeInfo.IsNullable)
						return NumericSerializers.ReadVarUInt32Nullable(reader);
					return NumericSerializers.ReadVarUInt32(reader);

				case EnBasicKnownType.UInt64:
					if (typeInfo.IsNullable)
						return NumericSerializers.ReadVarUInt64Nullable(reader);
					return NumericSerializers.ReadVarUInt64(reader);

				case EnBasicKnownType.Double:
					if (typeInfo.IsNullable)
						return NumericSerializers.ReadVarDoubleNullable(reader);
					return NumericSerializers.ReadVarDouble(reader);


				case EnBasicKnownType.Decimal:
					if (typeInfo.IsNullable)
						return NumericSerializers.ReadVarDecimalNullable(reader);
					return NumericSerializers.ReadVarDecimal(reader);

				case EnBasicKnownType.Single:
					if (typeInfo.IsNullable)
						return NumericSerializers.ReadVarSingleNullable(reader);
					return NumericSerializers.ReadVarSingle(reader);

				case EnBasicKnownType.Byte:
					if (typeInfo.IsNullable)
						return NumericSerializers.ReadVarByteNullable(reader);
					return reader.ReadByte();

				case EnBasicKnownType.SByte:
					if (typeInfo.IsNullable)
						return NumericSerializers.ReadVarSByteNullable(reader);
					return reader.ReadSByte();

				case EnBasicKnownType.Unknown:
					//
					break;
			}
			throw new Exception($"Not supported basic type '{type}' as root");
		}

		internal static Array ReadRootBasicTypedArray(BinaryReader reader, BoisBasicTypeInfo typeInfo, Encoding encoding)
		{
			var length = NumericSerializers.ReadVarUInt32Nullable(reader);
			if (length == null)
			{
				return null;
			}

			var arrayItemType = typeInfo.BareType;
			var arrayItemTypeType = BoisTypeCache.GetBasicType(typeInfo.BareType);

			var result = Array.CreateInstance(arrayItemType, (int)length.Value);

			for (int i = 0; i < length; i++)
			{
				var item = ReadRootBasicType(reader, arrayItemType, arrayItemTypeType, encoding);
				result.SetValue(item, i);
			}
			return result;
		}
	}
}
