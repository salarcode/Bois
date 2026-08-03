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
    public static string? ReadString<TReader>(ref TReader reader, Encoding encoding)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadString(ref reader, encoding);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char ReadChar<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadChar(ref reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char? ReadCharNullable<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadCharNullable(ref reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool? ReadBooleanNullable<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadBooleanNullable(ref reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ReadBoolean<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadBoolean(ref reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime? ReadDateTimeNullable<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadDateTimeNullable(ref reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime ReadDateTime<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadDateTime(ref reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTimeOffset? ReadDateTimeOffsetNullable<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadDateTimeOffsetNullable(ref reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTimeOffset ReadDateTimeOffset<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadDateTimeOffset(ref reader);
    }

#if NET6_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateOnly ReadDateOnly<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadDateOnly(ref reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateOnly? ReadDateOnlyNullable<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadDateOnlyNullable(ref reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeOnly ReadTimeOnly<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadTimeOnly(ref reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeOnly? ReadTimeOnlyNullable<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadTimeOnlyNullable(ref reader);
    }
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[]? ReadByteArray<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadByteArray(ref reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReadEnumInt32<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadEnumInt32(ref reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int? ReadEnumInt32Nullable<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadEnumInt32Nullable(ref reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ReadEnumInt64<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadEnumInt64(ref reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long? ReadEnumInt64Nullable<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadEnumInt64Nullable(ref reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short ReadEnumInt16<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadEnumInt16(ref reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short? ReadEnumInt16Nullable<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadEnumInt16Nullable(ref reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ReadEnumUInt16<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadEnumUInt16(ref reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort? ReadEnumUInt16Nullable<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadEnumUInt16Nullable(ref reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ReadEnumUInt32<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadEnumUInt32(ref reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint? ReadEnumUInt32Nullable<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadEnumUInt32Nullable(ref reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ReadEnumUInt64<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadEnumUInt64(ref reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong? ReadEnumUInt64Nullable<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadEnumUInt64Nullable(ref reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ReadEnumByte<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadEnumByte(ref reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte? ReadEnumByteNullable<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadEnumByteNullable(ref reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte ReadEnumSByte<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadEnumSByte(ref reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte? ReadEnumSByteNullable<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadEnumSByteNullable(ref reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeSpan? ReadTimeSpanNullable<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadTimeSpanNullable(ref reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeSpan ReadTimeSpan<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadTimeSpan(ref reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Version? ReadVersion<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadVersion(ref reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid? ReadGuidNullable<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadGuidNullable(ref reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid ReadGuid<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadGuid(ref reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DBNull? ReadDbNull<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadDbNull(ref reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color? ReadColorNullable<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadColorNullable(ref reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color ReadColor<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadColor(ref reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Uri? ReadUri<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadUri(ref reader);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DataTable? ReadDataTable<TReader>(ref TReader reader, Encoding encoding)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadDataTable(ref reader, encoding);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DataSet? ReadDataSet<TReader>(ref TReader reader, Encoding encoding)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        return PrimitiveReader.ReadDataSet(ref reader, encoding);
    }
}
