using System;
using System.Collections.Generic;
using Salar.Bois.Serializers;
using System.Text;

namespace Salar.Bois.CodeGen;

public static class BoisCodeGen
{
    public static Encoding DefaultEncoding
    {
        get;
        set => field = value ?? throw new ArgumentNullException(nameof(value));
    } = Encoding.UTF8;

    /// <summary>
    /// Arrays and collections declaring up to this many items are read without checking the data left, so a forged
    /// count can't make the reader allocate more than this many items. Declaring more must fit in the data left,
    /// which only readers that know their length (not non-seekable streams) can tell, otherwise reading fails.
    /// Raise it to read bigger arrays from such streams. Default is 1,000,000.
    /// </summary>
    public static int MaxUnverifiedCount
    {
        get => PrimitiveReader.MaxUnverifiedCount;
        set => PrimitiveReader.MaxUnverifiedCount = value;
    }

    /// <summary>
    /// Same as <see cref="MaxUnverifiedCount"/> for the byte length of strings and byte arrays. Default is 16 MB.
    /// </summary>
    public static int MaxUnverifiedByteLength
    {
        get => PrimitiveReader.MaxUnverifiedByteLength;
        set => PrimitiveReader.MaxUnverifiedByteLength = value;
    }
}
