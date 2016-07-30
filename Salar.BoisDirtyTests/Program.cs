using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Salar.Bois;

namespace Salar.BoisDirtyTests
{
	class Program
	{
		static void Main(string[] args)
		{
			var b = new BoisSerializer();

			var obj = new BasicTypes1();
			obj.Initialize();

			byte length = 0;
			byte[] data;
			//data = PrimitivesConvertion.ConvertToVarBinary(0f, out length);
			//data = PrimitivesConvertion.ConvertToVarBinary(5f, out length);
			//data = PrimitivesConvertion.ConvertToVarBinary(15.0005f, out length);
			//data = PrimitivesConvertion.ConvertToVarBinary(1111111.4444444f, out length);
			//data = PrimitivesConvertion.ConvertToVarBinary(1231111.000000001f, out length);
			
			//data = PrimitivesConvertion.ConvertToVarBinary(0, out length);
			//data = PrimitivesConvertion.ConvertToVarBinary(5, out length);
			//data = PrimitivesConvertion.ConvertToVarBinary(15888, out length);

			using (var mem = new MemoryStream())
			{
				b.Serialize(obj, mem);

				mem.Seek(0, SeekOrigin.Begin);

				var final = b.Deserialize<BasicTypes1>(mem);
			}




			Console.WriteLine("press a key...");
			Console.ReadKey(true);
		}
	}



	class BasicTypes1
	{

		public float Float { get; set; }
		public double Double { get; set; }
		public int Int { get; set; }

		public void Initialize()
		{
			Float = 1.5f;
			Double = 123.12323;
			Int = 20;
		}
	}

}
