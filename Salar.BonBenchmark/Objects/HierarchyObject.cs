using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Salar.BoisBenchmark.Objects
{
	[Serializable()]
	[ProtoContract]
	public class HierarchyObject
	{
		public static HierarchyObject CreateObject()
		{
			return
				new HierarchyObject
					{
						Num = 290,
						items = new List<class1>()
							        {
								        new class1()
									        {
										        Code = "10",
										        Name = "Salar",
										        guid = Guid.NewGuid()
									        },
								        new class1()
									        {
										        Code = "12",
										        Name = "Khalilzadeh",
										        guid = Guid.Empty
									        },
								        new class1()
									        {
										        Code = "15",
										        Name = "Salar.BON",
										        description = "This is sample text",
										        guid = Guid.NewGuid()
									        },

							        },
						date = DateTime.Now,
						multilineString = @"
						Salar.Bon is a fast, light and powerful binary serializer for .NET Framework.
						Stack Overflow is a question and answer
						site for professional and enthusiast programmers.
						",
						guid = Guid.Empty,
						isNew = false,
						done = true,
					};
		}


		[ProtoMember(8)]
		public string title = "ok";

		[ProtoMember(9)]
		public string done2 = "done";

		[ProtoMember(1)]
		public bool done { get; set; }

		[ProtoMember(2)]
		public DateTime date { get; set; }

		[ProtoMember(3)]
		public string multilineString { get; set; }

		[ProtoMember(4)]
		public List<class1> items { get; set; }

		[ProtoMember(20)]
		public int Num { get; set; }

		[ProtoMember(5)]
		public Guid guid { get; set; }

		[ProtoMember(6)]
		public decimal? dec { get; set; }

		[ProtoMember(7)]
		public bool isNew { get; set; }

		[ProtoMember(99)]
		public string Text { get; set; }

	}

	[Serializable]
	[ProtoContract]
	public class class1
	{
		[ProtoMember(1)]
		public string Name { get; set; }

		[ProtoMember(2)]
		public string Code { get; set; }

		[ProtoMember(3)]
		public string description { get; set; }

		[ProtoMember(4)]
		public Guid guid { get; set; }
	}
}
