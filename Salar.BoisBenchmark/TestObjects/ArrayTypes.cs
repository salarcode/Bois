using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace Salar.BoisBenchmark.TestObjects
{
	[Serializable()]
	[ProtoContract]
	[DataContract]
	public class ArrayTypes
	{
		[ProtoMember(1)]
		[DataMember]
		public string[] Names { get; set; }

		[ProtoMember(2)]
		[DataMember]
		public int[] Ages { get; set; }

		[ProtoMember(3)]
		[DataMember]
		public float[] Prices1 { get; set; }

		[ProtoMember(4)]
		[DataMember]
		public double[] Prices2 { get; set; }


		public static ArrayTypes CreateSimple()
		{
			return
				new ArrayTypes
				{
					Prices1 = new float[] { 30, 27, 17, 70 },
					Prices2 = new double[] { 30, 27, 17, 70 },
					Ages = new int[] { 30, 27, 17, 70 },
					Names = new string[] { "Salar", "BOIS", "Codeplex" },
				};
		}

		public static ArrayTypes CreateBig()
		{
			var obj=
				new ArrayTypes
				{
					Prices1 = new float[] { float.MaxValue-1, 27, 17, float.MaxValue-1 },
					Prices2 = new double[] { 30, 27, 17, 70 },
					Ages = new int[] { 30, 27, 17, 70 },
					Names = new string[] { "Salar", "BOIS", "Codeplex" },
				};

			obj.Prices1 = new float[(short.MaxValue) ];
            for (int i = 0; i < short.MaxValue; i++)
            {
	            obj.Prices1[i] = i;
            }
			return obj;
		}

	}
}
