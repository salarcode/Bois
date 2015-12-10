using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Salar.Bion.Demo.Samples
{
	[Serializable()]
	[ProtoContract]
	public class baseclass
	{
		[ProtoMember(1)]
		public string Name { get; set; }

		[ProtoMember(2)]
		public string Code { get; set; }
	}

	[Serializable()]
	[ProtoContract]
	public class class1 : baseclass
	{
		[ProtoMember(1)]
		public Guid guid { get; set; }
	}

	[Serializable()]
	[ProtoContract]
	public class class2 : baseclass
	{
		[ProtoMember(1)]
		public string description { get; set; }
	}

	[Serializable()]
	[ProtoContract]
	public class BenchmarkObject2
	{
		public BenchmarkObject2()
		{
			items = new List<baseclass>()
				        {
					        new baseclass()
						        {
							        Code = "10",
							        Name = "Salar",
						        },
					        new baseclass()
						        {
							        Code = "12",
							        Name = "Khalilzadeh",
							        //guid = Guid.Empty
						        },
					        new baseclass()
						        {
							        Code = "15",
							        Name = "Salar.BON",
							       // description = "This is sample text",
						        }
				        };
			date = DateTime.Now;
			multilineString = @"
						Salar.Bon is a fast, light and powerful binary serializer for .NET Framework.
						Stack Overflow is a question and answer
						site for professional and enthusiast programmers.
						";
			guid = Guid.NewGuid();
			isNew = false;
			done = true;
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
		public List<baseclass> items { get; set; }

		[ProtoMember(5)]
		public Guid guid { get; set; }

		[ProtoMember(6)]
		public decimal? dec { get; set; }

		[ProtoMember(7)]
		public bool isNew { get; set; }

		[ProtoMember(99)]
		public string Text { get; set; }

	}
}
