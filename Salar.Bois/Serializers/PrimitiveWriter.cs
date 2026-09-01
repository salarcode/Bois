using Salar.BinaryBuffers;
using Salar.Bois.Types;
using System;
using System.Buffers;
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
		internal static void WriteNullValue<TWriter>(ref TWriter writer)
				where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
					, allows ref struct
#endif
		{
			writer.Write(NumericSerializers.FlagIsNull);
		}

		/// <summary>
		/// Writes a raw byte. Used by the emitted IL, which can only pass the buffer by reference.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteValue<TWriter>(ref TWriter writer, byte num)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			writer.Write(num);
		}

		/// <summary>
		/// Writes a raw signed byte. Used by the emitted IL, which can only pass the buffer by reference.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteValue<TWriter>(ref TWriter writer, sbyte num)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			writer.Write(num);
		}

		/// <summary>
		/// String - Format: (Embedded-Nullable-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..63
		/// </summary>
		internal static void WriteValue<TWriter>(scoped ref TWriter writer, string str, Encoding encoding)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			if (str == null)
			{
				WriteNullValue(ref writer);
			}
			else if (str.Length == 0)
			{
				NumericSerializers.WriteUIntNullableMemberCount(ref writer, 0u);
			}
			else
			{
				var byteCount = encoding.GetByteCount(str);
#if NET6_0_OR_GREATER
				// a stackalloc buffer cannot be passed to a generic writer which may be a ref struct
				var rentedBytes = ArrayPool<byte>.Shared.Rent(byteCount);

				try
				{
					var bytesWritten = encoding.GetBytes(str.AsSpan(), rentedBytes);
					NumericSerializers.WriteUIntNullableMemberCount(ref writer, (uint)bytesWritten);
					writer.Write(rentedBytes, 0, bytesWritten);
				}
				finally
				{
					ArrayPool<byte>.Shared.Return(rentedBytes);
				}
#else
				var bytes = ArrayPool<byte>.Shared.Rent(byteCount);
				try
				{
					var bytesWritten = encoding.GetBytes(str, 0, str.Length, bytes, 0);
					NumericSerializers.WriteUIntNullableMemberCount(ref writer, (uint)bytesWritten);
					writer.Write(bytes, 0, bytesWritten);
				}
				finally
				{
					ArrayPool<byte>.Shared.Return(bytes);
				}
#endif
			}
		}

		/// <summary>
		/// char - Format: (Embedded-0-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..127
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteValue<TWriter>(ref TWriter writer, char c)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
        {
			writer.Write((ushort)c);
		}

		/// <summary>
		/// char? - Format: (Embedded-Nullable-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..63
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteValue<TWriter>(ref TWriter writer, char? c)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			NumericSerializers.WriteVarInt(ref writer, (ushort?)c);
		}

		/// <summary>
		/// bool - Format: (Embedded=true-0-0-0-0-0-0-0)
		/// Embeddable range: 0..127
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteValue<TWriter>(ref TWriter writer, bool b)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			writer.Write(b);
		}

		/// <summary>
		/// bool? - Format: (Embedded=true-Nullable-0-0-0-0-0-0)
		/// Embeddable range: 0..63
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteValue<TWriter>(ref TWriter writer, bool? b)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			byte? val = null;
			if (b.HasValue)
				val = b.Value ? (byte)1 : (byte)0;

			NumericSerializers.WriteVarInt(ref writer, val);
		}

		/// <summary>
		/// DateTime - Format: (Kind:0-0-0-0-0-0-0-0) (dateTimeTicks:Embedded-0-0-0-0-0-0-0)[if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable kind range: always embedded
		/// Embeddable ticks range: 0..127
		/// </summary>
		internal static void WriteValue<TWriter>(ref TWriter writer, DateTime dateTime)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			var kind = (byte)dateTime.Kind;

			if (dateTime == DateTime.MinValue)
			{
				writer.Write(kind);
				// min datetime indicator
				NumericSerializers.WriteVarInt(ref writer, 0L);
			}
			else if (dateTime == DateTime.MaxValue)
			{
				writer.Write(kind);
				// max datetime indicator
				NumericSerializers.WriteVarInt(ref writer, 1L);
			}
			else
			{
				writer.Write(kind);
				//Int64
				NumericSerializers.WriteVarInt(ref writer, dateTime.Ticks);
			}
		}

		/// <summary>
		/// DateTime? - Format: (Kind:Nullable-0-0-0-0-0-0-0) (dateTimeTicks:Embedded-0-0-0-0-0-0-0)[if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable kind range: always embedded
		/// Embeddable ticks range: 0..127
		/// </summary>
		internal static void WriteValue<TWriter>(ref TWriter writer, DateTime? dt)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			if (dt == null)
			{
				WriteNullValue(ref writer);
				return;
			}
			var dateTime = dt.Value;
			var kind = (byte?)dateTime.Kind;

			if (dateTime == DateTime.MinValue)
			{
				NumericSerializers.WriteVarInt(ref writer, kind);
				// min datetime indicator
				NumericSerializers.WriteVarInt(ref writer, 0L);
			}
			else if (dateTime == DateTime.MaxValue)
			{
				NumericSerializers.WriteVarInt(ref writer, kind);
				// max datetime indicator
				NumericSerializers.WriteVarInt(ref writer, 1L);
			}
			else
			{
				NumericSerializers.WriteVarInt(ref writer, kind);
				//Int64
				NumericSerializers.WriteVarInt(ref writer, dateTime.Ticks);
			}
		}

		/// <summary>
		/// DateTimeOffset - Format: (Offset:Embedded-0-0-0-0-0-0-0)[if ofset not embedded?0-0-0-0-0-0-0-0] (dateTimeOffsetTicks:Embedded-0-0-0-0-0-0-0)[if ticks not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable offset range: 0..127
		/// Embeddable ticks range: 0..127
		/// </summary>
		internal static void WriteValue<TWriter>(ref TWriter writer, DateTimeOffset dateTimeOffset)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			var offset = dateTimeOffset.Offset;
			short offsetMinutes;
			unchecked
			{
				offsetMinutes = (short)((offset.Hours * 60) + offset.Minutes);
			}
			// int16
			NumericSerializers.WriteVarInt(ref writer, offsetMinutes);

			// int64
			NumericSerializers.WriteVarInt(ref writer, dateTimeOffset.Ticks);
		}

		/// <summary>
		/// DateTimeOffset? - Format: (Offset:Embedded-Nullable-0-0-0-0-0-0)[if ofset not embedded?0-0-0-0-0-0-0-0] (dateTimeOffsetTicks:Embedded-0-0-0-0-0-0-0)[if ticks not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable offset range: 0..63
		/// Embeddable ticks range: 0..127
		/// </summary>
		internal static void WriteValue<TWriter>(ref TWriter writer, DateTimeOffset? dto)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			if (dto == null)
			{
				WriteNullValue(ref writer);
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
			NumericSerializers.WriteVarInt(ref writer, offsetMinutes);

			// int64
			NumericSerializers.WriteVarInt(ref writer, dateTimeOffset.Ticks);
		}

