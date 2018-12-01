using Salar.Bois.NetFx.Tests.Base;
using Salar.Bois.NetFx.Tests.TestObjects;
using System.IO;
using System.Threading.Tasks;
using Xunit;
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

namespace Salar.Bois.NetFx.Tests.Tests
{
	public class Test_Multithreading : TestBase
	{
		[Fact]
		public void TestBoisTypeCache_MultiThreaded()
		{
			var data = TestObjectGeneralNumbers.GetArray(100);

			Parallel.For(0, 20, i =>
			{
				using (var mem = new MemoryStream())
				{
					var BoisSerializer = new BoisSerializer();
					BoisSerializer.Serialize(data, mem);

					mem.Seek(0, SeekOrigin.Begin);
					BoisSerializer.Deserialize<TestObjectGeneralNumbers>(mem);
				}
			});
		}
	}
}