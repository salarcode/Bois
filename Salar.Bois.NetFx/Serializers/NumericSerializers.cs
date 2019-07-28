using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Salar.Bois.Serializers
{
	internal static class NumericSerializers
	{
		/// <summary>
		/// 0000 0000 = 0 -- No flags enabled
		/// </summary>
		internal const byte FlagNone = 0b0_0_0_0_0_0_0_0;

		/// <summary>
		/// 0100 0000 = 64
		/// </summary>
		internal const byte FlagIsNull = 0b0_1_0_0_0_0_0_0;

		/// <summary>
		/// 1000 0000 = 128
		/// </summary>
		private const byte FlagEmbedded = 0b1_0_0_0_0_0_0_0;

		/// <summary>
		/// 0111 1111 = 127
		/// </summary>
		private const byte EmbeddedMaxNumInByte = 0b0_1_1_1_1_1_1_1;

		/// <summary>
		/// 0011 1111 = 63
		/// </summary>
		private const byte EmbeddedNullableMaxNumInByte = 0b0_0_1_1_1_1_1_1;

		/// <summary>
		/// 0111 1111 = 127 
		/// </summary>
		private const byte MaskEmbedded = 0b0_1_1_1_1_1_1_1;

		/// <summary>
		/// 0011 1111 = 63 
		/// </summary>
		private const byte MaskEmbeddedNullable = 0b0_0_1_1_1_1_1_1;

		///// <summary>
		///// 0011 1111 = 63
		///// </summary>
		//private const byte EmbeddedSignedMaxNumInByte = 0b0_0_1_1_1_1_1_1;// 63;

		///// <summary>
		///// 0100 0000 = 64
		///// </summary>
		//private const byte FlagNonullNegativeNum = 0b0_1_0_0_0_0_0_0;

		///// <summary>
		///// 191
		///// </summary>
		//private const byte FlagNonullNegativeMask = 0b1_0_1_1_1_1_1_1;

		///// <summary>
		///// 192
		///// </summary>
		//private const byte FlagNonullNegativeNumEmbedded = 0b1_1_0_0_0_0_0_0;

		///// <summary>
		///// 63
		///// </summary>
		//private const byte FlagNonullNegativeNumEmbeddedMask = 0b0_0_1_1_1_1_1_1;

		///// <summary>
		///// 32
		///// </summary>
		//private const byte FlagNullableNegativeNum = 0b0_0_1_0_0_0_0_0;

		///// <summary>
		///// 223
		///// </summary>
		//private const byte FlagNullableNegativeMask = 0b1_1_0_1_1_1_1_1;

		///// <summary>
		///// 160
		///// </summary>
		//private const byte FlagNullableNegativeEmbeddedNum = 0b1_0_1_0_0_0_0_0;

		///// <summary>
		///// 95
		///// </summary>
		//private const byte FlagNullableNegativeEmbeddedMask = 0b0_1_0_1_1_1_1_1;

		///// <summary>
		///// 127
		///// </summary>
		//private const byte EmbeddedUnsignedMaxNumInByte = 0b0_1_1_1_1_1_1_1;

		/////// <summary>
		/////// 31
		/////// </summary>
		////private const byte EmbeddedSignedNullableMaxNumInByte = 0b0_0_0_1_1_1_1_1;

		///// <summary>
		///// 63
		///// </summary>
		//private const byte EmbeddedUnsignedNullableMaxNumInByte = 0b0_0_1_1_1_1_1_1;

		#region Array Pool

		private static class SharedArray
		{
			[ThreadStatic] private static byte[] _array;

			public static byte[] Get()
			{
				return _array ?? (_array = new byte[16]);
			}

			public static void ClearArray4()
			{
				_array[0] = 0;
				_array[1] = 0;
				_array[2] = 0;
				_array[3] = 0;
			}
			public static void ClearArray8()
			{
				_array[0] = 0;
				_array[1] = 0;
				_array[2] = 0;
				_array[3] = 0;
				_array[4] = 0;
				_array[5] = 0;
				_array[6] = 0;
				_array[7] = 0;
			}
		}

		#endregion

		#region Readers

		internal static sbyte? ReadVarSByteNullable(BinaryReader reader)
		{
			var input = reader.ReadByte();
			if (input == FlagIsNull)
				return null;

			if ((input & FlagEmbedded) == FlagEmbedded)
			{
				// number is embedded
				return (sbyte)(input & MaskEmbeddedNullable);
			}
			else
			{
				return reader.ReadSByte();
			}
		}

		internal static short? ReadVarInt16Nullable(BinaryReader reader)
		{
			var input = reader.ReadByte();
			if (input == FlagIsNull)
				return null;

			if ((input & FlagEmbedded) == FlagEmbedded)
			{
				// number is embedded
				return (short)(input & MaskEmbeddedNullable);
			}
			else
			{
				return ReadInt16Zigzag(reader);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static short ReadVarInt16(BinaryReader reader)
		{
			return ReadInt16Zigzag(reader);
		}

		internal static ushort? ReadVarUInt16Nullable(BinaryReader reader)
		{
			var input = reader.ReadByte();
			if (input == FlagIsNull)
				return null;

			if ((input & FlagEmbedded) == FlagEmbedded)
			{
				// number is embedded
				return (ushort)(input & MaskEmbeddedNullable);
			}
			else
			{
				return ReadUInt16Zigzag(reader);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static ushort ReadVarUInt16(BinaryReader reader)
		{
			return ReadUInt16Zigzag(reader);
		}

		internal static int? ReadVarInt32Nullable(BinaryReader reader)
		{
			var input = reader.ReadByte();
			if (input == FlagIsNull)
				return null;

			if ((input & FlagEmbedded) == FlagEmbedded)
			{
				// number is embedded
				return input & MaskEmbeddedNullable;
			}
			else
			{
				return ReadInt32Zigzag(reader);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static int ReadVarInt32(BinaryReader reader)
		{
			return ReadInt32Zigzag(reader);
		}

		internal static uint? ReadVarUInt32Nullable(BinaryReader reader)
		{
			var input = reader.ReadByte();
			if (input == FlagIsNull)
				return null;

			if ((input & FlagEmbedded) == FlagEmbedded)
			{
				// number is embedded
				return (uint)(input & MaskEmbeddedNullable);
			}
			else
			{
				return ReadUInt32Zigzag(reader);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static uint ReadVarUInt32(BinaryReader reader)
		{
			return ReadUInt32Zigzag(reader);
		}

		internal static long? ReadVarInt64Nullable(BinaryReader reader)
		{
			var input = reader.ReadByte();
			if (input == FlagIsNull)
				return null;

			if ((input & FlagEmbedded) == FlagEmbedded)
			{
				// number is embedded
				return input & MaskEmbeddedNullable;
			}
			else
			{
				return ReadInt64Zigzag(reader);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static long ReadVarInt64(BinaryReader reader)
		{
			return ReadInt64Zigzag(reader);
		}

		internal static ulong? ReadVarUInt64Nullable(BinaryReader reader)
		{
			var input = reader.ReadByte();
			if (input == FlagIsNull)
				return null;

			if ((input & FlagEmbedded) == FlagEmbedded)
			{
				// number is embedded
				return (ulong)(input & MaskEmbeddedNullable);
			}
			else
			{
				return ReadUInt64Zigzag(reader);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static ulong ReadVarUInt64(BinaryReader reader)
		{
			return ReadUInt64Zigzag(reader);
		}

		internal static decimal? ReadVarDecimalNullable(BinaryReader reader)
		{
			var input = reader.ReadByte();
			if (input == FlagIsNull)
				return null;

			var embedded = (input & FlagEmbedded) == FlagEmbedded;
			if (embedded)
			{
				return (input & MaskEmbeddedNullable);
			}

			int length = input;

			var numBuff = reader.ReadBytes(length);

			return ConvertFromVarBinaryDecimal(numBuff);
		}

		internal static decimal ReadVarDecimal(BinaryReader reader)
		{
			var input = reader.ReadByte();

			var embedded = (input & FlagEmbedded) == FlagEmbedded;
			if (embedded)
			{
				return (input & MaskEmbedded);
			}

			int length = input;

			var numBuff = reader.ReadBytes(length);

			return ConvertFromVarBinaryDecimal(numBuff);
		}

		internal static double? ReadVarDoubleNullable(BinaryReader reader)
		{
			var input = reader.ReadByte();
			if (input == FlagIsNull)
				return null;

			var buff = SharedArray.Get();

			var embedded = (input & FlagEmbedded) == FlagEmbedded;
			if (embedded)
			{
				SharedArray.ClearArray8();

				// last byte
				buff[7] = (byte)(input & MaskEmbeddedNullable);
				return BitConverter.ToDouble(buff, 0);
			}

			int length = input;

			if (length < 8)
				SharedArray.ClearArray8();

			reader.Read(buff, 8 - length, length);

			return BitConverter.ToDouble(buff, 0);
		}

		internal static double ReadVarDouble(BinaryReader reader)
		{
			var input = reader.ReadByte();

			var buff = SharedArray.Get();

			var embedded = (input & FlagEmbedded) == FlagEmbedded;
			if (embedded)
			{
				SharedArray.ClearArray8();

				// last byte
				buff[7] = (byte)(input & MaskEmbedded);
				return BitConverter.ToDouble(buff, 0);
			}

			int length = input;

			if (length < 8)
				SharedArray.ClearArray8();

			reader.Read(buff, 8 - length, length);

			return BitConverter.ToDouble(buff, 0);
		}

		internal static float? ReadVarSingleNullable(BinaryReader reader)
		{
			var input = reader.ReadByte();
			if (input == FlagIsNull)
				return null;

			var buff = SharedArray.Get();

			var embedded = (input & FlagEmbedded) == FlagEmbedded;
			if (embedded)
			{
				SharedArray.ClearArray4();

				// last byte
				buff[3] = (byte)(input & MaskEmbeddedNullable);
				return BitConverter.ToSingle(buff, 0);
			}

			int length = input;

			if (length < 4)
				SharedArray.ClearArray4();

			reader.Read(buff, 4 - length, length);

			return BitConverter.ToSingle(buff, 0);
		}

		internal static float ReadVarSingle(BinaryReader reader)
		{
			var input = reader.ReadByte();

			var buff = SharedArray.Get();

			var embedded = (input & FlagEmbedded) == FlagEmbedded;
			if (embedded)
			{
				SharedArray.ClearArray4();

				// last byte
				buff[3] = (byte)(input & MaskEmbedded);
				return BitConverter.ToSingle(buff, 0);
			}

			int length = input;

			if (length < 4)
				SharedArray.ClearArray4();

			reader.Read(buff, 4 - length, length);

			return BitConverter.ToSingle(buff, 0);
		}

		internal static byte? ReadVarByteNullable(BinaryReader reader)
		{
			var input = reader.ReadByte();
			if (input == FlagIsNull)
				return null;

			var embedded = (input & FlagEmbedded) == FlagEmbedded;
			if (embedded)
			{
				return (byte)(input & MaskEmbeddedNullable);
			}

			return reader.ReadByte();
		}

		#endregion

		#region Writers

		/// <summary>
		/// [int data as zigzag] not embeddable
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="num"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteVarInt(BinaryWriter writer, int num)
		{
			WriteZigzag(writer, num);
		}


		/// <summary>
		/// [EmbedIndicator-NullIndicator-0-0-0-0-0-0] [optional data]  0..63 can be embedded
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="num"></param>
		internal static void WriteVarInt(BinaryWriter writer, int? num)
		{
			if (num == null)
			{
				// null flag
				writer.Write(FlagIsNull);
				return;
			}

			if (num > EmbeddedNullableMaxNumInByte || num < 0)
			{
				// number is not embeddable 

				writer.Write(FlagNone);
				WriteZigzag(writer, num.Value);
			}
			else
			{
				byte numByte = (byte)num;

				// set the flag of inside
				numByte = (byte)(numByte | FlagEmbedded);
				writer.Write(numByte);
			}
		}

		/// <summary>
		/// [uint data as zigzag] not embeddable
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="num"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteVarInt(BinaryWriter writer, uint num)
		{
			WriteZigzag(writer, num);
		}

		/// <summary>
		/// [EmbedIndicator-NullIndicator-0-0-0-0-0-0] [optional data]  0..TODO can be embedded
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="num"></param>
		internal static void WriteVarInt(BinaryWriter writer, uint? num)
		{
			if (num == null)
			{
				// null flag
				writer.Write(FlagIsNull);
				return;
			}

			if (num > EmbeddedNullableMaxNumInByte)
			{
				// number is not embeddable 

				writer.Write(FlagNone);
				WriteZigzag(writer, num.Value);
			}
			else
			{
				byte numByte = (byte)num;

				// set the flag of inside
				numByte = (byte)(numByte | FlagEmbedded);
				writer.Write(numByte);
			}
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
		internal static void WriteUIntNullableMemberCount(BinaryWriter writer, uint num)
		{
			// NOTE:
			// Member count can be null, but not the place this method is being called
			// Hence why i'm storing null flag

			if (num > EmbeddedNullableMaxNumInByte)
			{
				// number is not embeddable 

				writer.Write(FlagNone);
				WriteZigzag(writer, num);
			}
			else
			{
				byte numByte = (byte)num;

				// set the flag of inside
				numByte = (byte)(numByte | FlagEmbedded);
				writer.Write(numByte);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteVarInt(BinaryWriter writer, short num)
		{
			WriteZigzag(writer, num);
		}

		/// <summary>
		/// 
		/// </summary>
		internal static void WriteVarInt(BinaryWriter writer, short? num)
		{
			if (num == null)
			{
				// null flag
				writer.Write(FlagIsNull);
				return;
			}

			if (num > EmbeddedNullableMaxNumInByte || num < 0)
			{
				// number is not embeddable 

				writer.Write(FlagNone);
				WriteZigzag(writer, num.Value);
			}
			else
			{
				byte numByte = (byte)num;

				// set the flag of inside
				numByte = (byte)(numByte | FlagEmbedded);
				writer.Write(numByte);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteVarInt(BinaryWriter writer, ushort num)
		{
			WriteZigzag(writer, num);
		}

		/// <summary>
		/// 
		/// </summary>
		internal static void WriteVarInt(BinaryWriter writer, ushort? num)
		{
			if (num == null)
			{
				// null flag
				writer.Write(FlagIsNull);
				return;
			}

			if (num > EmbeddedNullableMaxNumInByte || num < 0)
			{
				// number is not embeddable 

				writer.Write(FlagNone);
				WriteZigzag(writer, num.Value);
			}
			else
			{
				byte numByte = (byte)num;

				// set the flag of inside
				numByte = (byte)(numByte | FlagEmbedded);
				writer.Write(numByte);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteVarInt(BinaryWriter writer, long num)
		{
			WriteZigzag(writer, num);
		}

		/// <summary>
		/// 
		/// </summary>
		internal static void WriteVarInt(BinaryWriter writer, long? num)
		{
			if (num == null)
			{
				// null flag
				writer.Write(FlagIsNull);
				return;
			}

			if (num > EmbeddedNullableMaxNumInByte || num < 0)
			{
				// number is not embeddable 

				writer.Write(FlagNone);
				WriteZigzag(writer, num.Value);
			}
			else
			{
				byte numByte = (byte)num;

				// set the flag of inside
				numByte = (byte)(numByte | FlagEmbedded);
				writer.Write(numByte);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteVarInt(BinaryWriter writer, ulong num)
		{
			WriteZigzag(writer, num);
		}

		/// <summary>
		/// 
		/// </summary>
		internal static void WriteVarInt(BinaryWriter writer, ulong? num)
		{
			if (num == null)
			{
				// null flag
				writer.Write(FlagIsNull);
				return;
			}

			if (num > EmbeddedNullableMaxNumInByte || num < 0)
			{
				// number is not embeddable 

				writer.Write(FlagNone);
				WriteZigzag(writer, num.Value);
			}
			else
			{
				byte numByte = (byte)num;

				// set the flag of inside
				numByte = (byte)(numByte | FlagEmbedded);
				writer.Write(numByte);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		internal static void WriteVarInt(BinaryWriter writer, byte? num)
		{
			if (num == null)
			{
				// null flag
				writer.Write(FlagIsNull);
				return;
			}

			if (num > EmbeddedNullableMaxNumInByte || num < 0)
			{
				// number is not embeddable 

				writer.Write(FlagNone);
				writer.Write(num.Value);
			}
			else
			{
				byte numByte = (byte)num;

				// set the flag of inside
				numByte = (byte)(numByte | FlagEmbedded);
				writer.Write(numByte);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		internal static void WriteVarInt(BinaryWriter writer, sbyte? num)
		{
			if (num == null)
			{
				// null flag
				writer.Write(FlagIsNull);
				return;
			}

			if (num > EmbeddedNullableMaxNumInByte || num < 0)
			{
				// number is not embeddable 

				writer.Write(FlagNone);
				writer.Write(num.Value);
			}
			else
			{
				byte numByte = (byte)num;

				// set the flag of inside
				numByte = (byte)(numByte | FlagEmbedded);
				writer.Write(numByte);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		internal static void WriteVarDecimal(BinaryWriter writer, float num)
		{
			var numBuff = NumericSerializers.ConvertToVarBinary(num, out var numLen, out var position);
			var firstByte = numBuff[position];

			if (numLen == 1 && firstByte <= EmbeddedMaxNumInByte)
			{
				// number is embedded

				writer.Write((byte)(firstByte | FlagEmbedded));
			}
			else
			{
				// number is not embedded
				writer.Write(numLen);
				writer.Write(numBuff, position, numLen);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		internal static void WriteVarDecimal(BinaryWriter writer, float? num)
		{
			if (num == null)
			{
				// number is null

				writer.Write(FlagIsNull);
				return;
			}

			var numBuff = NumericSerializers.ConvertToVarBinary(num.Value, out var numLen, out var position);
			var firstByte = numBuff[position];

			if (numLen == 1 && firstByte <= EmbeddedNullableMaxNumInByte)
			{
				// number is embedded

				writer.Write((byte)(firstByte | FlagEmbedded));
			}
			else
			{
				// number is not embedded
				writer.Write(numLen);
				writer.Write(numBuff, position, numLen);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		internal static void WriteVarDecimal(BinaryWriter writer, double num)
		{
			var numBuff = NumericSerializers.ConvertToVarBinary(num, out var numLen, out var position);
			var firstByte = numBuff[position];

			if (numLen == 1 && firstByte <= EmbeddedMaxNumInByte)
			{
				// number is embedded

				writer.Write((byte)(firstByte | FlagEmbedded));
			}
			else
			{
				// number is not embedded
				writer.Write(numLen);
				writer.Write(numBuff, position, numLen);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		internal static void WriteVarDecimal(BinaryWriter writer, double? num)
		{
			if (num == null)
			{
				// number is null

				writer.Write(FlagIsNull);
				return;
			}

			var numBuff = NumericSerializers.ConvertToVarBinary(num.Value, out var numLen, out var position);
			var firstByte = numBuff[position];

			if (numLen == 1 && firstByte <= EmbeddedNullableMaxNumInByte)
			{
				// number is embedded

				writer.Write((byte)(firstByte | FlagEmbedded));
			}
			else
			{
				// number is not embedded
				writer.Write(numLen);
				writer.Write(numBuff, position, numLen);
			}
		}


		/// <summary>
		/// 
		/// </summary>
		internal static void WriteVarDecimal(BinaryWriter writer, decimal num)
		{
			var numBuff = NumericSerializers.ConvertToVarBinary(num, out var numLen);
			var firstByte = numBuff[0];

			if (numLen == 1 && firstByte <= EmbeddedMaxNumInByte)
			{
				// number is embedded

				writer.Write((byte)(firstByte | FlagEmbedded));
			}
			else
			{
				// number is not embedded
				writer.Write(numLen);
				writer.Write(numBuff, 0, numLen);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		internal static void WriteVarDecimal(BinaryWriter writer, decimal? num)
		{
			if (num == null)
			{
				// number is null

				writer.Write(FlagIsNull);
				return;
			}

			var numBuff = NumericSerializers.ConvertToVarBinary(num.Value, out var numLen);
			var firstByte = numBuff[0];

			if (numLen == 1 && firstByte <= EmbeddedNullableMaxNumInByte)
			{
				// number is embedded

				writer.Write((byte)(firstByte | FlagEmbedded));
			}
			else
			{
				// number is not embedded
				writer.Write(numLen);
				writer.Write(numBuff, 0, numLen);
			}
		}
		#endregion

		#region Binary Converters & Writers

		private static short ReadInt16Zigzag(BinaryReader reader)
		{
			var currentByte = (uint)reader.ReadByte();
			byte read = 1;
			uint result = currentByte & 0x7FU;
			int shift = 7;
			while ((currentByte & 0x80) != 0)
			{
				currentByte = (uint)reader.ReadByte();
				read++;
				result |= (currentByte & 0x7FU) << shift;
				shift += 7;
				if (read > 5)
				{
					throw new Exception("Invalid integer value in the input stream.");
				}
			}
			return (short)((-(result & 1)) ^ ((result >> 1) & (ushort)0x7FFFU));
		}

		private static ushort ReadUInt16Zigzag(BinaryReader reader)
		{
			var currentByte = (ushort)reader.ReadByte();
			byte read = 1;
			ushort result = (ushort)(currentByte & 0x7FU);
			int shift = 7;
			while ((currentByte & 0x80) != 0)
			{
				currentByte = (ushort)reader.ReadByte();
				read++;
				result |= (ushort)((currentByte & 0x7FU) << shift);
				shift += 7;
				if (read > 5)
				{
					throw new Exception("Invalid integer value in the input stream.");
				}
			}
			return result;
		}

		private static int ReadInt32Zigzag(BinaryReader reader)
		{
			var currentByte = (uint)reader.ReadByte();
			byte read = 1;
			uint result = currentByte & 0x7FU;
			int shift = 7;
			while ((currentByte & 0x80) != 0)
			{
				currentByte = (uint)reader.ReadByte();
				read++;
				result |= (currentByte & 0x7FU) << shift;
				shift += 7;
				if (read > 5)
				{
					throw new Exception("Invalid integer value in the input stream.");
				}
			}
			return (int)((-(result & 1)) ^ ((result >> 1) & 0x7FFFFFFFU));
		}

		private static uint ReadUInt32Zigzag(BinaryReader reader)
		{
			var currentByte = (uint)reader.ReadByte();
			byte read = 1;
			uint result = currentByte & 0x7FU;
			int shift = 7;
			while ((currentByte & 0x80) != 0)
			{
				currentByte = (uint)reader.ReadByte();
				read++;
				result |= (currentByte & 0x7FU) << shift;
				shift += 7;
				if (read > 5)
				{
					throw new Exception("Invalid integer value in the input stream.");
				}
			}
			return result;
		}

		private static long ReadInt64Zigzag(BinaryReader reader)
		{
			var value = (uint)reader.ReadByte();
			byte read = 1;
			ulong result = value & 0x7FUL;
			int shift = 7;
			while ((value & 0x80) != 0)
			{
				value = (uint)reader.ReadByte();
				read++;
				result |= (value & 0x7FUL) << shift;
				shift += 7;
				if (read > 10)
				{
					throw new Exception("Invalid integer long in the input stream.");
				}
			}
			var tmp = unchecked((long)result);
			return (-(tmp & 0x1L)) ^ ((tmp >> 1) & 0x7FFFFFFFFFFFFFFFL);
		}

		private static ulong ReadUInt64Zigzag(BinaryReader reader)
		{
			var value = (uint)reader.ReadByte();
			byte read = 1;
			ulong result = value & 0x7FUL;
			int shift = 7;
			while ((value & 0x80) != 0)
			{
				value = (uint)reader.ReadByte();
				read++;
				result |= (value & 0x7FUL) << shift;
				shift += 7;
				if (read > 10)
				{
					throw new Exception("Invalid integer long in the input stream.");
				}
			}
			return result;
		}


		private static void WriteZigzag(BinaryWriter writer, long num)
		{
			var zigZagEncoded = unchecked((ulong)((num << 1) ^ (num >> 63)));
			while ((zigZagEncoded & ~0x7FUL) != 0)
			{
				writer.Write((byte)((zigZagEncoded | 0x80) & 0xFF));
				zigZagEncoded >>= 7;
			}
			writer.Write((byte)zigZagEncoded);
		}

		private static void WriteZigzag(BinaryWriter writer, ulong num)
		{
			while ((num & ~0x7FUL) != 0)
			{
				writer.Write((byte)((num | 0x80) & 0xFF));
				num >>= 7;
			}
			writer.Write((byte)num);
		}

		private static void WriteZigzag(BinaryWriter writer, int num)
		{
			var zigZagEncoded = unchecked((uint)((num << 1) ^ (num >> 31)));
			while ((zigZagEncoded & ~0x7F) != 0)
			{
				writer.Write((byte)((zigZagEncoded | 0x80) & 0xFF));
				zigZagEncoded >>= 7;
			}
			writer.Write((byte)zigZagEncoded);
		}

		private static void WriteZigzag(BinaryWriter writer, uint num)
		{
			while ((num & ~0x7F) != 0)
			{
				writer.Write((byte)((num | 0x80) & 0xFF));
				num >>= 7;
			}
			writer.Write((byte)num);
		}

		private static void WriteZigzag(BinaryWriter writer, short num)
		{
			var zigZagEncoded = unchecked((ushort)((num << 1) ^ (num >> 15)));
			while ((zigZagEncoded & ~0x7F) != 0)
			{
				writer.Write((byte)((zigZagEncoded | 0x80) & 0xFF));
				zigZagEncoded >>= 7;
			}
			writer.Write((byte)zigZagEncoded);
		}

		private static void WriteZigzag(BinaryWriter writer, ushort num)
		{
			while ((num & ~0x7F) != 0)
			{
				writer.Write((byte)((num | 0x80) & 0xFF));
				num >>= 7;
			}
			writer.Write((byte)num);
		}

		#endregion

		#region Binary Converters

		private static short ConvertFromVarBinaryInt16(byte[] numBuff)
		{
			short result = numBuff[0];

			if (numBuff.Length == 1)
				return result;

			result = unchecked((short)((ushort)result | (numBuff[1] << 8)));
			return result;
		}

		private static ushort ConvertFromVarBinaryUInt16(byte[] numBuff)
		{
			ushort result;
			if (numBuff.Length == 4)
			{
				result = numBuff[0];
				result = unchecked((ushort)(short)(result | (numBuff[1] << 8)));
			}
			else
			{
				var len = numBuff.Length;

				result = numBuff[0];
				if (len == 1)
					return result;

				result = unchecked((ushort)(short)(result | (numBuff[1] << 8)));
				if (len == 2)
					return result;

				result = unchecked((ushort)(short)(result | (numBuff[2] << 16)));
				if (len == 3)
					return result;

				result = unchecked((ushort)(short)(result | (numBuff[3] << 24)));
			}
			return result;
		}

		private static int ConvertFromVarBinaryInt32(byte[] numBuff, int len)
		{
			int result;
			if (len == 4)
			{
				result = numBuff[0];
				result = unchecked(result | (numBuff[1] << 8));
				result = unchecked(result | (numBuff[2] << 16));
				result = unchecked(result | (numBuff[3] << 24));
			}
			else
			{
				result = numBuff[0];
				if (len == 1)
					return result;

				result = unchecked(result | (numBuff[1] << 8));
				if (len == 2)
					return result;

				result = unchecked(result | (numBuff[2] << 16));
				if (len == 3)
					return result;

				result = unchecked(result | (numBuff[3] << 24));
			}
			return result;
		}

		private static int ConvertFromVarBinaryInt32StartIndex(byte[] numBuff, int startIndex)
		{
			return ((numBuff[startIndex + 0] | (numBuff[startIndex + 1] << 8)) | (numBuff[startIndex + 2] << 16)) | (numBuff[startIndex + 3] << 24);
		}

		private static uint ConvertFromVarBinaryUInt32(byte[] numBuff, int len)
		{
			uint result;
			if (len == 4)
			{
				result = numBuff[0];
				result = unchecked((uint)((int)result | (numBuff[1] << 8)));
				result = unchecked((uint)((int)result | (numBuff[2] << 16)));
				result = unchecked((uint)((int)result | (numBuff[3] << 24)));
			}
			else
			{
				result = numBuff[0];
				if (len == 1)
					return result;

				result = unchecked((uint)((int)result | (numBuff[1] << 8)));
				if (len == 2)
					return result;

				result = unchecked((uint)((int)result | (numBuff[2] << 16)));
				if (len == 3)
					return result;

				result = unchecked((uint)((int)result | (numBuff[3] << 24)));
			}
			return result;
		}

		private static long ConvertFromVarBinaryInt64(byte[] numBuff)
		{
			uint num = unchecked((uint)(((numBuff[0] | (numBuff[1] << 8)) | (numBuff[2] << 16)) | (numBuff[3] << 24)));
			uint num2 = unchecked((uint)(((numBuff[4] | (numBuff[5] << 8)) | (numBuff[6] << 16)) | (numBuff[7] << 24)));
			return unchecked((long)((((ulong)num2) << 32) | ((ulong)num)));
		}

		private static ulong ConvertFromVarBinaryUInt64(byte[] numBuff)
		{
			uint num = unchecked((uint)(((numBuff[0] | (numBuff[1] << 8)) | (numBuff[2] << 16)) | (numBuff[3] << 24)));
			uint num2 = unchecked((uint)(((numBuff[4] | (numBuff[5] << 8)) | (numBuff[6] << 16)) | (numBuff[7] << 24)));
			return unchecked(((ulong)num2 << 32) | (ulong)num);

		}

		private static decimal ConvertFromVarBinaryDecimal(byte[] numBuff)
		{
			byte[] buff;
			if (numBuff.Length < 16)
			{
				// TODO: check why SharedArray.Get() decreases performance by 3x times
				buff = new byte[16]; // 16 required
				Array.Copy(numBuff, 0, buff, 0, numBuff.Length);
			}
			else
			{
				buff = numBuff;
			}

			var decimalBits = new int[4];
			decimalBits[0] = ConvertFromVarBinaryInt32StartIndex(buff, 4 * 0);
			decimalBits[1] = ConvertFromVarBinaryInt32StartIndex(buff, 4 * 1);
			decimalBits[2] = ConvertFromVarBinaryInt32StartIndex(buff, 4 * 2);
			decimalBits[3] = ConvertFromVarBinaryInt32StartIndex(buff, 4 * 3);

			return new decimal(decimalBits);
		}

		private static double ConvertFromVarBinaryDouble(byte[] numBuff)
		{
			if (numBuff.Length == 8)
			{
				return BitConverter.ToDouble(numBuff, 0);
			}
			else
			{
				var doubleBuff = new byte[8];

				Array.Copy(numBuff, 0, doubleBuff, 8 - numBuff.Length, numBuff.Length);
				return BitConverter.ToDouble(doubleBuff, 0);
			}
		}

		private static float ConvertFromVarBinarySingle(byte[] numBuff)
		{
			if (numBuff.Length == 4)
			{
				return BitConverter.ToSingle(numBuff, 0);
			}
			else
			{
				var doubleBuff = new byte[4];

				Array.Copy(numBuff, 0, doubleBuff, 4 - numBuff.Length, numBuff.Length);
				return BitConverter.ToSingle(doubleBuff, 0);
			}
		}

		/// <summary>
		/// TODO: is always the result 4 bytes?
		/// </summary>
		private static byte[] ConvertToVarBinaryZigzag(int value, out byte length)
		{
			var buff = SharedArray.Get();
			length = 0;

			var zigZagEncoded = unchecked((uint)((value << 1) ^ (value >> 31)));

			while ((zigZagEncoded & ~0x7F) != 0)
			{
				buff[length++] = (byte)((zigZagEncoded | 0x80) & 0xFF);
				zigZagEncoded >>= 7;
			}
			buff[length++] = (byte)zigZagEncoded;

			return buff;
		}

		/// <summary>
		/// TODO: is always the result 4 bytes?
		/// </summary>
		private static byte[] ConvertToVarBinaryZigzag(long value, out byte length)
		{
			var buff = SharedArray.Get();
			length = 0;

			var zigZagEncoded = unchecked((ulong)((value << 1) ^ (value >> 63)));

			while ((zigZagEncoded & ~0x7FUL) != 0)
			{
				buff[length++] = (byte)((zigZagEncoded | 0x80) & 0xFF);
				zigZagEncoded >>= 7;
			}
			buff[length++] = (byte)zigZagEncoded;

			return buff;
		}

		//private static byte[] ConvertToVarBinary(int value, out byte length)
		//{
		//	if (value < 0)
		//	{
		//		length = 4;
		//		var buff = SharedArray.Get();
		//		buff[0] = (byte)value;
		//		buff[1] = unchecked((byte)(value >> 8));
		//		buff[2] = unchecked((byte)(value >> 16));
		//		buff[3] = unchecked((byte)(value >> 24));
		//		return buff;
		//	}
		//	else
		//	{
		//		var buff = SharedArray.Get();
		//		SharedArray.ClearArray4();

		//		var num1 = (byte)value;
		//		var num2 = unchecked((byte)(value >> 8));
		//		var num3 = unchecked((byte)(value >> 16));
		//		var num4 = unchecked((byte)(value >> 24));


		//		buff[0] = num1;

		//		if (num2 > 0)
		//		{
		//			buff[1] = num2;
		//		}
		//		else if (num3 == 0 && num4 == 0)
		//		{
		//			length = 1;
		//			return buff;
		//		}

		//		if (num3 > 0)
		//		{
		//			buff[2] = num3;
		//		}
		//		else if (num4 == 0)
		//		{
		//			length = 2;
		//			return buff;
		//		}

		//		if (num4 > 0)
		//			buff[3] = num4;
		//		else
		//		{
		//			length = 3;
		//			return buff;
		//		}
		//		length = 4;
		//		return buff;
		//	}
		//}

		private static byte[] ConvertToVarBinary(uint value, out byte length)
		{
			if (value == 0)
			{
				length = 1;
				return new byte[] { 0 };
			}

			var buff = SharedArray.Get();
			SharedArray.ClearArray4();

			var num1 = (byte)value;
			var num2 = unchecked((byte)(value >> 8));
			var num3 = unchecked((byte)(value >> 16));
			var num4 = unchecked((byte)(value >> 24));


			buff[0] = num1;

			if (num2 > 0)
			{
				buff[1] = num2;
			}
			else if (num3 == 0 && num4 == 0)
			{
				length = 1;
				return buff;
			}

			if (num3 > 0)
			{
				buff[2] = num3;
			}
			else if (num4 == 0)
			{
				length = 2;
				return buff;
			}

			if (num4 > 0)
				buff[3] = num4;
			else
			{
				length = 3;
				return buff;
			}
			length = 4;
			return buff;
		}

		//private static byte[] ConvertToVarBinary(long value, out byte length)
		//{
		//	var buff = SharedArray.Get();
		//	buff[0] = (byte)value;
		//	buff[1] = unchecked((byte)(value >> 8));
		//	buff[2] = unchecked((byte)(value >> 16));
		//	buff[3] = unchecked((byte)(value >> 24));
		//	buff[4] = unchecked((byte)(value >> 32));
		//	buff[5] = unchecked((byte)(value >> 40));
		//	buff[6] = unchecked((byte)(value >> 48));
		//	buff[7] = unchecked((byte)(value >> 56));

		//	for (int i = 8 - 1; i >= 0; i--)
		//	{
		//		if (buff[i] > 0)
		//		{
		//			length = (byte)(i + 1);
		//			//if (length != 8)
		//			//	Array.Resize(ref buff, length);
		//			return buff;
		//		}
		//	}

		//	length = 1;
		//	return new byte[] { 0 };
		//}

		private static byte[] ConvertToVarBinary(ulong value, out byte length)
		{
			var buff = SharedArray.Get();
			buff[0] = (byte)value;
			buff[1] = unchecked((byte)(value >> 8));
			buff[2] = unchecked((byte)(value >> 16));
			buff[3] = unchecked((byte)(value >> 24));
			buff[4] = unchecked((byte)(value >> 32));
			buff[5] = unchecked((byte)(value >> 40));
			buff[6] = unchecked((byte)(value >> 48));
			buff[7] = unchecked((byte)(value >> 56));

			for (int i = 8 - 1; i >= 0; i--)
			{
				if (buff[i] > 0)
				{
					length = (byte)(i + 1);
					return buff;
				}
			}

			length = 1;
			return new byte[] { 0 };
		}

		private static byte[] ConvertToVarBinary(short value, out byte length)
		{
			if (value < 0)
			{
				length = 2;
				var buff = new byte[2];
				buff[0] = (byte)value;
				buff[1] = unchecked((byte)(value >> 8));
				return buff;
			}
			else
			{
				var buff = new byte[2];
				var num1 = (byte)value;
				var num2 = unchecked((byte)(value >> 8));

				buff[0] = num1;

				if (num2 > 0)
					buff[1] = num2;
				else
				{
					length = 1;
					return buff;
				}

				length = 2;
				return buff;
			}
		}

		private static byte[] ConvertToVarBinary(ushort value, out byte length)
		{
			var buff = new byte[2];
			var num1 = (byte)value;
			var num2 = unchecked((byte)(value >> 8));

			buff[0] = num1;

			if (num2 > 0)
				buff[1] = num2;
			else
			{
				length = 1;
				return buff;
			}

			length = 2;
			return buff;
		}

		private static byte[] ConvertToVarBinary(float value, out byte length, out int position)
		{
			var bitsArray = BitConverter.GetBytes(value);
			for (int i = 0; i < 4; i++)
			{
				if (bitsArray[i] > 0)
				{
					position = i;
					length = (byte)(4 - position);

					return bitsArray;
				}
			}
			length = 1;
			position = 0;
			return new byte[] { 0 };
		}

		private static byte[] ConvertToVarBinary(float value, out byte length)
		{
			// Float & double numeric formats stores valuable bytes from right to left

			var valueBuff = BitConverter.GetBytes(value);
			var num1 = valueBuff[0];
			var num2 = valueBuff[1];
			var num3 = valueBuff[2];
			var num4 = valueBuff[3];

			// zero
			if (num4 == 0 && num3 == 0 && num2 == 0 && num1 == 0)
			{
				length = 1;
				return new byte[] { 0 };
			}

			if (num3 == 0 && num2 == 0 && num1 == 0)
			{
				length = 1;
				return new byte[] { num4 };
			}

			if (num2 == 0 && num1 == 0)
			{
				length = 2;
				return new byte[] { num3, num4 };
			}

			if (num1 == 0)
			{
				length = 3;
				return new byte[] { num2, num3, num4 };
			}

			// no zeros
			length = 4;

			return valueBuff;
		}

		private static byte[] ConvertToVarBinary(double value, out byte length, out int position)
		{
			var bitsArray = BitConverter.GetBytes(value);
			for (int i = 0; i < 8; i++)
			{
				if (bitsArray[i] > 0)
				{
					position = i;
					length = (byte)(8 - position);

					return bitsArray;
				}
			}
			length = 1;
			position = 0;
			return new byte[] { 0 };
		}

		private static byte[] ConvertToVarBinary(double value, out byte length)
		{
			// Float  &double numeric formats stores valuable bytes from right to left

			var valueBuff = BitConverter.GetBytes(value);
			var num1 = valueBuff[0];
			var num2 = valueBuff[1];
			var num3 = valueBuff[2];
			var num4 = valueBuff[3];
			var num5 = valueBuff[4];
			var num6 = valueBuff[5];
			var num7 = valueBuff[6];
			var num8 = valueBuff[7];

			// zero
			if (num8 == 0 && num7 == 0 && num6 == 0 && num5 == 0 && num4 == 0 && num3 == 0 && num2 == 0 && num1 == 0)
			{
				length = 1;
				return new byte[] { 0 };
			}

			if (num7 == 0 && num6 == 0 && num5 == 0 && num4 == 0 && num3 == 0 && num2 == 0 && num1 == 0)
			{
				length = 1;
				return new byte[] { num8 };
			}

			if (num6 == 0 && num5 == 0 && num4 == 0 && num3 == 0 && num2 == 0 && num1 == 0)
			{
				length = 2;
				return new byte[] { num7, num8 };
			}

			if (num5 == 0 && num4 == 0 && num3 == 0 && num2 == 0 && num1 == 0)
			{
				length = 3;
				return new byte[] { num6, num7, num8 };
			}

			if (num4 == 0 && num3 == 0 && num2 == 0 && num1 == 0)
			{
				length = 4;
				return new byte[] { num5, num6, num7, num8 };
			}

			if (num3 == 0 && num2 == 0 && num1 == 0)
			{
				length = 5;
				return new byte[] { num4, num5, num6, num7, num8 };
			}

			if (num2 == 0 && num1 == 0)
			{
				length = 6;
				return new byte[] { num3, num4, num5, num6, num7, num8 };
			}

			if (num1 == 0)
			{
				length = 7;
				return new byte[] { num2, num3, num4, num5, num6, num7, num8 };
			}

			// no zeros
			length = 8;

			return valueBuff;
		}

		private static byte[] ConvertToVarBinary(decimal value, out byte length)
		{
			var bits = decimal.GetBits(value);
			var bitsArray = SharedArray.Get();

			for (byte i = 0; i < bits.Length; i++)
			{
				var bytes = BitConverter.GetBytes(bits[i]);
				Array.Copy(bytes, 0, bitsArray, i * 4, 4);
			}

			// finding the empty characters
			for (int i = bitsArray.Length - 1; i >= 0; i--)
			{
				if (bitsArray[i] > 0)
				{
					length = (byte)(i + 1);

					return bitsArray;
				}
			}
			length = 1;
			return new byte[] { 0 };
		}

		#endregion


	}
}
