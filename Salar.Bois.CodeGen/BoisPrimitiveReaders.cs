#nullable enable
using Salar.BinaryBuffers;
using Salar.Bois.Serializers;
using System;
using System.Data;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;

namespace Salar.Bois.CodeGen;

public static class BoisPrimitiveReaders
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? ReadString(BufferReaderBase reader, Encoding encoding)
    {
        return PrimitiveReader.ReadString(reader, encoding);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char ReadChar(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadChar(reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char? ReadCharNullable(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadCharNullable(reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool? ReadBooleanNullable(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadBooleanNullable(reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ReadBoolean(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadBoolean(reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime? ReadDateTimeNullable(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadDateTimeNullable(reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime ReadDateTime(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadDateTime(reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTimeOffset? ReadDateTimeOffsetNullable(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadDateTimeOffsetNullable(reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTimeOffset ReadDateTimeOffset(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadDateTimeOffset(reader);
    }

#if NET6_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateOnly ReadDateOnly(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadDateOnly(reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateOnly? ReadDateOnlyNullable(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadDateOnlyNullable(reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeOnly ReadTimeOnly(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadTimeOnly(reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeOnly? ReadTimeOnlyNullable(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadTimeOnlyNullable(reader);
    }
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[]? ReadByteArray(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadByteArray(reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReadEnumInt32(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadEnumInt32(reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int? ReadEnumInt32Nullable(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadEnumInt32Nullable(reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ReadEnumInt64(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadEnumInt64(reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long? ReadEnumInt64Nullable(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadEnumInt64Nullable(reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short ReadEnumInt16(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadEnumInt16(reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short? ReadEnumInt16Nullable(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadEnumInt16Nullable(reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ReadEnumUInt16(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadEnumUInt16(reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort? ReadEnumUInt16Nullable(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadEnumUInt16Nullable(reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ReadEnumUInt32(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadEnumUInt32(reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint? ReadEnumUInt32Nullable(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadEnumUInt32Nullable(reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ReadEnumUInt64(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadEnumUInt64(reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong? ReadEnumUInt64Nullable(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadEnumUInt64Nullable(reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ReadEnumByte(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadEnumByte(reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte? ReadEnumByteNullable(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadEnumByteNullable(reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte ReadEnumSByte(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadEnumSByte(reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte? ReadEnumSByteNullable(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadEnumSByteNullable(reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeSpan? ReadTimeSpanNullable(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadTimeSpanNullable(reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeSpan ReadTimeSpan(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadTimeSpan(reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Version? ReadVersion(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadVersion(reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid? ReadGuidNullable(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadGuidNullable(reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid ReadGuid(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadGuid(reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DBNull? ReadDbNull(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadDbNull(reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color? ReadColorNullable(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadColorNullable(reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color ReadColor(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadColor(reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Uri? ReadUri(BufferReaderBase reader)
    {
        return PrimitiveReader.ReadUri(reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DataTable? ReadDataTable(BufferReaderBase reader, Encoding encoding)
    {
        return PrimitiveReader.ReadDataTable(reader, encoding);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DataSet? ReadDataSet(BufferReaderBase reader, Encoding encoding)
    {
        return PrimitiveReader.ReadDataSet(reader, encoding);
    }
}
