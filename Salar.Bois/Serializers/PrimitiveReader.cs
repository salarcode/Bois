using Salar.BinaryBuffers;
using Salar.Bois.Types;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Salar.Bois.Serializers
{
	internal static class PrimitiveReader
	{
		/// <summary>
		/// Reads a raw byte. Used by the emitted IL, which can only pass the buffer by reference.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static byte ReadByte<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			return reader.ReadByte();
		}

		/// <summary>
		/// Reads a raw signed byte. Used by the emitted IL, which can only pass the buffer by reference.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static sbyte ReadSByte<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			return reader.ReadSByte();
		}

		internal static string ReadString<TReader>(ref TReader reader, Encoding encoding)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			uint? length = NumericSerializers.ReadVarUInt32Nullable(ref reader);
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
#if NETFRAMEWORK
				var strBuff = reader.ReadBytes((int)length.Value);
				return encoding.GetString(strBuff);
#else
				var strBuff = reader.ReadSpan((int)length.Value);
				return encoding.GetString(strBuff);
#endif
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static char ReadChar<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			var charByte = reader.ReadUInt16();
			return (char)charByte;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static char? ReadCharNullable<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			var charByte = NumericSerializers.ReadVarUInt16Nullable(ref reader);
			if (charByte == null)
				return null;
			return (char)charByte.Value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool? ReadBooleanNullable<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			var value = NumericSerializers.ReadVarByteNullable(ref reader);
			if (value == null)
				return null;
			return value.Value != 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool ReadBoolean<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			return reader.ReadByte() != 0;
		}

		internal static DateTime? ReadDateTimeNullable<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			var kind = NumericSerializers.ReadVarByteNullable(ref reader);
			if (kind == null)
				return null;

			var ticks = NumericSerializers.ReadVarInt64(ref reader);
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

		internal static DateTime ReadDateTime<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			var kind = reader.ReadByte();
			var ticks = NumericSerializers.ReadVarInt64(ref reader);
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

		internal static DateTimeOffset? ReadDateTimeOffsetNullable<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			var offsetMinutes = NumericSerializers.ReadVarInt16Nullable(ref reader);
			if (offsetMinutes == null)
			{
				return null;
			}

			var ticks = NumericSerializers.ReadVarInt64(ref reader);

			return new DateTimeOffset(ticks, TimeSpan.FromMinutes(offsetMinutes.Value));
		}

		internal static DateTimeOffset ReadDateTimeOffset<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			var offsetMinutes = NumericSerializers.ReadVarInt16(ref reader);

			var ticks = NumericSerializers.ReadVarInt64(ref reader);

			return new DateTimeOffset(ticks, TimeSpan.FromMinutes(offsetMinutes));
		}

#if NET6_0_OR_GREATER

		internal static DateOnly ReadDateOnly<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			var dayNumber = NumericSerializers.ReadVarInt32(ref reader);
			if (dayNumber == 0L)
			{
				return DateOnly.MinValue;
			}
			if (dayNumber == 1L)
			{
				return DateOnly.MaxValue;
			}

			return DateOnly.FromDayNumber(dayNumber);
		}

		internal static DateOnly? ReadDateOnlyNullable<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			var dayNumber = NumericSerializers.ReadVarInt32Nullable(ref reader);
			if (dayNumber is null)
				return null;

			if (dayNumber == 0)
			{
				return DateOnly.MinValue;
			}
			if (dayNumber == 1)
			{
				return DateOnly.MaxValue;
			}

			return DateOnly.FromDayNumber(dayNumber.Value);
		}

		internal static TimeOnly ReadTimeOnly<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			var ticks = NumericSerializers.ReadVarInt64(ref reader);
			if (ticks == 0L)
			{
				return TimeOnly.MinValue;
			}
			if (ticks == 1L)
			{
				return TimeOnly.MaxValue;
			}

			return new TimeOnly(ticks);
		}

		internal static TimeOnly? ReadTimeOnlyNullable<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			var ticks = NumericSerializers.ReadVarInt64Nullable(ref reader);
			if (ticks is null)
				return null;

			if (ticks == 0L)
			{
				return TimeOnly.MinValue;
			}
			if (ticks == 1L)
			{
				return TimeOnly.MaxValue;
			}

			return new TimeOnly(ticks.Value);
		}
