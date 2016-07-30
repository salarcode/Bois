using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace CompactBinarySerializer.Demo.Samples
{
	[ProtoContract]
	[Serializable]
	class SampleObject1
	{
		[ProtoMember(1)]
		public string Text { get; set; }

		[ProtoMember(2)]
		public DateTime Dt { get; set; }

		//[ProtoMember(3)]
		//public Color Clr { get; set; }

		[ProtoMember(4)]
		public byte[] Data { get; set; }

		[ProtoMember(5)]
		public Language Lng { get; set; }

		[ProtoMember(6)]
		public string TextField;

		[ProtoMember(7)]
		public string[] StrArray;

		[ProtoMember(8)]
		public ChildOf1[] Childs { get; set; }

		[ProtoMember(3)]
		public NameValueCollection NameValue { get; set; }

		[ProtoMember(9)]
		public Dictionary<string, string> Dic { get; set; }

		//[ProtoMember(10)]
		//public DataTable Table { get; set; }

		public static SampleObject1 CreateObject()
		{
			var obj = new SampleObject1()
			{
				TextField = "Hi",
				Lng = Language.Csharp,
				Dt = DateTime.Now,
				Text = "Hello ticks: " + DateTime.Now.Ticks.ToString(),
				//Clr = SystemColors.ActiveBorder,
				Data = new byte[] { 66, 20, 30, 50, 90, 122, 50, 22, 0, 0, 0, 16, 19, 177 },
				StrArray = new string[] { "T1", "Test2", "Something is not ok!", "Done" },
				Dic = new Dictionary<string, string>()
					    {
						    {"ok-name","ok-val"},
							{"hey-n","hey-v"}
					    },
			};

			obj.NameValue = new NameValueCollection()
				                {
					                {"Item1", "Value1"},
					                {"Item4", null},
					                {"Item2", "2222"},
					                {null, "5555"},
					                {"Item3", "33333"},
				                };


			//var dt = obj.Table = new DataTable("tbl");
			//dt.Columns.Add("Test1", typeof(int));
			//dt.Columns.Add("Col2", typeof(string));
			//dt.Columns.Add("Col3", typeof(decimal));
			//var row = dt.Rows.Add();
			//row[0] = 1;
			//row[1] = "Hello";
			//row[2] = 10.5;

			//row = dt.Rows.Add();
			//row[0] = 55;
			//row[1] = "Sam";
			//row[2] = 1.22321;

			obj.Childs = new ChildOf1[]
				           {
					           new ChildOf1()
						           {
							           Title = "CerateDynamic(Type ArrayType,Int32 Length)",
									   Dates =   new DateTime[] { DateTime.Now, DateTime.Now.AddDays(-45) }
						           }, 
							   new ChildOf1()
								   {
							           Title = "Get Element?",
									   Dates = null
								   }, 
							   new ChildOf1()
								   {
							           Title = null,
									   Dates =   new DateTime[] { DateTime.Now.AddYears(-6), DateTime.Now.AddDays(-14) }
								   }, 
				           };
			return obj;
		}


		[ProtoContract]
		[Serializable]
		public class ChildOf1
		{
			[ProtoMember(1)]
			public string Title { get; set; }//= "wrote in message";
			[ProtoMember(2)]
			public DateTime[] Dates;//= new DateTime[] { DateTime.Now, DateTime.Now.AddDays(-45) };
		}

	}
}
