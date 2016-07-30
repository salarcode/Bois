using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Salar.Bion.Demo.Samples
{
	class SampleObject3
	{
		public string Text { get; set; }
		public string[] Names { get; set; }
		public DataTable DtList { get; set; }
		public DataSet Ds { get; set; }

		public static SampleObject3 CreateObject()
		{
			var obj = new SampleObject3
						  {
							  Names = new string[] { "hi", "hello", "test" },
							  Text = "The Text of this!"
						  };
			obj.DtList = new DataTable("The name!");
			obj.DtList.Columns.Add("C1", typeof(string));
			obj.DtList.Columns.Add("C2", typeof(decimal));
			obj.DtList.Columns.Add("CI", typeof(int));
			var row = obj.DtList.Rows.Add();
			row[0] = "I1";
			row[1] = 2.40;
			row[2] = 90;

			row = obj.DtList.Rows.Add();
			row[0] = DBNull.Value;
			row[1] = DBNull.Value;
			row[2] = 90;
			obj.Ds = new DataSet();
			obj.Ds.Tables.Add(obj.DtList);

			return obj;
		}
	}
}