#endif

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static byte[] ReadByteArray<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			var length = NumericSerializers.ReadVarUInt32Nullable(ref reader);
			if (length == null)
			{
				return null;
			}

			if (length.Value == 0) return [];

			return reader.ReadBytes((int)length.Value);
		}

#if SupportsEmit

		internal static Enum ReadEnum<TReader>(ref TReader reader, Type type)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			var enumType = BoisTypeCache.GetEnumType(type);
			if (enumType == null)
				throw new InvalidDataException($"Cannot determine the type of enum '{type.Name}'");

			switch (enumType.KnownType)
			{
				case EnBasicEnumType.Int32:
					if (enumType.IsNullable)
					{
						var val = NumericSerializers.ReadVarInt32Nullable(ref reader);
						if (val == null)
							return null;
						return (Enum)Enum.ToObject(enumType.BareType, val);
					}
					else
					{
						var val = NumericSerializers.ReadVarInt32(ref reader);
						return (Enum)Enum.ToObject(enumType.BareType, val);
					}

				case EnBasicEnumType.Byte:
					if (enumType.IsNullable)
					{
						var val = NumericSerializers.ReadVarByteNullable(ref reader);
						if (val == null)
							return null;
						return (Enum)Enum.ToObject(enumType.BareType, val);
					}
					else
					{
						var val = reader.ReadByte();
						return (Enum)Enum.ToObject(enumType.BareType, val);
					}

				case EnBasicEnumType.Int16:
					if (enumType.IsNullable)
					{
						var val = NumericSerializers.ReadVarInt16Nullable(ref reader);
						if (val == null)
							return null;
						return (Enum)Enum.ToObject(enumType.BareType, val);
					}
					else
					{
						var val = NumericSerializers.ReadVarInt16(ref reader);
						return (Enum)Enum.ToObject(enumType.BareType, val);
					}

				case EnBasicEnumType.Int64:
					if (enumType.IsNullable)
					{
						var val = NumericSerializers.ReadVarInt64Nullable(ref reader);
						if (val == null)
							return null;
						return (Enum)Enum.ToObject(enumType.BareType, val);
					}
					else
					{
						var val = NumericSerializers.ReadVarInt64(ref reader);
						return (Enum)Enum.ToObject(enumType.BareType, val);
					}

				case EnBasicEnumType.UInt16:
					if (enumType.IsNullable)
					{
						var val = NumericSerializers.ReadVarUInt16Nullable(ref reader);
						if (val == null)
							return null;
						return (Enum)Enum.ToObject(enumType.BareType, val);
					}
					else
					{
						var val = NumericSerializers.ReadVarUInt16(ref reader);
						return (Enum)Enum.ToObject(enumType.BareType, val);
					}

				case EnBasicEnumType.UInt32:
					if (enumType.IsNullable)
					{
						var val = NumericSerializers.ReadVarUInt32Nullable(ref reader);
						if (val == null)
							return null;
						return (Enum)Enum.ToObject(enumType.BareType, val);
					}
					else
					{
						var val = NumericSerializers.ReadVarUInt32(ref reader);
						return (Enum)Enum.ToObject(enumType.BareType, val);
					}

				case EnBasicEnumType.UInt64:
					if (enumType.IsNullable)
					{
						var val = NumericSerializers.ReadVarUInt64Nullable(ref reader);
						if (val == null)
							return null;
						return (Enum)Enum.ToObject(enumType.BareType, val);
					}
					else
					{
						var val = NumericSerializers.ReadVarUInt64(ref reader);
						return (Enum)Enum.ToObject(enumType.BareType, val);
					}

				case EnBasicEnumType.SByte:
					if (enumType.IsNullable)
					{
						var val = NumericSerializers.ReadVarSByteNullable(ref reader);
						if (val == null)
							return null;
						return (Enum)Enum.ToObject(enumType.BareType, val);
					}
					else
					{
						var val = reader.ReadSByte();
						return (Enum)Enum.ToObject(enumType.BareType, val);
					}

				default:
					throw new InvalidDataException($"Enum type not supported '{type.Name}'. Please raise an issue here https://github.com/salarcode/Bois/issues ");
			}
		}
#endif

