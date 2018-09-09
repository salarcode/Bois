using Salar.Bois.NetFx.Tests.Base;
using System;
using System.Collections.Generic;
using System.Drawing;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Salar.Bois.NetFx.Tests.Tests
{
	public class Test_BasicType_Arrays : TestBase
	{
		public static IEnumerable<object[]> GetArrayData()
		{
			yield return new object[] { new DayOfWeek[] { DayOfWeek.Friday, DayOfWeek.Thursday, DayOfWeek.Wednesday } };
			yield return new object[] { new int[] { 10, 20 } };
			yield return new object[] { new uint[] { 10, 20 } };
			yield return new object[] { new uint?[] { 10, null, 20 } };
			yield return new object[] { new float?[] { 10.9998f, null, 10.15f, 19.5f } };
			yield return new object[] { new Uri[] { new Uri("https://www.nuget.org/packages/Salar.Bois"), new Uri("https://github.com/salarcode/Bois"), } };
			yield return new object[] { new ConsoleKey[] { ConsoleKey.Backspace, ConsoleKey.BrowserStop } };
			yield return new object[] { new ConsoleKey?[] { null, null, null, ConsoleKey.BrowserStop } };
			yield return new object[] { new Size[] { new Size(10, 20), new Size(90, 30), } };
			yield return new object[] { new Guid?[] { Guid.Empty, Guid.NewGuid(), null, Guid.NewGuid() } };
		}

		[Theory]
		[MemberData(nameof(GetArrayData))]
		public void TestingArrays(Array init)
		{
			ResetBois();

			BoisSerializer.Initialize(init.GetType());


			var arrayType = init.GetType();

			Bois.Serialize(init, arrayType, TestStream);
			ResetStream();

			var final = Bois.Deserialize(TestStream, arrayType);

			SerializeAreEqual(init, final);
		}
	}
}
