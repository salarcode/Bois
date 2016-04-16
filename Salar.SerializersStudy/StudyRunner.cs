using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Salar.SerializersStudy.Objects;
using Salar.SerializersStudy.Runners;
// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace Salar.SerializersStudy
{
	class StudyRunner
	{
		public static List<string> RunBenchmark()
		{
			var result = new List<string>();
			var counter = 0;

			//-------------------------------
			object nullTest = null;
			result.AddResult("------Null: ", "null", "-------");
			CallRunners(result, nullTest);
			//-------------------------------
			var boolTest = true;
			result.AddResult("------Bool: ", boolTest.ToString(), "-------");
			CallRunners(result, boolTest);



			//-------------------------------
			var bigByteArrray = new byte[short.MaxValue];
			for (short i = 0; i < 500; i++)
			{
				bigByteArrray[i]=100;
			}
			result.AddResult("------BIG Byte array : ", bigByteArrray.ToString(), "-------");
			CallRunners(result, bigByteArrray);

			//-------------------------------
			var ditionaryIntBig = new Dictionary<int, int>();
			for (var i = 0; i < 500; i++)
			{
				ditionaryIntBig.Add(i, i + 1);
			}
			result.AddResult("------BIG Dictionary of int: ", ditionaryIntBig.ToString(), "-------");
			CallRunners(result, ditionaryIntBig);

			//-------------------------------
			var ditionaryUIntBig = new Dictionary<uint, uint>();
			for (uint i = 0; i < 500; i++)
			{
				ditionaryUIntBig.Add(i, i + 1);
			}
			result.AddResult("------BIG Dictionary of uint: ", ditionaryUIntBig.ToString(), "-------");
			CallRunners(result, ditionaryUIntBig);

			//-------------------------------
			var ditionaryLongBig = new Dictionary<long, long>();
			for (var i = 0; i < 500; i++)
			{
				ditionaryLongBig.Add(i, i + 1);
			}
			result.AddResult("------BIG Dictionary of int64: ", ditionaryLongBig.ToString(), "-------");
			CallRunners(result, ditionaryLongBig);
			//-------------------------------
			var ditionaryULongBig = new Dictionary<ulong, ulong>();
			for (ulong i = 0; i < 500; i++)
			{
				ditionaryULongBig.Add(i, i + 1);
			}
			result.AddResult("------BIG Dictionary of uint64: ", ditionaryULongBig.ToString(), "-------");
			CallRunners(result, ditionaryLongBig);

			//-------------------------------
			var stringTest = new string(new char[] { (char)(ushort.MaxValue - 10), (char)(ushort.MaxValue / 2), (char)(ushort.MaxValue - 100), (char)(ushort.MaxValue - 100), (char)(ushort.MaxValue - 100), (char)(ushort.MaxValue - 200), (char)(ushort.MaxValue - 50), });
			result.AddResult("------String: ", stringTest.ToString(), "-------");
			CallRunners(result, stringTest);

			//-------------------------------
			var stringArray = new string[] { "Salar", "Khalilzadeh", "Salar.Bois" };
			result.AddResult("------String Array: ", stringArray.ToString(), "-------");
			CallRunners(result, stringArray);

			//-------------------------------
			var stringGeneric = new List<string> { "Salar", "Khalilzadeh", "Salar.Bois" };
			result.AddResult("------String Generic List: ", stringGeneric.ToString(), "-------");
			CallRunners(result, stringGeneric);
			//-------------------------------
			IList<string> stringInterfaceGeneric = new List<string> { "Salar", "Khalilzadeh", "Salar.Bois" };
			result.AddResult("------String Generic InterfaceList: ", stringGeneric.ToString(), "-------");
			CallRunners(result, stringInterfaceGeneric);
			//-------------------------------
			var stringDictionary = new Dictionary<string, string>
			{
				{"Name:", "Salar"},
				{"Family", "Khalilzadeh"},
				{"Product", "Salar.Bois"}
			};
			result.AddResult("------String Generic Dictionary: ", stringDictionary.ToString(), "-------");
			CallRunners(result, stringDictionary);

			//-------------------------------
			var intArray = new int[] { int.MaxValue, int.MinValue, int.MaxValue, int.MinValue, int.MaxValue, int.MinValue, };
			result.AddResult("------Int Big data Array: ", intArray.ToString(), "-------");
			CallRunners(result, intArray);

			//-------------------------------
			var int2Array = new int[] { 3, 5, 6, 5, 1, 9, };
			result.AddResult("------Int Small data Array: ", intArray.ToString(), "-------");
			CallRunners(result, int2Array);

			//-------------------------------
			var byeArray = new byte[] { byte.MaxValue, byte.MinValue, byte.MaxValue / 2, 1, 0, 0, 0, 0, 0, byte.MaxValue };
			result.AddResult("------Byte Array: ", byeArray.ToString(), "-------");
			CallRunners(result, byeArray);

			//-------------------------------
			var emptyClassTest = new TestEmptyClass();
			result.AddResult("------TestEmptyClass: ", emptyClassTest.ToString(), "-------");
			CallRunners(result, emptyClassTest);

			//-------------------------------
			var emptyStructTest = new TestEmptyStruct();
			result.AddResult("------TestEmptyStruct: ", emptyStructTest.ToString(), "-------");
			CallRunners(result, emptyStructTest);

			//-------------------------------
			var oneMemberClassTest = new TestOneMemberClass();
			result.AddResult("------TestOneMemberClass: ", oneMemberClassTest.ToString(), "-------");
			CallRunners(result, oneMemberClassTest);

			//-------------------------------
			var oneMemberStructTest = new TestOneMemberStruct();
			result.AddResult("------TestOneMemberStruct: ", oneMemberStructTest.ToString(), "-------");
			CallRunners(result, oneMemberStructTest);
			//-------------------------------
			var twoMemberClassTest = new TestTwoMemberClass();
			result.AddResult("------TestTwoMemberClass: ", twoMemberClassTest.ToString(), "-------");
			CallRunners(result, twoMemberClassTest);
			//-------------------------------
			var twoMemberStructTest = new TestTwoMemberStruct();
			result.AddResult("------TestTwoMemberStruct: ", twoMemberStructTest.ToString(), "-------");
			CallRunners(result, twoMemberStructTest);
			//-------------------------------
			var parentClassNullTest = new TestParentClassNull();
			result.AddResult("------TestParentClassNull: ", parentClassNullTest.ToString(), "-------");
			CallRunners(result, parentClassNullTest);
			//-------------------------------
			var testParentClassFull = TestParentClassFull.Create();
			result.AddResult("------TestParentClassFull: ", testParentClassFull.ToString(), "-------");
			CallRunners(result, testParentClassFull);

			//-------------------------------
			var listOfTestOneMemberClassTest = new List<TestOneMemberClass>();
			for (var i = 0; i < 10; i++)
			{
				listOfTestOneMemberClassTest.Add(new TestOneMemberClass());
			}
			result.AddResult("------List of TestOneMemberClass: ", listOfTestOneMemberClassTest.ToString(), "-------");
			CallRunners(result, listOfTestOneMemberClassTest);

			//-------------------------------
			var listOfTestTwoMemberClassTest = new List<TestTwoMemberClass>();
			for (var i = 0; i < 10; i++)
			{
				listOfTestTwoMemberClassTest.Add(new TestTwoMemberClass());
			}
			result.AddResult("------List of TestTwoMemberClass: ", listOfTestTwoMemberClassTest.ToString(), "-------");
			CallRunners(result, listOfTestTwoMemberClassTest);


			//-------------------------------
			var dictionaryOfTestOneMemberClassTest = new Dictionary<int, TestOneMemberClass>();
			for (var i = 0; i < 10; i++)
			{
				dictionaryOfTestOneMemberClassTest.Add(i + (int.MaxValue / 2), new TestOneMemberClass());
			}
			result.AddResult("------Dictionary of TestOneMemberClass: ", dictionaryOfTestOneMemberClassTest.ToString(), "-------");
			CallRunners(result, dictionaryOfTestOneMemberClassTest);

			//-------------------------------
			var dictionaryOfTestTwoMemberClassTest = new Dictionary<int, TestTwoMemberClass>();
			for (var i = 0; i < 10; i++)
			{
				dictionaryOfTestTwoMemberClassTest.Add(i + (int.MaxValue / 2), new TestTwoMemberClass());
			}
			result.AddResult("------Dictionary of TestTwoMemberClass: ", dictionaryOfTestTwoMemberClassTest.ToString(), "-------");
			CallRunners(result, dictionaryOfTestTwoMemberClassTest);

			//-------------------------------
			var dateTimeTest = DateTime.Now;
			result.AddResult("------DateTime: ", dateTimeTest.ToString(), "-------");
			CallRunners(result, dateTimeTest);

			//-------------------------------
			var dateTimeOffsetTest = DateTimeOffset.Now;
			result.AddResult("------DateTimeOffset: ", dateTimeOffsetTest.ToString(), "-------");
			CallRunners(result, dateTimeOffsetTest);

			//-------------------------------
			var guidTest = Guid.NewGuid();
			result.AddResult("------Guid: ", guidTest.ToString(), "-------");
			CallRunners(result, guidTest);

			//-------------------------------
			var guidEmptyTest = Guid.Empty;
			result.AddResult("------Guid: ", guidEmptyTest.ToString(), "-------");
			CallRunners(result, guidEmptyTest);
			//-------------------------------
			var enumTest = TestEnum.Javascript;
			result.AddResult("------Enum: ", enumTest.ToString(), "-------");
			CallRunners(result, enumTest);

			//-------------------------------
			var int16Test = (short)(short.MaxValue - 1);
			result.AddResult("------Int16: ", int16Test.ToString(), "-------");
			CallRunners(result, int16Test);
			//-------------------------------
			var int16NullTest = (short?)(short.MaxValue - 1);
			result.AddResult("------Int16 Null: ", int16NullTest.ToString(), "-------");
			CallRunners(result, int16NullTest);
			//-------------------------------
			var int16ZeroTest = (short)(0);
			result.AddResult("------Int16: ", int16ZeroTest.ToString(), "-------");
			CallRunners(result, int16ZeroTest);
			//-------------------------------
			var int16BridgeTest = (short)(127);
			result.AddResult("------Int16: ", int16BridgeTest.ToString(), "-------");
			CallRunners(result, int16BridgeTest);
			//-------------------------------
			var int16OutBridgeTest = (short)(129);
			result.AddResult("------Int16: ", int16OutBridgeTest.ToString(), "-------");
			CallRunners(result, int16OutBridgeTest);
			//-------------------------------
			var uint16Test = (ushort)(ushort.MaxValue - 1);
			result.AddResult("------UInt16: ", uint16Test.ToString(), "-------");
			CallRunners(result, uint16Test);
			//-------------------------------
			var int32Test = (int.MaxValue - 1);
			result.AddResult("------Int32: ", int32Test.ToString(), "-------");
			CallRunners(result, int32Test);
			//-------------------------------
			var int32NullTest = (int?)(int.MaxValue - 1);
			result.AddResult("------Int32 Null: ", int32NullTest.ToString(), "-------");
			CallRunners(result, int32NullTest);
			//-------------------------------
			var int32ZeroTest = (int)(0);
			result.AddResult("------Int32: ", int32ZeroTest.ToString(), "-------");
			CallRunners(result, int32ZeroTest);
			//-------------------------------
			var int32SubZeroTest = (int.MinValue + 1);
			result.AddResult("------Int32: ", int32SubZeroTest.ToString(), "-------");
			CallRunners(result, int32SubZeroTest);
			//-------------------------------
			var int32BridgeTest = (127);
			result.AddResult("------Int32: ", int32BridgeTest.ToString(), "-------");
			CallRunners(result, int32BridgeTest);
			//-------------------------------
			var int32OutBridgeTest = (128);
			result.AddResult("------Int32: ", int32OutBridgeTest.ToString(), "-------");
			CallRunners(result, int32OutBridgeTest);
			//-------------------------------
			var uint32Test = (uint.MaxValue - 10);
			result.AddResult("------UInt32: ", uint32Test.ToString(), "-------");
			CallRunners(result, uint32Test);
			//-------------------------------
			var uint32NullTest = (uint?)(uint.MaxValue - 10);
			result.AddResult("------UInt32 Null: ", uint32NullTest.ToString(), "-------");
			CallRunners(result, uint32NullTest);
			//-------------------------------
			var uint32ZeroTest = (uint)(0);
			result.AddResult("------UInt32: ", uint32ZeroTest.ToString(), "-------");
			CallRunners(result, uint32ZeroTest);
			//-------------------------------
			var int64Test = (long.MaxValue - 10);
			result.AddResult("------Int64: ", int64Test.ToString(), "-------");
			CallRunners(result, int64Test);
			//-------------------------------
			var int64NullTest = (long?)(long.MaxValue - 10);
			result.AddResult("------Int64 Null: ", int64NullTest.ToString(), "-------");
			CallRunners(result, int64NullTest);
			//-------------------------------
			var int64ZeroTest = (long)0;
			result.AddResult("------Int64: ", int64ZeroTest.ToString(), "-------");
			CallRunners(result, int64ZeroTest);
			//-------------------------------
			var int64SubZeroTest = (long.MinValue + 1);
			result.AddResult("------Int64: ", int64SubZeroTest.ToString(), "-------");
			CallRunners(result, int64SubZeroTest);
			//-------------------------------
			var uint64Test = (ulong.MaxValue - 1);
			result.AddResult("------UInt64: ", uint64Test.ToString(), "-------");
			CallRunners(result, uint64Test);
			//-------------------------------
			var uint64NullTest = (ulong?)(ulong.MaxValue - 1);
			result.AddResult("------UInt64 Null: ", uint64NullTest.ToString(), "-------");
			CallRunners(result, uint64NullTest);
			//-------------------------------
			var uint64ZeroTest = (ulong)0;
			result.AddResult("------UInt64: ", uint64ZeroTest.ToString(), "-------");
			CallRunners(result, uint64ZeroTest);

			//-------------------------------
			var floatTest = (float.MaxValue - 1);
			result.AddResult("------Float: ", floatTest.ToString(), "-------");
			CallRunners(result, floatTest);
			//-------------------------------
			var floatNullTest = (float?)(float.MaxValue - 1);
			result.AddResult("------Float Null: ", floatNullTest.ToString(), "-------");
			CallRunners(result, floatNullTest);
			//-------------------------------
			var floatZeroTest = (float)0;
			result.AddResult("------Float: ", floatZeroTest.ToString(), "-------");
			CallRunners(result, floatZeroTest);
			//-------------------------------
			var floatSubZeroTest = (float.MinValue + 1);
			result.AddResult("------Float: ", floatSubZeroTest.ToString(), "-------");
			CallRunners(result, floatSubZeroTest);

			//-------------------------------
			var decimalTest = (decimal.MaxValue - 1);
			result.AddResult("------Decimal: ", decimalTest.ToString(), "-------");
			CallRunners(result, decimalTest);
			//-------------------------------
			var decimalNullTest = (decimal?)(decimal.MaxValue - 1);
			result.AddResult("------Decimal Null: ", decimalNullTest.ToString(), "-------");
			CallRunners(result, decimalNullTest);
			//-------------------------------
			var decimalZeroTest = (decimal)0;
			result.AddResult("------Decimal: ", decimalZeroTest.ToString(), "-------");
			CallRunners(result, decimalZeroTest);
			//-------------------------------
			var decimalSubZeroTest = (decimal.MinValue + 1);
			result.AddResult("------Decimal: ", decimalSubZeroTest.ToString(), "-------");
			CallRunners(result, decimalSubZeroTest);

			//-------------------------------
			var doubleTest = (double.MaxValue - 1);
			result.AddResult("------Double: ", doubleTest.ToString(), "-------");
			CallRunners(result, doubleTest);
			//-------------------------------
			var doubleNullTest = (double?)(double.MaxValue - 1);
			result.AddResult("------Double Null: ", doubleNullTest.ToString(), "-------");
			CallRunners(result, doubleNullTest);
			//-------------------------------
			var doubleZeroTest = (double)0;
			result.AddResult("------Double: ", doubleZeroTest.ToString(), "-------");
			CallRunners(result, doubleZeroTest);
			//-------------------------------
			var doubleSubZeroTest = (double.MinValue + 1);
			result.AddResult("------Double: ", doubleSubZeroTest.ToString(), "-------");
			CallRunners(result, doubleSubZeroTest);


			//-------------------------------
			for (int num = 65530; num < 65540; num += 2)
			{
				result.AddResult("------Int32: ", num.ToString(), "-------");
				CallRunners(result, num);
			}

			return result;
		}

		private static void CallRunners<T>(List<string> result, T testValue)
		{
			result.AddResult("Salar.Bois  ", BoisRunner.GetPackedSize(testValue));
			result.AddResult("MicrosoftAvro", MsAvroRunner.GetPackedSize(testValue));
			result.AddResult("MessagePack", MessagePackRunner.GetPackedSize(testValue));
			result.AddResult("ProtocolBuff", ProtocolBuffRunner.GetPackedSize(testValue));
		}
	}


	public static class ExtentsionsStudyRunner
	{
		public static void AddResult(this List<string> list, string key, object value)
		{
			list.Add(string.Format("{0}\t\t: {1}", key, value));
		}
		public static void AddResult(this List<string> list, string key, long value)
		{
			if (value == (-1))
				list.Add(string.Format("{0}\t\t: {1}", key, "Failed"));
			else if (value == (-2))
				list.Add(string.Format("{0}\t\t: {1}", key, "Invalid"));
			else
				list.Add(string.Format("{0}\t\t: {1}", key, value));
		}
		public static void AddResult(this List<string> list, params string[] items)
		{

			list.Add(string.Join("", items));
		}
	}
}
