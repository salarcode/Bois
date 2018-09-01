using FluentAssertions;
using Salar.Bois.NetFx.Tests.Base;
using Salar.Bois.NetFx.Tests.TestObjects;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Salar.Bois.NetFx.Tests.Tests
{
	public class Test_Complex_Collections : TestBase
	{

		[Fact]
		public void TestGenericCollection_Simple_int()
		{
			var init = new List<int>()
			{
				10,
				20,
				500
			};

			ResetBois();

			Bois.Initialize<List<int>>();

			Bois.Serialize(init, TestStream);
			ResetStream();

			var final = Bois.Deserialize<List<int>>(TestStream);

			init.Should().BeEquivalentTo(final);
		}

		[Theory]
		[MemberData(nameof(TestObjectCollectionsPrimitive.GetTestData), MemberType = typeof(TestObjectCollectionsPrimitive))]
		public void TestCollectionsPrimitive(TestObjectCollectionsPrimitive testObject)
		{
			ResetBois();

			Bois.Serialize(testObject, TestStream);
			ResetStream();

			var final = Bois.Deserialize<TestObjectCollectionsPrimitive>(TestStream);

			// ConcurrentBag's items are never in order, had to order them for this test to pass
			testObject.ConcurrentBag = new ConcurrentBag<int>(testObject.ConcurrentBag.OrderBy(c => c));
			testObject.ConcurrentBagField = new ConcurrentBag<int?>(testObject.ConcurrentBagField.OrderBy(c => c));
			final.ConcurrentBag = new ConcurrentBag<int>(final.ConcurrentBag.OrderBy(c => c));
			final.ConcurrentBagField = new ConcurrentBag<int?>(final.ConcurrentBagField.OrderBy(c => c));

			SerializeAreEqual(testObject, final);
		}
	}
}
