//#define EnableStudyTests
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Salar.Bois.Serializers;
using Xunit;

namespace Salar.Bois.NetFx.Tests.StudyDontRun
{
	public class StudyNumericFormats
	{
		#region Definitions
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
		/// 0100 0000 = 64
		/// </summary>
		internal const byte FlagNullable = 0b0_1_0_0_0_0_0_0;

		/// <summary>
		/// 1000 0000 = 128
		/// </summary>
		private const byte FlagEmbdedded = 0b1_0_0_0_0_0_0_0;

		/// <summary>
		/// 127
		/// </summary>
		private const byte FlagEmbdeddedMask = 0b0_1_1_1_1_1_1_1;

		/// <summary>
		/// 0011 1111 = 63
		/// </summary>
		private const byte EmbeddedSignedMaxNumInByte = 0b0_0_1_1_1_1_1_1;// 63;

		/// <summary>
		/// 0100 0000 = 64
		/// </summary>
		private const byte FlagNonullNegativeNum = 0b0_1_0_0_0_0_0_0;

		/// <summary>
		/// 191
		/// </summary>
		private const byte FlagNonullNegativeMask = 0b1_0_1_1_1_1_1_1;

		/// <summary>
		/// 192
		/// </summary>
		private const byte FlagNonullNegativeNumEmbdedded = 0b1_1_0_0_0_0_0_0;

		/// <summary>
		/// 63
		/// </summary>
		private const byte FlagNonullNegativeNumEmbdeddedMask = 0b0_0_1_1_1_1_1_1;

		/// <summary>
		/// 32
		/// </summary>
		private const byte FlagNullableNegativeNum = 0b0_0_1_0_0_0_0_0;

		/// <summary>
		/// 223
		/// </summary>
		private const byte FlagNullableNegativeMask = 0b1_1_0_1_1_1_1_1;

		/// <summary>
		/// 160
		/// </summary>
		private const byte FlagNullableNegativeEmbdeddedNum = 0b1_0_1_0_0_0_0_0;

		/// <summary>
		/// 95
		/// </summary>
		private const byte FlagNullableNegativeEmbdeddedMask = 0b0_1_0_1_1_1_1_1;

		/// <summary>
		/// 127
		/// </summary>
		private const byte EmbeddedUnsignedMaxNumInByte = 0b0_1_1_1_1_1_1_1;

		/// <summary>
		/// 31
		/// </summary>
		private const byte EmbeddedSignedNullableMaxNumInByte = 0b0_0_0_1_1_1_1_1;

		/// <summary>
		/// 63
		/// </summary>
		private const byte EmbeddedUnsignedNullableMaxNumInByte = 0b0_0_1_1_1_1_1_1;



		#endregion

		private const uint RepeatCount = 26_000;
		private const int RepeatJump = 50;
		private const uint RepeatJumpU = 50;

