using System;
using System.Collections.Generic;

namespace Salar.Bois.NetFx.Tests.TestObjects
{
	public class TestObjectGeneralNumbers
	{
		public long Long { get; set; }

		public double Double { get; set; }

		public string Name { get; set; }

		public static TestObjectGeneralNumbers[] GetArray(int size)
		{
			var result = new List<TestObjectGeneralNumbers>();
			for (int i = 0; i < size; i++)
			{
				result.Add(new TestObjectGeneralNumbers
				{
					Long = i,
					Double = Math.Log(i),
					Name = "Item-" + i
				});
			}

			return result.ToArray();
		}

	}
}
