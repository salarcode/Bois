using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SalarCompactSerializer.Tests.Data
{
	public class SampleObject2
	{

		public string Text { get; set; }
		public DateTime Dt { get; set; }
		public Color Clr { get; set; }
		public byte[] Data { get; set; }
		public string TextField;
		public string[] StrArray;

		public static SampleObject2 CreateObject()
		{
			return new SampleObject2()
					   {
						   TextField = "Hi",
						   Dt = DateTime.Now,
						   Text = "Hello ticks: " + DateTime.Now.Ticks.ToString(),
						   Clr = SystemColors.ActiveBorder,
						   Data = new byte[] { 66, 20, 30, 50, 90, 122, 50, 22, 0, 0, 0, 16, 19, 177 },
						   StrArray = new string[] { "T1", "Test2", "Something is not ok!", "Done" },
					   };
		}
	}
}
