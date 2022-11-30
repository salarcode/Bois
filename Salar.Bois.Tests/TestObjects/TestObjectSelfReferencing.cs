using System;
using System.Collections.Generic;

namespace Salar.Bois.NetFx.Tests.TestObjects
{
	public class TestObjectSelfReferencing
	{
		public int ID { get; set; }

		public string Name { get; set; }

		public TestObjectSelfReferencing Self { get; set; }

		public static IEnumerable<object[]> GetTestData()
		{
			yield return new object[]
			{
				new TestObjectSelfReferencing
				{
					ID = 1,
					Name = "First",
					Self = new TestObjectSelfReferencing
					{
						ID = 2,
						Name = "Second",
						Self = new TestObjectSelfReferencing
						{
							ID = 3,
							Name = "Third",
							Self = null
						}
					}
				}
			};
		}

	}
}
