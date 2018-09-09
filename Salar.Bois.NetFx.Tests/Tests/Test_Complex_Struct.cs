using System.IO;
using System.Text;
using Salar.Bois.NetFx.Tests.Base;
using Salar.Bois.NetFx.Tests.TestObjects;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Salar.Bois.NetFx.Tests.Tests
{
	public class Test_Complex_Struct : TestBase
	{
		[Theory]
		[MemberData(nameof(TestStructComplex.GetTestData), MemberType = typeof(TestStructComplex))]
		public void TestingStructComplex(TestStructComplex init)
		{
			ResetBois();

			BoisSerializer.Initialize<TestStructComplex>();

			Bois.Serialize(init, TestStream);
			ResetStream();

			var final = Bois.Deserialize<TestStructComplex>(TestStream);

			SerializeAreEqual(init, final);
		}

		[Theory]
		[MemberData(nameof(TestStructPrimitives.GetTestData), MemberType = typeof(TestStructPrimitives))]
		public void TestingStructSelfReferencing(TestStructPrimitives init)
		{
			ResetBois();

			Bois.Serialize(init, TestStream);
			ResetStream();

			var final = Bois.Deserialize<TestStructPrimitives>(TestStream);

			SerializeAreEqual(init, final);
		}


		[Theory]
		[MemberData(nameof(TestStructSimple.GetTestData), MemberType = typeof(TestStructSimple))]
		public void TestingStructStructSimple(TestStructSimple init)
		{
			ResetBois();

			Bois.Serialize(init, TestStream);
			ResetStream();

			var final = Bois.Deserialize<TestStructSimple>(TestStream);

			SerializeAreEqual(init, final);
		}



	}
}
