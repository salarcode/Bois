using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using ReflectionMagic;
using Salar.Bon;
using SharpTestsEx;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Salar.Bion.Tests
{
	[TestClass]
	public class SerializationCollectionsTest
	{
		private BonSerializer _bonSerializer;
		private dynamic bion;
		private MemoryStream bionStream;
		private BinaryReader bionReader;
		private BinaryWriter bionWriter;

		[TestInitialize]
		public void Initialize()
		{
			_bonSerializer = new BonSerializer();
			bion = _bonSerializer.AsDynamic();
			bionStream = new MemoryStream();
			bionReader = new BinaryReader(bionStream);
			bionWriter = new BinaryWriter(bionStream);

			bion._serializeOut = bionWriter;
			bion._input = bionReader;
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
			bion.WriteBytes(init);
			ResetStream();

			var final = (byte[])bion.ReadBytes();

			AssertAreEqual(init, final);
		}

		[TestMethod]
		public void WriteBytes_Empty()
		{
			ResetStream();
			var init = new byte[] { };
			bion.WriteBytes(init);
			ResetStream();

			var final = (byte[])bion.ReadBytes();

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
			bion.WriteDictionary(init);
			ResetStream();

			var final = (Dictionary<string, string>)bion.ReadDictionary(typeof(Dictionary<string, string>));
			final.Should().Have.SameSequenceAs(init);
		}

		[TestMethod]
		public void WriteDictionary_StringEmpty()
		{
			ResetStream();
			var init = new Dictionary<string, string>();
			bion.WriteDictionary(init);
			ResetStream();

			var final = (Dictionary<string, string>)bion.ReadDictionary(typeof(Dictionary<string, string>));

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
			bion.WriteDictionary(init);
			ResetStream();

			var final = (Dictionary<int, long>)bion.ReadDictionary(typeof(Dictionary<int, long>));
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
			bion.WriteDictionary(init);
			ResetStream();

			var final = (Dictionary<int?, long?>)bion.ReadDictionary(typeof(Dictionary<int?, long?>));

			final.Should().Have.SameSequenceAs(init);
		}

		[TestMethod]
		public void WriteArray_StringNormal()
		{
			ResetStream();
			var init = new string[] { "Hi", " ~!@#$%^&*())_+ ?><|\"'}{[]\\';/.,`=- ", "Sample", "Text" };
			bion.WriteArray(init);
			ResetStream();

			var final = (string[])bion.ReadArray(typeof(string[]));

			final.Should().Have.SameSequenceAs(init);
		}

		[TestMethod]
		public void WriteArray_StringEmpty()
		{
			ResetStream();
			var init = new string[] { };
			bion.WriteArray(init);
			ResetStream();

			var final = (string[])bion.ReadArray(typeof(string[]));

			final.Should().Have.SameSequenceAs(init);
		}

		[TestMethod]
		public void WriteArray_DoubleNormal()
		{
			ResetStream();
			var init = new double[] { 0.000, 0.0001, double.MaxValue, double.MinValue, 99.001 };
			bion.WriteArray(init);
			ResetStream();

			var final = (double[])bion.ReadArray(typeof(double[]));

			final.Should().Have.SameSequenceAs(init);
		}

		[TestMethod]
		public void WriteArray_DoubleEmpty()
		{
			ResetStream();
			var init = new double[] { };
			bion.WriteArray(init);
			ResetStream();

			var final = (double[])bion.ReadArray(typeof(double[]));

			final.Should().Have.SameSequenceAs(init);
		}

		[TestMethod]
		public void WriteArray_DateTimeNormal()
		{
			ResetStream();
			var init = new DateTime[] { DateTime.MinValue, DateTime.MaxValue, DateTime.Now };
			bion.WriteArray(init);
			ResetStream();

			var final = (DateTime[])bion.ReadArray(typeof(DateTime[]));

			AssertionHelper.AssetArrayEqual<DateTime>(init, final);
		}

		[TestMethod]
		public void WriteArray_ColorNormal()
		{
			ResetStream();
			var init = new Color[] { SystemColors.Control, Color.DarkBlue };
			bion.WriteArray(init);
			ResetStream();

			var final = (Color[])bion.ReadArray(typeof(Color[]));

			AssertionHelper.AssetArrayEqual<Color>(init, final);
		}

		[TestMethod]
		public void WriteArray_VersionNormal()
		{
			ResetStream();
			var init = new Version[] { new Version(1, 2, 3, 4), new Version(0, 0, 0, 1), new Version(0, 0, 0, 0), };
			bion.WriteArray(init);
			ResetStream();

			var final = (Version[])bion.ReadArray(typeof(Version[]));

			final.Should().Have.SameSequenceAs(init);
		}


		/// <summary>
		/// ArrayList is not supported, IGNORE this test
		/// </summary>
		public void WriteArray_ArrayListNormal()
		{
			ResetStream();
			var init = new ArrayList { new Version(1, 2, 3, 4), Color.DarkBlue, "test1", 99.200022 };
			bion.WriteArray(init);
			ResetStream();

			var final = (ArrayList)bion.ReadArray(typeof(ArrayList));

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
			bion.WriteStringDictionary(init);
			ResetStream();

			var final = (Dictionary<string, int>)bion.ReadStringDictionary(typeof(Dictionary<string, int>));

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
			bion.WriteStringDictionary(init);
			ResetStream();

			var final = (Dictionary<string, int?>)bion.ReadStringDictionary(typeof(Dictionary<string, int?>));

			final.Should().Have.SameSequenceAs(init);
		}

		[TestMethod]
		public void WriteStringDictionary_StringEmpty()
		{
			ResetStream();
			var init = new Dictionary<string, int?>();
			bion.WriteStringDictionary(init);
			ResetStream();

			var final = (Dictionary<string, int?>)bion.ReadStringDictionary(typeof(Dictionary<string, int?>));

			final.Should().Have.SameSequenceAs(init);
		}

		[TestMethod]
		public void GenericList_StringNormal()
		{
			ResetStream();
			var init = new List<string>() { "This test", "is", " for BON " };
			bion.WriteGenericList(init);
			ResetStream();

			var final = (List<string>)bion.ReadGenericList(typeof(List<string>));

			final.Should().Have.SameSequenceAs(init);
		}

		[TestMethod]
		public void GenericList_StringNull()
		{
			ResetStream();
			var init = new List<string>() { "This test", null, " for BON " };
			bion.WriteGenericList(init);
			ResetStream();

			var final = (List<string>)bion.ReadGenericList(typeof(List<string>));

			final.Should().Have.SameSequenceAs(init);
		}

		[TestMethod]
		public void GenericList_NumberNormal()
		{
			ResetStream();
			var init = new List<float>() { 11, 23.0009f, 0.00f, -90.33033f, 22 };
			bion.WriteGenericList(init);
			ResetStream();

			var final = (List<float>)bion.ReadGenericList(typeof(List<float>));

			final.Should().Have.SameSequenceAs(init);

			EchoStreamSize();
		}

		[TestMethod]
		public void GenericList_NumberNullable()
		{
			ResetStream();
			var init = new List<float?>() { 11, null, 23.0009f, 0.00f, -90.33033f, null, 22 };
			bion.WriteGenericList(init);
			ResetStream();

			var final = (List<float?>)bion.ReadGenericList(typeof(List<float?>));

			final.Should().Have.SameSequenceAs(init);
			EchoStreamSize();
		}

		[TestMethod]
		public void GenericList_ListVersionNormal()
		{
			ResetStream();
			var init = new List<Version>() { new Version(1, 2, 3, 4), new Version(0, 0, 0, 1), new Version(0, 0, 0, 0), };
			bion.WriteGenericList(init);
			ResetStream();

			var final = (List<Version>)bion.ReadGenericList(typeof(List<Version>));

			final.Should().Have.SameSequenceAs(init);
		}

	}
}
