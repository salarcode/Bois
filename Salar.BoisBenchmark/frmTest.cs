using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Polenter.Serialization;
using ProtoBuf;
using Salar.Bois;
using Salar.Bois.Tests.Objects;
using Salar.BoisBenchmark.Objects;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using HelloWorldApp.BusinessObjects;

namespace Salar.BoisBenchmark
{
	public partial class frmTest : Form
	{
		public frmTest()
		{
			InitializeComponent();
		}

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
			Initialize();

			Log("");
			Log("BasicTypes benchmark---------- repeat count: " + count);
			var benchobj1 = BasicTypes.CreateObject();

			BoisBenchmark(benchobj1, count, 2);

			ProtoBufNetBenchmark(benchobj1, count);

			NetSerializerBenchmark(benchobj1, count);

			SharpSerializerBenchmark(benchobj1, count);

			BinaryFormatterBenchmark(benchobj1, count);

			BsonBenchmark(benchobj1, count);

			JsonNetBenchmark(benchobj1, count);

			ServiceStackBechmark(benchobj1, count);

			MsAvroPackBenchmark(benchobj1, count);

			MessagePackBenchmark(benchobj1, count);

			Log("");
			Log("RootContainer benchmark---------- repeat count: " + count);
			var rootContainer = RootContainer.CreateContainerArray(100);

			BoisBenchmark(rootContainer, count, 2);

			ProtoBufNetBenchmark(rootContainer, count);

			NetSerializerBenchmark(rootContainer, count);

			SharpSerializerBenchmark(rootContainer, count);

			BinaryFormatterBenchmark(rootContainer, count);

			BsonBenchmark(rootContainer, count);

			JsonNetBenchmark(rootContainer, count);

			ServiceStackBechmark(rootContainer, count);

			MsAvroPackBenchmark(rootContainer, count);

			MessagePackBenchmark(rootContainer, count);


			Log("");
			Log("Contained Collection benchmark---------- repeat count: " + count);
			var inheritanceObj = CommonListChildObject.CreateObject();

			BoisBenchmark(inheritanceObj, count, 2);

			ProtoBufNetBenchmark(inheritanceObj, count);

			NetSerializerBenchmark(inheritanceObj, count);

			SharpSerializerBenchmark(inheritanceObj, count);

			BinaryFormatterBenchmark(inheritanceObj, count);

			BsonBenchmark(inheritanceObj, count);

			JsonNetBenchmark(inheritanceObj, count);

			ServiceStackBechmark(inheritanceObj, count);

			MsAvroPackBenchmark(inheritanceObj, count);

			MessagePackBenchmark(inheritanceObj, count);

			Log("");
			Log("HierarchyObject benchmark---------- repeat count: " + count);

			var benchobj2 = HierarchyObject.CreateObject();
			BoisBenchmark(benchobj2, count, 2);

			ProtoBufNetBenchmark(benchobj2, count);

			NetSerializerBenchmark(benchobj2, count);

			SharpSerializerBenchmark(benchobj2, count);

			BinaryFormatterBenchmark(benchobj2, count);

			BsonBenchmark(benchobj2, count);

			JsonNetBenchmark(benchobj2, count);

			ServiceStackBechmark(benchobj2, count);

			MsAvroPackBenchmark(benchobj2, count);

			MessagePackBenchmark(benchobj2, count);

			Log("");
			Log("Collections benchmark---------- repeat count: " + count);

			var benchobj3 = Collections.CreateObject();
			BoisBenchmark(benchobj3, count, 2);

			ProtoBufNetBenchmark(benchobj3, count);

			NetSerializerBenchmark(benchobj3, count);

			SharpSerializerBenchmark(benchobj3, count);

			BinaryFormatterBenchmark(benchobj3, count);

			BsonBenchmark(benchobj3, count);

			JsonNetBenchmark(benchobj3, count);

			ServiceStackBechmark(benchobj3, count);

			MsAvroPackBenchmark(benchobj3, count);

			//MessagePackBenchmark(benchobj3, count);

			Log("");
			Log("SpecialCollections benchmark---------- repeat count: " + count);

			var benchobj4 = SpecialCollections.CreateObject();
			BoisBenchmark(benchobj4, count, 2);

			ProtoBufNetBenchmark(benchobj4, count);

			NetSerializerBenchmark(benchobj4, count);

			SharpSerializerBenchmark(benchobj4, count);

