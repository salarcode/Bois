using Salar.Bois.NetFx.Tests.Base;
using Salar.Bois.NetFx.Tests.TestObjects;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Salar.Bois.NetFx.Tests.Tests
{
	public class Test_Complex_Objects : TestBase
	{

		[Theory]
		[MemberData(nameof(TestObjectPrimitiveTypes.GetTestData), MemberType = typeof(TestObjectPrimitiveTypes))]
		public void TestingObjectPrimitiveTypes(TestObjectPrimitiveTypes init)
		{
			ResetBois();

			Bois.Serialize(init, TestStream);
			ResetStream();

			var final = Bois.Deserialize<TestObjectPrimitiveTypes>(TestStream);

			SerializeAreEqual(init, final);
		}

		[Theory]
		[MemberData(nameof(TestObjectPrimitiveTypes.GetTestData), MemberType = typeof(TestObjectPrimitiveTypes))]
		public void TestingObjectPrimitiveTypesTyped(TestObjectPrimitiveTypes init)
		{
			ResetBois();

			Bois.Serialize(init, TestStream);
			ResetStream();

			var final = Bois.Deserialize(TestStream, init.GetType());

			SerializeAreEqual(init, final);
		}

		[Theory]
		[MemberData(nameof(TestObjectSelfReferencing.GetTestData), MemberType = typeof(TestObjectSelfReferencing))]
		public void TestingObjectSelfReferencing(TestObjectSelfReferencing init)
		{
			ResetBois();

			Bois.Serialize(init, TestStream);
			ResetStream();

			var final = Bois.Deserialize<TestObjectSelfReferencing>(TestStream);

			SerializeAreEqual(init, final);
		}

		[Theory]
		[MemberData(nameof(TestObjectNullableProps.GetTestData), MemberType = typeof(TestObjectNullableProps))]
		public void TestingTestObjectNullableProps(TestObjectNullableProps init)
		{
			ResetBois();

			Bois.Serialize(init, TestStream);
			ResetStream();

			var final = Bois.Deserialize<TestObjectNullableProps>(TestStream);

			SerializeAreEqual(init, final);
		}
	}
}
