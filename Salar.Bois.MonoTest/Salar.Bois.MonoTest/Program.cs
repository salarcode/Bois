using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Salar.Bois.MonoTest
{
	class Program
	{
		[Serializable]
		public class ByteArray64K
		{
			public byte[] Arr { get; set; }
		}


		class MainClass
		{
			public static void Main(string[] args)
			{
				var ser = new BoisSerializer();

				var arr = new ByteArray64K() { Arr = new byte[65536] };
				byte[] data;

				using (MemoryStream ms = new MemoryStream())
				{
					ser.Serialize(arr, ms);
					data = ms.ToArray();
				}

				ByteArray64K des1;

				using (MemoryStream ms = new MemoryStream(data))
				{
					des1 = ser.Deserialize<ByteArray64K>(ms);
				}

				Console.WriteLine("Stream size: {2}, length1={0}, length2={1}", arr.Arr.Length, des1.Arr.Length, data.Length);
				Console.Read();
			}
		}
	}
}
