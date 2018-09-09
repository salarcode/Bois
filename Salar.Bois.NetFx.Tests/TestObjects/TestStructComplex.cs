using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salar.Bois.NetFx.Tests.TestObjects
{
	public struct TestStructComplex
	{
		public int ID { get; set; }

		public Size? SizeNullF;

		public Collection<TestStructComplex> Collection { get; set; }

		public List<TestStructComplex> List { get; set; }

		public Dictionary<Size,TestStructComplex> Dictionary { get; set; }

		public static IEnumerable<object[]> GetTestData()
		{
			yield return new object[]
			{
				new TestStructComplex
				{
					ID = short.MaxValue,
					List = new List<TestStructComplex>()
					{
						new TestStructComplex()
						{
							ID = byte.MaxValue,
							Collection = new Collection<TestStructComplex>()
							{
								new TestStructComplex()
								{
									ID = ushort.MaxValue,
									SizeNullF = new Size(90, 10)
								}
							},
							Dictionary = new Dictionary<Size, TestStructComplex>()
							{
								{
									new Size(10, 10), new TestStructComplex()
									{
										ID = 90
									}
								}
							}
						}
					},
					SizeNullF = new Size(10, 222)
				}
			};
		}
	}

}
