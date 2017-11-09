using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Salar.Bois.Tests.Objects
{
	public struct StructType1 : IBaseType
	{
		public string Text { get; set; }
		public string Text2 { get; set; }
		public string Text3 { get; set; }
		public char AcceptChar { get; set; }
		public DateTime TestDate { get; set; }
		public Color ForeColor { get; set; }


		public void Initialize()
		{

		}

		public static StructType1 InitializeThis()
		{
			return new StructType1()
						   {
							   Text = "Well, hello!",
							   Text2 = "This is Salar.Bois",
							   Text3 = null,
							   AcceptChar = 'c',
							   TestDate = DateTime.Now.AddDays(7),
							   ForeColor = Color.MidnightBlue,
						   };
		}
	}
}
