using FluentAssertions;
using Salar.Bois.NetFx.Tests.Base;
using Salar.Bois.Serializers;
using System.Collections.Generic;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Salar.Bois.NetFx.Tests.Tests
{
	public class Test_BasicTypes_Numbers : TestBase
	{
		public static IEnumerable<object[]> GetIntBytes()
		{
			int num = int.MaxValue;
			do
			{
				num = (int)(num / 8);
				yield return new object[] { num };
			} while (num > 0);

			num = int.MinValue;
			do
			{
				num = (int)(num / 8);
				yield return new object[] { num };
			} while (num < 0);
		}


		[Theory]
		[InlineData(0)]
		[InlineData(31), InlineData(-31)]
		[InlineData(32), InlineData(-32)]
		[InlineData(64), InlineData(-64)]
		[InlineData(65), InlineData(-65)]
		[InlineData(127), InlineData(-127)]
		[InlineData(128), InlineData(-128)]
		[InlineData(256), InlineData(-256)]
		[InlineData(short.MaxValue + 1)]
		[InlineData(ushort.MaxValue + 1)]
		[InlineData(int.MaxValue)]
		[InlineData(int.MinValue)]
		[MemberData(nameof(GetIntBytes))]
		public void Numbers_Int_Normal(int number)
		{
			ResetBois();

			NumericSerializers.WriteVarInt(Writer, number);
			ResetStream();

			var final = NumericSerializers.ReadVarInt32(Reader);

			final.Should().Be(number);
		}

		[Theory]
		[InlineData(null)]
		[InlineData(0)]
		[InlineData(31), InlineData(-31)]
		[InlineData(32), InlineData(-32)]
		[InlineData(64), InlineData(-64)]
		[InlineData(65), InlineData(-65)]
		[InlineData(127), InlineData(-127)]
		[InlineData(128), InlineData(-128)]
		[InlineData(256), InlineData(-256)]
		[InlineData(int.MaxValue)]
		[InlineData(int.MinValue)]
		public void Numbers_IntNullable_Normal(int? number)
		{
			ResetBois();

			NumericSerializers.WriteVarInt(Writer, number);
			ResetStream();

			var final = NumericSerializers.ReadVarInt32Nullable(Reader);

			final.Should().Be(number);
		}

		public static IEnumerable<object[]> GetUIntBytes()
		{
			uint num = uint.MaxValue;
			do
			{
				num = (uint)(num / 8);
				yield return new object[] { num };
			} while (num > 0);
		}

		[Theory]
		[InlineData((uint)0)]
		[InlineData((uint)31)]
		[InlineData((uint)32)]
		[InlineData((uint)64)]
		[InlineData((uint)65)]
		[InlineData((uint)127)]
		[InlineData((uint)128)]
		[InlineData((uint)256)]
		[InlineData(uint.MaxValue)]
		[InlineData(uint.MinValue)]
		[MemberData(nameof(GetUIntBytes))]
		public void Numbers_UInt_Normal(uint number)
		{
			ResetBois();

			NumericSerializers.WriteVarInt(Writer, number);
			ResetStream();

			var final = NumericSerializers.ReadVarUInt32(Reader);

			final.Should().Be(number);
		}

		[Theory]
		[InlineData(null)]
		[InlineData((uint)0)]
		[InlineData((uint)31)]
		[InlineData((uint)32)]
		[InlineData((uint)64)]
		[InlineData((uint)65)]
		[InlineData((uint)127)]
		[InlineData((uint)128)]
		[InlineData((uint)256)]
		[InlineData(uint.MaxValue)]
		[InlineData(uint.MinValue)]
		public void Numbers_UIntNullable_Normal(uint? number)
		{
			ResetBois();

			NumericSerializers.WriteVarInt(Writer, number);
			ResetStream();

			var final = NumericSerializers.ReadVarUInt32Nullable(Reader);

			final.Should().Be(number);
		}


		public static IEnumerable<object[]> GetSbyteBytes()
		{
			sbyte num = sbyte.MaxValue;
			do
			{
				num = (sbyte)(num / 8);
				yield return new object[] { num };
			} while (num > 0);

			num = sbyte.MinValue;
			do
			{
				num = (sbyte)(num / 8);
				yield return new object[] { num };
			} while (num < 0);
		}

		[Theory]
		[InlineData(null)]
		[InlineData((sbyte)0)]
		[InlineData((sbyte)31), InlineData((sbyte)-31)]
		[InlineData((sbyte)32), InlineData((sbyte)-32)]
		[InlineData((sbyte)64), InlineData((sbyte)-64)]
		[InlineData((sbyte)65), InlineData((sbyte)-65)]
		[InlineData((sbyte)127), InlineData((sbyte)-127)]
		[InlineData(sbyte.MaxValue)]
		[InlineData(sbyte.MinValue)]
		[MemberData(nameof(GetSbyteBytes))]
		public void Numbers_sbyteNullable_Normal(sbyte? number)
		{
			ResetBois();

			NumericSerializers.WriteVarInt(Writer, number);
			ResetStream();

			var final = NumericSerializers.ReadVarSByteNullable(Reader);

			final.Should().Be(number);
		}


		[Theory]
		[InlineData(null)]
		[InlineData((byte)0)]
		[InlineData((byte)31)]
		[InlineData((byte)32)]
		[InlineData((byte)64)]
		[InlineData((byte)65)]
		[InlineData((byte)127)]
		[InlineData((byte)128)]
		[InlineData((byte)255)]
		[InlineData(byte.MaxValue)]
		[InlineData(byte.MinValue)]
		public void Numbers_byteNullable_Normal(byte? number)
		{
			ResetBois();

			NumericSerializers.WriteVarInt(Writer, number);
			ResetStream();

			var final = NumericSerializers.ReadVarByteNullable(Reader);

			final.Should().Be(number);
		}

		public static IEnumerable<object[]> GetShortBytes()
		{
			var num = short.MaxValue;
			do
			{
				num = (short)(num / 8);
				yield return new object[] { num };
			} while (num > 0);

			num = short.MinValue;
			do
			{
				num = (short)(num / 8);
				yield return new object[] { num };
			} while (num < 0);
		}

		[Theory]
		[InlineData((short)0)]
		[InlineData((short)31), InlineData((short)-31)]
		[InlineData((short)32), InlineData((short)-32)]
		[InlineData((short)64), InlineData((short)-64)]
		[InlineData((short)65), InlineData((short)-65)]
		[InlineData((short)127), InlineData((short)-127)]
		[InlineData((short)128), InlineData((short)-128)]
		[InlineData((short)256), InlineData((short)-256)]
		[InlineData(short.MaxValue)]
		[InlineData(short.MinValue)]
		[MemberData(nameof(GetShortBytes))]
		public void Numbers_short_Normal(short number)
		{
			ResetBois();

			NumericSerializers.WriteVarInt(Writer, number);
			ResetStream();

			var final = NumericSerializers.ReadVarInt16(Reader);

			final.Should().Be(number);
		}

		[Theory]
		[InlineData(null)]
		[InlineData((short)0)]
		[InlineData((short)31), InlineData((short)-31)]
		[InlineData((short)32), InlineData((short)-32)]
		[InlineData((short)64), InlineData((short)-64)]
		[InlineData((short)65), InlineData((short)-65)]
		[InlineData((short)127), InlineData((short)-127)]
		[InlineData((short)128), InlineData((short)-128)]
		[InlineData((short)256), InlineData((short)-256)]
		[InlineData(short.MaxValue)]
		[InlineData(short.MinValue)]
		public void Numbers_shortNullable_Normal(short? number)
		{
			ResetBois();

			NumericSerializers.WriteVarInt(Writer, number);
			ResetStream();

			var final = NumericSerializers.ReadVarInt16Nullable(Reader);

			final.Should().Be(number);
		}


		public static IEnumerable<object[]> GetUshortBytes()
		{
			var num = ushort.MaxValue;
			do
			{
				num = (ushort)(num / 8);
				yield return new object[] { num };
			} while (num > 0);
		}

		[Theory]
		[InlineData((ushort)0)]
		[InlineData((ushort)31)]
		[InlineData((ushort)32)]
		[InlineData((ushort)64)]
		[InlineData((ushort)65)]
		[InlineData((ushort)127)]
		[InlineData((ushort)128)]
		[InlineData((ushort)256)]
		[InlineData(ushort.MaxValue)]
		[InlineData(ushort.MinValue)]
		[MemberData(nameof(GetUshortBytes))]
		public void Numbers_ushort_Normal(ushort number)
		{
			ResetBois();

			NumericSerializers.WriteVarInt(Writer, number);
			ResetStream();

			var final = NumericSerializers.ReadVarUInt16(Reader);

			final.Should().Be(number);
		}

		[Theory]
		[InlineData(null)]
		[InlineData((ushort)0)]
		[InlineData((ushort)31)]
		[InlineData((ushort)32)]
		[InlineData((ushort)64)]
		[InlineData((ushort)65)]
		[InlineData((ushort)127)]
		[InlineData((ushort)128)]
		[InlineData((ushort)256)]
		[InlineData(ushort.MaxValue)]
		[InlineData(ushort.MinValue)]
		public void Numbers_ushortNullable_Normal(ushort? number)
		{
			ResetBois();

			NumericSerializers.WriteVarInt(Writer, number);
			ResetStream();

			var final = NumericSerializers.ReadVarUInt16Nullable(Reader);

			final.Should().Be(number);
		}

		public static IEnumerable<object[]> GetLongBytes()
		{
			var num = long.MaxValue;
			do
			{
				num = (long)(num / 8);
				yield return new object[] { num };
			} while (num > 0);

			num = short.MinValue;
			do
			{
				num = (long)(num / 8);
				yield return new object[] { num };
			} while (num < 0);
		}


		[Theory]
		[InlineData((long)0)]
		[InlineData((long)31), InlineData((long)-31)]
		[InlineData((long)32), InlineData((long)-32)]
		[InlineData((long)64), InlineData((long)-64)]
		[InlineData((long)65), InlineData((long)-65)]
		[InlineData((long)127), InlineData((long)-127)]
		[InlineData((long)128), InlineData((long)-128)]
		[InlineData((long)256), InlineData((long)-256)]
		[InlineData(short.MaxValue + 1)]
		[InlineData(ushort.MinValue + 1)]
		[InlineData((long)int.MaxValue + 1)]
		[InlineData((long)uint.MaxValue + 1)]
		[InlineData(long.MaxValue)]
		[InlineData(long.MinValue)]
		[MemberData(nameof(GetLongBytes))]
		public void Numbers_long_Normal(long number)
		{
			ResetBois();

			NumericSerializers.WriteVarInt(Writer, number);
			ResetStream();

			var final = NumericSerializers.ReadVarInt64(Reader);

			final.Should().Be(number);
		}

		[Theory]
		[InlineData(null)]
		[InlineData((long)0)]
		[InlineData((long)31), InlineData((long)-31)]
		[InlineData((long)32), InlineData((long)-32)]
		[InlineData((long)64), InlineData((long)-64)]
		[InlineData((long)65), InlineData((long)-65)]
		[InlineData((long)127), InlineData((long)-127)]
		[InlineData((long)128), InlineData((long)-128)]
		[InlineData((long)256), InlineData((long)-256)]
		[InlineData(long.MaxValue)]
		[InlineData(long.MinValue)]
		public void Numbers_longNullable_Normal(long? number)
		{
			ResetBois();

			NumericSerializers.WriteVarInt(Writer, number);
			ResetStream();

			var final = NumericSerializers.ReadVarInt64Nullable(Reader);

			final.Should().Be(number);
		}


		public static IEnumerable<object[]> GetULongBytes()
		{
			var num = ulong.MaxValue;
			do
			{
				num = (ulong)(num / 8);
				yield return new object[] { num };
			} while (num > 0);
		}

		[Theory]
		[InlineData((ulong)0)]
		[InlineData((ulong)31)]
		[InlineData((ulong)32)]
		[InlineData((ulong)64)]
		[InlineData((ulong)65)]
		[InlineData((ulong)127)]
		[InlineData((ulong)128)]
		[InlineData((ulong)256)]
		[InlineData(short.MaxValue + 1)]
		[InlineData(ushort.MinValue + 1)]
		[InlineData((long)int.MaxValue + 1)]
		[InlineData((long)uint.MaxValue + 1)]
		[InlineData(ulong.MaxValue)]
		[InlineData(ulong.MinValue)]
		[MemberData(nameof(GetULongBytes))]
		public void Numbers_ulong_Normal(ulong number)
		{
			ResetBois();

			NumericSerializers.WriteVarInt(Writer, number);
			ResetStream();

			var final = NumericSerializers.ReadVarUInt64(Reader);

			final.Should().Be(number);
		}

		[Theory]
		[InlineData(null)]
		[InlineData((ulong)0)]
		[InlineData((ulong)31)]
		[InlineData((ulong)32)]
		[InlineData((ulong)64)]
		[InlineData((ulong)65)]
		[InlineData((ulong)127)]
		[InlineData((ulong)128)]
		[InlineData((ulong)256)]
		[InlineData(ulong.MaxValue)]
		[InlineData(ulong.MinValue)]
		public void Numbers_ulongNullable_Normal(ulong? number)
		{
			ResetBois();

			NumericSerializers.WriteVarInt(Writer, number);
			ResetStream();

			var final = NumericSerializers.ReadVarUInt64Nullable(Reader);

			final.Should().Be(number);
		}



		public static IEnumerable<object[]> GetFloatBytes()
		{
			var num = float.MaxValue;
			do
			{
				num = (float)(num / 8);
				yield return new object[] { num };
			} while (num > 0);

			num = float.MinValue;
			do
			{
				num = (float)(num / 8);
				yield return new object[] { num };
			} while (num < 0);
		}

		[Theory]
		[InlineData((float)0)]
		[InlineData((float)31), InlineData((float)-31)]
		[InlineData((float)32), InlineData((float)-32)]
		[InlineData((float)64), InlineData((float)-64)]
		[InlineData((float)65), InlineData((float)-65)]
		[InlineData((float)127), InlineData((float)-127)]
		[InlineData((float)128), InlineData((float)-128)]
		[InlineData((float)256), InlineData((float)-256)]
		[InlineData((float)0.66), InlineData((float)0.44)]
		[InlineData((float)31.44), InlineData((float)-31.66)]
		[InlineData((float)32.44), InlineData((float)-32.66)]
		[InlineData((float)64.44), InlineData((float)-64.66)]
		[InlineData((float)65.44), InlineData((float)-65.66)]
		[InlineData((float)127.44), InlineData((float)-127.66)]
		[InlineData((float)128.44), InlineData((float)-128.66)]
		[InlineData((float)256.44), InlineData((float)-256.66)]
		[InlineData(float.MaxValue)]
		[InlineData(float.MinValue)]
		[MemberData(nameof(GetFloatBytes))]
		public void Numbers_float_Normal(float number)
		{
			ResetBois();

			NumericSerializers.WriteVarDecimal(Writer, number);
			ResetStream();

			var final = NumericSerializers.ReadVarSingle(Reader);

			final.Should().Be(number);
		}


		[Theory]
		[InlineData((float)0)]
		[InlineData((float)31), InlineData((float)-31)]
		[InlineData((float)32), InlineData((float)-32)]
		[InlineData((float)64), InlineData((float)-64)]
		[InlineData((float)65), InlineData((float)-65)]
		[InlineData((float)127), InlineData((float)-127)]
		[InlineData((float)128), InlineData((float)-128)]
		[InlineData((float)256), InlineData((float)-256)]
		[InlineData((float)0.66), InlineData((float)0.44)]
		[InlineData((float)31.44), InlineData((float)-31.66)]
		[InlineData((float)32.44), InlineData((float)-32.66)]
		[InlineData((float)64.44), InlineData((float)-64.66)]
		[InlineData((float)65.44), InlineData((float)-65.66)]
		[InlineData((float)127.44), InlineData((float)-127.66)]
		[InlineData((float)128.44), InlineData((float)-128.66)]
		[InlineData((float)256.44), InlineData((float)-256.66)]
		[InlineData(float.MaxValue)]
		[InlineData(float.MinValue)]
		public void Numbers_floatNullable_Normal(float number)
		{
			ResetBois();

			NumericSerializers.WriteVarDecimal(Writer, number);
			ResetStream();

			var final = NumericSerializers.ReadVarSingleNullable(Reader);

			final.Should().Be(number);
		}


		public static IEnumerable<object[]> GetDoubleBytes()
		{
			var num = double.MaxValue;
			do
			{
				num = (double)(num / 8);
				yield return new object[] { num };
			} while (num > 0);

			num = float.MinValue;
			do
			{
				num = (double)(num / 8);
				yield return new object[] { num };
			} while (num < 0);
		}

		[Theory]
		[InlineData((double)0)]
		[InlineData((double)31), InlineData((double)-31)]
		[InlineData((double)32), InlineData((double)-32)]
		[InlineData((double)64), InlineData((double)-64)]
		[InlineData((double)65), InlineData((double)-65)]
		[InlineData((double)127), InlineData((double)-127)]
		[InlineData((double)128), InlineData((double)-128)]
		[InlineData((double)256), InlineData((double)-256)]
		[InlineData((double)0.66), InlineData((double)0.44)]
		[InlineData((double)31.44), InlineData((double)-31.66)]
		[InlineData((double)32.44), InlineData((double)-32.66)]
		[InlineData((double)64.44), InlineData((double)-64.66)]
		[InlineData((double)65.44), InlineData((double)-65.66)]
		[InlineData((double)127.44), InlineData((double)-127.66)]
		[InlineData((double)128.44), InlineData((double)-128.66)]
		[InlineData((double)256.44), InlineData((double)-256.66)]
		[InlineData(double.MaxValue)]
		[InlineData(double.MinValue)]
		[MemberData(nameof(GetDoubleBytes))]
		public void Numbers_double_Normal(double number)
		{
			ResetBois();

			NumericSerializers.WriteVarDecimal(Writer, number);
			ResetStream();

			var final = NumericSerializers.ReadVarDouble(Reader);

			final.Should().Be(number);
		}


		[Theory]
		[InlineData((double)0)]
		[InlineData((double)31), InlineData((double)-31)]
		[InlineData((double)32), InlineData((double)-32)]
		[InlineData((double)64), InlineData((double)-64)]
		[InlineData((double)65), InlineData((double)-65)]
		[InlineData((double)127), InlineData((double)-127)]
		[InlineData((double)128), InlineData((double)-128)]
		[InlineData((double)256), InlineData((double)-256)]
		[InlineData((double)0.66), InlineData((double)0.44)]
		[InlineData((double)31.44), InlineData((double)-31.66)]
		[InlineData((double)32.44), InlineData((double)-32.66)]
		[InlineData((double)64.44), InlineData((double)-64.66)]
		[InlineData((double)65.44), InlineData((double)-65.66)]
		[InlineData((double)127.44), InlineData((double)-127.66)]
		[InlineData((double)128.44), InlineData((double)-128.66)]
		[InlineData((double)256.44), InlineData((double)-256.66)]
		[InlineData(double.MaxValue)]
		[InlineData(double.MinValue)]
		public void Numbers_doubleNullable_Normal(double number)
		{
			ResetBois();

			NumericSerializers.WriteVarDecimal(Writer, number);
			ResetStream();

			var final = NumericSerializers.ReadVarDoubleNullable(Reader);

			final.Should().Be(number);
		}

		public static IEnumerable<object[]> GetDecimalData(bool withNull)
		{
			yield return new object[] { decimal.MinValue };
			yield return new object[] { decimal.MaxValue };
			if (withNull)
				yield return new object[] { null };
			yield return new object[] { 0m };
			yield return new object[] { 31m };
			yield return new object[] { 32m };
			yield return new object[] { 64m };
			yield return new object[] { 65m };
			yield return new object[] { 127m };
			yield return new object[] { 128m };
			yield return new object[] { 256m };

			yield return new object[] { -0m };
			yield return new object[] { -31m };
			yield return new object[] { -32m };
			yield return new object[] { -64m };
			yield return new object[] { -65m };
			yield return new object[] { -127m };
			yield return new object[] { -128m };
			yield return new object[] { -256m };

			yield return new object[] { 0.44m };
			yield return new object[] { 31.44m };
			yield return new object[] { 32.44m };
			yield return new object[] { 64.44m };
			yield return new object[] { 65.44m };
			yield return new object[] { 127.44m };
			yield return new object[] { 128.44m };
			yield return new object[] { 256.44m };

			yield return new object[] { 0.66m };
			yield return new object[] { 31.66m };
			yield return new object[] { 32.66m };
			yield return new object[] { 64.66m };
			yield return new object[] { 65.66m };
			yield return new object[] { 127.66m };
			yield return new object[] { 128.66m };
			yield return new object[] { 256.66m };

			yield return new object[] { -0.44m };
			yield return new object[] { -31.44m };
			yield return new object[] { -32.44m };
			yield return new object[] { -64.44m };
			yield return new object[] { -65.44m };
			yield return new object[] { -127.44m };
			yield return new object[] { -128.44m };
			yield return new object[] { -256.44m };

			yield return new object[] { -0.66m };
			yield return new object[] { -31.66m };
			yield return new object[] { -32.66m };
			yield return new object[] { -64.66m };
			yield return new object[] { -65.66m };
			yield return new object[] { -127.66m };
			yield return new object[] { -128.66m };
			yield return new object[] { -256.66m };


			var num = decimal.MaxValue;
			do
			{
				num = (decimal)(num / 8);
				yield return new object[] { num };
			} while (num > 0);

			num = decimal.MinValue;
			do
			{
				num = (decimal)(num / 8);
				yield return new object[] { num };
			} while (num < 0);

		}

		[Theory]
		[MemberData(nameof(GetDecimalData), parameters: new object[] { false })]
		public void Numbers_decimal_Normal(decimal number)
		{
			ResetBois();

			NumericSerializers.WriteVarDecimal(Writer, number);
			ResetStream();

			var final = NumericSerializers.ReadVarDecimal(Reader);

			final.Should().Be(number);
		}


		[Theory]
		[MemberData(nameof(GetDecimalData), parameters: new object[] { true })]
		public void Numbers_decimalNullable_Normal(decimal? number)
		{
			ResetBois();

			NumericSerializers.WriteVarDecimal(Writer, number);
			ResetStream();

			var final = NumericSerializers.ReadVarDecimalNullable(Reader);

			final.Should().Be(number);
		}

	}
}