			BinaryFormatterBenchmark(benchobj4, count);

			BsonBenchmark(benchobj4, count);

			JsonNetBenchmark(benchobj4, count);

			ServiceStackBechmark(benchobj4, count);

			MsAvroPackBenchmark(benchobj4, count);

			MessagePackBenchmark(benchobj4, count);
		}



		private void Log(string text)
		{
			txtLog.Text = txtLog.Text + text + "\r\n";
		}


		static string ToString(TimeSpan ts)
		{
			return string.Format(" {0} s\t=\t{1}s, {2} ms", ts.TotalMilliseconds / 1000, ts.Seconds, ts.Milliseconds);
		}

		private void Initialize()
		{
			if (!_netSerializer)
			{
				try
				{
					NetSerializer.Serializer.Initialize(
						new Type[]
							{
								typeof (CommonListChildObject),
								typeof (BasicTypes),
								typeof (HierarchyObject),
								//typeof (SpecialCollections),
								typeof (Collections),
							});
					_netSerializer = true;
				}
				catch (Exception ex)
				{
					Log("Failed to initialize NetSerializer: " + ex.Message);
				}
			}
		}


		private void BoisBenchmark<T>(T obj, int count, int which)
		{
			try
			{
				Stopwatch sw;
				//-----------------------------------
				var boisSerializer = new BoisSerializer();
				boisSerializer.Initialize<BasicTypes>();
				boisSerializer.Initialize<HierarchyObject>();
				boisSerializer.Initialize<CommonListChildObject>();
				//boisSerializer.Initialize<RootContainer>();


				var boisMem = new MemoryStream();

				boisSerializer.Serialize(obj, boisMem);
				boisMem.Seek(0, SeekOrigin.Begin);
				boisSerializer.Deserialize<T>(boisMem);

				if (which != 0)
				{
					using (var mem = new MemoryStream())
					{
						sw = Stopwatch.StartNew();
						for (int i = 0; i < count; i++)
						{
							boisSerializer.Serialize(obj, mem);
							mem.SetLength(0);
						}
					}
					sw.Stop();
					Log("BoisSerializer.Serialize		took: " + ToString(sw.Elapsed) + "  data-size: " + boisMem.Length);
				}

				if (which != 1)
				{
					sw = Stopwatch.StartNew();
					for (int i = 0; i < count; i++)
					{
						boisMem.Seek(0, SeekOrigin.Begin);
						boisSerializer.Deserialize<T>(boisMem);
					}
					sw.Stop();
					Log("BoisDeserializer.Deserialize		took: " + ToString(sw.Elapsed));
				}
			}
			catch (Exception ex)
			{
				Log("Bois failed, " + ex.Message);
			}
		}

		private void ProtoBufNetBenchmark<T>(T obj, int count)
		{
			try
			{
				Stopwatch sw;
				//-----------------------------------
				var pbuffMem = new MemoryStream();
				Serializer.Serialize(pbuffMem, obj);

				using (var mem = new MemoryStream())
				{
					sw = Stopwatch.StartNew();
					for (int i = 0; i < count; i++)
					{
						Serializer.Serialize(mem, obj);
						mem.SetLength(0);
					}
				}
				sw.Stop();
				Log("protobuf-net Serialize		took: " + ToString(sw.Elapsed) + "  data-size: " + pbuffMem.Length);


				sw = Stopwatch.StartNew();
				for (int i = 0; i < count; i++)
				{
					pbuffMem.Seek(0, SeekOrigin.Begin);
					Serializer.Deserialize<T>(pbuffMem);
				}
				sw.Stop();
				Log("protobuf-net Deserialize		took: " + ToString(sw.Elapsed));
			}
			catch (Exception ex)
			{
				Log("ProtocolBuffer failed, " + ex.Message);
			}
		}

