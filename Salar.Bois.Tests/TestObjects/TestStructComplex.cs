using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Drawing;

namespace Salar.Bois.NetFx.Tests.TestObjects
{
	public struct TestStructComplex
	{
		public int ID { get; set; }

		public Size? SizeNullF;
		private Size? _sizeNullProp;

		public Size? SizeNullProp
		{
			get => _sizeNullProp;
			set => _sizeNullProp = value;
		}

		public int[] IntArr { get; set; }

		public DataSet DataSet { get; set; }

		public DataTable DataTable { get; set; }

		public Collection<TestStructComplex> Collection { get; set; }

		public List<TestStructComplex> List { get; set; }

		public Dictionary<Size, TestStructComplex> Dictionary { get; set; }

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
									//SizeNullF = new Size(90, 10)
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
