using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Salar.Bion.Demo.Samples
{
	public class SampleObject4
	{
		public string Text { get; set; }
		public Dictionary<int, string> DtList { get; set; }
		public string[] Names { get; set; }
		public Dictionary<string, double> Deaths { get; set; }

		public static SampleObject4 CreateObject()
		{
			var obj = new SampleObject4
			{
				Names = new string[] { "hi", "hello", "test" },
				Text = "The Text of this!"
			};
			obj.DtList = new Dictionary<int, string>()
				             {
					             {1, "index"},
					             {2, "hello"},
					             {4, "done!"}
				             };
			obj.Deaths = new Dictionary<string, double>()
				             {
					             {"hello", 9.22},
					             {"gone", 9.2}
				             };

			return obj;
		}
	}
}