#if NET6_0_OR_GREATER
		/// <summary>
		/// DateOnly - Format: 
		/// Embeddable day-number range: 
		/// </summary>
		internal static void WriteValue<TWriter>(ref TWriter writer, DateOnly dateOnly)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			if (dateOnly == DateOnly.MinValue)
			{
				// min dateOnly indicator
				NumericSerializers.WriteVarInt(ref writer, 0);
			}
			else if (dateOnly == DateOnly.MaxValue)
			{
				// max dateOnly indicator
				NumericSerializers.WriteVarInt(ref writer, 1);
			}
			else
			{
				//Int32
				NumericSerializers.WriteVarInt(ref writer, dateOnly.DayNumber);
			}
		}

		/// <summary>
		/// DateOnly? - Format:
		/// Embeddable day-number range: 
		/// </summary>
		internal static void WriteValue<TWriter>(ref TWriter writer, DateOnly? dto)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			if (dto == null)
			{
				WriteNullValue(ref writer);
				return;
			}
			var dateOnly = dto.Value;

			if (dateOnly == DateOnly.MinValue)
			{
				// min dateOnly indicator
				NumericSerializers.WriteVarInt(ref writer, (int?)0);
			}
			else if (dateOnly == DateOnly.MaxValue)
			{
				// max dateOnly indicator
				NumericSerializers.WriteVarInt(ref writer, (int?)1);
			}
			else
			{
				//Int32
				NumericSerializers.WriteVarInt(ref writer, (int?)dateOnly.DayNumber);
			}
		}


		/// <summary>
		/// DateOnly - Format: 
		/// Embeddable ticks range: 
		/// </summary>
		internal static void WriteValue<TWriter>(ref TWriter writer, TimeOnly timeOnly)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			if (timeOnly == TimeOnly.MinValue)
			{
				// min dateOnly indicator
				NumericSerializers.WriteVarInt(ref writer, 0L);
			}
			else if (timeOnly == TimeOnly.MaxValue)
			{
				// max dateOnly indicator
				NumericSerializers.WriteVarInt(ref writer, 1L);
			}
			else
			{
				//Int64
				NumericSerializers.WriteVarInt(ref writer, timeOnly.Ticks);
			}
		}

		/// <summary>
		/// TimeOnly? - Format: 
		/// Embeddable ticks range: 
		/// </summary>
		internal static void WriteValue<TWriter>(ref TWriter writer, TimeOnly? dto)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			if (dto == null)
			{
				WriteNullValue(ref writer);
				return;
			}
			var timeOnly = dto.Value;

			if (timeOnly == TimeOnly.MinValue)
			{
				// min timeOnly indicator
				NumericSerializers.WriteVarInt(ref writer, (long?)0L);
			}
			else if (timeOnly == TimeOnly.MaxValue)
			{
				// max timeOnly indicator
				NumericSerializers.WriteVarInt(ref writer, (long?)1L);
			}
			else
			{
				//Int64
				NumericSerializers.WriteVarInt(ref writer, (long?)timeOnly.Ticks);
			}
		}
