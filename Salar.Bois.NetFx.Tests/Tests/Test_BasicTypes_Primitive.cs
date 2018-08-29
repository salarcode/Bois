using FluentAssertions;
using Salar.Bois.NetFx.Tests.Base;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Salar.Bois.NetFx.Tests.Tests
{
	public class Test_BasicTypes_Primitive : TestBase
	{
		[Fact]
		public void Numbers_Int_Normal()
		{
			ResetBois();
			int init = 250;

			Bois.Serialize(init, TestStream);
			ResetStream();

			var final = Bois.Deserialize<int>(TestStream);

			final.Should().Be(init);
		}

	}
}
