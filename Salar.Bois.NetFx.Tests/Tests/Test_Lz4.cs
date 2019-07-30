using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Salar.Bois.LZ4;
using Salar.Bois.NetFx.Tests.Base;
using Salar.Bois.NetFx.Tests.TestObjects;
using Xunit;

namespace Salar.Bois.NetFx.Tests.Tests
{
	public class Test_Lz4 : TestBase
	{
		[Fact]
		public void Test_Lz4_NumberArrays()
		{
			var data = TestObjectGeneralNumbers.GetArray(100);

			using (var mem = new MemoryStream())
			{
				var boisLz4Serializer = new BoisLz4Serializer();
				boisLz4Serializer.Serialize(data, mem);

				mem.Seek(0, SeekOrigin.Begin);
				boisLz4Serializer.Deserialize<TestObjectGeneralNumbers>(mem);
			}
		}

	}
}
