using Salar.BinaryBuffers;
using Salar.Bois.Serializers;
using System;
using System.Data;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;

namespace Salar.Bois.Generator.Serializers;

public static class BoisPrimitiveWriters
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteNullValue(BufferWriterBase writer)
	{
		PrimitiveWriter.WriteNullValue(writer);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue(BufferWriterBase writer, string? str, Encoding encoding)
	{
		PrimitiveWriter.WriteValue(writer, str, encoding);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue(BufferWriterBase writer, char c)
	{
		PrimitiveWriter.WriteValue(writer, c);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue(BufferWriterBase writer, char? c)
	{
		PrimitiveWriter.WriteValue(writer, c);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue(BufferWriterBase writer, bool b)
	{
		PrimitiveWriter.WriteValue(writer, b);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue(BufferWriterBase writer, bool? b)
	{
		PrimitiveWriter.WriteValue(writer, b);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue(BufferWriterBase writer, DateTime dateTime)
	{
		PrimitiveWriter.WriteValue(writer, dateTime);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue(BufferWriterBase writer, DateTime? dt)
	{
		PrimitiveWriter.WriteValue(writer, dt);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue(BufferWriterBase writer, DateTimeOffset dateTimeOffset)
	{
		PrimitiveWriter.WriteValue(writer, dateTimeOffset);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue(BufferWriterBase writer, DateTimeOffset? dto)
	{
		PrimitiveWriter.WriteValue(writer, dto);
	}

#if NET6_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue(BufferWriterBase writer, DateOnly dateOnly)
	{
		PrimitiveWriter.WriteValue(writer, dateOnly);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue(BufferWriterBase writer, DateOnly? dto)
	{
		PrimitiveWriter.WriteValue(writer, dto);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue(BufferWriterBase writer, TimeOnly timeOnly)
	{
		PrimitiveWriter.WriteValue(writer, timeOnly);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue(BufferWriterBase writer, TimeOnly? dto)
	{
		PrimitiveWriter.WriteValue(writer, dto);
	}
#endif

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue(BufferWriterBase writer, byte[]? bytes)
	{
		PrimitiveWriter.WriteValue(writer, bytes);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue(BufferWriterBase writer, Enum? e)
	{
		PrimitiveWriter.WriteValue(writer, e);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue(BufferWriterBase writer, Enum? e, bool nullable)
	{
		PrimitiveWriter.WriteValue(writer, e, nullable);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue(BufferWriterBase writer, Enum? e, Type type, bool? memberIsNullable)
	{
		PrimitiveWriter.WriteValue(writer, e, type, memberIsNullable);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue(BufferWriterBase writer, TimeSpan timeSpan)
	{
		PrimitiveWriter.WriteValue(writer, timeSpan);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue(BufferWriterBase writer, TimeSpan? timeSpan)
	{
		PrimitiveWriter.WriteValue(writer, timeSpan);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue(BufferWriterBase writer, Version? version)
	{
		PrimitiveWriter.WriteValue(writer, version);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue(BufferWriterBase writer, Uri? uri)
	{
		PrimitiveWriter.WriteValue(writer, uri);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue(BufferWriterBase writer, Guid guid)
	{
		PrimitiveWriter.WriteValue(writer, guid);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue(BufferWriterBase writer, Guid? g)
	{
		PrimitiveWriter.WriteValue(writer, g);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue(BufferWriterBase writer, DBNull? dbNull)
	{
		PrimitiveWriter.WriteValue(writer, dbNull);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue(BufferWriterBase writer, Color color)
	{
		PrimitiveWriter.WriteValue(writer, color);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue(BufferWriterBase writer, Color? color)
	{
		PrimitiveWriter.WriteValue(writer, color);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue(BufferWriterBase writer, DataSet? ds, Encoding encoding)
	{
		PrimitiveWriter.WriteValue(writer, ds, encoding);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteValue(BufferWriterBase writer, DataTable? dt, Encoding encoding)
	{
		PrimitiveWriter.WriteValue(writer, dt, encoding);
	}
}