#if SupportsEmit
		internal static T ReadEnumGeneric<T, TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			var type = typeof(T);
			var enumTypeInfo = BoisTypeCache.GetEnumType(type);
			if (enumTypeInfo == null)
				throw new InvalidDataException($"Cannot determine the type of enum '{type.Name}'");

			switch (enumTypeInfo.KnownType)
			{
				case EnBasicEnumType.Int32:
					if (enumTypeInfo.IsNullable)
					{
						var val = NumericSerializers.ReadVarInt32Nullable(ref reader);
						if (val == null)
							return default;
						return (T)Enum.ToObject(enumTypeInfo.BareType, val);
					}
					else
					{
						var val = NumericSerializers.ReadVarInt32(ref reader);
						return (T)Enum.ToObject(enumTypeInfo.BareType, val);
					}

				case EnBasicEnumType.Byte:
					if (enumTypeInfo.IsNullable)
					{
						var val = NumericSerializers.ReadVarByteNullable(ref reader);
						if (val == null)
							return default;
						return (T)Enum.ToObject(enumTypeInfo.BareType, val);
					}
					else
					{
						var val = reader.ReadByte();
						return (T)Enum.ToObject(enumTypeInfo.BareType, val);
					}

				case EnBasicEnumType.Int16:
					if (enumTypeInfo.IsNullable)
					{
						var val = NumericSerializers.ReadVarInt16Nullable(ref reader);
						if (val == null)
							return default;
						return (T)Enum.ToObject(enumTypeInfo.BareType, val);
					}
					else
					{
						var val = NumericSerializers.ReadVarInt16(ref reader);
						return (T)Enum.ToObject(enumTypeInfo.BareType, val);
					}

				case EnBasicEnumType.Int64:
					if (enumTypeInfo.IsNullable)
					{
						var val = NumericSerializers.ReadVarInt64Nullable(ref reader);
						if (val == null)
							return default;
						return (T)Enum.ToObject(enumTypeInfo.BareType, val);
					}
					else
					{
						var val = NumericSerializers.ReadVarInt64(ref reader);
						return (T)Enum.ToObject(enumTypeInfo.BareType, val);
					}

				case EnBasicEnumType.UInt16:
					if (enumTypeInfo.IsNullable)
					{
						var val = NumericSerializers.ReadVarUInt16Nullable(ref reader);
						if (val == null)
							return default;
						return (T)Enum.ToObject(enumTypeInfo.BareType, val);
					}
					else
					{
						var val = NumericSerializers.ReadVarUInt16(ref reader);
						return (T)Enum.ToObject(enumTypeInfo.BareType, val);
					}

				case EnBasicEnumType.UInt32:
					if (enumTypeInfo.IsNullable)
					{
						var val = NumericSerializers.ReadVarUInt32Nullable(ref reader);
						if (val == null)
							return default;
						return (T)Enum.ToObject(enumTypeInfo.BareType, val);
					}
					else
					{
						var val = NumericSerializers.ReadVarUInt32(ref reader);
						return (T)Enum.ToObject(enumTypeInfo.BareType, val);
					}

				case EnBasicEnumType.UInt64:
					if (enumTypeInfo.IsNullable)
					{
						var val = NumericSerializers.ReadVarUInt64Nullable(ref reader);
						if (val == null)
							return default;
						return (T)Enum.ToObject(enumTypeInfo.BareType, val);
					}
					else
					{
						var val = NumericSerializers.ReadVarUInt64(ref reader);
						return (T)Enum.ToObject(enumTypeInfo.BareType, val);
					}

				case EnBasicEnumType.SByte:
					if (enumTypeInfo.IsNullable)
					{
						var val = NumericSerializers.ReadVarSByteNullable(ref reader);
						if (val == null)
							return default;
						return (T)Enum.ToObject(enumTypeInfo.BareType, val);
					}
					else
					{
						var val = reader.ReadSByte();
						return (T)Enum.ToObject(enumTypeInfo.BareType, val);
					}

				default:
					throw new InvalidDataException($"Enum type not supported '{type.Name}'. Please raise an issue here https://github.com/salarcode/Bois/issues ");
			}
		}
