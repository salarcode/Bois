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
				return string.Empty;
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
			return new Version();
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
			// just moving one step forward
			reader.ReadByte();
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
			return new Uri(uri);
		}

		internal static DataTable ReadDataTable(BinaryReader reader, Encoding encoding)
		{
			var data = ReadString(reader, encoding);
			if (string.IsNullOrEmpty(data))
				return null;

			return DeserializeDataTable(data);
		}

		internal static DataSet ReadDataSet(BinaryReader reader, Encoding encoding)
		{
			var data = ReadString(reader, encoding);
			if (string.IsNullOrEmpty(data))
				return null;

			return DeserializeDataSet(data);
		}


		#region Private helpers

		private static DataSet DeserializeDataSet(string data)
		{
			var ds = new DataSet();
			ds.ReadXml(data);

			return ds;
		}

		private static DataTable DeserializeDataTable(string data)
		{
			var dt = new DataTable();
			dt.ReadXml(data);

			return dt;
		}


		#endregion
	}
}
