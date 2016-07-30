using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SalarCompactSerializer.Tests.Data;

namespace SalarCompactSerializer.Tests
{
	[TestClass]
	public class CompactSerializerTests
	{
		[TestMethod]
		public void TestMethod1()
		{
			var obj = SampleObject1.CreateObject();
			var csc = new CompactSerializer();
			csc.SerializeNullValues = false;
			csc.OrdinalNotation = true;
			var str = csc.Serialize(obj);

			Assert.IsNotNull(str);
		}
	}
}