#endif

		internal static int ReadEnumInt32<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			return NumericSerializers.ReadVarInt32(ref reader);
		}

		internal static int? ReadEnumInt32Nullable<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			return NumericSerializers.ReadVarInt32Nullable(ref reader);
		}

		internal static long ReadEnumInt64<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			return NumericSerializers.ReadVarInt64(ref reader);
		}

		internal static long? ReadEnumInt64Nullable<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			return NumericSerializers.ReadVarInt64Nullable(ref reader);
		}

		internal static short ReadEnumInt16<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			return NumericSerializers.ReadVarInt16(ref reader);
		}

		internal static short? ReadEnumInt16Nullable<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			return NumericSerializers.ReadVarInt16Nullable(ref reader);
		}

		internal static ushort ReadEnumUInt16<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			return NumericSerializers.ReadVarUInt16(ref reader);
		}

		internal static ushort? ReadEnumUInt16Nullable<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			return NumericSerializers.ReadVarUInt16Nullable(ref reader);
		}

		internal static uint ReadEnumUInt32<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			return NumericSerializers.ReadVarUInt32(ref reader);
		}

		internal static uint? ReadEnumUInt32Nullable<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			return NumericSerializers.ReadVarUInt32Nullable(ref reader);
		}

		internal static ulong ReadEnumUInt64<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			return NumericSerializers.ReadVarUInt64(ref reader);
		}

		internal static ulong? ReadEnumUInt64Nullable<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			return NumericSerializers.ReadVarUInt64Nullable(ref reader);
		}

		internal static byte ReadEnumByte<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			return reader.ReadByte();
		}

		internal static byte? ReadEnumByteNullable<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			return NumericSerializers.ReadVarByteNullable(ref reader);
		}

		internal static sbyte ReadEnumSByte<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			return reader.ReadSByte();
		}

		internal static sbyte? ReadEnumSByteNullable<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			return NumericSerializers.ReadVarSByteNullable(ref reader);
		}

		internal static TimeSpan? ReadTimeSpanNullable<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			var ticks = NumericSerializers.ReadVarInt64Nullable(ref reader);
			if (ticks == null)
				return null;

			return new TimeSpan(ticks.Value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static TimeSpan ReadTimeSpan<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			var ticks = NumericSerializers.ReadVarInt64(ref reader);
			return new TimeSpan(ticks);
		}

		internal static Version ReadVersion<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			var version = ReadString(ref reader, Encoding.ASCII);
			if (version == null)
				return null;
			return new Version(version);
		}

		internal static Guid? ReadGuidNullable<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			uint? len = NumericSerializers.ReadVarUInt32Nullable(ref reader);

			if (len == null)
				return null;

			if (len == 0)
				return Guid.Empty;

#if NETFRAMEWORK
			var gbuff = reader.ReadBytes((int)len.Value);
			return new Guid(gbuff);
#else
			var gbuff = reader.ReadSpan((int)len.Value);
			return new Guid(gbuff);
#endif
		}

		internal static Guid ReadGuid<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			uint len = NumericSerializers.ReadVarUInt32(ref reader);
			if (len == 0)
				return Guid.Empty;

#if NETFRAMEWORK
			var gbuff = reader.ReadBytes((int)len);
			return new Guid(gbuff);
#else
			var gbuff = reader.ReadSpan((int)len);
			return new Guid(gbuff);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static DBNull ReadDbNull<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			if (reader.ReadByte() == NumericSerializers.FlagIsNull)
				return null;
			return DBNull.Value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static Color? ReadColorNullable<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			var argb = NumericSerializers.ReadVarInt32Nullable(ref reader);
			if (argb == null)
				return null;
			return Color.FromArgb(argb.Value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static Color ReadColor<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			return Color.FromArgb(NumericSerializers.ReadVarInt32(ref reader));
		}

		internal static Uri ReadUri<TReader>(ref TReader reader)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			var uri = ReadString(ref reader, Encoding.UTF8);
			if (uri == null)
				return null;
			return new Uri(uri, UriKind.RelativeOrAbsolute);
		}

		internal static DataTable ReadDataTable<TReader>(ref TReader reader, Encoding encoding)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
