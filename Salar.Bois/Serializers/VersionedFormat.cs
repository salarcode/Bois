using Salar.BinaryBuffers;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if BOIS_CODEGEN
using Salar.Bois.CodeGen;
#else
using Salar.Bois.Types;
#endif

/*
 * Salar BOIS (Binary Object Indexed Serialization)
 * by Salar Khalilzadeh
 *
 * https://github.com/salarcode/Bois
 * Mozilla Public License v2
 */
namespace Salar.Bois.Serializers;

// Shared by both packages. Used only by versioned types ([BoisContract(Versioned = true)]); other types never
// call into this file.

/// <summary>
/// The prefix of a versioned object: its byte size, in the place where other objects write their member count.
/// <list type="bullet">
/// <item><c>0xC0 | n</c>: size n, 0 to 63.</item>
/// <item><c>0x01</c> then a varint: a bigger size.</item>
/// </list>
/// Old readers decode both as a member count and ignore its value, so they still read versioned data.
/// </summary>
internal static class VersionedFormat
{
    internal const byte ShortLengthFlag = 0b1100_0000;
    internal const byte ShortLengthMask = 0b0011_1111;
    internal const byte LongLengthFlag = 0x01;
    internal const int MaxShortLength = 63;

    /// <summary>
    /// How many bytes the varint of a long size takes. The long prefix is <c>0x01</c> plus these bytes, so this is
    /// also how many bytes it needs beyond the one placeholder byte.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int LongLengthExtraBytes(int length)
    {
        var value = (uint)length;
        var count = 1;
        while ((value >>= 7) != 0)
            count++;
        return count;
    }

    /// <summary>
    /// Writes <c>0x01</c> and the varint of <paramref name="length"/>, the same varint as
    /// <c>NumericSerializers.ReadVarUInt32</c> reads.
    /// </summary>
    internal static void WriteLongLength(Span<byte> prefix, int length)
    {
        prefix[0] = LongLengthFlag;
        var value = (uint)length;
        var index = 1;
        while ((value & ~0x7Fu) != 0)
        {
            prefix[index++] = (byte)((value | 0x80) & 0xFF);
            value >>= 7;
        }
        prefix[index] = (byte)value;
    }
}

/// <summary>
/// Writes the size prefix of versioned objects. The size is only known after the members are written, so a
/// placeholder byte is written first and filled in at the end.
/// </summary>
internal static class VersionedWriter
{
    /// <summary>
    /// Starts a versioned object: writes the placeholder and returns where the object starts.
    /// </summary>
    internal static int Begin<TWriter>(ref TWriter writer)
        where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        if (!typeof(TWriter).IsValueType)
        {
            var instance = AsObject(ref writer);
            if (instance is BoisBufferWriter boisWriter)
                return boisWriter.BeginVersioned();
            if (instance is not BinaryBufferWriter)
                throw NotSupported(instance.GetType());
        }
#if NET9_0_OR_GREATER
        else if (typeof(TWriter) != typeof(BinarySpanBufferWriter))
#else
		else
#endif
        {
            throw NotSupported(typeof(TWriter));
        }

        var start = writer.Position;
        writer.Write((byte)0);
        return start;
    }

    /// <summary>
    /// Ends a versioned object: fills in its size.
    /// </summary>
    internal static void End<TWriter>(ref TWriter writer, int start)
        where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        if (!typeof(TWriter).IsValueType && AsObject(ref writer) is BoisBufferWriter boisWriter)
        {
            boisWriter.EndVersioned(start);
            return;
        }

        var length = writer.Position - start - 1;
        if (length <= VersionedFormat.MaxShortLength)
        {
            // the common case: the size fits in the placeholder, nothing moves
            WrittenBytes(ref writer)[start] = (byte)(VersionedFormat.ShortLengthFlag | length);
            return;
        }

        EndLong(ref writer, start, length);
    }

    // Direct overloads, called by the runtime serializer's generated code for these writer classes, so it skips the
    // generic code shared by all classes and its type checks.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int Begin(BoisBufferWriter writer) => writer.BeginVersioned();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void End(BoisBufferWriter writer, int start) => writer.EndVersioned(start);

    internal static int Begin(BinaryBufferWriter writer)
    {
        var start = writer.Position;
        writer.Write((byte)0);
        return start;
    }

    internal static void End(BinaryBufferWriter writer, int start)
    {
        var length = writer.Position - start - 1;
        if (length <= VersionedFormat.MaxShortLength)
        {
            var written = writer.ToArraySegment();
            written.Array![written.Offset + start] = (byte)(VersionedFormat.ShortLengthFlag | length);
            return;
        }

        EndLong(ref writer, start, length);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void EndLong<TWriter>(ref TWriter writer, int start, int length)
        where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        // grow the written data through the writer, so it checks there is room
        var extra = VersionedFormat.LongLengthExtraBytes(length);
        for (var i = 0; i < extra; i++)
            writer.Write((byte)0);

        // move the object's bytes forward in one copy, then write the long prefix in front of them
        var buffer = WrittenBytes(ref writer);
        buffer.Slice(start + 1, length).CopyTo(buffer.Slice(start + 1 + extra));
        VersionedFormat.WriteLongLength(buffer.Slice(start, 1 + extra), length);
    }

    /// <summary>
    /// The writer's bytes, indexed by its <see cref="IBufferWriter.Position"/>.
    /// </summary>
    private static Span<byte> WrittenBytes<TWriter>(ref TWriter writer)
        where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
#if NET9_0_OR_GREATER
        if (typeof(TWriter) == typeof(BinarySpanBufferWriter))
        {
            // the bytes are the caller's span, it is safe to write to them
            var written = Unsafe.As<TWriter, BinarySpanBufferWriter>(ref writer).ToReadOnlySpan();
            return MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(written), written.Length);
        }
