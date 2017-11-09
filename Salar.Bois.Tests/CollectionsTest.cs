using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReflectionMagic;
using SharpTestsEx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Salar.Bois.Tests
{
	[TestClass]
	public class CollectionsTest
	{
		private BoisSerializer _boisSerializer;
		private dynamic bion;
		private MemoryStream bionStream;
		private BinaryReader bionReader;
		private BinaryWriter bionWriter;

		[TestInitialize]
		public void Initialize()
		{
			_boisSerializer = new BoisSerializer();
			bion = _boisSerializer.AsDynamic();
			bionStream = new MemoryStream();
			bionReader = new BinaryReader(bionStream);
			bionWriter = new BinaryWriter(bionStream);
		}

		private void EchoStreamSize()
		{
			Console.WriteLine("DataStream size: " + bionStream.Length);
		}

		void ResetStream()
		{
			bionStream.Seek(0, SeekOrigin.Begin);
		}

		void AssertAreEqual<T>(IList<T> expected, IList<T> actual)
		{
			Assert.AreEqual(expected.Count, actual.Count);

			for (int i = 0; i < expected.Count; i++)
			{
				Assert.AreEqual(expected[i], actual[i]);
			}
		}

		[TestMethod]
		public void WriteBytes_Normal()
		{
			ResetStream();
			var init = new byte[] { 10, 50, 0, 250, 98 };
			bion.WriteBytes(bionWriter, init);
			ResetStream();

			var final = (byte[])bion.ReadBytes(bionReader);

			AssertAreEqual(init, final);
		}

		[TestMethod]
		public void WriteBytes_Empty()
		{
			ResetStream();
			var init = new byte[] { };
			bion.WriteBytes(bionWriter, init);
			ResetStream();

			var final = (byte[])bion.ReadBytes(bionReader);

			final.Should().Have.SameSequenceAs(init);
		}


		[TestMethod]
		public void WriteDictionary_StringNormal()
		{
			ResetStream();
			var init = new Dictionary<string, string>()
						   {
							   {"man", "down"},
							   {"Random chars", "~!@# $ %^& * ()"}
						   };
			bion.WriteDictionary(bionWriter, init);
			ResetStream();

			var final = (Dictionary<string, string>)bion.ReadDictionary(bionReader, typeof(Dictionary<string, string>));
			final.Should().Have.SameSequenceAs(init);
		}

		[TestMethod]
		public void WriteDictionary_StringEmpty()
		{
			ResetStream();
			var init = new Dictionary<string, string>();
			bion.WriteDictionary(bionWriter, init);
			ResetStream();

			var final = (Dictionary<string, string>)bion.ReadDictionary(bionReader, typeof(Dictionary<string, string>));

			final.Should().Have.SameSequenceAs(init);
		}

		[TestMethod]
		public void WriteDictionary_NumberNormal()
		{
			ResetStream();
			var init = new Dictionary<int, long>()
						   {
							   {50, 177},
							   {10, 42677},
							   {25000000, 90L},
						   };
			bion.WriteDictionary(bionWriter, init);
			ResetStream();

			var final = (Dictionary<int, long>)bion.ReadDictionary(bionReader, typeof(Dictionary<int, long>));
			final.Should().Have.SameSequenceAs(init);
		}
		[TestMethod]
		public void WriteDictionary_NullableNormal()
		{
			ResetStream();
			var init = new Dictionary<int?, long?>()
						   {
							   {50, null},
							   {10, null},
							   {25000000, 90L},
						   };
			bion.WriteDictionary(bionWriter, init);
			ResetStream();

			var final = (Dictionary<int?, long?>)bion.ReadDictionary(bionReader, typeof(Dictionary<int?, long?>));

			final.Should().Have.SameSequenceAs(init);
		}

		[TestMethod]
		public void WriteArray_StringNormal()
		{
			ResetStream();
			var init = new string[] { "Hi", " ~!@#$%^&*())_+ ?><|\"'}{[]\\';/.,`=- ", "Sample", "Text" };
			bion.WriteArray(bionWriter, init);
			ResetStream();

			var final = (string[])bion.ReadArray(bionReader, typeof(string[]));

			final.Should().Have.SameSequenceAs(init);
		}

		[TestMethod]
		public void WriteArray_StringEmpty()
		{
			ResetStream();
			var init = new string[] { };
			bion.WriteArray(bionWriter, init);
			ResetStream();

			var final = (string[])bion.ReadArray(bionReader, typeof(string[]));

			final.Should().Have.SameSequenceAs(init);
		}

		[TestMethod]
		public void WriteArray_DoubleNormal()
		{
			ResetStream();
			var init = new double[] { 0.000, 0.0001, double.MaxValue, double.MinValue, 99.001 };
			bion.WriteArray(bionWriter, init);
			ResetStream();

			var final = (double[])bion.ReadArray(bionReader, typeof(double[]));

			final.Should().Have.SameSequenceAs(init);
		}

		[TestMethod]
		public void WriteArray_DoubleEmpty()
		{
			ResetStream();
			var init = new double[] { };
			bion.WriteArray(bionWriter, init);
			ResetStream();

			var final = (double[])bion.ReadArray(bionReader, typeof(double[]));

			final.Should().Have.SameSequenceAs(init);
		}

		[TestMethod]
		public void WriteArray_DateTimeNormal()
		{
			ResetStream();
			var init = new DateTime[] { DateTime.MinValue, DateTime.MaxValue, DateTime.Now };
			bion.WriteArray(bionWriter, init);
			ResetStream();

			var final = (DateTime[])bion.ReadArray(bionReader, typeof(DateTime[]));

			AssertionHelper.AssetArrayEqual<DateTime>(init, final);
		}

		[TestMethod]
		public void WriteArray_ColorNormal()
		{
			ResetStream();
			var init = new Color[] { Color.Gainsboro, Color.DarkBlue };
			bion.WriteArray(bionWriter, init);
			ResetStream();

			var final = (Color[])bion.ReadArray(bionReader, typeof(Color[]));

			AssertionHelper.AssetArrayEqual<Color>(init, final);
		}

		[TestMethod]
		public void WriteArray_VersionNormal()
		{
			ResetStream();
			var init = new Version[] { new Version(1, 2, 3, 4), new Version(0, 0, 0, 1), new Version(0, 0, 0, 0), };
			bion.WriteArray(bionWriter, init);
			ResetStream();

			var final = (Version[])bion.ReadArray(bionReader, typeof(Version[]));

			final.Should().Have.SameSequenceAs(init);
		}


		/// <summary>
		/// ArrayList is not supported, IGNORE this test
		/// </summary>
		public void WriteArray_ArrayListNormal()
		{
			ResetStream();
			var init = new ArrayList { new Version(1, 2, 3, 4), Color.DarkBlue, "test1", 99.200022 };
			bion.WriteArray(bionWriter, init);
			ResetStream();

			var final = (ArrayList)bion.ReadArray(bionReader, typeof(ArrayList));

			AssertionHelper.AssetArrayEqual(init, final);
		}

		[TestMethod]
		public void WriteStringDictionary_StringNormal()
		{
			ResetStream();
			var init = new Dictionary<string, int>()
						   {
							   {"Bion", 100},
							   {"Salar", 20},
							   {"", 0},
							   {"Khalilzadeh", -100}
						   };
			bion.WriteStringDictionary(bionWriter, init);
			ResetStream();

			var final = (Dictionary<string, int>)bion.ReadStringDictionary(bionReader, typeof(Dictionary<string, int>));

			final.Should().Have.SameSequenceAs(init);
		}

		[TestMethod]
		public void WriteStringDictionary_StringNullable()
		{
			ResetStream();
			var init = new Dictionary<string, int?>()
						   {
							   {"Bion", null},
							   {"Salar", null},
							   {"Khalilzadeh", -100}
						   };
			bion.WriteStringDictionary(bionWriter, init);
			ResetStream();

			var final = (Dictionary<string, int?>)bion.ReadStringDictionary(bionReader, typeof(Dictionary<string, int?>));

			final.Should().Have.SameSequenceAs(init);
		}

		[TestMethod]
		public void WriteStringDictionary_StringEmpty()
		{
			ResetStream();
			var init = new Dictionary<string, int?>();
			bion.WriteStringDictionary(bionWriter, init);
			ResetStream();

			var final = (Dictionary<string, int?>)bion.ReadStringDictionary(bionReader, typeof(Dictionary<string, int?>));

			final.Should().Have.SameSequenceAs(init);
		}

		[TestMethod]
		public void GenericList_StringNormal()
		{
			ResetStream();
			var init = new List<string>() { "This test", "is", " for BOIS " };
			bion.WriteGenericList(bionWriter, init);
			ResetStream();

			var final = (List<string>)bion.ReadGenericList(bionReader, typeof(List<string>));

			final.Should().Have.SameSequenceAs(init);
		}

		[TestMethod]
		public void GenericInterfaceList_StringNormal()
		{
			ResetStream();
			IList<string> init = new List<string>() { "This test", "is", " for BOIS " };
			bion.WriteGenericList(bionWriter, init);
			ResetStream();

			var result = bion.ReadGenericList(bionReader, typeof(IList<string>));

			// Testing Bug Fix: This is Reflection Magic's that cannot cast to the interface
			// var final = (IList<string>) result;
			var final = (List<string>)result;

			final.Should().Have.SameSequenceAs(init);
		}

		[TestMethod]
		public void GenericList_StringNull()
		{
			ResetStream();
			var init = new List<string>() { "This test", null, " for BOIS " };
			bion.WriteGenericList(bionWriter, init);
			ResetStream();

			var final = (List<string>)bion.ReadGenericList(bionReader, typeof(List<string>));

			final.Should().Have.SameSequenceAs(init);
		}

		[TestMethod]
		public void GenericList_NumberNormal()
		{
			ResetStream();
			var init = new List<float>() { 11, 23.0009f, 0.00f, -90.33033f, 22 };
			bion.WriteGenericList(bionWriter, init);
			ResetStream();

			var final = (List<float>)bion.ReadGenericList(bionReader, typeof(List<float>));

			final.Should().Have.SameSequenceAs(init);

			EchoStreamSize();
		}

		[TestMethod]
		public void GenericList_NumberNullable()
		{
			ResetStream();
			var init = new List<float?>() { 11, null, 23.0009f, 0.00f, -90.33033f, null, 22 };
			bion.WriteGenericList(bionWriter, init);
			ResetStream();

			var final = (List<float?>)bion.ReadGenericList(bionReader, typeof(List<float?>));

			final.Should().Have.SameSequenceAs(init);
			EchoStreamSize();
		}

		[TestMethod]
		public void GenericList_ListVersionNormal()
		{
			ResetStream();
			var init = new List<Version>() { new Version(1, 2, 3, 4), new Version(0, 0, 0, 1), new Version(0, 0, 0, 0), };
			bion.WriteGenericList(bionWriter, init);
			ResetStream();

			var final = (List<Version>)bion.ReadGenericList(bionReader, typeof(List<Version>));

			final.Should().Have.SameSequenceAs(init);
		}

	}
}