		private MemoryStream mem;
		public StudyNumericFormats()
		{
			mem = new MemoryStream(10_000);
		}

#if EnableStudyTests
		[Theory]
		[InlineData(RepeatCount, RepeatJump)]
		[InlineData(256, 1)]
#endif
		public void Testing1_V3TotalSize(int count, int jump)
		{
			mem.Position = 0;
			using (var writer = new BinaryWriter(mem))
			{
				var sw = Stopwatch.StartNew();
				long v3TotalSize = 0;
				for (int i = 0; i < count; i += jump)
				{
					WriteVarInt_V3(writer, i);
					v3TotalSize += mem.Length;
					mem.Position = 0;
				}
				sw.Stop();
				mem.Position = 0;

				long v3TotalSize2 = 0;
				for (int i = 0; i > -count; i -= jump)
				{
					WriteVarInt_V3(writer, i);
					v3TotalSize2 += mem.Length;
					mem.Position = 0;
				}

				var message = $"V3-singed length: {v3TotalSize}, time: {sw.ElapsedTicks.ToString()}, time: {sw.ElapsedMilliseconds.ToString()} , Negative V3-singed length: {v3TotalSize2}";
				throw new Exception(message);
			}
		}

#if EnableStudyTests
		[Theory]
		[InlineData(RepeatCount, RepeatJump)]
		[InlineData(256, 1)]
#endif
		public void Testing2_V31_TotalSize(int count, int jump)
		{
			mem.Position = 0;
			using (var writer = new BinaryWriter(mem))
			{
				var sw = Stopwatch.StartNew();
				long v3TotalSize = 0;
				for (int i = 0; i < count; i += jump)
				{
					WriteVarInt_V31(writer, i);
					v3TotalSize += mem.Length;
					mem.Position = 0;
				}
				sw.Stop();
				mem.Position = 0;

				long v3TotalSize2 = 0;
				for (int i = 0; i > -count; i -= jump)
				{
					WriteVarInt_V31(writer, i);
					v3TotalSize2 += mem.Length;
					mem.Position = 0;
				}

				var message = $"V31-singed length: {v3TotalSize}, time: {sw.ElapsedTicks.ToString()}, time: {sw.ElapsedMilliseconds.ToString()} , Negative V31-singed length: {v3TotalSize2}";
				throw new Exception(message);
			}
		}

#if EnableStudyTests
		[Theory]
		[InlineData(RepeatCount, RepeatJump)]
		[InlineData(256, 1)]
#endif
		public void Testing3_ZigzagTotalSize(int count, int jump)
		{
			mem.Position = 0;
			using (var writer = new BinaryWriter(mem))
			using (var reader = new BinaryReader(mem))
			{
				int run = 0;
				var sw = Stopwatch.StartNew();
				long zigazTotalSize = 0;
				for (int i = 0; i < count; i += jump)
				{
					WriteIntZigzag(writer, i);
					zigazTotalSize += mem.Length;
					mem.Position = 0;

					//var val = ReadInt16Zigzag(reader);
					//mem.Position = 0;

					//if (val != i)
					//{
					//	throw new Exception($"Zigzag is not valid, expected: {i} , got: {val}");
					//}
				}
				sw.Stop();
				mem.Position = 0;

				long zigazTotalSize2 = 0;
				for (int i = 0; i > -count; i -= jump)
				{
					WriteIntZigzag(writer, i);
					zigazTotalSize2 += mem.Length;
					mem.Position = 0;
					run++;
				}

				var message = $"ZigZag length: {zigazTotalSize}, time: {sw.ElapsedTicks.ToString()}, time: {sw.ElapsedMilliseconds.ToString()} , Negative ZigZag length: {zigazTotalSize2} , run: {run}";
				throw new Exception(message);
			}
		}


//#if EnableStudyTests
//		[Theory]
//		[InlineData(RepeatCount, RepeatJump)]
//		[InlineData(256, 1)]
//#endif
//		public void Testing3_Zigzag2TotalSize(int count, int jump)
//		{
//			mem.Position = 0;
//			using (var writer = new BinaryWriter(mem))
//			{
//				int run = 0;

//				var sw = Stopwatch.StartNew();
//				long zigazTotalSize = 0;
//				for (int i = 0; i < count; i += jump)
//				{
//					var bytes = VarintBitConverter.GetVarintBytes(i);
//					writer.Write(bytes);

//					zigazTotalSize += mem.Length;
//					mem.Position = 0;
//				}
//				sw.Stop();
//				mem.Position = 0;

//				long zigazTotalSize2 = 0;
//				for (int i = 0; i > -count; i -= jump)
//				{
//					WriteIntZigzag(writer, i);
//					zigazTotalSize2 += mem.Length;
//					mem.Position = 0;
//					run++;
//				}

//				var message = $"ZigZagVarintBitConverter length: {zigazTotalSize}, time: {sw.ElapsedTicks.ToString()}, time: {sw.ElapsedMilliseconds.ToString()} , Negative ZigZagVarintBitConverter length: {zigazTotalSize2}  , run: {run}";
//				throw new Exception(message);
//			}
//		}
#if EnableStudyTests
		[Theory]
		[InlineData(RepeatCount, RepeatJump)]
		[InlineData(256, 1)]
#endif
		public void Testing3_ZigzagInt16TotalSize(int count, short jump)
		{
			mem.Position = 0;
			using (var writer = new BinaryWriter(mem))
			using (var reader = new BinaryReader(mem))
			{

				var sw = Stopwatch.StartNew();
				long zigazTotalSize = 0;
				for (short i = 0; i < count; i += jump)
				{
					WriteIntZigzag(writer, i);
					zigazTotalSize += mem.Length;
					mem.Position = 0;

					//var val = ReadInt16Zigzag(reader);
					//mem.Position = 0;

					//if (val != i)
					//{
					//	throw new Exception($"ZigzagInt16 is not valid, expected: {i} , got: {val}");
					//}
				}
				sw.Stop();
				mem.Position = 0;

				long zigazTotalSize2 = 0;
				for (short i = 0; i > -count; i -= jump)
				{
					WriteIntZigzag(writer, i);
					zigazTotalSize2 += mem.Length;
					mem.Position = 0;
				}

				var message = $"ZigZag16 length: {zigazTotalSize}, time: {sw.ElapsedTicks.ToString()}, time: {sw.ElapsedMilliseconds.ToString()} , Negative ZigZag16 length: {zigazTotalSize2} ";
				throw new Exception(message);
			}
		}


#if EnableStudyTests
		[Theory]
		[InlineData(RepeatCount, RepeatJump)]
		[InlineData(256, 1)]
#endif
		public void Testing5_ZigzagArrayTotalSize(int count, int jump)
		{
			mem.Position = 0;
			using (var writer = new BinaryWriter(mem))
			{
				byte length;

				var sw = Stopwatch.StartNew();
				long zigazTotalSize = 0;
				for (int i = 0; i < count; i += jump)
				{
					var array = ConvertToVarBinaryZigzag(i, out length);
					writer.Write(array, 0, length);

					zigazTotalSize += mem.Length;
					mem.Position = 0;
				}
				sw.Stop();
				mem.Position = 0;

				long zigazTotalSize2 = 0;
				for (int i = 0; i > -count; i -= jump)
				{
					var array = ConvertToVarBinaryZigzag(i, out length);
					writer.Write(array, 0, length);

					zigazTotalSize2 += mem.Length;
					mem.Position = 0;
				}

				var message = $"ZigZagArray length: {zigazTotalSize}, time: {sw.ElapsedTicks.ToString()}, time: {sw.ElapsedMilliseconds.ToString()} , Negative ZigZagArray length: {zigazTotalSize2} ";
				throw new Exception(message);
			}
		}


#if EnableStudyTests
		[Theory]
		[InlineData(RepeatCount, RepeatJumpU)]
		[InlineData(256, 1)]
#endif
		public void Testing6_SteppedTotalSize(int count, int jump)
		{
			mem.Position = 0;
			using (var writer = new BinaryWriter(mem))
			{
				var sw = Stopwatch.StartNew();
				long steppedTotalSize = 0;
				for (uint i = 0; i < count; i += RepeatJumpU)
				{
					WriteUIntStepped(writer, i);
					steppedTotalSize += mem.Length;
					mem.Position = 0;
				}
				sw.Stop();

				var message = $"Stepped length: {steppedTotalSize}, time: {sw.ElapsedTicks.ToString()}, time: {sw.ElapsedMilliseconds.ToString()} ";
				throw new Exception(message);
			}
		}

