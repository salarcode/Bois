using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Salar.Bois;
using Salar.BoisBenchmark.TestObjects;

namespace Salar.BoisBenchmark
{
	public class BenchmarkRun
	{
		BoisSerializer _boisSerializer = new BoisSerializer();
		BinaryFormatter _binf = new BinaryFormatter();

		public Action<string> OnLog;

		public void Initialize<T>()
		{
			_boisSerializer.Initialize<T>();
		}

		public void Run(int count)
		{
			var bigCount = count/50;
			if (bigCount == 0)
				bigCount = 1;

			Log("");
			Log("ComplexContainer benchmark---------- repeat count: " + count);
			var objComplexContainer = ComplexContainer.CreateContainerArray(10);

			RunBenchmarkBois(objComplexContainer, count);
			RunBenchmarkMsAvroPack(objComplexContainer, count);
			RunBenchmarkMessagePack(objComplexContainer, count);
			RunBenchmarkProtoBufNet(objComplexContainer, count);
			RunBenchmarkBinaryFormatter(objComplexContainer, count);
			RunBenchmarkNetSerializer(objComplexContainer, count);

			Log("");
			Log("PrimitiveTypes.Simple benchmark---------- repeat count: " + count);
			var objPrimitiveTypes = PrimitiveTypes.CreateSimple();

			RunBenchmarkBois(objPrimitiveTypes, count);
			RunBenchmarkMsAvroPack(objPrimitiveTypes, count);
			RunBenchmarkMessagePack(objPrimitiveTypes, count);
			RunBenchmarkProtoBufNet(objPrimitiveTypes, count);
			RunBenchmarkBinaryFormatter(objPrimitiveTypes, count);
			RunBenchmarkNetSerializer(objPrimitiveTypes, count);

			Log("");
			Log("PrimitiveTypes.Big benchmark---------- repeat count: " + bigCount);
			objPrimitiveTypes = PrimitiveTypes.CreateBig();

			RunBenchmarkBois(objPrimitiveTypes, bigCount);
			RunBenchmarkMsAvroPack(objPrimitiveTypes, bigCount);
			RunBenchmarkMessagePack(objPrimitiveTypes, bigCount);
			RunBenchmarkProtoBufNet(objPrimitiveTypes, bigCount);
			RunBenchmarkBinaryFormatter(objPrimitiveTypes, bigCount);
			RunBenchmarkNetSerializer(objPrimitiveTypes, bigCount);

			Log("");
			Log("ArrayTypes.Simple benchmark---------- repeat count: " + count);
			var objArrayTypes = ArrayTypes.CreateSimple();

			RunBenchmarkBois(objArrayTypes, count);
			RunBenchmarkMsAvroPack(objArrayTypes, count);
			RunBenchmarkMessagePack(objArrayTypes, count);
			RunBenchmarkProtoBufNet(objArrayTypes, count);
			RunBenchmarkBinaryFormatter(objArrayTypes, count);
			RunBenchmarkNetSerializer(objArrayTypes, count);

			Log("");
			Log("ArrayTypes.Big benchmark---------- repeat count: " + bigCount);
			objArrayTypes = ArrayTypes.CreateBig();

			RunBenchmarkBois(objArrayTypes, bigCount);
			RunBenchmarkMsAvroPack(objArrayTypes, bigCount);
			RunBenchmarkMessagePack(objArrayTypes, bigCount);
			RunBenchmarkProtoBufNet(objArrayTypes, bigCount);
			RunBenchmarkBinaryFormatter(objArrayTypes, bigCount);
			RunBenchmarkNetSerializer(objArrayTypes, bigCount);

			Log("");
			Log("SimpleCollections.Simple benchmark---------- repeat count: " + count);
			var objStandardCollectionTypes = SimpleCollections.CreateSimple();

			RunBenchmarkBois(objStandardCollectionTypes, count);
			RunBenchmarkMsAvroPack(objStandardCollectionTypes, count);
			RunBenchmarkMessagePack(objStandardCollectionTypes, count);
			RunBenchmarkProtoBufNet(objStandardCollectionTypes, count);
			RunBenchmarkBinaryFormatter(objStandardCollectionTypes, count);
			RunBenchmarkNetSerializer(objStandardCollectionTypes, count);

			Log("");
			Log("SimpleCollections.Big benchmark---------- repeat count: " + bigCount);
			objStandardCollectionTypes = SimpleCollections.CreateBig();

			RunBenchmarkBois(objStandardCollectionTypes, bigCount);
			RunBenchmarkMsAvroPack(objStandardCollectionTypes, bigCount);
			RunBenchmarkMessagePack(objStandardCollectionTypes, bigCount);
			RunBenchmarkProtoBufNet(objStandardCollectionTypes, bigCount);
			RunBenchmarkBinaryFormatter(objStandardCollectionTypes, bigCount);
			RunBenchmarkNetSerializer(objStandardCollectionTypes, bigCount);

			Log("");
			Log("ComplexCollections.Simple benchmark---------- repeat count: " + count);
			var objComplexCollections = ComplexCollections.CreateSimple();

			RunBenchmarkBois(objComplexCollections, count);
			RunBenchmarkMsAvroPack(objComplexCollections, count);
			RunBenchmarkMessagePack(objComplexCollections, count);
			RunBenchmarkProtoBufNet(objComplexCollections, count);
			RunBenchmarkBinaryFormatter(objComplexCollections, count);
			RunBenchmarkNetSerializer(objComplexCollections, count);

			Log("");
			Log("ComplexCollections.Big benchmark---------- repeat count: " + bigCount);
			var objComplexCollections2 = ComplexCollections.CreateBig();

			RunBenchmarkBois(objComplexCollections2, bigCount);
			RunBenchmarkMsAvroPack(objComplexCollections2, bigCount);
			RunBenchmarkMessagePack(objComplexCollections2, bigCount);
			RunBenchmarkProtoBufNet(objComplexCollections2, bigCount);
			RunBenchmarkBinaryFormatter(objComplexCollections2, bigCount);
			RunBenchmarkNetSerializer(objComplexCollections2, bigCount);

			Log("");
			Log("SpecializedCollections.Simple benchmark---------- repeat count: " + count);
			var objSpecializedCollections = SpecializedCollections.CreateSimple();

			RunBenchmarkBois(objSpecializedCollections, count);
			RunBenchmarkMsAvroPack(objSpecializedCollections, count);
			RunBenchmarkMessagePack(objSpecializedCollections, count);
			RunBenchmarkProtoBufNet(objSpecializedCollections, count);
			RunBenchmarkBinaryFormatter(objSpecializedCollections, count);
			RunBenchmarkNetSerializer(objSpecializedCollections, count);

			Log("");
			Log("Contained Collection benchmark---------- repeat count: " + count);
			var objContainedListChild = ContainedListChild.CreateSimple();

			RunBenchmarkBois(objContainedListChild, count);
			RunBenchmarkMsAvroPack(objContainedListChild, count);
			RunBenchmarkMessagePack(objContainedListChild, count);
			RunBenchmarkProtoBufNet(objContainedListChild, count);
			RunBenchmarkBinaryFormatter(objContainedListChild, count);
			RunBenchmarkNetSerializer(objContainedListChild, count);
		}