#endif

		/// <summary>
		/// byte[] - Format: (Array Length:Embedded-Nullable-0-0-0-0-0-0) [if array length not embedded?0-0-0-0-0-0-0-0] (data:0-0-0-0-0-0-0-0)
		/// Embeddable Array Length range: 0..63
		/// </summary>
		internal static void WriteValue<TWriter>(ref TWriter writer, byte[] bytes)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			if (bytes == null)
			{
				WriteNullValue(ref writer);
				return;
			}

			// uint doesn't deal with negative numbers
			NumericSerializers.WriteUIntNullableMemberCount(ref writer, (uint)bytes.Length);
			writer.Write(bytes);
		}

#if SupportsEmit
		/// <summary>
		/// Caches the underlying primitive type of an enum so it is resolved once per closed generic.
		/// </summary>
		private static class EnumInfo<TEnum>
			where TEnum : struct, Enum
		{
			internal static readonly TypeCode UnderlyingTypeCode =
				Type.GetTypeCode(Enum.GetUnderlyingType(typeof(TEnum)));
		}

		/// <summary>
		/// Writes an enum without boxing by reinterpreting it as its underlying primitive.
		/// VarInt - Format: (Embedded-Nullable-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..63
		/// </summary>
		internal static void WriteEnumGeneric<TEnum, TWriter>(ref TWriter writer, TEnum e, bool nullable)
			where TEnum : struct, Enum
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			switch (EnumInfo<TEnum>.UnderlyingTypeCode)
			{
				case TypeCode.Int32:
					{
						var value = Unsafe.As<TEnum, int>(ref e);
						if (nullable)
							NumericSerializers.WriteVarInt(ref writer, (int?)value);
						else
							NumericSerializers.WriteVarInt(ref writer, value);
						break;
					}

				case TypeCode.Byte:
					{
						var value = Unsafe.As<TEnum, byte>(ref e);
						if (nullable)
							NumericSerializers.WriteVarInt(ref writer, (byte?)value);
						else
							writer.Write(value);
						break;
					}

				case TypeCode.Int16:
					{
						var value = Unsafe.As<TEnum, short>(ref e);
						if (nullable)
							NumericSerializers.WriteVarInt(ref writer, (short?)value);
						else
							NumericSerializers.WriteVarInt(ref writer, value);
						break;
					}

				case TypeCode.Int64:
					{
						var value = Unsafe.As<TEnum, long>(ref e);
						if (nullable)
							NumericSerializers.WriteVarInt(ref writer, (long?)value);
						else
							NumericSerializers.WriteVarInt(ref writer, value);
						break;
					}

				case TypeCode.UInt16:
					{
						var value = Unsafe.As<TEnum, ushort>(ref e);
						if (nullable)
							NumericSerializers.WriteVarInt(ref writer, (ushort?)value);
						else
							NumericSerializers.WriteVarInt(ref writer, value);
						break;
					}

				case TypeCode.UInt32:
					{
						var value = Unsafe.As<TEnum, uint>(ref e);
						if (nullable)
							NumericSerializers.WriteVarInt(ref writer, (uint?)value);
						else
							NumericSerializers.WriteVarInt(ref writer, value);
						break;
					}

				case TypeCode.UInt64:
					{
						var value = Unsafe.As<TEnum, ulong>(ref e);
						if (nullable)
							NumericSerializers.WriteVarInt(ref writer, (ulong?)value);
						else
							NumericSerializers.WriteVarInt(ref writer, value);
						break;
					}

				case TypeCode.SByte:
					{
						var value = Unsafe.As<TEnum, sbyte>(ref e);
						if (nullable)
							NumericSerializers.WriteVarInt(ref writer, (sbyte?)value);
						else
							writer.Write(value);
						break;
					}

				default:
					throw new ArgumentException($"Enum type not supported '{typeof(TEnum).Name}'. Contact the author please https://github.com/salarcode/Bois/issues ", nameof(e));
			}
		}

		/// <summary>
		/// Writes a nullable enum without boxing.
		/// </summary>
		internal static void WriteEnumGeneric<TEnum, TWriter>(ref TWriter writer, TEnum? e, bool nullable)
			where TEnum : struct, Enum
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			if (e == null)
			{
				WriteNullValue(ref writer);
				return;
			}
			WriteEnumGeneric<TEnum, TWriter>(ref writer, e.Value, nullable);
		}

		/// <summary>
		/// VarInt - Format: (Embedded-Nullable-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..63
		/// </summary>
		internal static void WriteValue<TWriter>(ref TWriter writer, Enum e)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			if (e == null)
			{
				WriteNullValue(ref writer);
				return;
			}
			WriteValue(ref writer, e, e.GetType(), null);
		}

		/// <summary>
		/// VarInt - Format: (Embedded-Nullable-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..63
		/// </summary>
		internal static void WriteValue<TWriter>(ref TWriter writer, Enum e, bool nullable)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			if (e == null)
			{
				WriteNullValue(ref writer);
				return;
			}
			WriteValue(ref writer, e, e.GetType(), nullable);
		}


		/// <summary>
		/// VarInt - Format: (Embedded-Nullable-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..63
		/// </summary>
		internal static void WriteValue<TWriter>(ref TWriter writer, Enum e, Type type, bool? memberIsNullable)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			if (e == null)
			{
				WriteNullValue(ref writer);
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
						NumericSerializers.WriteVarInt(ref writer, (int?)(int)(object)e);
					else
						NumericSerializers.WriteVarInt(ref writer, (int)(object)e);
					break;

				case EnBasicEnumType.Byte:
					if (isNullable)
						NumericSerializers.WriteVarInt(ref writer, (byte?)(byte)(object)e);
					else
						writer.Write((byte)(object)e);
					break;

				case EnBasicEnumType.Int16:
					if (isNullable)
						NumericSerializers.WriteVarInt(ref writer, (short?)(short)(object)e);
					else
						NumericSerializers.WriteVarInt(ref writer, (short)(object)e);
					break;

				case EnBasicEnumType.Int64:
					if (isNullable)
						NumericSerializers.WriteVarInt(ref writer, (long?)(long)(object)e);
					else
						NumericSerializers.WriteVarInt(ref writer, (long)(object)e);
					break;

				case EnBasicEnumType.UInt16:
					if (isNullable)
						NumericSerializers.WriteVarInt(ref writer, (ushort?)(ushort)(object)e);
					else
						NumericSerializers.WriteVarInt(ref writer, (ushort)(object)e);
					break;

				case EnBasicEnumType.UInt32:
					if (isNullable)
						NumericSerializers.WriteVarInt(ref writer, (uint?)(uint)(object)e);
					else
						NumericSerializers.WriteVarInt(ref writer, (uint)(object)e);
					break;

				case EnBasicEnumType.UInt64:
					if (isNullable)
						NumericSerializers.WriteVarInt(ref writer, (ulong?)(ulong)(object)e);
					else
						NumericSerializers.WriteVarInt(ref writer, (ulong)(object)e);
					break;

				case EnBasicEnumType.SByte:
					if (isNullable)
						NumericSerializers.WriteVarInt(ref writer, (sbyte?)(sbyte)(object)e);
					else
						writer.Write((sbyte)(object)e);
					break;

				default:
					throw new ArgumentException($"Enum type not supported '{type.Name}'. Contact the author please https://github.com/salarcode/Bois/issues ", nameof(type));
			}
		}
