using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salar.Bois.Serializers
{
	public static class NumericSerializers
	{
		#region Writers

		/// <summary>
		/// [EmbedIndicator-SignIndicator-0-0-0-0-0-0] [optional data]  0..63 can be embedded
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="num"></param>
		public static void WriteSignedVarInt(BinaryWriter writer, int num)
		{
			const byte EmbeddedSignedMaxNumInByte = 0b0_0_1_1_1_1_1_1;// 63;
			const byte FlagEmbdedded = 0b1_0_0_0_0_0_0_0;
			const byte FlagNegativeNum = 0b0_1_0_0_0_0_0_0;
			const byte FlagNegativeNumEmbdedded = 0b1_1_0_0_0_0_0_0;

			if (num > EmbeddedSignedMaxNumInByte)
			{
				// number is not negative
				// number is not embedded

				var numBuff = NumericSerializers.ConvertToVarBinary(num, out var numLen);

				writer.Write(numLen);
				writer.Write(numBuff, 0, numLen);
			}
			else if (num < 0)
			{
				num = -num;
				if (num > EmbeddedSignedMaxNumInByte)
				{
					// number is negative
					// number is not embedded

					var numBuff = NumericSerializers.ConvertToVarBinary(num, out var numLen);

					writer.Write((byte)(numLen | FlagNegativeNum));
					writer.Write(numBuff, 0, numLen);
				}
				else
				{
					// number is negative
					// number is embedded

					writer.Write((byte)(num | FlagNegativeNumEmbdedded));
				}
			}
			else
			{
				// number is not negative
				// number is embedded

				writer.Write((byte)(num | FlagEmbdedded));
			}
		}

		/// <summary>
		/// [EmbedIndicator-0-0-0-0-0-0-0] [optional data]  0..127 can be embedded
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="num"></param>
		static void WriteUnsignedVarInt(BinaryWriter writer, uint num)
		{
			byte EmbeddedSignedMaxNumInByte = 0b0_1_1_1_1_1_1_1;// 127;
			byte FlagEmbdedded = 0b1_0_0_0_0_0_0_0;

			if (num > EmbeddedSignedMaxNumInByte)
			{
				// number is not embedded

				var numBuff = NumericSerializers.ConvertToVarBinary(num, out var numLen);

				writer.Write(numLen);
				writer.Write(numBuff, 0, numLen);
			}
			else
			{
				// number is embedded

				writer.Write((byte)(num | FlagEmbdedded));
			}
		}

		/// <summary>
		/// [EmbedIndicator-NullIndicator-SignIndicator-0-0-0-0-0] [optional data]  0..31 can be embedded
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="num"></param>
		static void WriteSignedVarIntNullable(BinaryWriter writer, int? num)
		{
			byte EmbeddedSignedMaxNumInByte = 0b0_0_0_1_1_1_1_1;// 31;
			byte FlagEmbdedded = 0b1_0_0_0_0_0_0_0;
			byte FlagNullable = 0b0_1_0_0_0_0_0_0;
			byte FlagNullableNegativeNum = 0b0_0_1_0_0_0_0_0;
			byte FlagNullableNegativeEmbdeddedNum = 0b1_0_1_0_0_0_0_0;

			if (num == null)
			{
				// number is null

				writer.Write(FlagNullable);
			}
			else if (num > EmbeddedSignedMaxNumInByte)
			{
				// number is not null
				// number is not negative
				// number is not embedded

				var numBuff = NumericSerializers.ConvertToVarBinary(num.Value, out var numLen);

				writer.Write(numLen);
				writer.Write(numBuff, 0, numLen);
			}
			else if (num < 0)
			{
				num = -num;
				if (num > EmbeddedSignedMaxNumInByte)
				{
					// number is not null
					// number is negative
					// number is not embedded

					var numBuff = NumericSerializers.ConvertToVarBinary(num.Value, out var numLen);

					writer.Write((byte)(numLen | FlagNullableNegativeNum));
					writer.Write(numBuff, 0, numLen);
				}
				else
				{
					// number is not null
					// number is negative
					// number is embedded

					writer.Write((byte)(num.Value | FlagNullableNegativeEmbdeddedNum));
				}
			}
			else
			{
				// number is not null
				// number is not negative
				// number is embedded

				writer.Write((byte)(num.Value | FlagEmbdedded));
			}
		}

		/// <summary>
		/// [NullIndicator-EmbedIndicator-0-0-0-0-0-0] [optional data]  0..127 can be embedded
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="num"></param>
		static void WriteUnsignedVarIntNullable(BinaryWriter writer, uint? num)
		{
			byte EmbeddedSignedMaxNumInByte = 0b0_0_0_1_1_1_1_1;// 31;
			byte FlagEmbdedded = 0b1_0_0_0_0_0_0_0;
			byte FlagNullable = 0b0_1_0_0_0_0_0_0;

			if (num == null)
			{
				// number is null

				writer.Write(FlagNullable);
			}
			else if (num > EmbeddedSignedMaxNumInByte)
			{
				// number is not null
				// number is not negative
				// number is not embedded

				var numBuff = NumericSerializers.ConvertToVarBinary(num.Value, out var numLen);

				writer.Write(numLen);
				writer.Write(numBuff, 0, numLen);
			}
			else
			{
				// number is not null
				// number is not negative
				// number is embedded

				writer.Write((byte)(num.Value | FlagEmbdedded));
			}
		}


		#endregion

		#region Binary Converters

		private static long ReadInt64(byte[] intBytes)
		{
			uint num = (uint)(((intBytes[0] | (intBytes[1] << 8)) | (intBytes[2] << 16)) | (intBytes[3] << 24));
			uint num2 = (uint)(((intBytes[4] | (intBytes[5] << 8)) | (intBytes[6] << 16)) | (intBytes[7] << 24));
			return (long)((((ulong)num2) << 32) | ((ulong)num));
		}
		private static ulong ReadBinaryUInt64(byte[] intBytes)
		{
			uint num = (uint)(((intBytes[0] | (intBytes[1] << 8)) | (intBytes[2] << 16)) | (intBytes[3] << 24));
			uint num2 = (uint)(((intBytes[4] | (intBytes[5] << 8)) | (intBytes[6] << 16)) | (intBytes[7] << 24));
			return (ulong)((((ulong)num2) << 32) | ((ulong)num));
		}
		private static uint ReadBinaryUInt32(byte[] intBytes)
		{
			return (uint)(((intBytes[0] | (intBytes[1] << 8)) | (intBytes[2] << 16)) | (intBytes[3] << 24));
		}
		private static int ReadBinaryInt32(byte[] intBytes)
		{
			return ((intBytes[0] | (intBytes[1] << 8)) | (intBytes[2] << 16)) | (intBytes[3] << 24);
		}
		private static int ReadBinaryInt32(byte[] intBytes, int startIndex)
		{
			return ((intBytes[startIndex + 0] | (intBytes[startIndex + 1] << 8)) | (intBytes[startIndex + 2] << 16)) | (intBytes[startIndex + 3] << 24);
		}
		private static short ReadBinaryInt16(byte[] intBytes)
		{
			return (short)(intBytes[0] | (intBytes[1] << 8));
		}
		private static ushort ReadBinaryUInt16(byte[] intBytes)
		{
			return (ushort)(intBytes[0] | (intBytes[1] << 8));
		}

		private static decimal ReadBinaryDecimal(byte[] decimalBytes)
		{
			var decimalBits = new int[4];
			decimalBits[0] = ReadBinaryInt32(decimalBytes, 4 * 0);
			decimalBits[1] = ReadBinaryInt32(decimalBytes, 4 * 1);
			decimalBits[2] = ReadBinaryInt32(decimalBytes, 4 * 2);
			decimalBits[3] = ReadBinaryInt32(decimalBytes, 4 * 3);

			return new decimal(decimalBits);
		}


		public static byte[] ConvertToVarBinary(int value, out byte length)
		{
			if (value < 0)
			{
				length = 4;
				var buff = new byte[4];
				buff[0] = (byte)value;
				buff[1] = (byte)(value >> 8);
				buff[2] = (byte)(value >> 16);
				buff[3] = (byte)(value >> 24);
				return buff;
			}
			else
			{
				var buff = new byte[4];
				var num1 = (byte)value;
				var num2 = (byte)(value >> 8);
				var num3 = (byte)(value >> 16);
				var num4 = (byte)(value >> 24);


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
		}

		public static byte[] ConvertToVarBinary(uint value, out byte length)
		{
			if (value == 0)
			{
				length = 1;
				return new byte[] { 0 };
			}

			var buff = new byte[4];
			var num1 = (byte)value;
			var num2 = (byte)(value >> 8);
			var num3 = (byte)(value >> 16);
			var num4 = (byte)(value >> 24);


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

		private static byte[] ConvertToVarBinary(long value, out byte length)
		{
			var buff = new byte[8];
			buff[0] = (byte)value;
			buff[1] = (byte)(value >> 8);
			buff[2] = (byte)(value >> 16);
			buff[3] = (byte)(value >> 24);
			buff[4] = (byte)(value >> 32);
			buff[5] = (byte)(value >> 40);
			buff[6] = (byte)(value >> 48);
			buff[7] = (byte)(value >> 56);

			for (int i = 8 - 1; i >= 0; i--)
			{
				if (buff[i] > 0)
				{
					length = (byte)(i + 1);
					//if (length != 8)
					//	Array.Resize(ref buff, length);
					return buff;
				}
			}

			length = 1;
			return new byte[] { 0 };
		}

		private static byte[] ConvertToVarBinary(ulong value, out byte length)
		{
			var buff = new byte[8];
			buff[0] = (byte)value;
			buff[1] = (byte)(value >> 8);
			buff[2] = (byte)(value >> 16);
			buff[3] = (byte)(value >> 24);
			buff[4] = (byte)(value >> 32);
			buff[5] = (byte)(value >> 40);
			buff[6] = (byte)(value >> 48);
			buff[7] = (byte)(value >> 56);

			for (int i = 8 - 1; i >= 0; i--)
			{
				if (buff[i] > 0)
				{
					length = (byte)(i + 1);
					//if (length != 8)
					//	Array.Resize(ref buff, length);
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
				buff[1] = (byte)(value >> 8);
				return buff;
			}
			else
			{
				var buff = new byte[2];
				var num1 = (byte)value;
				var num2 = (byte)(value >> 8);

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
			var num2 = (byte)(value >> 8);

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
			var bitsArray = new byte[16];

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
					length = (Byte)(i + 1);
					//if (length != 16)
					//	Array.Resize(ref bitsArray, length);

					return bitsArray;
				}
			}
			length = 0;
			return new byte[0];
		}

		#endregion


	}
}
