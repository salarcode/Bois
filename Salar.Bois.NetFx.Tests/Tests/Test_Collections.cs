using Salar.Bois.NetFx.Tests.Base;
using Salar.Bois.NetFx.Tests.TestObjects;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit;
using Assert = Xunit.Assert;

// ReSharper disable InconsistentNaming

namespace Salar.Bois.NetFx.Tests.Tests
{
	public class Test_Collections : TestBase
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

			CollectionAssert.AreEqual(init, final);
		}
		public static List<int> Computed_List_Reader_1829665514(BinaryReader reader, Encoding encoding)
		{
			int? num = Salar.Bois.Serializers.NumericSerializers.ReadVarInt32Nullable(reader);
			if (num.HasValue)
			{
				int value = num.Value;
				List<int> list = new List<int>();
				for (int i = 0; i < value; i++)
				{
					list.Add(Salar.Bois.Serializers.NumericSerializers.ReadVarInt32(reader));
				}
				return list;
			}
			return null;
		}


		[Theory]
		[ClassData(typeof(TestObjectCollectionsPrimitive))]
		public void TestObjectCollectionsPrimitive(TestObjectCollectionsPrimitive testObject)
		{
			ResetBois();

			Bois.Serialize(testObject, TestStream);
			ResetStream();

			var final = Bois.Deserialize<TestObjectCollectionsPrimitive>(TestStream);

			//final.Should().AllBeEquivalentTo(testObject);
			Assert.Equal(testObject, final);
		}

	}
}
