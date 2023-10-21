using Salar.BinaryBuffers;
using Salar.Bois.Serializers;
using System.Runtime.CompilerServices;

namespace Salar.Bois.Generator.Serializers;

public static class BoisNumericSerializers
{
	#region Readers

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static sbyte? ReadVarSByteNullable(BufferReaderBase reader)
	{
		return NumericSerializers.ReadVarSByteNullable(reader);
	}

	public static short? ReadVarInt16Nullable(BufferReaderBase reader)
	{
		return NumericSerializers.ReadVarInt16Nullable(reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static short ReadVarInt16(BufferReaderBase reader)
	{
		return NumericSerializers.ReadVarInt16(reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ushort? ReadVarUInt16Nullable(BufferReaderBase reader)
	{
		return NumericSerializers.ReadVarUInt16Nullable(reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ushort ReadVarUInt16(BufferReaderBase reader)
	{
		return NumericSerializers.ReadVarUInt16(reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int? ReadVarInt32Nullable(BufferReaderBase reader)
	{
		return NumericSerializers.ReadVarInt32Nullable(reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int ReadVarInt32(BufferReaderBase reader)
	{
		return NumericSerializers.ReadVarInt32(reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint? ReadVarUInt32Nullable(BufferReaderBase reader)
	{
		return NumericSerializers.ReadVarUInt32Nullable(reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint ReadVarUInt32(BufferReaderBase reader)
	{
		return NumericSerializers.ReadVarUInt32(reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static long? ReadVarInt64Nullable(BufferReaderBase reader)
	{
		return NumericSerializers.ReadVarInt64Nullable(reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static long ReadVarInt64(BufferReaderBase reader)
	{
		return NumericSerializers.ReadVarInt64(reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong? ReadVarUInt64Nullable(BufferReaderBase reader)
	{
		return NumericSerializers.ReadVarUInt64Nullable(reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong ReadVarUInt64(BufferReaderBase reader)
	{
		return NumericSerializers.ReadVarUInt64(reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static decimal? ReadVarDecimalNullable(BufferReaderBase reader)
	{
		return NumericSerializers.ReadVarDecimalNullable(reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static decimal ReadVarDecimal(BufferReaderBase reader)
	{
		return NumericSerializers.ReadVarDecimal(reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double? ReadVarDoubleNullable(BufferReaderBase reader)
	{
		return NumericSerializers.ReadVarDoubleNullable(reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double ReadVarDouble(BufferReaderBase reader)
	{
		return NumericSerializers.ReadVarDouble(reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float? ReadVarSingleNullable(BufferReaderBase reader)
	{
		return NumericSerializers.ReadVarSingleNullable(reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float ReadVarSingle(BufferReaderBase reader)
	{
		return NumericSerializers.ReadVarSingle(reader);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static byte? ReadVarByteNullable(BufferReaderBase reader)
	{
		return NumericSerializers.ReadVarByteNullable(reader);
	}

	#endregion

	#region Writers

	/// <summary>
	/// [int data as zigzag] not embeddable
	/// </summary>
	/// <param name="writer"></param>
	/// <param name="num"></param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarInt(BufferWriterBase writer, int num)
	{
		NumericSerializers.WriteVarInt(writer, num);
	}


	/// <summary>
	/// [EmbedIndicator-NullIndicator-0-0-0-0-0-0] [optional data]  0..63 can be embedded
	/// </summary>
	/// <param name="writer"></param>
	/// <param name="num"></param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarInt(BufferWriterBase writer, int? num)
	{
		NumericSerializers.WriteVarInt(writer, num);
	}

	/// <summary>
	/// [uint data as zigzag] not embeddable
	/// </summary>
	/// <param name="writer"></param>
	/// <param name="num"></param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarInt(BufferWriterBase writer, uint num)
	{
		NumericSerializers.WriteVarInt(writer, num);
	}

	/// <summary>
	/// [EmbedIndicator-NullIndicator-0-0-0-0-0-0] [optional data]  0..TODO can be embedded
	/// </summary>
	/// <param name="writer"></param>
	/// <param name="num"></param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarInt(BufferWriterBase writer, uint? num)
	{
		NumericSerializers.WriteVarInt(writer, num);
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
	public static void WriteUIntNullableMemberCount(BufferWriterBase writer, uint num)
	{
		NumericSerializers.WriteUIntNullableMemberCount(writer, num);
	}

	/// <summary>
	/// 
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarInt(BufferWriterBase writer, short num)
	{
		NumericSerializers.WriteVarInt(writer, num);
	}

	/// <summary>
	/// 
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarInt(BufferWriterBase writer, short? num)
	{
		NumericSerializers.WriteVarInt(writer, num);
	}

	/// <summary>
	/// 
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarInt(BufferWriterBase writer, ushort num)
	{
		NumericSerializers.WriteVarInt(writer, num);
	}

	/// <summary>
	/// 
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarInt(BufferWriterBase writer, ushort? num)
	{
		NumericSerializers.WriteVarInt(writer, num);
	}

	/// <summary>
	/// 
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarInt(BufferWriterBase writer, long num)
	{
		NumericSerializers.WriteVarInt(writer, num);
	}

	/// <summary>
	/// 
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarInt(BufferWriterBase writer, long? num)
	{
		NumericSerializers.WriteVarInt(writer, num);
	}

	/// <summary>
	/// 
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarInt(BufferWriterBase writer, ulong num)
	{
		NumericSerializers.WriteVarInt(writer, num);
	}

	/// <summary>
	/// 
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarInt(BufferWriterBase writer, ulong? num)
	{
		NumericSerializers.WriteVarInt(writer, num);
	}

	/// <summary>
	/// 
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarInt(BufferWriterBase writer, byte? num)
	{
		NumericSerializers.WriteVarInt(writer, num);
	}

	/// <summary>
	/// 
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarInt(BufferWriterBase writer, sbyte? num)
	{
		NumericSerializers.WriteVarInt(writer, num);
	}

	/// <summary>
	/// 
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarDecimal(BufferWriterBase writer, float num)
	{
		NumericSerializers.WriteVarDecimal(writer, num);
	}

	/// <summary>
	/// 
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarDecimal(BufferWriterBase writer, float? num)
	{
		NumericSerializers.WriteVarDecimal(writer, num);
	}

	/// <summary>
	/// 
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarDecimal(BufferWriterBase writer, double num)
	{
		NumericSerializers.WriteVarDecimal(writer, num);
	}

	/// <summary>
	/// 
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarDecimal(BufferWriterBase writer, double? num)
	{
		NumericSerializers.WriteVarDecimal(writer, num);
	}


	/// <summary>
	/// 
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarDecimal(BufferWriterBase writer, decimal num)
	{
		NumericSerializers.WriteVarDecimal(writer, num);
	}

	/// <summary>
	/// 
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteVarDecimal(BufferWriterBase writer, decimal? num)
	{
		NumericSerializers.WriteVarDecimal(writer, num);
	}
	#endregion

}