#else
		internal static void WriteEnumInt32<TWriter>(ref TWriter writer, Enum e, bool nullable)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			if (e == null)
			{
				WriteNullValue(ref writer);
				return;
			}
			if (nullable)
				NumericSerializers.WriteVarInt(ref writer, (int?)(int)(object)e);
			else
				NumericSerializers.WriteVarInt(ref writer, (int)(object)e);
		}

		internal static void WriteEnumInt64<TWriter>(ref TWriter writer, Enum e, bool nullable)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			if (e == null)
			{
				WriteNullValue(ref writer);
				return;
			}
			if (nullable)
				NumericSerializers.WriteVarInt(ref writer, (long?)(long)(object)e);
			else
				NumericSerializers.WriteVarInt(ref writer, (long)(object)e);
		}

		internal static void WriteEnumInt16<TWriter>(ref TWriter writer, Enum e, bool nullable)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			if (e == null)
			{
				WriteNullValue(ref writer);
				return;
			}
			if (nullable)
				NumericSerializers.WriteVarInt(ref writer, (short?)(short)(object)e);
			else
				NumericSerializers.WriteVarInt(ref writer, (short)(object)e);
		}

		internal static void WriteEnumUInt16<TWriter>(ref TWriter writer, Enum e, bool nullable)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			if (e == null)
			{
				WriteNullValue(ref writer);
				return;
			}
			if (nullable)
				NumericSerializers.WriteVarInt(ref writer, (ushort?)(ushort)(object)e);
			else
				NumericSerializers.WriteVarInt(ref writer, (ushort)(object)e);
		}

		internal static void WriteEnumUInt32<TWriter>(ref TWriter writer, Enum e, bool nullable)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			if (e == null)
			{
				WriteNullValue(ref writer);
				return;
			}
			if (nullable)
				NumericSerializers.WriteVarInt(ref writer, (uint?)(uint)(object)e);
			else
				NumericSerializers.WriteVarInt(ref writer, (uint)(object)e);
		}

		internal static void WriteEnumUInt64<TWriter>(ref TWriter writer, Enum e, bool nullable)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			if (e == null)
			{
				WriteNullValue(ref writer);
				return;
			}
			if (nullable)
				NumericSerializers.WriteVarInt(ref writer, (ulong?)(ulong)(object)e);
			else
				NumericSerializers.WriteVarInt(ref writer, (ulong)(object)e);
		}

		internal static void WriteEnumByte<TWriter>(ref TWriter writer, Enum e, bool nullable)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			if (e == null)
			{
				WriteNullValue(ref writer);
				return;
			}
			if (nullable)
				NumericSerializers.WriteVarInt(ref writer, (byte?)(byte)(object)e);
			else
				writer.Write((byte)(object)e);
		}

		internal static void WriteEnumSByte<TWriter>(ref TWriter writer, Enum e, bool nullable)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			if (e == null)
			{
				WriteNullValue(ref writer);
				return;
			}
			if (nullable)
				NumericSerializers.WriteVarInt(ref writer, (sbyte?)(sbyte)(object)e);
			else
				writer.Write((sbyte)(object)e);
		}
