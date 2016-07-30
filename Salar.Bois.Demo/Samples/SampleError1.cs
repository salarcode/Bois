using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Salar.Bion.Demo.Samples
{
	[Serializable()]
	[ProtoContract]
	public class SampleError1
	{
		public SampleError1()
		{
			Text = DateTime.Now.ToLongTimeString();
			ChildList = new List<SampleErrorChild1>()
					   {
						   new SampleErrorChild1()
							   {
								   Title ="CH_1_"+ DateTime.Now.Ticks.ToString()
							   },

						   new SampleErrorChild1()
							   {
								   Title ="CH_2_"+ DateTime.Now.Ticks.ToString()
							   }
					   };
			ChildArr = new SampleErrorChild1[]
				         {
						   new SampleErrorChild1()
							   {
								   Title ="CH_1_"+ DateTime.Now.Ticks.ToString()
							   },

						   new SampleErrorChild1()
							   {
								   Title ="CH_2_"+ DateTime.Now.Ticks.ToString()
							   }
				         };

			Data = new double[] { 9.3, 20.33, 40.20 };
			//ArrList = new ArrayList();
			//ArrList.Add("Item 1");
			//ArrList.Add(20.5);
			//ArrList.Add(new byte[] { 90, 233, 255, 226 });
		}

		//[ProtoMember(5)]
		//public ArrayList ArrList { get; set; }

		[ProtoMember(1)]
		public string Text { get; set; }

		[ProtoMember(2)]
		public List<SampleErrorChild1> ChildList { get; set; }

		[ProtoMember(3)]
		public SampleErrorChild1[] ChildArr { get; set; }

		[ProtoMember(4)]
		public double[] Data { get; set; }



		[Serializable()]
		[ProtoContract]
		public class SampleErrorChild1
		{
			[ProtoMember(1)]
			public string Title { get; set; }

			[NonSerialized]
			public uint Good;
		}
	}
}
