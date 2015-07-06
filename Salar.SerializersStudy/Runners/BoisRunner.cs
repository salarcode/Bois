using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Salar.Bois;

namespace Salar.SerializersStudy.Runners
{
	class BoisRunner
	{
		public static long GetPackedSize<T>(T obj)
		{
			try
			{
				var msgPack = new BoisSerializer();

				using (var mem = new MemoryStream())
				{
					msgPack.Serialize(obj, mem);
					mem.Seek(0, SeekOrigin.Begin);

					var newObj = msgPack.Deserialize<T>(mem);

					if (typeof(T).IsValueType)
						if (!newObj.Equals(obj))
						{
							return -2;
						}
					return mem.Length;
				}
			}
			catch (Exception)
			{
				return -1;
			}
		}
	}
}
