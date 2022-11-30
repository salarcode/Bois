using System;
using System.Collections.Generic;

namespace Salar.Bois.NetFx.Tests.TestObjects
{
	public class TestObjectNullableProps
	{
		public byte? Range1 { get; set; }

		public DayOfWeek? Day { get; set; }

		public DateTimeKind? Kind { get; set; }

		public sbyte? Range2 { get; set; }

		public uint? Year1 { get; set; }

		public int? Year2 { get; set; }


		public static IEnumerable<object[]> GetTestData()
		{

			yield return new object[]
			{
				new TestObjectNullableProps
				{
					Day = DayOfWeek.Monday,
					Kind = DateTimeKind.Local,
					Year1 = 2021,
					Year2 = 2022,
					Range1 = 100,
					Range2 = 5
				}
			};
			yield return new object[]
			{
				new TestObjectNullableProps
				{
					// all null
				}
			};
		}
	}
}
