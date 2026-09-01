#nullable enable
using Salar.BinaryBuffers;
using Salar.Bois.Serializers;
using System.Runtime.CompilerServices;

namespace Salar.Bois.CodeGen;

public static class BoisNumericSerializers
{
	#region Readers

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static sbyte? ReadVarSByteNullable<TReader>(ref TReader reader)
		where TReader : IBufferReader
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		return NumericSerializers.ReadVarSByteNullable(ref reader);
	}

	public static short? ReadVarInt16Nullable<TReader>(ref TReader reader)
		where TReader : IBufferReader
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		return NumericSerializers.ReadVarInt16Nullable(ref reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static short ReadVarInt16<TReader>(ref TReader reader)
		where TReader : IBufferReader
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		return NumericSerializers.ReadVarInt16(ref reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ushort? ReadVarUInt16Nullable<TReader>(ref TReader reader)
		where TReader : IBufferReader
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		return NumericSerializers.ReadVarUInt16Nullable(ref reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ushort ReadVarUInt16<TReader>(ref TReader reader)
		where TReader : IBufferReader
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		return NumericSerializers.ReadVarUInt16(ref reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int? ReadVarInt32Nullable<TReader>(ref TReader reader)
		where TReader : IBufferReader
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		return NumericSerializers.ReadVarInt32Nullable(ref reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int ReadVarInt32<TReader>(ref TReader reader)
		where TReader : IBufferReader
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		return NumericSerializers.ReadVarInt32(ref reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint? ReadVarUInt32Nullable<TReader>(ref TReader reader)
		where TReader : IBufferReader
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		return NumericSerializers.ReadVarUInt32Nullable(ref reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint ReadVarUInt32<TReader>(ref TReader reader)
		where TReader : IBufferReader
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		return NumericSerializers.ReadVarUInt32(ref reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static long? ReadVarInt64Nullable<TReader>(ref TReader reader)
		where TReader : IBufferReader
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		return NumericSerializers.ReadVarInt64Nullable(ref reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static long ReadVarInt64<TReader>(ref TReader reader)
		where TReader : IBufferReader
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		return NumericSerializers.ReadVarInt64(ref reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong? ReadVarUInt64Nullable<TReader>(ref TReader reader)
		where TReader : IBufferReader
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		return NumericSerializers.ReadVarUInt64Nullable(ref reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong ReadVarUInt64<TReader>(ref TReader reader)
		where TReader : IBufferReader
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		return NumericSerializers.ReadVarUInt64(ref reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static decimal? ReadVarDecimalNullable<TReader>(ref TReader reader)
		where TReader : IBufferReader
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		return NumericSerializers.ReadVarDecimalNullable(ref reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static decimal ReadVarDecimal<TReader>(ref TReader reader)
		where TReader : IBufferReader
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		return NumericSerializers.ReadVarDecimal(ref reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double? ReadVarDoubleNullable<TReader>(ref TReader reader)
		where TReader : IBufferReader
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		return NumericSerializers.ReadVarDoubleNullable(ref reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double ReadVarDouble<TReader>(ref TReader reader)
		where TReader : IBufferReader
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		return NumericSerializers.ReadVarDouble(ref reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float? ReadVarSingleNullable<TReader>(ref TReader reader)
		where TReader : IBufferReader
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		return NumericSerializers.ReadVarSingleNullable(ref reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float ReadVarSingle<TReader>(ref TReader reader)
		where TReader : IBufferReader
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		return NumericSerializers.ReadVarSingle(ref reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static byte? ReadVarByteNullable<TReader>(ref TReader reader)
		where TReader : IBufferReader
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		return NumericSerializers.ReadVarByteNullable(ref reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static byte ReadByte<TReader>(ref TReader reader)
		where TReader : IBufferReader
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		return reader.ReadByte();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static sbyte ReadSByte<TReader>(ref TReader reader)
		where TReader : IBufferReader
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		return reader.ReadSByte();
	}

	#endregion

	#region Writers

	/// <summary>
	/// [int data as zigzag] not embeddable
	/// </summary>
	/// <param name="writer"></param>
	/// <param name="num"></param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarInt<TWriter>(ref TWriter writer, int num)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		NumericSerializers.WriteVarInt(ref writer, num);
	}


	/// <summary>
	/// [EmbedIndicator-NullIndicator-0-0-0-0-0-0] [optional data]  0..63 can be embedded
	/// </summary>
	/// <param name="writer"></param>
	/// <param name="num"></param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarInt<TWriter>(ref TWriter writer, int? num)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		NumericSerializers.WriteVarInt(ref writer, num);
	}

	/// <summary>
	/// [uint data as zigzag] not embeddable
	/// </summary>
	/// <param name="writer"></param>
	/// <param name="num"></param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarInt<TWriter>(ref TWriter writer, uint num)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		NumericSerializers.WriteVarInt(ref writer, num);
	}

	/// <summary>
	/// [EmbedIndicator-NullIndicator-0-0-0-0-0-0] [optional data]  0..TODO can be embedded
	/// </summary>
	/// <param name="writer"></param>
	/// <param name="num"></param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarInt<TWriter>(ref TWriter writer, uint? num)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		NumericSerializers.WriteVarInt(ref writer, num);
	}

	/// <summary>
	/// Same as "uint?" except that it doesn't store null value, but still preserves null flag.
	/// To be used to store member counts
	/// </summary>
	/// <remarks>
	/// The value stored as member count can be 'null', but since this method is called where it is obvious that 
	/// the don't have null value, there is no point creating Nullable object to convert it
	/// </remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteUIntNullableMemberCount<TWriter>(ref TWriter writer, uint num)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		NumericSerializers.WriteUIntNullableMemberCount(ref writer, num);
	}

	/// <summary>
	/// 
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarInt<TWriter>(ref TWriter writer, short num)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		NumericSerializers.WriteVarInt(ref writer, num);
	}

	/// <summary>
	/// 
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarInt<TWriter>(ref TWriter writer, short? num)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		NumericSerializers.WriteVarInt(ref writer, num);
	}

	/// <summary>
	/// 
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarInt<TWriter>(ref TWriter writer, ushort num)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		NumericSerializers.WriteVarInt(ref writer, num);
	}

	/// <summary>
	/// 
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarInt<TWriter>(ref TWriter writer, ushort? num)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		NumericSerializers.WriteVarInt(ref writer, num);
	}

	/// <summary>
	/// 
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarInt<TWriter>(ref TWriter writer, long num)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		NumericSerializers.WriteVarInt(ref writer, num);
	}

	/// <summary>
	/// 
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarInt<TWriter>(ref TWriter writer, long? num)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		NumericSerializers.WriteVarInt(ref writer, num);
	}

	/// <summary>
	/// 
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarInt<TWriter>(ref TWriter writer, ulong num)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		NumericSerializers.WriteVarInt(ref writer, num);
	}

	/// <summary>
	/// 
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarInt<TWriter>(ref TWriter writer, ulong? num)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		NumericSerializers.WriteVarInt(ref writer, num);
	}

	/// <summary>
	/// 
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarInt<TWriter>(ref TWriter writer, byte? num)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		NumericSerializers.WriteVarInt(ref writer, num);
	}

	/// <summary>
	/// 
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarInt<TWriter>(ref TWriter writer, sbyte? num)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		NumericSerializers.WriteVarInt(ref writer, num);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteByte<TWriter>(ref TWriter writer, byte num)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		writer.Write(num);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteSByte<TWriter>(ref TWriter writer, sbyte num)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		writer.Write(num);
	}

	/// <summary>
	/// 
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarDecimal<TWriter>(ref TWriter writer, float num)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		NumericSerializers.WriteVarDecimal(ref writer, num);
	}

	/// <summary>
	/// 
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarDecimal<TWriter>(ref TWriter writer, float? num)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		NumericSerializers.WriteVarDecimal(ref writer, num);
	}

	/// <summary>
	/// 
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarDecimal<TWriter>(ref TWriter writer, double num)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		NumericSerializers.WriteVarDecimal(ref writer, num);
	}

	/// <summary>
	/// 
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarDecimal<TWriter>(ref TWriter writer, double? num)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		NumericSerializers.WriteVarDecimal(ref writer, num);
	}


	/// <summary>
	/// 
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarDecimal<TWriter>(ref TWriter writer, decimal num)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		NumericSerializers.WriteVarDecimal(ref writer, num);
	}

	/// <summary>
	/// 
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarDecimal<TWriter>(ref TWriter writer, decimal? num)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
	{
		NumericSerializers.WriteVarDecimal(ref writer, num);
	}
	#endregion

}
