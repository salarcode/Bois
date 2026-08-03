#nullable enable
using Salar.BinaryBuffers;
using Salar.Bois.Serializers;
using System;
using System.Data;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;

namespace Salar.Bois.CodeGen;

public static class BoisPrimitiveWriters
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteNullValue<TWriter>(ref TWriter writer)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		PrimitiveWriter.WriteNullValue(ref writer);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue<TWriter>(ref TWriter writer, string? str, Encoding encoding)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		PrimitiveWriter.WriteValue(ref writer, str, encoding);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue<TWriter>(ref TWriter writer, char c)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		PrimitiveWriter.WriteValue(ref writer, c);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue<TWriter>(ref TWriter writer, char? c)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		PrimitiveWriter.WriteValue(ref writer, c);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue<TWriter>(ref TWriter writer, bool b)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		PrimitiveWriter.WriteValue(ref writer, b);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue<TWriter>(ref TWriter writer, bool? b)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		PrimitiveWriter.WriteValue(ref writer, b);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue<TWriter>(ref TWriter writer, DateTime dateTime)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		PrimitiveWriter.WriteValue(ref writer, dateTime);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue<TWriter>(ref TWriter writer, DateTime? dt)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		PrimitiveWriter.WriteValue(ref writer, dt);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue<TWriter>(ref TWriter writer, DateTimeOffset dateTimeOffset)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		PrimitiveWriter.WriteValue(ref writer, dateTimeOffset);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue<TWriter>(ref TWriter writer, DateTimeOffset? dto)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		PrimitiveWriter.WriteValue(ref writer, dto);
	}

#if NET6_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue<TWriter>(ref TWriter writer, DateOnly dateOnly)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		PrimitiveWriter.WriteValue(ref writer, dateOnly);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue<TWriter>(ref TWriter writer, DateOnly? dto)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		PrimitiveWriter.WriteValue(ref writer, dto);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue<TWriter>(ref TWriter writer, TimeOnly timeOnly)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		PrimitiveWriter.WriteValue(ref writer, timeOnly);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue<TWriter>(ref TWriter writer, TimeOnly? dto)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		PrimitiveWriter.WriteValue(ref writer, dto);
	}
#endif

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue<TWriter>(ref TWriter writer, byte[]? bytes)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		PrimitiveWriter.WriteValue(ref writer, bytes);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteEnumInt32<TWriter>(ref TWriter writer, Enum? e, bool nullable)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		PrimitiveWriter.WriteEnumInt32(ref writer, e, nullable);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteEnumInt64<TWriter>(ref TWriter writer, Enum? e, bool nullable)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		PrimitiveWriter.WriteEnumInt64(ref writer, e, nullable);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteEnumInt16<TWriter>(ref TWriter writer, Enum? e, bool nullable)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		PrimitiveWriter.WriteEnumInt16(ref writer, e, nullable);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteEnumUInt16<TWriter>(ref TWriter writer, Enum? e, bool nullable)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		PrimitiveWriter.WriteEnumUInt16(ref writer, e, nullable);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteEnumUInt32<TWriter>(ref TWriter writer, Enum? e, bool nullable)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		PrimitiveWriter.WriteEnumUInt32(ref writer, e, nullable);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteEnumUInt64<TWriter>(ref TWriter writer, Enum? e, bool nullable)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		PrimitiveWriter.WriteEnumUInt64(ref writer, e, nullable);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteEnumByte<TWriter>(ref TWriter writer, Enum? e, bool nullable)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		PrimitiveWriter.WriteEnumByte(ref writer, e, nullable);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteEnumSByte<TWriter>(ref TWriter writer, Enum? e, bool nullable)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		PrimitiveWriter.WriteEnumSByte(ref writer, e, nullable);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue<TWriter>(ref TWriter writer, TimeSpan timeSpan)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		PrimitiveWriter.WriteValue(ref writer, timeSpan);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue<TWriter>(ref TWriter writer, TimeSpan? timeSpan)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		PrimitiveWriter.WriteValue(ref writer, timeSpan);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue<TWriter>(ref TWriter writer, Version? version)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		PrimitiveWriter.WriteValue(ref writer, version);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue<TWriter>(ref TWriter writer, Uri? uri)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		PrimitiveWriter.WriteValue(ref writer, uri);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue<TWriter>(ref TWriter writer, Guid guid)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		PrimitiveWriter.WriteValue(ref writer, guid);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue<TWriter>(ref TWriter writer, Guid? g)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		PrimitiveWriter.WriteValue(ref writer, g);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue<TWriter>(ref TWriter writer, DBNull? dbNull)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		PrimitiveWriter.WriteValue(ref writer, dbNull);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue<TWriter>(ref TWriter writer, Color color)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		PrimitiveWriter.WriteValue(ref writer, color);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue<TWriter>(ref TWriter writer, Color? color)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		PrimitiveWriter.WriteValue(ref writer, color);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue<TWriter>(ref TWriter writer, DataSet? ds, Encoding encoding)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		PrimitiveWriter.WriteValue(ref writer, ds, encoding);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue<TWriter>(ref TWriter writer, DataTable? dt, Encoding encoding)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		PrimitiveWriter.WriteValue(ref writer, dt, encoding);
	}
}