#endif
        var segment = ((BinaryBufferWriter)AsObject(ref writer)).ToArraySegment();
        return new Span<byte>(segment.Array, segment.Offset, segment.Array!.Length - segment.Offset);
    }

    /// <summary>
    /// Only valid when <typeparamref name="TWriter"/> is a reference type.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static object AsObject<TWriter>(ref TWriter writer)
        where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
#if NET9_0_OR_GREATER
        return Unsafe.As<TWriter, object>(ref writer);
#else
		return writer;
#endif
    }

    private static NotSupportedException NotSupported(Type writerType)
        => new NotSupportedException(
            $"Versioned types can't be written with {writerType.Name}, because it can't fill in the object size after the object is written. " +
            "Write to a byte[], a span, a Stream, BinaryBufferWriter, BinarySpanBufferWriter or BoisBufferWriter.");
}

/// <summary>
/// Reads the size prefix of versioned objects and jumps over data.
/// </summary>
internal static class VersionedReader
{
    /// <summary><see cref="ReadPrefix"/> read <c>0x40</c>: the object is null.</summary>
    internal const int PrefixNull = 0;

    /// <summary><see cref="ReadPrefix"/> read a member count: the object was written in today's (not versioned) format.</summary>
    internal const int PrefixMemberCount = 1;

    /// <summary><see cref="ReadPrefix"/> read a byte size: the object was written as versioned.</summary>
    internal const int PrefixSize = 2;

    /// <summary>
    /// Reads the prefix of a versioned type and tells which format follows.
    /// <list type="bullet">
    /// <item><see cref="PrefixNull"/>: null. Throws instead when <paramref name="allowNull"/> is false (structs).</item>
    /// <item><see cref="PrefixMemberCount"/>: data in today's format, <paramref name="value"/> is the member count.
    /// Member i is slot i. Throws when the count is above <paramref name="slotCount"/>.</item>
    /// <item><see cref="PrefixSize"/>: versioned data, <paramref name="value"/> is the object's size in bytes.</item>
    /// </list>
    /// </summary>
    internal static int ReadPrefix<TReader>(ref TReader reader, int slotCount, bool allowNull, string typeName, out int value)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        var first = reader.ReadByte();
        if (first >= VersionedFormat.ShortLengthFlag)
        {
            // the common case: a size of 0 to 63, no more bytes to read
            value = first & VersionedFormat.ShortLengthMask;
            if (value > PrimitiveReader.MaxVersionedObjectLength)
                value = PrimitiveReader.CheckedObjectLength(ref reader, (uint)value);
            return PrefixSize;
        }

        if (first == NumericSerializers.FlagIsNull)
        {
            if (!allowNull)
                throw new InvalidDataException($"Invalid data for '{typeName}': a struct can't be null.");
            value = 0;
            return PrefixNull;
        }

        if (first == 0 || (first & VersionedFormat.ShortLengthFlag) == 0x80)
        {
            // today's format: 0x80 | count, or 0x00 then a varint
            var count = first == 0 ? NumericSerializers.ReadVarUInt32(ref reader) : (uint)(first & VersionedFormat.ShortLengthMask);
            if (count > (uint)slotCount)
                throw new InvalidDataException(
                    $"Invalid data for '{typeName}': it was written in the non-versioned format with {count} members, but the type has {slotCount} slots.");
            value = (int)count;
            return PrefixMemberCount;
        }

        value = ReadLength(ref reader, first, typeName);
        return PrefixSize;
    }

    /// <summary>
    /// Thrown by the generated reader of a versioned type when the reader can't tell its position.
    /// </summary>
    internal static void ThrowNoPosition(string typeName)
        => throw new NotSupportedException(
            $"Versioned type '{typeName}' needs a reader that knows its position, and StreamBufferReader doesn't. " +
            "Read from a byte[], a span, a Stream, or a reader whose Position works.");

    /// <summary>
    /// The size of a versioned object, from its first prefix byte. Throws when the prefix is not a size.
    /// </summary>
    internal static int ReadLength<TReader>(ref TReader reader, byte first, string typeName)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        uint length;
        if (first >= VersionedFormat.ShortLengthFlag)
            length = (uint)(first & VersionedFormat.ShortLengthMask);
        else if (first == VersionedFormat.LongLengthFlag)
            length = NumericSerializers.ReadVarUInt32(ref reader);
        else
            throw new InvalidDataException($"Invalid data for '{typeName}': the object prefix 0x{first:X2} is not a valid value.");

        return PrimitiveReader.CheckedObjectLength(ref reader, length);
    }

    /// <summary>
    /// Moves to the end of a versioned object, skipping slots from a newer version. Throws when the members
    /// read went past the end, which means the data doesn't match the type.
    /// </summary>
    internal static void SkipTo<TReader>(ref TReader reader, long end, string typeName)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        var position = reader.Position;
        if (position == end)
            return;
        if (position > end)
            throw new InvalidDataException($"Invalid data for '{typeName}': its members were read past the end of the object.");

        Skip(ref reader, (int)(end - position));
    }

    /// <summary>
    /// Jumps over <paramref name="count"/> bytes without allocating.
    /// </summary>
    internal static void Skip<TReader>(ref TReader reader, int count)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        if (!typeof(TReader).IsValueType && AsObject(ref reader) is BoisStreamReader streamReader)
        {
            streamReader.Skip(count);
            return;
        }

        // on byte[] and span readers this is only a view of the data, nothing is copied
        reader.ReadSpan(count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static object AsObject<TReader>(ref TReader reader)
        where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
#if NET9_0_OR_GREATER
        return Unsafe.As<TReader, object>(ref reader);
#else
		return reader;
#endif
    }
}
