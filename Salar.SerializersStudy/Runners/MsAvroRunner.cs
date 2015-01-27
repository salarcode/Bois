using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Hadoop.Avro;

namespace Salar.SerializersStudy.Runners
{
	class MsAvroRunner
	{
		public static long GetPackedSize<T>(T obj)
		{
			try
			{
				var msgPack = AvroSerializer.Create<T>();

				using (var mem = new MemoryStream())
				{
					msgPack.Serialize(mem, obj);
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
