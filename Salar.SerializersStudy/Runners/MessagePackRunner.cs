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

		//private void MessagePackBenchmark<T>(T obj, int count)
		//{
		//	try
		//	{
		//		long initlength = 0;
		//		Stopwatch sw;
		//		//-----------------------------------
		//		var msgPack = MsgPack.Serialization.MessagePackSerializer.Create<T>();

		//		var mem = new MemoryStream();
		//		msgPack.Pack(mem, obj);
		//		initlength = mem.Length;

		//		mem.Seek(0, SeekOrigin.Begin);
		//		msgPack.Unpack(mem);


		//		using (var sharperMem = new MemoryStream())
		//		{
		//			sw = Stopwatch.StartNew();
		//			for (int i = 0; i < count; i++)
		//			{
		//				msgPack.Pack(sharperMem, obj);
		//			}
		//		}
		//		sw.Stop();
		//		Log("MessagePack Serialize		took: " + ToString(sw.Elapsed) + "  data-size: " + initlength);


		//		sw = Stopwatch.StartNew();
		//		for (int i = 0; i < count; i++)
		//		{
		//			mem.Seek(0, SeekOrigin.Begin);
		//			msgPack.Unpack(mem);
		//		}
		//		sw.Stop();
		//		Log("MessagePack Deserialize		took: " + ToString(sw.Elapsed));
		//	}
		//	catch (Exception ex)
		//	{
		//		Log("MessagePack failed, " + ex.Message);
		//	}
		//}
	}
}