		private void Log(string text)
		{
			if (OnLog != null)
				OnLog(text);
		}
		static string ToString(TimeSpan ts)
		{
			return $" {ts.TotalMilliseconds / 1000} s\t=\t{ts.Seconds}s, {ts.Milliseconds} ms";
		}

		#region Benchmarks

		private void RunBenchmarkBois<T>(T obj, int count, bool serialize = true, bool deserialize = true)
		{
			try
			{
				Stopwatch sw;
				//-----------------------------------

				var boisMem = new MemoryStream();

				
				try
				{
					_boisSerializer.Serialize(obj, boisMem);
					boisMem.Seek(0, SeekOrigin.Begin);
					_boisSerializer.Deserialize<T>(boisMem);
				}
				catch {}

				if (serialize)
				{
					using (var mem = new MemoryStream())
					{
						sw = Stopwatch.StartNew();
						for (int i = 0; i < count; i++)
						{
							_boisSerializer.Serialize(obj, mem);
							mem.Position = 0;
						}
						sw.Stop();
					}
					Log("BoisSerializer.Serialize		took: " + ToString(sw.Elapsed) + "  data-size: " + boisMem.Length);
				}

				if (deserialize)
				{
					sw = Stopwatch.StartNew();
					for (int i = 0; i < count; i++)
					{
						boisMem.Position = 0;
						_boisSerializer.Deserialize<T>(boisMem);
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

		private void RunBenchmarkMsAvroPack<T>(T obj, int count)
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
						sharperMem.Position = 0;
					}
					sw.Stop();
				}
				Log("Microsoft.Avro Serialize		took: " + ToString(sw.Elapsed) + "  data-size: " + initlength);


				sw = Stopwatch.StartNew();
				for (int i = 0; i < count; i++)
				{
					mem.Position = 0;
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

		private void RunBenchmarkMessagePack<T>(T obj, int count)
		{
			try
			{
				long initlength = 0;
				Stopwatch sw;
				//-----------------------------------
				var msgPack = MsgPack.Serialization.MessagePackSerializer.Get<T>();

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
						sharperMem.Position = 0;
					}
				}
				sw.Stop();
				Log("MessagePack Serialize		took: " + ToString(sw.Elapsed) + "  data-size: " + initlength);


				sw = Stopwatch.StartNew();
				for (int i = 0; i < count; i++)
				{
					mem.Position = 0;
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

		private void RunBenchmarkProtoBufNet<T>(T obj, int count)
		{
			try
			{
				Stopwatch sw;
				//-----------------------------------
				var pbuffMem = new MemoryStream();
				ProtoBuf.Serializer.Serialize(pbuffMem, obj);

				using (var mem = new MemoryStream())
				{
					sw = Stopwatch.StartNew();
					for (int i = 0; i < count; i++)
					{
						ProtoBuf.Serializer.Serialize(mem, obj);
						mem.Position = 0;
					}
				}
				sw.Stop();
				Log("protobuf-net Serialize		took: " + ToString(sw.Elapsed) + "  data-size: " + pbuffMem.Length);


				sw = Stopwatch.StartNew();
				for (int i = 0; i < count; i++)
				{
					pbuffMem.Position = 0;
					ProtoBuf.Serializer.Deserialize<T>(pbuffMem);
				}
				sw.Stop();
				Log("protobuf-net Deserialize		took: " + ToString(sw.Elapsed));
			}
			catch (Exception ex)
			{
				Log("ProtocolBuffer failed, " + ex.Message);
			}
		}

		private void RunBenchmarkBinaryFormatter<T>(T obj, int count)
		{
			try
			{
				long initlength = 0;
				Stopwatch sw;
				//-----------------------------------

				var mem = new MemoryStream();
				_binf.Serialize(mem, obj);
				initlength = mem.Length;

				mem.Seek(0, SeekOrigin.Begin);
				_binf.Deserialize(mem);


				using (var sharperMem = new MemoryStream())
				{
					sw = Stopwatch.StartNew();
					for (int i = 0; i < count; i++)
					{
						_binf.Serialize(sharperMem, obj);
					}
				}
				sw.Stop();
				Log("BinaryFormatter Serialize		took: " + ToString(sw.Elapsed) + "  data-size: " + initlength);


				sw = Stopwatch.StartNew();
				for (int i = 0; i < count; i++)
				{
					mem.Seek(0, SeekOrigin.Begin);
					_binf.Deserialize(mem);
				}
				sw.Stop();
				Log("BinaryFormatter Deserialize		took: " + ToString(sw.Elapsed));
			}
			catch (Exception ex)
			{
				Log("BinaryFormatter failed, " + ex.Message);
			}
		}


		private void RunBenchmarkNetSerializer<T>(T obj, int count)
		{
			try
			{

				Stopwatch sw;
				//-----------------------------------

				var netSerializer = new NetSerializer.Serializer(new[] {typeof (T)});
 
				var pbuffMem = new MemoryStream();
				netSerializer.Serialize(pbuffMem, obj);

				using (var mem = new MemoryStream())
				{
					sw = Stopwatch.StartNew();
					for (int i = 0; i < count; i++)
					{
						netSerializer.Serialize(mem, obj);
						mem.SetLength(0);
					}
				}
				sw.Stop();
				Log("NetSerializer.Serialize		took: " + ToString(sw.Elapsed) + "  data-size: " + pbuffMem.Length);


				sw = Stopwatch.StartNew();
				for (int i = 0; i < count; i++)
				{
					pbuffMem.Seek(0, SeekOrigin.Begin);
					netSerializer.Deserialize(pbuffMem);
				}
				sw.Stop();
				Log("NetSerializer.Deserialize		took: " + ToString(sw.Elapsed));
			}
			catch (Exception ex)
			{
				Log("NetSerializer failed, " + ex.Message);
			}
		}


		#endregion
	}
}