		private bool _netSerializer = false;
		private void NetSerializerBenchmark<T>(T obj, int count)
		{
			try
			{

				Stopwatch sw;
				//-----------------------------------

				var pbuffMem = new MemoryStream();
				if (!_netSerializer)
				{
					NetSerializer.Serializer.Initialize(new Type[] { typeof(T) });
					_netSerializer = true;
				}
				NetSerializer.Serializer.Serialize(pbuffMem, obj);

				using (var mem = new MemoryStream())
				{
					sw = Stopwatch.StartNew();
					for (int i = 0; i < count; i++)
					{
						NetSerializer.Serializer.Serialize(mem, obj);
						mem.SetLength(0);
					}
				}
				sw.Stop();
				Log("NetSerializer.Serialize		took: " + ToString(sw.Elapsed) + "  data-size: " + pbuffMem.Length);


				sw = Stopwatch.StartNew();
				for (int i = 0; i < count; i++)
				{
					pbuffMem.Seek(0, SeekOrigin.Begin);
					NetSerializer.Serializer.Deserialize(pbuffMem);
				}
				sw.Stop();
				Log("NetSerializer.Deserialize		took: " + ToString(sw.Elapsed));
			}
			catch (Exception ex)
			{
				Log("NetSerializer failed, " + ex.Message);
			}
		}


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

		private void BinaryFormatterBenchmark<T>(T obj, int count)
		{
			try
			{
				long initlength = 0;
				Stopwatch sw;
				//-----------------------------------
				var binf = new BinaryFormatter();

				var mem = new MemoryStream();
				binf.Serialize(mem, obj);
				initlength = mem.Length;

				mem.Seek(0, SeekOrigin.Begin);
				binf.Deserialize(mem);


				using (var sharperMem = new MemoryStream())
				{
					sw = Stopwatch.StartNew();
					for (int i = 0; i < count; i++)
					{
						binf.Serialize(sharperMem, obj);
					}
				}
				sw.Stop();
				Log("BinaryFormatter Serialize		took: " + ToString(sw.Elapsed) + "  data-size: " + initlength);


				sw = Stopwatch.StartNew();
				for (int i = 0; i < count; i++)
				{
					mem.Seek(0, SeekOrigin.Begin);
					binf.Deserialize(mem);
				}
				sw.Stop();
				Log("BinaryFormatter Deserialize		took: " + ToString(sw.Elapsed));
			}
			catch (Exception ex)
			{
				Log("BinaryFormatter failed, " + ex.Message);
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


		private void MsAvroPackBenchmark<T>(T obj, int count)
		{
			try
			{
				long initlength = 0;
				Stopwatch sw;
				//-----------------------------------
				var avro = Microsoft.Hadoop.Avro.AvroSerializer.Create<T>();

				var mem = new MemoryStream();
				avro.Serialize(mem, obj);
				initlength = mem.Length;

				mem.Seek(0, SeekOrigin.Begin);
				avro.Deserialize(mem);


				using (var sharperMem = new MemoryStream())
				{
					sw = Stopwatch.StartNew();
					for (int i = 0; i < count; i++)
					{
						avro.Serialize(sharperMem, obj);
					}
				}
				sw.Stop();
				Log("Microsoft.Avro Serialize		took: " + ToString(sw.Elapsed) + "  data-size: " + initlength);


				sw = Stopwatch.StartNew();
				for (int i = 0; i < count; i++)
				{
					mem.Seek(0, SeekOrigin.Begin);
					avro.Deserialize(mem);
				}
				sw.Stop();
				Log("Microsoft.Avro Deserialize		took: " + ToString(sw.Elapsed));
			}
			catch (Exception ex)
			{
				Log("Microsoft.Avro failed, " + ex.Message);
			}
		}

		private void MessagePackBenchmark<T>(T obj, int count)
		{
			try
			{
				long initlength = 0;
				Stopwatch sw;
				//-----------------------------------
				var msgPack = MsgPack.Serialization.MessagePackSerializer.Create<T>();

				var mem = new MemoryStream();
				msgPack.Pack(mem, obj);
				initlength = mem.Length;

				mem.Seek(0, SeekOrigin.Begin);
				msgPack.Unpack(mem);


				using (var sharperMem = new MemoryStream())
				{
					sw = Stopwatch.StartNew();
					for (int i = 0; i < count; i++)
					{
						msgPack.Pack(sharperMem, obj);
					}
				}
				sw.Stop();
				Log("MessagePack Serialize		took: " + ToString(sw.Elapsed) + "  data-size: " + initlength);


				sw = Stopwatch.StartNew();
				for (int i = 0; i < count; i++)
				{
					mem.Seek(0, SeekOrigin.Begin);
					msgPack.Unpack(mem);
				}
				sw.Stop();
				Log("MessagePack Deserialize		took: " + ToString(sw.Elapsed));
			}
			catch (Exception ex)
			{
				Log("MessagePack failed, " + ex.Message);
			}
		}


	}
}