#if SupportsEmit
			var columnCount = NumericSerializers.ReadVarUInt32Nullable(ref reader);
			if (columnCount == null)
				return null;

			// table name
			var tableName = ReadString(ref reader, encoding);

			var dt = new DataTable(tableName);

			// columns
			for (int index = 0; index < columnCount.Value; index++)
			{
				var caption = ReadString(ref reader, encoding);
				var columnName = ReadString(ref reader, encoding);
				var dataTypeStr = ReadString(ref reader, encoding);

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

			var rowsCount = NumericSerializers.ReadVarInt32(ref reader);
			for (int index = 0; index < rowsCount; index++)
			{
				var itemArray = new object[columnCount.Value];

				for (int colIndex = 0; colIndex < columnCount.Value; colIndex++)
				{
					var itemType = dt.Columns[colIndex].DataType;

					var basicTypeInfo = BoisTypeCache.GetBasicType(itemType);

					if (basicTypeInfo.KnownType == EnBasicKnownType.Unknown)
						throw new InvalidDataException($"Deserialization of DataTable with item type of '{itemType}' is not supported.");

					var item = ReadRootBasicType(ref reader, itemType, basicTypeInfo, encoding);

					itemArray[colIndex] = item;
				}

				dt.Rows.Add(itemArray);
			}


			return dt;
#else
			throw new NotSupportedException("DataTable is not supported with source generator.");
#endif
		}

		internal static DataSet ReadDataSet<TReader>(ref TReader reader, Encoding encoding)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
#if SupportsEmit
			var tablesCount = NumericSerializers.ReadVarUInt32Nullable(ref reader);
			if (tablesCount == null)
				return null;

			// set name
			var setName = ReadString(ref reader, encoding);

			var ds = new DataSet(setName);

			for (int index = 0; index < tablesCount.Value; index++)
			{
				var dt = ReadDataTable(ref reader, encoding);
				ds.Tables.Add(dt);
			}
			return ds;
#else
			throw new NotSupportedException("DataSet is not supported with source generator.");
#endif
		}