#endif


		/// <summary>
		/// TimeSpan - Format: (Embedded-0-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..127
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteValue<TWriter>(ref TWriter writer, TimeSpan timeSpan)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			NumericSerializers.WriteVarInt(ref writer, timeSpan.Ticks);
		}

		/// <summary>
		/// TimeSpan? - Format: (Embedded-Nullable-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..63
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteValue<TWriter>(ref TWriter writer, TimeSpan? timeSpan)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			if (timeSpan == null)
			{
				WriteNullValue(ref writer);
				return;
			}
			NumericSerializers.WriteVarInt(ref writer, (long?)timeSpan.Value.Ticks);
		}

		/// <summary>
		/// Version - Format: (Embedded-Nullable-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..63
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteValue<TWriter>(ref TWriter writer, Version version)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			if (version == null)
			{
				WriteNullValue(ref writer);
				return;
			}
			WriteValue(ref writer, version.ToString(), Encoding.ASCII);
		}

		/// <summary>
		/// Same as String
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteValue<TWriter>(ref TWriter writer, Uri uri)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			PrimitiveWriter.WriteValue(ref writer, uri?.ToString(), Encoding.UTF8);
		}

		/// <summary>
		/// Guid - Format: (Embedded-0-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..127
		/// </summary>
		internal static void WriteValue<TWriter>(ref TWriter writer, Guid guid)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			if (guid == Guid.Empty)
			{
				// Int32
				NumericSerializers.WriteVarInt(ref writer, (uint)0);
				return;
			}

			var data = guid.ToByteArray();

			// Int32
			NumericSerializers.WriteVarInt(ref writer, (uint)data.Length);
			writer.Write(data);
		}

		/// <summary>
		/// Guid? - Format: (Embedded-Nullable-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..63
		/// </summary>
		internal static void WriteValue<TWriter>(ref TWriter writer, Guid? g)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			if (g == null)
			{
				WriteNullValue(ref writer);
				return;
			}

			var guid = g.Value;
			if (guid == Guid.Empty)
			{
				// UInt32
				NumericSerializers.WriteUIntNullableMemberCount(ref writer, 0u);
				return;
			}

			var data = guid.ToByteArray();

			// UInt32
			NumericSerializers.WriteUIntNullableMemberCount(ref writer, (uint)data.Length);
			writer.Write(data);
		}

		/// <summary>
		/// DBNull? - Format: (Embedded=true-Nullable=true-0-0-0-0-0-0)
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteValue<TWriter>(ref TWriter writer, DBNull dbNull)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			if (dbNull == null)
				WriteNullValue(ref writer);
			else
			{
				WriteValue(ref writer, true);
			}
		}
		/// <summary>
		/// Same as Int32
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteValue<TWriter>(ref TWriter writer, Color color)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			int argb = color.ToArgb();
			// Int32
			NumericSerializers.WriteVarInt(ref writer, argb);
		}

		/// <summary>
		/// Same as Nullable<Int32>
		/// </summary>
		internal static void WriteValue<TWriter>(ref TWriter writer, Color? color)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			if (color == null)
			{
				WriteNullValue(ref writer);
				return;
			}
			int? argb = color.Value.ToArgb();
			// Int32
			NumericSerializers.WriteVarInt(ref writer, argb);
		}

		/// <summary>
		/// Obsolete - only backward compatibility
		/// </summary>
		internal static void WriteValue<TWriter>(ref TWriter writer, DataSet ds, Encoding encoding)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
