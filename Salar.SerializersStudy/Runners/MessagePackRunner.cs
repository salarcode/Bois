using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using MsgPack.Serialization;

namespace Salar.SerializersStudy.Runners
{
	class MessagePackRunner
	{
		public static long GetPackedSize<T>(T obj)
		{
			try
			{
				var msgPack = MessagePackSerializer.Get<T>();

				using (var mem = new MemoryStream())
				{
					msgPack.Pack(mem, obj);
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
