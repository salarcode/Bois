using System;
using System.Collections.Generic;
using System.Text;
using HelloWorldApp.BusinessObjects;
using System.IO;
using Polenter.Serialization;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using Salar.Bois;

namespace sharpSerializerSpeedTests
{
	class Program
	{
		static void Main(string[] args)
		{
			RootContainer[] containerArray = createContainerArray(100);

			Console.WriteLine("Serializing");
			serializeWithBinaryFormatter(containerArray, "BinaryFormatter.bin");
			serializeWithSharpSerializer(containerArray, BinarySerializationMode.Burst, "sharpSerializerBurst.bin");
			serializeWithSharpSerializer(containerArray, BinarySerializationMode.SizeOptimized, "sharpSerializerOptimized.bin");
			serializeWithXmlSharpSerializer(containerArray, "sharpSerializer.xml");
			serializeWithSalarBois(containerArray, "SalarBois.bin");

			Console.WriteLine();
			Console.WriteLine("Deserializing");
			deserializeWithBinaryFormatter("BinaryFormatter.bin");
			deserializeWithSharpSerializer(BinarySerializationMode.Burst, "sharpSerializerBurst.bin");
			deserializeWithSharpSerializer(BinarySerializationMode.SizeOptimized, "sharpSerializerOptimized.bin");
			deserializeWithXmlSharpSerializer("sharpSerializer.xml");
		}

		private static void deserializeWithXmlSharpSerializer(string shortFilename)
		{
			string filename = getFilename(shortFilename);

			var serializer = new SharpSerializer();
			Stopwatch watch = new Stopwatch();

			Console.WriteLine(string.Format("Starting deserialization with SharpSerializer XML"));
			watch.Start();
			var fakeObj = serializer.Deserialize(filename);
			watch.Stop();
			Console.WriteLine(string.Format("Stopped after {0}ms", watch.ElapsedMilliseconds));
		}

		private static void deserializeWithSharpSerializer(BinarySerializationMode mode, string shortFilename)
		{
			string filename = getFilename(shortFilename);

			var serializer = new SharpSerializer(mode);
			Stopwatch watch = new Stopwatch();

			Console.WriteLine(string.Format("Starting deserialization with SharpSerializer ({0})", Enum.GetName(typeof(BinarySerializationMode), mode)));
			watch.Start();
			var fakeObj = serializer.Deserialize(filename);
			watch.Stop();
			Console.WriteLine(string.Format("Stopped after {0}ms", watch.ElapsedMilliseconds));
		}

		private static void deserializeWithBinaryFormatter(string shortFilename)
		{
			string filename = getFilename(shortFilename);
			using (var stream = new FileStream(filename, FileMode.Open))
			{
				var formatter = new BinaryFormatter();

				Stopwatch watch = new Stopwatch();
				Console.WriteLine("Starting deserialization with BinaryFormatter");
				watch.Start();
				var fakeObj = formatter.Deserialize(stream);
				watch.Stop();
				Console.WriteLine(string.Format("Stopped after {0}ms", watch.ElapsedMilliseconds));
			}
		}

		private static void serializeWithBinaryFormatter(RootContainer[] containerArray, string shortFilename)
		{
			string filename = getFilename(shortFilename);
			using (var stream = new FileStream(filename, FileMode.Create))
			{
				var formatter = new BinaryFormatter();

				Stopwatch watch = new Stopwatch();
				Console.WriteLine("Starting serialization with BinaryFormatter");
				watch.Start();
				formatter.Serialize(stream, containerArray);
				watch.Stop();
				Console.WriteLine(string.Format("Stopped after {0}ms", watch.ElapsedMilliseconds));
			}
		}

		private static void serializeWithSharpSerializer(RootContainer[] containerArray, BinarySerializationMode mode, string shortFilename)
		{
			string filename = getFilename(shortFilename);
			var serializer = new SharpSerializer(mode);
			Stopwatch watch = new Stopwatch();

			Console.WriteLine(string.Format("Starting serialization with SharpSerializer ({0})", Enum.GetName(typeof(BinarySerializationMode), mode)));
			watch.Start();
			serializer.Serialize(containerArray, filename);
			watch.Stop();
			Console.WriteLine(string.Format("Stopped after {0}ms", watch.ElapsedMilliseconds));
		}

		private static void serializeWithXmlSharpSerializer(RootContainer[] containerArray, string shortFilename)
		{
			string filename = getFilename(shortFilename);

			var serializer = new SharpSerializer();
			Stopwatch watch = new Stopwatch();

			Console.WriteLine(string.Format("Starting serialization with SharpSerializer XML"));
			watch.Start();
			serializer.Serialize(containerArray, filename);
			watch.Stop();

			Console.WriteLine(string.Format("Stopped after {0}ms , size: {1}", watch.ElapsedMilliseconds, new FileInfo(filename).Length));
		}

		private static void serializeWithSalarBois(RootContainer[] containerArray, string shortFilename)
		{
			string filename = getFilename(shortFilename);
			using (var stream = new FileStream(filename, FileMode.Create))
			{
				var formatter = new BoisSerializer();

				Stopwatch watch = new Stopwatch();
				Console.WriteLine("Starting serialization with BoisSerializer");
				watch.Start();
				formatter.Serialize(containerArray, stream);
				watch.Stop();
				Console.WriteLine(string.Format("Stopped after {0}ms", watch.ElapsedMilliseconds));
			}
		}
		private static string getFilename(string shortFilename)
		{
			var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
			var fullFilename = Path.Combine(path, shortFilename);
			return fullFilename;
		}

		private static RootContainer[] createContainerArray(int itemCount)
		{
			var result = new List<RootContainer>();
			for (int i = 0; i < itemCount; i++)
			{
				result.Add(RootContainer.CreateFakeRoot());
			}
			return result.ToArray();
		}
	}
}