#if SupportsEmit
			if (ds == null)
			{
				WriteNullValue(ref writer);
				return;
			}
			// tables count
			NumericSerializers.WriteUIntNullableMemberCount(ref writer, (uint)ds.Tables.Count);

			WriteValue(ref writer, ds.DataSetName, encoding);

			foreach (DataTable dt in ds.Tables)
			{
				WriteValue(ref writer, dt, encoding);
			}
#else
			throw new NotSupportedException("DataSet is not supported with source generator.");
#endif
		}

		/// <summary>
		/// Obsolete - only backward compatibility
		/// </summary>
		internal static void WriteValue<TWriter>(ref TWriter writer, DataTable dt, Encoding encoding)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
#if SupportsEmit
			if (dt == null)
			{
				WriteNullValue(ref writer);
				return;
			}
			// column count
			NumericSerializers.WriteUIntNullableMemberCount(ref writer, (uint)dt.Columns.Count);

			// table name
			WriteValue(ref writer, dt.TableName, encoding);

			// columns
			foreach (DataColumn col in dt.Columns)
			{
				WriteValue(ref writer, col.Caption, encoding);
				WriteValue(ref writer, col.ColumnName, encoding);

				var dataType = col.DataType?.ToString();
				if (dataType != null && dataType.StartsWith("System."))
				{
					dataType = "0." + dataType.Remove(0, "System.".Length);
				}
				WriteValue(ref writer, dataType, encoding);
			}

			NumericSerializers.WriteVarInt(ref writer, dt.Rows.Count);
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
					WriteRootBasicType(ref writer, itemToWrite, itemType, basicTypeInfo, encoding);
				}
			}
