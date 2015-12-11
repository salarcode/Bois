using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Polenter.Serialization;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Salar.BoisBenchmark
{
	public partial class frmTest : Form
	{
		public frmTest()
		{
			InitializeComponent();
			_benchmark.OnLog += Log;
		}

		private BenchmarkRun _benchmark = new BenchmarkRun();

		public static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.Run(new frmTest());
		}

		private void btnClear_Click(object sender, EventArgs e)
		{
			ClearLog();
		}

		private void ClearLog()
		{
			txtLog.Text = "";
		}

		private void frmTest_Load(object sender, EventArgs e)
		{

		}

		private void frmTest_Shown(object sender, EventArgs e)
		{
			RunTheTests(1);
		}
		private void btnBenchmark_Click(object sender, EventArgs e)
		{
			RunTheTests(5000);
		}


		void RunTheTests(int count)
		{
			ClearLog();

			_benchmark.Run(count);
		}

		
		private void Log(string text)
		{
			txtLog.Text = txtLog.Text + text + "\r\n";
		}


		static string ToString(TimeSpan ts)
		{
			return string.Format(" {0} s\t=\t{1}s, {2} ms", ts.TotalMilliseconds / 1000, ts.Seconds, ts.Milliseconds);
		}

		//private void Initialize()
		//{
		//	if (!_netSerializer)
		//	{
		//		try
		//		{
		//			NetSerializer.Serializer.Initialize(
		//				new Type[]
		//					{
		//						//typeof (CommonListChildObject),
		//						//typeof (BasicTypes),
		//						//typeof (HierarchyObject),
		//						////typeof (SpecialCollections),
		//						//typeof (Collections),
		//					});
		//			_netSerializer = true;
		//		}
		//		catch (Exception ex)
		//		{
		//			Log("Failed to initialize NetSerializer: " + ex.Message);
		//		}
		//	}
		//}
 
		private bool _netSerializer = false;
		//private void NetSerializerBenchmark<T>(T obj, int count)
		//{
		//	try
		//	{

		//		Stopwatch sw;
		//		//-----------------------------------

		//		var pbuffMem = new MemoryStream();
		//		if (!_netSerializer)
		//		{
		//			NetSerializer.Serializer.Initialize(new Type[] { typeof(T) });
		//			_netSerializer = true;
		//		}
		//		NetSerializer.Serializer.Serialize(pbuffMem, obj);

		//		using (var mem = new MemoryStream())
		//		{
		//			sw = Stopwatch.StartNew();
		//			for (int i = 0; i < count; i++)
		//			{
		//				NetSerializer.Serializer.Serialize(mem, obj);
		//				mem.SetLength(0);
		//			}
		//		}
		//		sw.Stop();
		//		Log("NetSerializer.Serialize		took: " + ToString(sw.Elapsed) + "  data-size: " + pbuffMem.Length);


		//		sw = Stopwatch.StartNew();
		//		for (int i = 0; i < count; i++)
		//		{
		//			pbuffMem.Seek(0, SeekOrigin.Begin);
		//			NetSerializer.Serializer.Deserialize(pbuffMem);
		//		}
		//		sw.Stop();
		//		Log("NetSerializer.Deserialize		took: " + ToString(sw.Elapsed));
		//	}
		//	catch (Exception ex)
		//	{
		//		Log("NetSerializer failed, " + ex.Message);
		//	}
		//}


		private void SharpSerializerBenchmark<T>(T obj, int count)
		{
			try
			{

				long initlength = 0;
				Stopwatch sw;
				//-----------------------------------
				var sharper = new SharpSerializer(new SharpSerializerBinarySettings(BinarySerializationMode.SizeOptimized));

				var mem = new MemoryStream();
				sharper.Serialize(obj, mem);
				initlength = mem.Length;

				mem.Seek(0, SeekOrigin.Begin);
				sharper.Deserialize(mem);


				using (var sharperMem = new MemoryStream())
				{
					sw = Stopwatch.StartNew();
					for (int i = 0; i < count; i++)
					{
						sharper.Serialize(obj, sharperMem);
					}
				}
				sw.Stop();
				Log("SharpSerializer Serialize		took: " + ToString(sw.Elapsed) + "  data-size: " + initlength);


				sw = Stopwatch.StartNew();
				for (int i = 0; i < count; i++)
				{
					mem.Seek(0, SeekOrigin.Begin);
					sharper.Deserialize(mem);
				}
				sw.Stop();
				Log("SharpSerializer Deserialize		took: " + ToString(sw.Elapsed));
			}
			catch (Exception ex)
			{
				Log("SharpSerializer failed, " + ex.Message);
			}
		}

 
		private void BsonBenchmark<T>(T obj, int count)
		{
			try
			{
				Stopwatch sw;
				//-----------------------------------
				var jsonNet = new JsonSerializer();
				jsonNet.NullValueHandling = NullValueHandling.Ignore;
				var mainMem = new MemoryStream();
				var bsonWriter = new BsonWriter(mainMem);
				jsonNet.Serialize(bsonWriter, obj);
				mainMem.Seek(0, SeekOrigin.Begin);

				var bsonReader = new BsonReader(mainMem);


				using (var tbsonMem = new MemoryStream())
				using (var tbsonWriter = new BsonWriter(tbsonMem))
				{
					sw = Stopwatch.StartNew();
					for (int i = 0; i < count; i++)
					{
						jsonNet.Serialize(tbsonWriter, obj);
						tbsonMem.SetLength(0);
					}
				}
				sw.Stop();
				Log("BSON bson.Serialize		took: " + ToString(sw.Elapsed) + "  data-size: " + mainMem.Length);

				sw = Stopwatch.StartNew();
				for (int i = 0; i < count; i++)
				{
					bsonReader = new BsonReader(mainMem);
					mainMem.Seek(0, SeekOrigin.Begin);
					jsonNet.Deserialize<T>(bsonReader);
				}
				sw.Stop();
				Log("BSON bson.Deserialize		took: " + ToString(sw.Elapsed));

			}
			catch (Exception ex)
			{
				Log("BSON failed, " + ex.Message);
			}
		}

		private void JsonNetBenchmark<T>(T obj, int count)
		{
			try
			{

				Stopwatch sw;
				//-----------------------------------
				var jsonNet = new JsonSerializer();
				jsonNet.NullValueHandling = NullValueHandling.Ignore;

				var strWriter = new StringWriter();
				var jsonWriter = new JsonTextWriter(strWriter);
				jsonNet.Serialize(jsonWriter, obj);
				var initJsonString = strWriter.ToString();

				var strReader = new StringReader(initJsonString);

				var bsonReader = new JsonTextReader(strReader);


				using (var tbsonMem = new StringWriter())
				using (var tbsonWriter = new JsonTextWriter(tbsonMem))
				{
					sw = Stopwatch.StartNew();
					for (int i = 0; i < count; i++)
					{
						jsonNet.Serialize(tbsonWriter, obj);
					}
				}
				sw.Stop();
				Log("Json.NET Serialize			took: " + ToString(sw.Elapsed) + "  data-size: " + initJsonString.Length);

				sw = Stopwatch.StartNew();
				for (int i = 0; i < count; i++)
				{
					bsonReader = new JsonTextReader(strReader);
					jsonNet.Deserialize<T>(bsonReader);
				}
				sw.Stop();
				Log("Json.NET Deserialize		took: " + ToString(sw.Elapsed));
			}
			catch (Exception ex)
			{
				Log("Json.Net failed, " + ex.Message);
			}
		}


		private void ServiceStackBechmark<T>(T obj, int count)
		{
			return;
			try
			{
				long initlength = 0;
				Stopwatch sw;
				//-----------------------------------
				var sstSerializer = new ServiceStack.Text.JsonSerializer<T>();


				var initSerialized = sstSerializer.SerializeToString(obj);
				initlength = initSerialized.Length;
				sstSerializer.DeserializeFromString(initSerialized);


				using (var sharperMem = new MemoryStream())
				{
					sw = Stopwatch.StartNew();
					for (int i = 0; i < count; i++)
					{
						sstSerializer.SerializeToString(obj);
					}
				}
				sw.Stop();
				Log("ServiceStackText Serialize		took: " + ToString(sw.Elapsed) + "  data-size: " + initlength);


				sw = Stopwatch.StartNew();
				for (int i = 0; i < count; i++)
				{
					sstSerializer.DeserializeFromString(initSerialized);
				}
				sw.Stop();
				Log("ServiceStackText Deserialize		took: " + ToString(sw.Elapsed));
			}
			catch (Exception ex)
			{
				Log("ServiceStackText failed, " + ex.Message);
			}
		}


 
	}
}
