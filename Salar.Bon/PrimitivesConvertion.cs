using System;
using System.IO;

namespace Salar.Bon
{
	internal static class PrimitivesConvertion
	{
		/// <summary>
		/// 0011 1111
		/// </summary>
		private const byte NullableMaxNumInByte = 63;

		/// <summary>
		/// 0111 1111
		/// </summary>
		private const byte NullableFlagNullMask = 127;

		/// <summary>
		/// 1000 0000
		/// </summary>
		private const byte NullableFlagNullNum = 128;

		/// <summary>
		/// 1011 1111
		/// </summary>
		private const byte NullableFlagInsideMask = 191;

		/// <summary>
		/// 0100 0000
		/// </summary>
		private const byte NullableFlagInsideNum = 64;

		/// <summary>
		/// 0111 1111
		/// </summary>
		private const byte ActualMaxNumInByte = 127;

		/// <summary>
		/// 0111 1111
		/// </summary>
		private const byte ActualFlagInsideMask = 127;

		/// <summary>
		/// 1000 0000
		/// </summary>
		private const byte ActualFlagInsideNum = 128;

		/// <summary>
		/// 
		/// </summary>
		public static short ReadVarInt16(BinaryReader reader)
		{
			var input = reader.ReadByte();
			var isItInside = (input & ActualFlagInsideNum) == ActualFlagInsideNum;

			var insideNum = (short)(input & ActualFlagInsideMask);
			if (isItInside)
			{
				return insideNum;
			}
			else
			{
				return ReadInt16(reader, insideNum);
				//return reader.ReadInt32();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public static short? ReadVarInt16Nullable(BinaryReader reader)
		{
			var input = reader.ReadByte();
			if (input == NullableFlagNullNum)
				return null;

			var isnull = (input & NullableFlagNullNum) == NullableFlagNullNum;
			if (isnull)
				return null;

			var insideNum = (byte)(input & NullableFlagInsideMask);
			insideNum = (byte)(insideNum & NullableFlagNullMask);

			var isItInside = (input & NullableFlagInsideNum) == NullableFlagInsideNum;
			if (isItInside)
			{
				return insideNum;
			}
			else
			{
				return ReadInt16(reader, insideNum);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public static int ReadVarInt32(BinaryReader reader)
		{
			var input = reader.ReadByte();
			var isItInside = (input & ActualFlagInsideNum) == ActualFlagInsideNum;

			var insideNum = (input & ActualFlagInsideMask);
			if (isItInside)
			{
				return insideNum;
			}
			else
			{
				return ReadInt32(reader, insideNum);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public static int? ReadVarInt32Nullable(BinaryReader reader)
		{
			var input = reader.ReadByte();
			if (input == NullableFlagNullNum)
				return null;

			var isnull = (input & NullableFlagNullNum) == NullableFlagNullNum;
			if (isnull)
				return null;


			var insideNum = (byte)(input & NullableFlagInsideMask);
			insideNum = (byte)(insideNum & NullableFlagNullMask);

			var isItInside = (input & NullableFlagInsideNum) == NullableFlagInsideNum;
			if (isItInside)
			{
				return insideNum;
			}
			else
			{
				return ReadInt32(reader, insideNum);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public static long ReadVarInt64(BinaryReader reader)
		{
			var input = reader.ReadByte();
			var isItInside = (input & ActualFlagInsideNum) == ActualFlagInsideNum;

			var insideNum = (input & ActualFlagInsideMask);
			if (isItInside)
			{
				return insideNum;
			}
			else
			{
				return ReadInt64(reader, insideNum);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public static long? ReadVarInt64Nullable(BinaryReader reader)
		{
			var input = reader.ReadByte();
			if (input == 0)
				return null;

			var isnull = (input & NullableFlagNullNum) == NullableFlagNullNum;
			if (isnull)
				return null;


			var insideNum = (byte)(input & NullableFlagInsideMask);
			insideNum = (byte)(insideNum & NullableFlagNullMask);

			var isItInside = (input & NullableFlagInsideNum) == NullableFlagInsideNum;
			if (isItInside)
			{
				return insideNum;
			}
			else
			{
				return ReadInt64(reader, insideNum);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public static void WriteVarInt(BinaryWriter writer, short num)
		{
			// store more space
			if (num > ActualMaxNumInByte || num < 0)
			{
				// No Flag is required
				// length of the integer bytes
				WriteInt(writer, num);
			}
			else
			{
				byte numByte = (byte)num;

				// set the flag of inside
				numByte = (byte)(numByte | ActualFlagInsideNum);
				writer.Write(numByte);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public static void WriteVarInt(BinaryWriter writer, short? num)
		{
			if (num == null)
			{
				writer.Write(NullableFlagNullNum);
				return;
			}
			// store more space
			if (num > NullableMaxNumInByte || num < 0)
			{
				// No Flag is required
				// length of the integer bytes
				WriteInt(writer, num.Value);
			}
			else
			{
				byte numByte = (byte)num;

				// set the flag of inside
				numByte = (byte)(numByte | NullableFlagInsideNum);
				writer.Write(numByte);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public static void WriteVarInt(BinaryWriter writer, int num)
		{
			// store more space
			if (num > ActualMaxNumInByte || num < 0)
			{
				// No Flag is required
				// length of the integer bytes
				WriteInt(writer, num);
			}
			else
			{
				byte numByte = (byte)num;

				// set the flag of inside
				numByte = (byte)(numByte | ActualFlagInsideNum);
				writer.Write(numByte);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public static void WriteVarInt(BinaryWriter writer, int? num)
		{
			if (num == null)
			{
				writer.Write(NullableFlagNullNum);
				return;
			}

			// store more space
			if (num > NullableMaxNumInByte || num < 0)
			{
				// No Flag is required
				// length of the integer bytes
				//writer.Write(num.Value);
				WriteInt(writer, num.Value);
			}
			else
			{
				byte numByte = (byte)num.Value;

				// set the flag of inside
				numByte = (byte)(numByte | NullableFlagInsideNum);
				writer.Write(numByte);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public static void WriteVarInt(BinaryWriter writer, long num)
		{
			// store more space
			if (num > ActualMaxNumInByte || num < 0)
			{
				// No Flag is required
				// length of the integer bytes
				WriteInt(writer, num);
			}
			else
			{
				byte numByte = (byte)num;

				// set the flag of inside
				numByte = (byte)(numByte | ActualFlagInsideNum);
				writer.Write(numByte);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public static void WriteVarInt(BinaryWriter writer, long? num)
		{
			if (num == null)
			{
				writer.Write(NullableFlagNullNum);
				return;
			}

			// store more space
			if (num > NullableMaxNumInByte || num < 0)
			{
				// No Flag is required
				// length of the integer bytes
				//writer.Write(num.Value);
				WriteInt(writer, num.Value);
			}
			else
			{
				byte numByte = (byte)num.Value;

				// set the flag of inside
				numByte = (byte)(numByte | NullableFlagInsideNum);
				writer.Write(numByte);
			}
		}


		private static void WriteInt(BinaryWriter writer, long num)
		{
			byte numLen;
			var numBuff = ConvertToVarBinary(num, out numLen);
			writer.Write(numLen);
			writer.Write(numBuff, 0, numBuff.Length);
		}
		private static void WriteInt(BinaryWriter writer, int num)
		{
			byte numLen;
			var numBuff = ConvertToVarBinary(num, out numLen);
			writer.Write(numLen);
			writer.Write(numBuff, 0, numBuff.Length);
		}
		private static void WriteInt(BinaryWriter writer, short num)
		{
			byte numLen;
			var numBuff = ConvertToVarBinary(num, out numLen);
			writer.Write(numLen);
			writer.Write(numBuff, 0, numBuff.Length);
		}

		private static long ReadInt64(BinaryReader reader, int length)
		{
			var intBuff = reader.ReadBytes(length);
			if (intBuff.Length == 8)
			{
				return ReadInt64(intBuff);
			}
			else
			{
				var intFinalBuff = new byte[8];

				Array.Copy(intBuff, intFinalBuff, intBuff.Length);
				return ReadInt64(intFinalBuff);
			}
		}
		private static int ReadInt32(BinaryReader reader, int length)
		{
			var intBuff = reader.ReadBytes(length);
			if (intBuff.Length == 4)
			{
				return ReadInt32(intBuff);
			}
			else
			{
				var intFinalBuff = new byte[4];

				Array.Copy(intBuff, intFinalBuff, intBuff.Length);
				return ReadInt32(intFinalBuff);
			}
		}
		private static short ReadInt16(BinaryReader reader, short length)
		{
			var intBuff = reader.ReadBytes(length);
			if (intBuff.Length == 2)
			{
				return ReadInt16(intBuff);
			}
			else
			{
				var intFinalBuff = new byte[2];

				Array.Copy(intBuff, intFinalBuff, intBuff.Length);
				return ReadInt16(intFinalBuff);
			}
		}

		private static long ReadInt64(byte[] intBytes)
		{
			uint num = (uint)(((intBytes[0] | (intBytes[1] << 8)) | (intBytes[2] << 16)) | (intBytes[3] << 24));
			uint num2 = (uint)(((intBytes[4] | (intBytes[5] << 8)) | (intBytes[6] << 16)) | (intBytes[7] << 24));
			return (long)((((ulong)num2) << 32) | ((ulong)num));
		}
		private static int ReadInt32(byte[] intBytes)
		{
			return ((intBytes[0] | (intBytes[1] << 8)) | (intBytes[2] << 16)) | (intBytes[3] << 24);
		}
		private static short ReadInt16(byte[] intBytes)
		{
			return (short)(intBytes[0] | (intBytes[1] << 8));

		}

		private static byte[] ConvertToVarBinary(int value, out byte length)
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
					buff[1] = num2;
				else
				{
					Array.Resize(ref buff, 1);
					length = 1;
					return buff;
				}

				if (num3 > 0)
					buff[2] = num3;
				else
				{
					Array.Resize(ref buff, 2);
					length = 2;
					return buff;
				}

				if (num4 > 0)
					buff[3] = num4;
				else
				{
					Array.Resize(ref buff, 3);
					length = 3;
					return buff;
				}
				length = 4;
				return buff;
			}
		}
		private static byte[] ConvertToVarBinary(long value, out byte length)
		{
			if (value < 0)
			{
				length = 8;
				var buff = new byte[8];
				buff[0] = (byte)value;
				buff[1] = (byte)(value >> 8);
				buff[2] = (byte)(value >> 16);
				buff[3] = (byte)(value >> 24);
				buff[4] = (byte)(value >> 32);
				buff[5] = (byte)(value >> 40);
				buff[6] = (byte)(value >> 48);
				buff[7] = (byte)(value >> 56);

				return buff;
			}
			else
			{
				var buff = new byte[8];
				var num1 = (byte)value;
				var num2 = (byte)(value >> 8);
				var num3 = (byte)(value >> 16);
				var num4 = (byte)(value >> 24);
				var num5 = (byte)(value >> 32);
				var num6 = (byte)(value >> 40);
				var num7 = (byte)(value >> 48);
				var num8 = (byte)(value >> 56);


				buff[0] = num1;

				if (num2 > 0)
					buff[1] = num2;
				else
				{
					Array.Resize(ref buff, 1);
					length = 1;
					return buff;
				}

				if (num3 > 0)
					buff[2] = num3;
				else
				{
					Array.Resize(ref buff, 2);
					length = 2;
					return buff;
				}

				if (num4 > 0)
					buff[3] = num4;
				else
				{
					Array.Resize(ref buff, 3);
					length = 3;
					return buff;
				}

				if (num5 > 0)
					buff[4] = num5;
				else
				{
					Array.Resize(ref buff, 4);
					length = 4;
					return buff;
				}

				if (num6 > 0)
					buff[5] = num6;
				else
				{
					Array.Resize(ref buff, 5);
					length = 5;
					return buff;
				}

				if (num7 > 0)
					buff[6] = num7;
				else
				{
					Array.Resize(ref buff, 6);
					length = 6;
					return buff;
				}

				if (num8 > 0)
					buff[7] = num8;
				else
				{
					Array.Resize(ref buff, 7);
					length = 7;
					return buff;
				}

				length = 8;
				return buff;
			}
		}
		private static byte[] ConvertToVarBinary(Int16 value, out byte length)
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
					Array.Resize(ref buff, 1);
					length = 1;
					return buff;
				}

				length = 2;
				return buff;
			}
		}

	}
}
