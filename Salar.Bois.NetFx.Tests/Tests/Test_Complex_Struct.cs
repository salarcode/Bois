using Salar.Bois.NetFx.Tests.Base;
using Salar.Bois.NetFx.Tests.TestObjects;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Salar.Bois.NetFx.Tests.Tests
{
	public class Test_Complex_Struct : TestBase
	{
		[Theory]
		[MemberData(nameof(TestStructPrimitives.GetTestData), MemberType = typeof(TestStructPrimitives))]
		public void TestingObjectSelfReferencing(TestStructPrimitives init)
		{
			ResetBois();

			Bois.Serialize(init, TestStream);
			ResetStream();

			var final = Bois.Deserialize<TestStructPrimitives>(TestStream);

			SerializeAreEqual(init, final);
		}
	}
}