#if SupportsEmit
		internal static object ReadRootBasicType<TReader>(ref TReader reader, Type type, BoisBasicTypeInfo typeInfo, Encoding encoding)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			switch (typeInfo.KnownType)
			{
				case EnBasicKnownType.String:
					return PrimitiveReader.ReadString(ref reader, encoding);

				case EnBasicKnownType.Char:
					if (typeInfo.IsNullable)
						return PrimitiveReader.ReadCharNullable(ref reader);
					return PrimitiveReader.ReadChar(ref reader);

				case EnBasicKnownType.Guid:
					if (typeInfo.IsNullable)
						return PrimitiveReader.ReadGuidNullable(ref reader);
					return PrimitiveReader.ReadGuid(ref reader);

				case EnBasicKnownType.Bool:
					if (typeInfo.IsNullable)
						return PrimitiveReader.ReadBooleanNullable(ref reader);
					return PrimitiveReader.ReadBoolean(ref reader);

				case EnBasicKnownType.Enum:
					return PrimitiveReader.ReadEnum(ref reader, type);

				case EnBasicKnownType.DateTime:
					if (typeInfo.IsNullable)
						return PrimitiveReader.ReadDateTimeNullable(ref reader);
					return PrimitiveReader.ReadDateTime(ref reader);

				case EnBasicKnownType.DateTimeOffset:
					if (typeInfo.IsNullable)
						return PrimitiveReader.ReadDateTimeOffsetNullable(ref reader);
					return PrimitiveReader.ReadDateTimeOffset(ref reader);

#if NET6_0_OR_GREATER
				case EnBasicKnownType.DateOnly:
					if (typeInfo.IsNullable)
						return PrimitiveReader.ReadDateOnlyNullable(ref reader);
					return PrimitiveReader.ReadDateOnly(ref reader);

				case EnBasicKnownType.TimeOnly:
					if (typeInfo.IsNullable)
						return PrimitiveReader.ReadTimeOnlyNullable(ref reader);
					return PrimitiveReader.ReadTimeOnly(ref reader);
#endif

				case EnBasicKnownType.TimeSpan:
					if (typeInfo.IsNullable)
						return PrimitiveReader.ReadTimeSpanNullable(ref reader);
					return PrimitiveReader.ReadTimeSpan(ref reader);

				case EnBasicKnownType.ByteArray:
					return PrimitiveReader.ReadByteArray(ref reader);

				case EnBasicKnownType.KnownTypeArray:
					return ReadRootBasicTypedArray(ref reader, typeInfo, encoding);

				case EnBasicKnownType.Color:
					if (typeInfo.IsNullable)
						return PrimitiveReader.ReadColorNullable(ref reader);
					return PrimitiveReader.ReadColor(ref reader);

				case EnBasicKnownType.Version:
					return PrimitiveReader.ReadVersion(ref reader);

				case EnBasicKnownType.DbNull:
					return PrimitiveReader.ReadDbNull(ref reader);

				case EnBasicKnownType.DataTable:
					return PrimitiveReader.ReadDataTable(ref reader, encoding);

				case EnBasicKnownType.DataSet:
					return PrimitiveReader.ReadDataSet(ref reader, encoding);

				case EnBasicKnownType.Uri:
					return PrimitiveReader.ReadUri(ref reader);

				case EnBasicKnownType.Int16:
					if (typeInfo.IsNullable)
						return NumericSerializers.ReadVarInt16Nullable(ref reader);
					return NumericSerializers.ReadVarInt16(ref reader);

				case EnBasicKnownType.Int32:
					if (typeInfo.IsNullable)
						return NumericSerializers.ReadVarInt32Nullable(ref reader);
					return NumericSerializers.ReadVarInt32(ref reader);

				case EnBasicKnownType.Int64:
					if (typeInfo.IsNullable)
						return NumericSerializers.ReadVarInt64Nullable(ref reader);
					return NumericSerializers.ReadVarInt64(ref reader);

				case EnBasicKnownType.UInt16:
					if (typeInfo.IsNullable)
						return NumericSerializers.ReadVarUInt16Nullable(ref reader);
					return NumericSerializers.ReadVarUInt16(ref reader);

				case EnBasicKnownType.UInt32:
					if (typeInfo.IsNullable)
						return NumericSerializers.ReadVarUInt32Nullable(ref reader);
					return NumericSerializers.ReadVarUInt32(ref reader);

				case EnBasicKnownType.UInt64:
					if (typeInfo.IsNullable)
						return NumericSerializers.ReadVarUInt64Nullable(ref reader);
					return NumericSerializers.ReadVarUInt64(ref reader);

				case EnBasicKnownType.Double:
					if (typeInfo.IsNullable)
						return NumericSerializers.ReadVarDoubleNullable(ref reader);
					return NumericSerializers.ReadVarDouble(ref reader);


				case EnBasicKnownType.Decimal:
					if (typeInfo.IsNullable)
						return NumericSerializers.ReadVarDecimalNullable(ref reader);
					return NumericSerializers.ReadVarDecimal(ref reader);

				case EnBasicKnownType.Single:
					if (typeInfo.IsNullable)
						return NumericSerializers.ReadVarSingleNullable(ref reader);
					return NumericSerializers.ReadVarSingle(ref reader);

				case EnBasicKnownType.Byte:
					if (typeInfo.IsNullable)
						return NumericSerializers.ReadVarByteNullable(ref reader);
					return reader.ReadByte();

				case EnBasicKnownType.SByte:
					if (typeInfo.IsNullable)
						return NumericSerializers.ReadVarSByteNullable(ref reader);
					return reader.ReadSByte();

				case EnBasicKnownType.Unknown:
					//
					break;
			}
			throw new ArgumentException($"Not supported basic type '{type}' as root", nameof(type));
		}
#endif

#if SupportsEmit
		internal static Array ReadRootBasicTypedArray<TReader>(ref TReader reader, BoisBasicTypeInfo typeInfo, Encoding encoding)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			var length = NumericSerializers.ReadVarUInt32Nullable(ref reader);
			if (length == null)
			{
				return null;
			}

			var arrayItemType = typeInfo.BareType;
			var boisBasicTypeInfo = BoisTypeCache.GetBasicType(typeInfo.BareType);

			var result = Array.CreateInstance(arrayItemType, (int)length.Value);

			for (int i = 0; i < length; i++)
			{
				var item = ReadRootBasicType(ref reader, arrayItemType, boisBasicTypeInfo, encoding);
				result.SetValue(item, i);
			}
			return result;
		}
#endif
	}
}
