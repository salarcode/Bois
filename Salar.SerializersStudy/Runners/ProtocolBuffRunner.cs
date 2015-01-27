using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Salar.SerializersStudy.Runners
{
	class ProtocolBuffRunner
	{
		public static long GetPackedSize<T>(T obj)
		{
			try
			{
				using (var mem = new MemoryStream())
				{
					Serializer.Serialize(mem, obj);
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