#else
			throw new NotSupportedException("DataTable is not supported with source generator.");
#endif
		}

#if SupportsEmit

		internal static void WriteRootBasicType<TWriter>(ref TWriter writer, object obj, Type type, BoisBasicTypeInfo typeInfo, Encoding encoding)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			switch (typeInfo.KnownType)
			{
				case EnBasicKnownType.String:
					PrimitiveWriter.WriteValue(ref writer, obj as string, encoding);
					return;

				case EnBasicKnownType.Char:
					if (typeInfo.IsNullable)
						PrimitiveWriter.WriteValue(ref writer, obj as char?);
					else
						PrimitiveWriter.WriteValue(ref writer, (char)obj);
					return;

				case EnBasicKnownType.Guid:
					if (typeInfo.IsNullable)
						PrimitiveWriter.WriteValue(ref writer, obj as Guid?);
					else
						PrimitiveWriter.WriteValue(ref writer, (Guid)obj);
					return;

				case EnBasicKnownType.Bool:
					if (typeInfo.IsNullable)
						PrimitiveWriter.WriteValue(ref writer, obj as bool?);
					else
						PrimitiveWriter.WriteValue(ref writer, (bool)obj);
					return;

				case EnBasicKnownType.Enum:
					if (obj == null)
						PrimitiveWriter.WriteNullValue(ref writer);
					else
						PrimitiveWriter.WriteValue(ref writer, obj as Enum, type, null);
					return;

				case EnBasicKnownType.DateTime:
					if (typeInfo.IsNullable)
						PrimitiveWriter.WriteValue(ref writer, obj as DateTime?);
					else
						PrimitiveWriter.WriteValue(ref writer, (DateTime)obj);
					return;

				case EnBasicKnownType.DateTimeOffset:
					if (typeInfo.IsNullable)
						PrimitiveWriter.WriteValue(ref writer, obj as DateTimeOffset?);
					else
						PrimitiveWriter.WriteValue(ref writer, (DateTimeOffset)obj);
					return;

#if NET6_0_OR_GREATER
				case EnBasicKnownType.DateOnly:
					if (typeInfo.IsNullable)
						PrimitiveWriter.WriteValue(ref writer, obj as DateOnly?);
					else
						PrimitiveWriter.WriteValue(ref writer, (DateOnly)obj);
					return;

				case EnBasicKnownType.TimeOnly:
					if (typeInfo.IsNullable)
						PrimitiveWriter.WriteValue(ref writer, obj as TimeOnly?);
					else
						PrimitiveWriter.WriteValue(ref writer, (TimeOnly)obj);
					return;
#endif

				case EnBasicKnownType.TimeSpan:
					if (typeInfo.IsNullable)
						PrimitiveWriter.WriteValue(ref writer, obj as TimeSpan?);
					else
						PrimitiveWriter.WriteValue(ref writer, (TimeSpan)obj);
					return;

				case EnBasicKnownType.ByteArray:
					PrimitiveWriter.WriteValue(ref writer, obj as byte[]);
					return;

				case EnBasicKnownType.KnownTypeArray:

					// calling for subitem
					WriteRootBasicTypedArray(ref writer, obj as Array, typeInfo, encoding);
					return;

				case EnBasicKnownType.Color:
					if (typeInfo.IsNullable)
						PrimitiveWriter.WriteValue(ref writer, obj as Color?);
					else
						PrimitiveWriter.WriteValue(ref writer, (Color)obj);
					break;

				case EnBasicKnownType.Version:
					PrimitiveWriter.WriteValue(ref writer, obj as Version);
					return;

				case EnBasicKnownType.DbNull:
					PrimitiveWriter.WriteValue(ref writer, obj as DBNull);
					return;

				case EnBasicKnownType.Uri:
					PrimitiveWriter.WriteValue(ref writer, (obj as Uri));
					break;

				case EnBasicKnownType.DataTable:
					PrimitiveWriter.WriteValue(ref writer, obj as DataTable, encoding);
					return;

				case EnBasicKnownType.DataSet:
					PrimitiveWriter.WriteValue(ref writer, obj as DataSet, encoding);
					return;

				case EnBasicKnownType.Int16:
					if (typeInfo.IsNullable)
						NumericSerializers.WriteVarInt(ref writer, obj as short?);
					else
						NumericSerializers.WriteVarInt(ref writer, (short)obj);
					break;

				case EnBasicKnownType.Int32:
					if (typeInfo.IsNullable)
						NumericSerializers.WriteVarInt(ref writer, obj as int?);
					else
						NumericSerializers.WriteVarInt(ref writer, (int)obj);
					return;

				case EnBasicKnownType.Int64:
					if (typeInfo.IsNullable)
						NumericSerializers.WriteVarInt(ref writer, obj as long?);
					else
						NumericSerializers.WriteVarInt(ref writer, (long)obj);
					return;

				case EnBasicKnownType.UInt16:
					if (typeInfo.IsNullable)
						NumericSerializers.WriteVarInt(ref writer, obj as ushort?);
					else
						NumericSerializers.WriteVarInt(ref writer, (ushort)obj);
					return;

				case EnBasicKnownType.UInt32:
					if (typeInfo.IsNullable)
						NumericSerializers.WriteVarInt(ref writer, obj as uint?);
					else
						NumericSerializers.WriteVarInt(ref writer, (uint)obj);
					return;

				case EnBasicKnownType.UInt64:
					if (typeInfo.IsNullable)
						NumericSerializers.WriteVarInt(ref writer, obj as ulong?);
					else
						NumericSerializers.WriteVarInt(ref writer, (ulong)obj);
					return;

				case EnBasicKnownType.Double:
					if (typeInfo.IsNullable)
						NumericSerializers.WriteVarDecimal(ref writer, obj as double?);
					else
						NumericSerializers.WriteVarDecimal(ref writer, (double)obj);
					return;

				case EnBasicKnownType.Decimal:
					if (typeInfo.IsNullable)
						NumericSerializers.WriteVarDecimal(ref writer, obj as decimal?);
					else
						NumericSerializers.WriteVarDecimal(ref writer, (decimal)obj);
					return;

				case EnBasicKnownType.Single:
					if (typeInfo.IsNullable)
						NumericSerializers.WriteVarDecimal(ref writer, obj as float?);
					else
						NumericSerializers.WriteVarDecimal(ref writer, (float)obj);
					return;

				case EnBasicKnownType.Byte:
					if (typeInfo.IsNullable)
						NumericSerializers.WriteVarInt(ref writer, obj as byte?);
					else
						writer.Write((byte)obj);
					return;

				case EnBasicKnownType.SByte:
					if (typeInfo.IsNullable)
						NumericSerializers.WriteVarInt(ref writer, obj as sbyte?);
					else
						writer.Write((sbyte)obj);
					return;


				case EnBasicKnownType.Unknown:
				default:
					// should never reach here
					throw new ArgumentException($"Not supported type '{type}' as root", nameof(type));
			}
		}

		internal static void WriteRootBasicTypedArray<TWriter>(ref TWriter writer, Array array, BoisBasicTypeInfo typeInfo, Encoding encoding)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
				, allows ref struct
#endif
		{
			if (array == null)
			{
				PrimitiveWriter.WriteNullValue(ref writer);
				return;
			}

			var arrayItemType = typeInfo.BareType;
			var arrayItemTypeType = BoisTypeCache.GetBasicType(typeInfo.BareType);

			// Int32
			NumericSerializers.WriteUIntNullableMemberCount(ref writer, (uint)array.Length);

			for (int i = 0; i < array.Length; i++)
			{
				WriteRootBasicType(ref writer, array.GetValue(i), arrayItemType, arrayItemTypeType, encoding);
			}
		}
#endif

	}
}
