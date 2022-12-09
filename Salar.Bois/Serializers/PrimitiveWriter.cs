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
	internal static class PrimitiveWriter
	{
		/// <summary>
		/// there is no data and the value is null
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteNullValue(BufferWriterBase writer)
		{
			writer.Write(NumericSerializers.FlagIsNull);
		}

		/// <summary>
		/// String - Format: (Embedded-Nullable-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..63
		/// </summary>
		internal static void WriteValue(BufferWriterBase writer, string str, Encoding encoding)
		{
			if (str == null)
			{
				WriteNullValue(writer);
			}
			else if (str.Length == 0)
			{
				NumericSerializers.WriteUIntNullableMemberCount(writer, 0u);
			}
			else
			{
				byte[] strBytes;
				if (str.Length > 64)
					strBytes = GetStringBytes(ref str, encoding);
				else
					strBytes = GetStringBytes(str, encoding);

				// Int32
				NumericSerializers.WriteUIntNullableMemberCount(writer, (uint)strBytes.Length);
				writer.Write(strBytes);
			}
		}

		/// <summary>
		/// char - Format: (Embedded-0-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..127
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteValue(BufferWriterBase writer, char c)
		{
			writer.Write((ushort)c);
		}

		/// <summary>
		/// char? - Format: (Embedded-Nullable-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..63
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteValue(BufferWriterBase writer, char? c)
		{
			NumericSerializers.WriteVarInt(writer, (ushort?)c);
		}

		/// <summary>
		/// bool - Format: (Embedded=true-0-0-0-0-0-0-0)
		/// Embeddable range: 0..127
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteValue(BufferWriterBase writer, bool b)
		{
			writer.Write(b);
		}

		/// <summary>
		/// bool? - Format: (Embedded=true-Nullable-0-0-0-0-0-0)
		/// Embeddable range: 0..63
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteValue(BufferWriterBase writer, bool? b)
		{
			byte? val = null;
			if (b.HasValue)
				val = b.Value ? (byte)1 : (byte)0;

			NumericSerializers.WriteVarInt(writer, val);
		}

		/// <summary>
		/// DateTime - Format: (Kind:0-0-0-0-0-0-0-0) (dateTimeTicks:Embedded-0-0-0-0-0-0-0)[if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable kind range: always embedded
		/// Embeddable ticks range: 0..127
		/// </summary>
		internal static void WriteValue(BufferWriterBase writer, DateTime dateTime)
		{
			var kind = (byte)dateTime.Kind;

			if (dateTime == DateTime.MinValue)
			{
				writer.Write(kind);
				// min datetime indicator
				NumericSerializers.WriteVarInt(writer, 0L);
			}
			else if (dateTime == DateTime.MaxValue)
			{
				writer.Write(kind);
				// max datetime indicator
				NumericSerializers.WriteVarInt(writer, 1L);
			}
			else
			{
				writer.Write(kind);
				//Int64
				NumericSerializers.WriteVarInt(writer, dateTime.Ticks);
			}
		}

		/// <summary>
		/// DateTime? - Format: (Kind:Nullable-0-0-0-0-0-0-0) (dateTimeTicks:Embedded-0-0-0-0-0-0-0)[if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable kind range: always embedded
		/// Embeddable ticks range: 0..127
		/// </summary>
		internal static void WriteValue(BufferWriterBase writer, DateTime? dt)
		{
			if (dt == null)
			{
				WriteNullValue(writer);
				return;
			}
			var dateTime = dt.Value;
			var kind = (byte?)dateTime.Kind;

			if (dateTime == DateTime.MinValue)
			{
				NumericSerializers.WriteVarInt(writer, kind);
				// min datetime indicator
				NumericSerializers.WriteVarInt(writer, 0L);
			}
			else if (dateTime == DateTime.MaxValue)
			{
				NumericSerializers.WriteVarInt(writer, kind);
				// max datetime indicator
				NumericSerializers.WriteVarInt(writer, 1L);
			}
			else
			{
				NumericSerializers.WriteVarInt(writer, kind);
				//Int64
				NumericSerializers.WriteVarInt(writer, dateTime.Ticks);
			}
		}

		/// <summary>
		/// DateTimeOffset - Format: (Offset:Embedded-0-0-0-0-0-0-0)[if ofset not embedded?0-0-0-0-0-0-0-0] (dateTimeOffsetTicks:Embedded-0-0-0-0-0-0-0)[if ticks not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable offset range: 0..127
		/// Embeddable ticks range: 0..127
		/// </summary>
		internal static void WriteValue(BufferWriterBase writer, DateTimeOffset dateTimeOffset)
		{
			var offset = dateTimeOffset.Offset;
			short offsetMinutes;
			unchecked
			{
				offsetMinutes = (short)((offset.Hours * 60) + offset.Minutes);
			}
			// int16
			NumericSerializers.WriteVarInt(writer, offsetMinutes);

			// int64
			NumericSerializers.WriteVarInt(writer, dateTimeOffset.Ticks);
		}

		/// <summary>
		/// DateTimeOffset? - Format: (Offset:Embedded-Nullable-0-0-0-0-0-0)[if ofset not embedded?0-0-0-0-0-0-0-0] (dateTimeOffsetTicks:Embedded-0-0-0-0-0-0-0)[if ticks not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable offset range: 0..63
		/// Embeddable ticks range: 0..127
		/// </summary>
		internal static void WriteValue(BufferWriterBase writer, DateTimeOffset? dto)
		{
			if (dto == null)
			{
				WriteNullValue(writer);
				return;
			}
			var dateTimeOffset = dto.Value;

			var offset = dateTimeOffset.Offset;
			short? offsetMinutes;
			unchecked
			{
				offsetMinutes = (short)((offset.Hours * 60) + offset.Minutes);
			}
			// int16
			NumericSerializers.WriteVarInt(writer, offsetMinutes);

			// int64
			NumericSerializers.WriteVarInt(writer, dateTimeOffset.Ticks);
		}

		/// <summary>
		/// byte[] - Format: (Array Length:Embedded-Nullable-0-0-0-0-0-0) [if array length not embedded?0-0-0-0-0-0-0-0] (data:0-0-0-0-0-0-0-0)
		/// Embeddable Array Length range: 0..63
		/// </summary>
		internal static void WriteValue(BufferWriterBase writer, byte[] bytes)
		{
			if (bytes == null)
			{
				WriteNullValue(writer);
				return;
			}

			// uint doesn't deal with negative numbers
			NumericSerializers.WriteUIntNullableMemberCount(writer, (uint)bytes.Length);
			writer.Write(bytes);
		}

		/// <summary>
		/// VarInt - Format: (Embedded-Nullable-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..63
		/// </summary>
		internal static void WriteValue(BufferWriterBase writer, Enum e)
		{
			if (e == null)
			{
				WriteNullValue(writer);
				return;
			}
			WriteValue(writer, e, e.GetType(), null);
		}

		/// <summary>
		/// VarInt - Format: (Embedded-Nullable-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..63
		/// </summary>
		internal static void WriteValue(BufferWriterBase writer, Enum e, bool nullable)
		{
			if (e == null)
			{
				WriteNullValue(writer);
				return;
			}
			WriteValue(writer, e, e.GetType(), nullable);
		}

		/// <summary>
		/// VarInt - Format: (Embedded-Nullable-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..63
		/// </summary>
		internal static void WriteValue(BufferWriterBase writer, Enum e, Type type, bool? memberIsNullable)
		{
			if (e == null)
			{
				WriteNullValue(writer);
				return;
			}
			var enumType = BoisTypeCache.GetEnumType(type);
			if (enumType == null)
				throw new InvalidDataException($"Cannot determine the type of enum '{type.Name}'");

			var isNullable = enumType.IsNullable;
			if (memberIsNullable.HasValue)
				isNullable = memberIsNullable.Value;

			switch (enumType.KnownType)
			{
				case EnBasicEnumType.Int32:
					if (isNullable)
						NumericSerializers.WriteVarInt(writer, (int?)(int)(object)e);
					else
						NumericSerializers.WriteVarInt(writer, (int)(object)e);
					break;

				case EnBasicEnumType.Byte:
					if (isNullable)
						NumericSerializers.WriteVarInt(writer, (byte?)(byte)(object)e);
					else
						writer.Write((byte)(object)e);
					break;

				case EnBasicEnumType.Int16:
					if (isNullable)
						NumericSerializers.WriteVarInt(writer, (short?)(short)(object)e);
					else
						NumericSerializers.WriteVarInt(writer, (short)(object)e);
					break;

				case EnBasicEnumType.Int64:
					if (isNullable)
						NumericSerializers.WriteVarInt(writer, (long?)(long)(object)e);
					else
						NumericSerializers.WriteVarInt(writer, (long)(object)e);
					break;

				case EnBasicEnumType.UInt16:
					if (isNullable)
						NumericSerializers.WriteVarInt(writer, (ushort?)(ushort)(object)e);
					else
						NumericSerializers.WriteVarInt(writer, (ushort)(object)e);
					break;

				case EnBasicEnumType.UInt32:
					if (isNullable)
						NumericSerializers.WriteVarInt(writer, (uint?)(uint)(object)e);
					else
						NumericSerializers.WriteVarInt(writer, (uint)(object)e);
					break;

				case EnBasicEnumType.UInt64:
					if (isNullable)
						NumericSerializers.WriteVarInt(writer, (ulong?)(ulong)(object)e);
					else
						NumericSerializers.WriteVarInt(writer, (ulong)(object)e);
					break;

				case EnBasicEnumType.SByte:
					if (isNullable)
						NumericSerializers.WriteVarInt(writer, (sbyte?)(sbyte)(object)e);
					else
						writer.Write((sbyte)(object)e);
					break;

				default:
					throw new ArgumentException($"Enum type not supported '{type.Name}'. Contact the author please https://github.com/salarcode/Bois/issues ", nameof(type));
			}
		}


		/// <summary>
		/// TimeSpan - Format: (Embedded-0-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..127
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteValue(BufferWriterBase writer, TimeSpan timeSpan)
		{
			NumericSerializers.WriteVarInt(writer, timeSpan.Ticks);
		}

		/// <summary>
		/// TimeSpan? - Format: (Embedded-Nullable-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..63
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteValue(BufferWriterBase writer, TimeSpan? timeSpan)
		{
			if (timeSpan == null)
			{
				WriteNullValue(writer);
				return;
			}
			NumericSerializers.WriteVarInt(writer, (long?)timeSpan.Value.Ticks);
		}

		/// <summary>
		/// Version - Format: (Embedded-Nullable-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..63
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteValue(BufferWriterBase writer, Version version)
		{
			if (version == null)
			{
				WriteNullValue(writer);
				return;
			}
			WriteValue(writer, version.ToString(), Encoding.ASCII);
		}

		/// <summary>
		/// Same as String
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteValue(BufferWriterBase writer, Uri uri)
		{
			PrimitiveWriter.WriteValue(writer, uri?.ToString(), Encoding.UTF8);
		}

		/// <summary>
		/// Guid - Format: (Embedded-0-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..127
		/// </summary>
		internal static void WriteValue(BufferWriterBase writer, Guid guid)
		{
			if (guid == Guid.Empty)
			{
				// Int32
				NumericSerializers.WriteVarInt(writer, (uint)0);
				return;
			}

			var data = guid.ToByteArray();

			// Int32
			NumericSerializers.WriteVarInt(writer, (uint)data.Length);
			writer.Write(data);
		}

		/// <summary>
		/// Guid? - Format: (Embedded-Nullable-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..63
		/// </summary>
		internal static void WriteValue(BufferWriterBase writer, Guid? g)
		{
			if (g == null)
			{
				WriteNullValue(writer);
				return;
			}

			var guid = g.Value;
			if (guid == Guid.Empty)
			{
				// UInt32
				NumericSerializers.WriteUIntNullableMemberCount(writer, 0u);
				return;
			}

			var data = guid.ToByteArray();

			// UInt32
			NumericSerializers.WriteUIntNullableMemberCount(writer, (uint)data.Length);
			writer.Write(data);
		}

		/// <summary>
		/// DBNull? - Format: (Embedded=true-Nullable=true-0-0-0-0-0-0)
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteValue(BufferWriterBase writer, DBNull dbNull)
		{
			if (dbNull == null)
				WriteNullValue(writer);
			else
			{
				WriteValue(writer, true);
			}
		}
		/// <summary>
		/// Same as Int32
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteValue(BufferWriterBase writer, Color color)
		{
			int argb = color.ToArgb();
			// Int32
			NumericSerializers.WriteVarInt(writer, argb);
		}

		/// <summary>
		/// Same as Nullable<Int32>
		/// </summary>
		internal static void WriteValue(BufferWriterBase writer, Color? color)
		{
			if (color == null)
			{
				WriteNullValue(writer);
				return;
			}
			int? argb = color.Value.ToArgb();
			// Int32
			NumericSerializers.WriteVarInt(writer, argb);
		}

		/// <summary>
		/// Obsolete - only backward compatibility
		/// </summary>
		internal static void WriteValue(BufferWriterBase writer, DataSet ds, Encoding encoding)
		{
			if (ds == null)
			{
				WriteNullValue(writer);
				return;
			}
			// tables count
			NumericSerializers.WriteUIntNullableMemberCount(writer, (uint)ds.Tables.Count);

			WriteValue(writer, ds.DataSetName, encoding);

			foreach (DataTable dt in ds.Tables)
			{
				WriteValue(writer, dt, encoding);
			}
		}

		/// <summary>
		/// Obsolete - only backward compatibility
		/// </summary>
		internal static void WriteValue(BufferWriterBase writer, DataTable dt, Encoding encoding)
		{
			if (dt == null)
			{
				WriteNullValue(writer);
				return;
			}
			// column count
			NumericSerializers.WriteUIntNullableMemberCount(writer, (uint)dt.Columns.Count);

			// table name
			WriteValue(writer, dt.TableName, encoding);

			// columns
			foreach (DataColumn col in dt.Columns)
			{
				WriteValue(writer, col.Caption, encoding);
				WriteValue(writer, col.ColumnName, encoding);

				var dataType = col.DataType?.ToString();
				if (dataType != null && dataType.StartsWith("System."))
				{
					dataType = "0." + dataType.Remove(0, "System.".Length);
				}
				WriteValue(writer, dataType, encoding);
			}

			NumericSerializers.WriteVarInt(writer, dt.Rows.Count);
			foreach (DataRow row in dt.Rows)
			{
				for (var colIndex = 0; colIndex < row.ItemArray.Length; colIndex++)
				{
					var item = row.ItemArray[colIndex];
					var itemType = dt.Columns[colIndex].DataType;

					var basicTypeInfo = BoisTypeCache.GetBasicType(itemType);

					if (basicTypeInfo.KnownType == EnBasicKnownType.Unknown)
						throw new InvalidDataException($"Serialization of DataTable with item type of '{itemType}' is not supported.");

					var itemToWrite = item;
					if (itemType == typeof(string))
					{
						itemToWrite =
							item == DBNull.Value
								? null
								: item.ToString();
					}
					else if (item == DBNull.Value)
						itemToWrite = null;

					// write the object
					WriteRootBasicType(writer, itemToWrite, itemType, basicTypeInfo, encoding);
				}
			}
		}

		internal static void WriteRootBasicType(BufferWriterBase writer, object obj, Type type, BoisBasicTypeInfo typeInfo, Encoding encoding)
		{
			switch (typeInfo.KnownType)
			{
				case EnBasicKnownType.String:
					PrimitiveWriter.WriteValue(writer, obj as string, encoding);
					return;

				case EnBasicKnownType.Char:
					if (typeInfo.IsNullable)
						PrimitiveWriter.WriteValue(writer, obj as char?);
					else
						PrimitiveWriter.WriteValue(writer, (char)obj);
					return;

				case EnBasicKnownType.Guid:
					if (typeInfo.IsNullable)
						PrimitiveWriter.WriteValue(writer, obj as Guid?);
					else
						PrimitiveWriter.WriteValue(writer, (Guid)obj);
					return;

				case EnBasicKnownType.Bool:
					if (typeInfo.IsNullable)
						PrimitiveWriter.WriteValue(writer, obj as bool?);
					else
						PrimitiveWriter.WriteValue(writer, (bool)obj);
					return;

				case EnBasicKnownType.Enum:
					if (obj == null)
						PrimitiveWriter.WriteNullValue(writer);
					else
						PrimitiveWriter.WriteValue(writer, obj as Enum, type, null);
					return;

				case EnBasicKnownType.DateTime:
					if (typeInfo.IsNullable)
						PrimitiveWriter.WriteValue(writer, obj as DateTime?);
					else
						PrimitiveWriter.WriteValue(writer, (DateTime)obj);
					return;

				case EnBasicKnownType.DateTimeOffset:
					if (typeInfo.IsNullable)
						PrimitiveWriter.WriteValue(writer, obj as DateTimeOffset?);
					else
						PrimitiveWriter.WriteValue(writer, (DateTimeOffset)obj);
					return;

				case EnBasicKnownType.TimeSpan:
					if (typeInfo.IsNullable)
						PrimitiveWriter.WriteValue(writer, obj as TimeSpan?);
					else
						PrimitiveWriter.WriteValue(writer, (TimeSpan)obj);
					return;

				case EnBasicKnownType.ByteArray:
					PrimitiveWriter.WriteValue(writer, obj as byte[]);
					return;

				case EnBasicKnownType.KnownTypeArray:

					// calling for subitem
					WriteRootBasicTypedArray(writer, obj as Array, typeInfo, encoding);
					return;

				case EnBasicKnownType.Color:
					if (typeInfo.IsNullable)
						PrimitiveWriter.WriteValue(writer, obj as Color?);
					else
						PrimitiveWriter.WriteValue(writer, (Color)obj);
					break;

				case EnBasicKnownType.Version:
					PrimitiveWriter.WriteValue(writer, obj as Version);
					return;

				case EnBasicKnownType.DbNull:
					PrimitiveWriter.WriteValue(writer, obj as DBNull);
					return;

				case EnBasicKnownType.Uri:
					PrimitiveWriter.WriteValue(writer, (obj as Uri));
					break;

				case EnBasicKnownType.DataTable:
					PrimitiveWriter.WriteValue(writer, obj as DataTable, encoding);
					return;

				case EnBasicKnownType.DataSet:
					PrimitiveWriter.WriteValue(writer, obj as DataSet, encoding);
					return;

				case EnBasicKnownType.Int16:
					if (typeInfo.IsNullable)
						NumericSerializers.WriteVarInt(writer, obj as short?);
					else
						NumericSerializers.WriteVarInt(writer, (short)obj);
					break;

				case EnBasicKnownType.Int32:
					if (typeInfo.IsNullable)
						NumericSerializers.WriteVarInt(writer, obj as int?);
					else
						NumericSerializers.WriteVarInt(writer, (int)obj);
					return;

				case EnBasicKnownType.Int64:
					if (typeInfo.IsNullable)
						NumericSerializers.WriteVarInt(writer, obj as long?);
					else
						NumericSerializers.WriteVarInt(writer, (long)obj);
					return;

				case EnBasicKnownType.UInt16:
					if (typeInfo.IsNullable)
						NumericSerializers.WriteVarInt(writer, obj as ushort?);
					else
						NumericSerializers.WriteVarInt(writer, (ushort)obj);
					return;

				case EnBasicKnownType.UInt32:
					if (typeInfo.IsNullable)
						NumericSerializers.WriteVarInt(writer, obj as uint?);
					else
						NumericSerializers.WriteVarInt(writer, (uint)obj);
					return;

				case EnBasicKnownType.UInt64:
					if (typeInfo.IsNullable)
						NumericSerializers.WriteVarInt(writer, obj as ulong?);
					else
						NumericSerializers.WriteVarInt(writer, (ulong)obj);
					return;

				case EnBasicKnownType.Double:
					if (typeInfo.IsNullable)
						NumericSerializers.WriteVarDecimal(writer, obj as double?);
					else
						NumericSerializers.WriteVarDecimal(writer, (double)obj);
					return;

				case EnBasicKnownType.Decimal:
					if (typeInfo.IsNullable)
						NumericSerializers.WriteVarDecimal(writer, obj as decimal?);
					else
						NumericSerializers.WriteVarDecimal(writer, (decimal)obj);
					return;

				case EnBasicKnownType.Single:
					if (typeInfo.IsNullable)
						NumericSerializers.WriteVarDecimal(writer, obj as float?);
					else
						NumericSerializers.WriteVarDecimal(writer, (float)obj);
					return;

				case EnBasicKnownType.Byte:
					if (typeInfo.IsNullable)
						NumericSerializers.WriteVarInt(writer, obj as byte?);
					else
						writer.Write((byte)obj);
					return;

				case EnBasicKnownType.SByte:
					if (typeInfo.IsNullable)
						NumericSerializers.WriteVarInt(writer, obj as sbyte?);
					else
						writer.Write((sbyte)obj);
					return;


				case EnBasicKnownType.Unknown:
				default:
					// should never reach here
					throw new ArgumentException($"Not supported type '{type}' as root", nameof(type));
			}
		}

		internal static void WriteRootBasicTypedArray(BufferWriterBase writer, Array array, BoisBasicTypeInfo typeInfo, Encoding encoding)
		{
			if (array == null)
			{
				PrimitiveWriter.WriteNullValue(writer);
				return;
			}

			var arrayItemType = typeInfo.BareType;
			var arrayItemTypeType = BoisTypeCache.GetBasicType(typeInfo.BareType);

			// Int32
			NumericSerializers.WriteUIntNullableMemberCount(writer, (uint)array.Length);

			for (int i = 0; i < array.Length; i++)
			{
				WriteRootBasicType(writer, array.GetValue(i), arrayItemType, arrayItemTypeType, encoding);
			}

		}

		/// <summary>
		/// Does return `Encoding.GetBytes`
		/// One less call method to `encoding.GetBytes` means one less string copy
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static byte[] GetStringBytes(string str, Encoding encoding)
		{
			var chars = str.ToCharArray();
			var bytes = new byte[encoding.GetByteCount(chars, 0, chars.Length)];
			encoding.GetBytes(chars, 0, chars.Length, bytes, 0);

			return bytes;
		}

		/// <summary>
		/// Does return `Encoding.GetBytes`
		/// One less call method to `encoding.GetBytes` means one less string copy
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static byte[] GetStringBytes(ref string str, Encoding encoding)
		{
			var chars = str.ToCharArray();
			var bytes = new byte[encoding.GetByteCount(chars, 0, chars.Length)];
			encoding.GetBytes(chars, 0, chars.Length, bytes, 0);

			return bytes;
		}
	}
}
