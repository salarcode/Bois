using CompactBinarySerializer.Demo.Samples;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Polenter.Serialization;
using ProtoBuf;
using Salar.Bion.Demo.Samples;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace CompactBinarySerializer.Demo
{
	public partial class frmTest : Form
	{
		public frmTest()
		{
			InitializeComponent();
		}

		void Log(string text)
		{
			txtData.Text = txtData.Text + text + "\r\n";
		}

		private void btnToBinary_Click(object sender, EventArgs e)
		{
			var obj = SampleObject2.CreateObject();


			var bin = new BonSerializer();
			using (var f = File.Create(txtFile.Text))
			{
				bin.Serialize(obj, f);
			}
			Log("Bin serialized.");
		}

		private void btnFromBinary_Click(object sender, EventArgs e)
		{
			var bin = new BonSerializer();
			using (var f = File.OpenRead(txtFile.Text))
			{
				var obj = bin.Deserialize<SampleObject2>(f);
				if (obj.Text == "Hi")
				{
					MessageBox.Show(obj.Text);
				}
			}
			Log("Bin deserialized.");
		}

		private void btn_Click(object sender, EventArgs e)
		{

		}

		private void btnBuff_Click(object sender, EventArgs e)
		{
			var obj = SampleObject1.CreateObject();
			var bin = new BonSerializer();
			using (var f = File.Create(txtFile.Text))
			{
				Serializer.Serialize(f, obj);
			}
			using (var f = File.OpenRead(txtFile.Text))
			{
				var de = Serializer.Deserialize<SampleObject1>(f);
				if (de != null)
				{

				}
			}
			Log("Bin buff serializer.");
		}

		private void btnBenchmark_Click(object sender, EventArgs e)
		{
			txtData.Text = "";
			BonCacheThem();

			if (!_netSerializer)
			{
				NetSerializer.Serializer.Initialize(new Type[] {typeof (BenchmarkObject1), typeof (BenchmarkObject2)});
				_netSerializer = true;
			}


			var count = 10000;
			var obj = new BenchmarkObject1();

			Log("BenchmarkObject1 Operation count: " + count);
			try
			{
				BonBenchmark(obj, count, 2);
			}
			catch (Exception ex)
			{
				Log("Bon failed, " + ex.Message);
			}
			try
			{
				ProtocolBufferBenchmark(obj, count);
			}
			catch (Exception ex)
			{
				Log("ProtocolBuffer failed, " + ex.Message);
			}
			try
			{
				NetSerializerBenchmark(obj, count);
			}
			catch (Exception ex)
			{
				Log("NetSerializer failed, " + ex.Message);
			}
			try
			{
				SharpSerializerBenchmark(obj, count);
			}
			catch (Exception ex)
			{
				Log("SharpSerializer failed, " + ex.Message);
			}
			try
			{
				BsonBenchmark(obj, count);
			}
			catch (Exception ex)
			{
				Log("Bson failed, " + ex.Message);
			}
			try
			{
				FastJsonBenchmark(obj, count);
			}
			catch (Exception ex)
			{
				Log("FastJson failed, " + ex.Message);
			}
			try
			{
				JsonNetBenchmark(obj, count);
			}
			catch (Exception ex)
			{
				Log("Json.Net failed, " + ex.Message);
			}

			//-------------------------------------
			var obj2 = new BenchmarkObject2();
			Log("");
			Log("BenchmarkObject2 count: " + count);

			try
			{
				BonBenchmark(obj2, count, 2);
			}
			catch (Exception ex)
			{
				Log("Bon failed, " + ex.Message);
			}
			try
			{
				ProtocolBufferBenchmark(obj2, count);
			}
			catch (Exception ex)
			{
				Log("ProtocolBuffer failed, " + ex.Message);
			}
			try
			{
				NetSerializerBenchmark(obj2, count);
			}
			catch (Exception ex)
			{
				Log("NetSerializer failed, " + ex.Message);
			}
			try
			{
				SharpSerializerBenchmark(obj2, count);
			}
			catch (Exception ex)
			{
				Log("SharpSerializer failed, " + ex.Message);
			}
			try
			{
				BsonBenchmark(obj2, count);
			}
			catch (Exception ex)
			{
				Log("Bson failed, " + ex.Message);
			}
			try
			{
				FastJsonBenchmark(obj2, count);
			}
			catch (Exception ex)
			{
				Log("FastJson failed, " + ex.Message);
			}
			try
			{
				JsonNetBenchmark(obj2, count);
			}
			catch (Exception ex)
			{
				Log("Json.Net failed, " + ex.Message);
			}

		}

		private void FastJsonBenchmark<T>(T obj, int count)
		{
			Stopwatch sw;
			//-----------------------------------

			var json = fastJSON.JSON.Instance;
			json.Parameters.SerializeNullValues = false;
			json.Parameters.UsingGlobalTypes = false;
			json.Parameters.EnableAnonymousTypes = true;
			json.Parameters.UseExtensions = false;

			var jsonString = json.ToJSON(obj);


			sw = Stopwatch.StartNew();
			for (int i = 0; i < count; i++)
			{
				json.ToJSON(obj);
			}
			sw.Stop();
			Log("fastJSON ToJSON			took: " + ToString(sw.Elapsed) + "  data-size: " + jsonString.Length);

			sw = Stopwatch.StartNew();
			for (int i = 0; i < count; i++)
			{
				json.ToObject<T>(jsonString);
			}
			sw.Stop();
			Log("fastJSON ToObject			took: " + ToString(sw.Elapsed));
		}

		private void BonBenchmark<T>(T obj, int count, int which)
		{
			Stopwatch sw;
			//-----------------------------------
			var bonSerializer = new BonSerializer();
			var bonMem = new MemoryStream();

			bonSerializer.Serialize(obj, bonMem);
			bonMem.Seek(0, SeekOrigin.Begin);
			bonSerializer.Deserialize<T>(bonMem);

			if (which != 0)
			{
				using (var mem = new MemoryStream())
				{
					sw = Stopwatch.StartNew();
					for (int i = 0; i < count; i++)
					{
						bonSerializer.Serialize(obj, mem);
						mem.SetLength(0);
					}
				}
				sw.Stop();
				Log("bonSerializer.Serialize		took: " + ToString(sw.Elapsed) + "  data-size: " + bonMem.Length);
			}

			if (which != 1)
			{
				sw = Stopwatch.StartNew();
				for (int i = 0; i < count; i++)
				{
					bonMem.Seek(0, SeekOrigin.Begin);
					bonSerializer.Deserialize<T>(bonMem);
				}
				sw.Stop();
				Log("bonDeserializer.Deserialize		took: " + ToString(sw.Elapsed));
			}

		}

		private void BsonBenchmark<T>(T obj, int count)
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
			Log("BSON bson.Serialize			took: " + ToString(sw.Elapsed) + "  data-size: " + mainMem.Length);

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
		private void JsonNetBenchmark<T>(T obj, int count)
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

			//jsonNet = new JsonSerializer();
			//jsonNet.NullValueHandling = NullValueHandling.Ignore;

			sw = Stopwatch.StartNew();
			for (int i = 0; i < count; i++)
			{
				bsonReader = new JsonTextReader(strReader);
				jsonNet.Deserialize<T>(bsonReader);
			}
			sw.Stop();
			Log("Json.NET Deserialize			took: " + ToString(sw.Elapsed));
		}

		private bool _netSerializer = false;
		private void NetSerializerBenchmark<T>(T obj, int count)
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

		private void SharpSerializerBenchmark<T>(T obj, int count)
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

		private void ProtocolBufferBenchmark<T>(T obj, int count)
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
			Log("PBuff Serializer.Serialize		took: " + ToString(sw.Elapsed) + "  data-size: " + pbuffMem.Length);


			sw = Stopwatch.StartNew();
			for (int i = 0; i < count; i++)
			{
				pbuffMem.Seek(0, SeekOrigin.Begin);
				Serializer.Deserialize<T>(pbuffMem);
			}
			sw.Stop();
			Log("PBuff Serializer.Deserialize		took: " + ToString(sw.Elapsed));
		}

		static string ToString(TimeSpan ts)
		{
			return string.Format(" {0} s\t=\t{1}s, {2} ms", ts.TotalMilliseconds / 1000, ts.Seconds, ts.Milliseconds);
		}

		private void btnClr_Click(object sender, EventArgs e)
		{
			txtData.Text = "";
		}

		private void btnBonBecnhmark_Click(object sender, EventArgs e)
		{
			BonCacheThem();

			var count = 2000;
			var obj1 = new BenchmarkObject1();
			var obj2 = new BenchmarkObject2();
			var obj3 = new SampleError1();
			var obj4 = SampleObject1.CreateObject();
			var obj5 = SampleObject2.CreateObject();
			var obj6 = SampleObject3.CreateObject();
			var obj7 = SampleObject4.CreateObject();
			var obj8 = new string[] { "hi,", "sdsdds", "000000" };

			Log("--------count " + count);
			BonBenchmark(obj1, count, 2);
			Log("--------BenchmarkObject1");
			BonBenchmark(obj2, count, 2);
			Log("--------BenchmarkObject2");
			BonBenchmark(obj3, count, 2);
			Log("--------SampleError1");
			BonBenchmark(obj4, count, 2);
			Log("--------SampleObject1");
			BonBenchmark(obj5, count, 2);
			Log("--------SampleObject2");
			BonBenchmark(obj6, count, 2);
			Log("--------SampleObject3");
			BonBenchmark(obj7, count, 2);
			Log("--------SampleObject4");
			BonBenchmark(obj8, count, 2);
			Log("--------String[]");
		}

		private void btnWriteBenchmark_Click(object sender, EventArgs e)
		{
			txtData.Text = "";

			var count = 10000;
			//var obj = SampleObject1.CreateObject();
			//var obj = new BenchmarkObject2();
			var obj = SampleObject4.CreateObject();

			BonBenchmark(obj, count, 1);
		}

		private void btnReadBenchmark_Click(object sender, EventArgs e)
		{
			txtData.Text = "";

			var count = 10000;
			//var obj = SampleObject1.CreateObject();
			var obj = SampleObject4.CreateObject();

			BonBenchmark(obj, count, 0);
		}

		public string test;
		public string Title { get; set; }

		private void btnBin_Click(object sender, EventArgs e)
		{
			using (var ff = File.Create(txtFile.Text))
			using (var b = new BinaryWriter(ff))
			{
				b.Write(0);
			}

			Process.Start(txtFile.Text);
		}

		private void btnCacheAll_Click(object sender, EventArgs e)
		{
			BonTypeCache.ClearCache();
			Stopwatch sw;
			sw = Stopwatch.StartNew();

			BonCacheThem();
			sw.Stop();
			Log("BonTypeCache			took: " + ToString(sw.Elapsed));
		}

		private static void BonCacheThem()
		{
			BonTypeCache.Initialize<BenchmarkObject1>();
			BonTypeCache.Initialize<BenchmarkObject2>();
			BonTypeCache.Initialize<SampleError1>();
			BonTypeCache.Initialize<SampleObject1>();
			BonTypeCache.Initialize<SampleObject2>();
			BonTypeCache.Initialize<SampleObject3>();
			BonTypeCache.Initialize<SampleObject4>();
			BonTypeCache.Initialize<int>();
			BonTypeCache.Initialize<string>();
			BonTypeCache.Initialize<double>();
			BonTypeCache.Initialize<DateTime>();

		}

		private void btnPrimitives_Click(object sender, EventArgs e)
		{
			//using (var mem = new MemoryStream())
			//using (var reader = new BinaryReader(mem))
			//using (var writer = new BinaryWriter(mem))
			//{
			//	PrimitivesConvertion.WriteVarInt(writer, (int)1);
			//	PrimitivesConvertion.WriteVarInt(writer, (int)-1);
			//	PrimitivesConvertion.WriteVarInt(writer, (int)0);
			//	PrimitivesConvertion.WriteVarInt(writer, (int)0);
			//	PrimitivesConvertion.WriteVarInt(writer, (int)-250);
			//	PrimitivesConvertion.WriteVarInt(writer, (short?)-399);
			//	PrimitivesConvertion.WriteVarInt(writer, (long?)-95);
			//	PrimitivesConvertion.WriteVarInt(writer, (short)-25);

			//	mem.Seek(0, SeekOrigin.Begin);

			//	var num1 = PrimitivesConvertion.ReadVarInt32(reader);
			//	num1 = PrimitivesConvertion.ReadVarInt32(reader);
			//	num1 = PrimitivesConvertion.ReadVarInt32(reader);
			//	num1 = PrimitivesConvertion.ReadVarInt32(reader);
			//	var num2 = PrimitivesConvertion.ReadVarInt16Nullable(reader);
			//	var num4 = PrimitivesConvertion.ReadVarInt64Nullable(reader);
			//	var num3 = PrimitivesConvertion.ReadVarInt16(reader);

			//	Text = (num1 + num2 + num3 + num4).ToString();

			//}

		}

		private void btnStruxt_Click(object sender, EventArgs e)
		{
			txtData.Text = "";
			BonCacheThem();

			var count = 10000;
			var obj = StructType1.InitializeThis();

			Log("Operation count: " + count);

			try
			{
					BonBenchmark(obj, count, 2);
			}
			catch (Exception ex)
			{
				Log("Bon failed, " + ex.Message);
			}
			try
			{
				ProtocolBufferBenchmark(obj, count);
			}
			catch (Exception ex)
			{
				Log("ProtocolBuffer failed, " + ex.Message);
			}
			try
			{
				NetSerializerBenchmark(obj, count);
			}
			catch (Exception ex)
			{
				Log("NetSerializer failed, " + ex.Message);
			}
			try
			{
				SharpSerializerBenchmark(obj, count);
			}
			catch (Exception ex)
			{
				Log("SharpSerializer failed, " + ex.Message);
			}
			try
			{
				BsonBenchmark(obj, count);
			}
			catch (Exception ex)
			{
				Log("Bson failed, " + ex.Message);
			}
			try
			{
				FastJsonBenchmark(obj, count);
			}
			catch (Exception ex)
			{
				Log("FastJson failed, " + ex.Message);
			}
			try
			{
				JsonNetBenchmark(obj, count);
			}
			catch (Exception ex)
			{
				Log("Json.Net failed, " + ex.Message);
			}
		}

		private void frmTest_Load(object sender, EventArgs e)
		{

		}
	}
}
