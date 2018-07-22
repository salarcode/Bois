using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salar.Bois.Serializers
{
	public  static class PrimitiveReader
	{
		public static string ReadString(BinaryReader reader, Encoding encoding)
		{
			int? length = PrimitivesConvertion.ReadVarInt32Nullable(reader);
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

		public static char ReadChar(BinaryReader reader)
		{
			var charByte = reader.ReadUInt16();
			return (char)charByte;
		}

		public static char? ReadCharNullable(BinaryReader reader)
		{
			var charByte = PrimitivesConvertion.ReadVarUInt16Nullable(reader);
			if (charByte == null)
				return null;
			return (char)charByte.Value;
		}

		public static bool? ReadBooleanNullable(BinaryReader reader)
		{
			var value = PrimitivesConvertion.ReadVarByteNullable(reader);
			if (value == null)
				return null;
			return value.Value != 0;
		}

		public static bool ReadBoolean(BinaryReader reader)
		{
			return reader.ReadByte() != 0;
		}

		public static DateTime? ReadDateTimeNullable(BinaryReader reader)
		{
			var kind = PrimitivesConvertion.ReadVarByteNullable(reader);
			if (kind == null)
				return null;

			var ticks = PrimitivesConvertion.ReadVarInt64(reader);
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

		public static DateTime ReadDateTime(BinaryReader reader)
		{
			var kind = reader.ReadByte();
			var ticks = PrimitivesConvertion.ReadVarInt64(reader);
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

		public static DateTimeOffset? ReadDateTimeOffsetNullable(BinaryReader reader)
		{
			throw new NotImplementedException();
		}

		public static DateTimeOffset ReadDateTimeOffset(BinaryReader reader)
		{
			throw new NotImplementedException();
		}

		public static byte[] ReadByteArray(BinaryReader reader)
		{
			throw new NotImplementedException();
		}

		public static Enum ReadEnum(BinaryReader reader)
		{
			throw new NotImplementedException();
		}

		public static TimeSpan? ReadTimeSpanNullable(BinaryReader reader)
		{
			throw new NotImplementedException();
		}

		public static TimeSpan ReadTimeSpan(BinaryReader reader)
		{
			throw new NotImplementedException();
		}

		public static Version ReadVersion(BinaryReader reader)
		{
			throw new NotImplementedException();
		}

		public static Guid? ReadGuidNullable(BinaryReader reader)
		{
			throw new NotImplementedException();
		}

		public static Guid ReadGuid(BinaryReader reader)
		{
			throw new NotImplementedException();
		}

		public static DBNull ReadDBNull(BinaryReader reader)
		{
			throw new NotImplementedException();
		}
	}
}
