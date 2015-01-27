using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ProtoBuf;

namespace Salar.BoisBenchmark.Objects
{
	[Serializable()]
	[ProtoContract]
	[DataContract]
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
										        Name = "Salar.Bois",
										        description = "This is sample text",
										        guid = Guid.NewGuid()
									        },

							        },
						date = DateTime.Now,
						multilineString = @"
						Salar.Bois is a fast, light and powerful binary serializer for .NET Framework.
						Stack Overflow is a question and answer
						site for professional and enthusiast programmers.
						",
						guid = Guid.Empty,
						isNew = false,
						done = true,
					};
		}


		[ProtoMember(8)]
		[DataMember]
		public string title = "ok";

		[ProtoMember(9)]
		[DataMember]
		public string done2 = "done";

		[ProtoMember(1)]
		[DataMember]
		public bool done { get; set; }

		[ProtoMember(2)]
		[DataMember]
		public DateTime date { get; set; }

		[ProtoMember(3)]
		[DataMember]
		public string multilineString { get; set; }

		[ProtoMember(4)]
		[DataMember]
		public List<class1> items { get; set; }

		[ProtoMember(20)]
		[DataMember]
		public int Num { get; set; }

		[ProtoMember(5)]
		[DataMember]
		public Guid guid { get; set; }

		[ProtoMember(6)]
		[DataMember]
		public decimal? dec { get; set; }

		[ProtoMember(7)]
		[DataMember]
		public bool isNew { get; set; }

		[ProtoMember(99)]
		[DataMember]
		public string Text { get; set; }

	}

	[Serializable]
	[ProtoContract]
	[DataContract]
	public class class1
	{
		[ProtoMember(1)]
		[DataMember]
		public string Name { get; set; }

		[ProtoMember(2)]
		[DataMember]
		public string Code { get; set; }

		[ProtoMember(3)]
		[DataMember]
		public string description { get; set; }

		[ProtoMember(4)]
		[DataMember]
		public Guid guid { get; set; }
	}
}
