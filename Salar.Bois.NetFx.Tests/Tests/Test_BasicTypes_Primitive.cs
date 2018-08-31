using FluentAssertions;
using Salar.Bois.NetFx.Tests.Base;
using Salar.Bois.Serializers;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Salar.Bois.NetFx.Tests.Tests
{
	public class Test_BasicTypes_Primitive : TestBase
	{
		[Theory]
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
		public void Numbers_Int_Normal(int number)
		{
			ResetBois();

			NumericSerializers.WriteVarInt(Writer, number);
			Bois.Serialize(number, TestStream);
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
		public void Numbers_UInt_Normal(uint number)
		{
			ResetBois();

			NumericSerializers.WriteVarInt(Writer, number);
			Bois.Serialize(number, TestStream);
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

	}
}