		/// <summary>
		/// 1000 0000 -> 1000 0000 -> 0000 0000 | stop
		/// </summary>
		private static void WriteUIntStepped(BinaryWriter writer, uint num)
		{
			//var localNum = num;
			while ((num & ~0x7F) != 0)
			{
				writer.Write((byte)((num | 0x80) & 0xFF));
				num >>= 7;
			}
			writer.Write((byte)num);
		}


		private static void WriteIntZigzag(BinaryWriter writer, short num)
		{
			var zigZagEncoded = unchecked((ushort)((num << 1) ^ (num >> 15)));
			while ((zigZagEncoded & ~0x7F) != 0)
			{
				writer.Write((byte)((zigZagEncoded | 0x80) & 0xFF));
				zigZagEncoded >>= 7;
			}
			writer.Write((byte)zigZagEncoded);
		}


		private static void WriteIntZigzag(BinaryWriter writer, int num)
		{
			var zigZagEncoded = unchecked((uint)((num << 1) ^ (num >> 31)));
			while ((zigZagEncoded & ~0x7F) != 0)
			{
				writer.Write((byte)((zigZagEncoded | 0x80) & 0xFF));
				zigZagEncoded >>= 7;
			}
			writer.Write((byte)zigZagEncoded);
		}


		internal static void WriteVarInt_V31(BinaryWriter writer, int num)
		{
			if (num > EmbeddedUnsignedMaxNumInByte || num < 0)
			{
				// number is not embeddable

				writer.Write(FlagNone);
				WriteIntZigzag(writer, num);
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
		/// [EmbedIndicator-SignIndicator-0-0-0-0-0-0] [optional data]  0..63 can be embedded
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="num"></param>
		internal static void WriteVarInt_V3(BinaryWriter writer, int num)
		{
			void WriteAsBigPositive()
			{
				var numBuff = ConvertToVarBinary(num, out var numLen);

				writer.Write(numLen);
				writer.Write(numBuff, 0, numLen);
			}

			if (num > EmbeddedSignedMaxNumInByte)
			{
				// number is not negative
				// number is not embedded

				WriteAsBigPositive();
			}
			else if (num < 0)
			{
				if (num == int.MinValue)
				{
					// Very special case, int.minValut cannot be converted

					WriteAsBigPositive();
					return;
				}
				num = -num;
				if (num > EmbeddedSignedMaxNumInByte)
				{
					// number is negative
					// number is not embedded

					var numBuff = ConvertToVarBinary(num, out var numLen);

					writer.Write((byte)(numLen | FlagNonullNegativeNum));
					writer.Write(numBuff, 0, numLen);
				}
				else
				{
					// number is negative
					// number is embedded

					writer.Write((byte)(num | FlagNonullNegativeNumEmbdedded));
				}
			}
			else
			{
				// number is not negative
				// number is embedded

				writer.Write((byte)(num | FlagEmbdedded));
			}
		}


		private static long EncodeZigZag(long value, int bitLength)
		{
			return (value << 1) ^ (value >> (bitLength - 1));
		}

		private static long DecodeZigZag(ulong value)
		{
			if ((value & 0x1) == 0x1)
			{
				return (-1 * ((long)(value >> 1) + 1));
			}

			return (long)(value >> 1);
		}

		/// <summary>
		/// Read using zig zag
		/// </summary>
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

		/// <summary>
		/// Read using zig zag
		/// </summary>
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

		private static byte[] ConvertToVarBinary(int value, out byte length)
		{
			if (value < 0)
			{
				length = 4;
				var buff = SharedArray.Get();
				buff[0] = (byte)value;
				buff[1] = unchecked((byte)(value >> 8));
				buff[2] = unchecked((byte)(value >> 16));
				buff[3] = unchecked((byte)(value >> 24));
				return buff;
			}
			else
			{
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
		}

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

	}
}
