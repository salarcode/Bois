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


			//-------------------------------
			var stringTest = new string(new char[] { (char)(ushort.MaxValue - 10), (char)(ushort.MaxValue / 2), (char)(ushort.MaxValue - 100), (char)(ushort.MaxValue - 100), (char)(ushort.MaxValue - 100), (char)(ushort.MaxValue - 200), (char)(ushort.MaxValue - 50), });
			result.AddResult("------String: ", stringTest.ToString(), "-------");
			result.AddResult("Salar.Bois  ", BoisRunner.GetPackedSize(stringTest));
			result.AddResult("MessagePack", MessagePackRunner.GetPackedSize(stringTest));
			result.AddResult("MicrosoftAvro", MsAvroRunner.GetPackedSize(stringTest));
			result.AddResult("ProtocolBuff", ProtocolBuffRunner.GetPackedSize(stringTest));

			//-------------------------------
			var stringArray = new string[] { "Salar", "Khalilzadeh", "Salar.Bois" };
			result.AddResult("------String Array: ", stringArray.ToString(), "-------");
			result.AddResult("Salar.Bois  ", BoisRunner.GetPackedSize(stringArray));
			result.AddResult("MessagePack", MessagePackRunner.GetPackedSize(stringArray));
			result.AddResult("MicrosoftAvro", MsAvroRunner.GetPackedSize(stringArray));
			result.AddResult("ProtocolBuff", ProtocolBuffRunner.GetPackedSize(stringArray));

			//-------------------------------
			var byeArray = new byte[] { byte.MaxValue, byte.MinValue, byte.MaxValue / 2, 1, 0, 0, 0, 0, 0, byte.MaxValue };
			result.AddResult("------Byte Array: ", byeArray.ToString(), "-------");
			result.AddResult("Salar.Bois  ", BoisRunner.GetPackedSize(byeArray));
			result.AddResult("MessagePack", MessagePackRunner.GetPackedSize(byeArray));
			result.AddResult("MicrosoftAvro", MsAvroRunner.GetPackedSize(byeArray));
			result.AddResult("ProtocolBuff", ProtocolBuffRunner.GetPackedSize(byeArray));

			//-------------------------------
			var emptyClassTest = new TestEmptyClass();
			result.AddResult("------TestEmptyClass: ", emptyClassTest.ToString(), "-------");
			result.AddResult("Salar.Bois  ", BoisRunner.GetPackedSize(emptyClassTest));
			result.AddResult("MessagePack", MessagePackRunner.GetPackedSize(emptyClassTest));
			result.AddResult("MicrosoftAvro", MsAvroRunner.GetPackedSize(emptyClassTest));
			result.AddResult("ProtocolBuff", ProtocolBuffRunner.GetPackedSize(emptyClassTest));

			//-------------------------------
			var emptyStructTest = new TestEmptyStruct();
			result.AddResult("------TestEmptyStruct: ", emptyStructTest.ToString(), "-------");
			result.AddResult("Salar.Bois  ", BoisRunner.GetPackedSize(emptyStructTest));
			result.AddResult("MessagePack", MessagePackRunner.GetPackedSize(emptyStructTest));
			result.AddResult("MicrosoftAvro", MsAvroRunner.GetPackedSize(emptyStructTest));
			result.AddResult("ProtocolBuff", ProtocolBuffRunner.GetPackedSize(emptyStructTest));

			//-------------------------------
			var oneMemberClassTest = new TestOneMemberClass();
			result.AddResult("------TestOneMemberClass: ", oneMemberClassTest.ToString(), "-------");
			result.AddResult("Salar.Bois  ", BoisRunner.GetPackedSize(oneMemberClassTest));
			result.AddResult("MessagePack", MessagePackRunner.GetPackedSize(oneMemberClassTest));
			result.AddResult("MicrosoftAvro", MsAvroRunner.GetPackedSize(oneMemberClassTest));
			result.AddResult("ProtocolBuff", ProtocolBuffRunner.GetPackedSize(oneMemberClassTest));

			//-------------------------------
			var oneMemberStructTest = new TestOneMemberStruct();
			result.AddResult("------TestOneMemberStruct: ", oneMemberStructTest.ToString(), "-------");
			result.AddResult("Salar.Bois  ", BoisRunner.GetPackedSize(oneMemberStructTest));
			result.AddResult("MessagePack", MessagePackRunner.GetPackedSize(oneMemberStructTest));
			result.AddResult("MicrosoftAvro", MsAvroRunner.GetPackedSize(oneMemberStructTest));
			result.AddResult("ProtocolBuff", ProtocolBuffRunner.GetPackedSize(oneMemberStructTest));
			//-------------------------------
			var twoMemberClassTest = new TestTwoMemberClass();
			result.AddResult("------TestTwoMemberClass: ", twoMemberClassTest.ToString(), "-------");
			result.AddResult("Salar.Bois  ", BoisRunner.GetPackedSize(twoMemberClassTest));
			result.AddResult("MessagePack", MessagePackRunner.GetPackedSize(twoMemberClassTest));
			result.AddResult("MicrosoftAvro", MsAvroRunner.GetPackedSize(twoMemberClassTest));
			result.AddResult("ProtocolBuff", ProtocolBuffRunner.GetPackedSize(twoMemberClassTest));
			//-------------------------------
			var twoMemberStructTest = new TestTwoMemberStruct();
			result.AddResult("------TestTwoMemberStruct: ", twoMemberStructTest.ToString(), "-------");
			result.AddResult("Salar.Bois  ", BoisRunner.GetPackedSize(twoMemberStructTest));
			result.AddResult("MessagePack", MessagePackRunner.GetPackedSize(twoMemberStructTest));
			result.AddResult("MicrosoftAvro", MsAvroRunner.GetPackedSize(twoMemberStructTest));
			result.AddResult("ProtocolBuff", ProtocolBuffRunner.GetPackedSize(twoMemberStructTest));
			//-------------------------------
			var dateTimeTest = DateTime.Now;
			result.AddResult("------DateTime: ", dateTimeTest.ToString(), "-------");
			result.AddResult("Salar.Bois  ", BoisRunner.GetPackedSize(dateTimeTest));
			result.AddResult("MessagePack", MessagePackRunner.GetPackedSize(dateTimeTest));
			result.AddResult("MicrosoftAvro", MsAvroRunner.GetPackedSize(dateTimeTest));
			result.AddResult("ProtocolBuff", ProtocolBuffRunner.GetPackedSize(dateTimeTest));

			//-------------------------------
			var guidTest = Guid.NewGuid();
			result.AddResult("------Guid: ", guidTest.ToString(), "-------");
			result.AddResult("Salar.Bois  ", BoisRunner.GetPackedSize(guidTest));
			result.AddResult("MessagePack", MessagePackRunner.GetPackedSize(guidTest));
			result.AddResult("MicrosoftAvro", MsAvroRunner.GetPackedSize(guidTest));
			result.AddResult("ProtocolBuff", ProtocolBuffRunner.GetPackedSize(guidTest));

			//-------------------------------
			var enumTest = TestEnum.Javascript;
			result.AddResult("------Enum: ", enumTest.ToString(), "-------");
			result.AddResult("Salar.Bois  ", BoisRunner.GetPackedSize(enumTest));
			result.AddResult("MessagePack", MessagePackRunner.GetPackedSize(enumTest));
			result.AddResult("MicrosoftAvro", MsAvroRunner.GetPackedSize(enumTest));
			result.AddResult("ProtocolBuff", ProtocolBuffRunner.GetPackedSize(enumTest));

			//-------------------------------
			var int16Test = (short)(short.MaxValue - 1);
			result.AddResult("------Int16: ", int16Test.ToString(), "-------");
			result.AddResult("Salar.Bois  ", BoisRunner.GetPackedSize(int16Test));
			result.AddResult("MessagePack", MessagePackRunner.GetPackedSize(int16Test));
			result.AddResult("MicrosoftAvro", MsAvroRunner.GetPackedSize(int16Test));
			result.AddResult("ProtocolBuff", ProtocolBuffRunner.GetPackedSize(int16Test));
			//-------------------------------
			var uint16Test = (ushort)(ushort.MaxValue - 1);
			result.AddResult("------UInt16: ", uint16Test.ToString(), "-------");
			result.AddResult("Salar.Bois  ", BoisRunner.GetPackedSize(uint16Test));
			result.AddResult("MessagePack", MessagePackRunner.GetPackedSize(uint16Test));
			result.AddResult("MicrosoftAvro", MsAvroRunner.GetPackedSize(uint16Test));
			result.AddResult("ProtocolBuff", ProtocolBuffRunner.GetPackedSize(uint16Test));
			//-------------------------------
			var int32Test = (int.MaxValue - 1);
			result.AddResult("------Int32: ", int32Test.ToString(), "-------");
			result.AddResult("Salar.Bois  ", BoisRunner.GetPackedSize(int32Test));
			result.AddResult("MessagePack", MessagePackRunner.GetPackedSize(int32Test));
			result.AddResult("MicrosoftAvro", MsAvroRunner.GetPackedSize(int32Test));
			result.AddResult("ProtocolBuff", ProtocolBuffRunner.GetPackedSize(int32Test));
			//-------------------------------
			var int32ZeroTest = (int)(0);
			result.AddResult("------Int32: ", int32ZeroTest.ToString(), "-------");
			result.AddResult("Salar.Bois  ", BoisRunner.GetPackedSize(int32ZeroTest));
			result.AddResult("MessagePack", MessagePackRunner.GetPackedSize(int32ZeroTest));
			result.AddResult("MicrosoftAvro", MsAvroRunner.GetPackedSize(int32ZeroTest));
			result.AddResult("ProtocolBuff", ProtocolBuffRunner.GetPackedSize(int32ZeroTest));
			//-------------------------------
			var int32SubZeroTest = (int.MinValue + 1);
			result.AddResult("------Int32: ", int32SubZeroTest.ToString(), "-------");
			result.AddResult("Salar.Bois  ", BoisRunner.GetPackedSize(int32SubZeroTest));
			result.AddResult("MessagePack", MessagePackRunner.GetPackedSize(int32SubZeroTest));
			result.AddResult("MicrosoftAvro", MsAvroRunner.GetPackedSize(int32SubZeroTest));
			result.AddResult("ProtocolBuff", ProtocolBuffRunner.GetPackedSize(int32SubZeroTest));
			//-------------------------------
			var uint32Test = (uint.MaxValue - 1);
			result.AddResult("------UInt32: ", uint32Test.ToString(), "-------");
			result.AddResult("Salar.Bois  ", BoisRunner.GetPackedSize(uint32Test));
			result.AddResult("MessagePack", MessagePackRunner.GetPackedSize(uint32Test));
			result.AddResult("MicrosoftAvro", MsAvroRunner.GetPackedSize(uint32Test));
			result.AddResult("ProtocolBuff", ProtocolBuffRunner.GetPackedSize(uint32Test));
			//-------------------------------
			var uint32ZeroTest = (uint)(0);
			result.AddResult("------UInt32: ", uint32ZeroTest.ToString(), "-------");
			result.AddResult("Salar.Bois  ", BoisRunner.GetPackedSize(uint32ZeroTest));
			result.AddResult("MessagePack", MessagePackRunner.GetPackedSize(uint32ZeroTest));
			result.AddResult("MicrosoftAvro", MsAvroRunner.GetPackedSize(uint32ZeroTest));
			result.AddResult("ProtocolBuff", ProtocolBuffRunner.GetPackedSize(uint32ZeroTest));
			//-------------------------------
			var int64Test = (long.MaxValue - 1);
			result.AddResult("------Int64: ", int64Test.ToString(), "-------");
			result.AddResult("Salar.Bois  ", BoisRunner.GetPackedSize(int64Test));
			result.AddResult("MessagePack", MessagePackRunner.GetPackedSize(int64Test));
			result.AddResult("MicrosoftAvro", MsAvroRunner.GetPackedSize(int64Test));
			result.AddResult("ProtocolBuff", ProtocolBuffRunner.GetPackedSize(int64Test));
			//-------------------------------
			var int64ZeroTest = (long)0;
			result.AddResult("------Int64: ", int64ZeroTest.ToString(), "-------");
			result.AddResult("Salar.Bois  ", BoisRunner.GetPackedSize(int64ZeroTest));
			result.AddResult("MessagePack", MessagePackRunner.GetPackedSize(int64ZeroTest));
			result.AddResult("MicrosoftAvro", MsAvroRunner.GetPackedSize(int64ZeroTest));
			result.AddResult("ProtocolBuff", ProtocolBuffRunner.GetPackedSize(int64ZeroTest));
			//-------------------------------
			var int64SubZeroTest = (long.MinValue + 1);
			result.AddResult("------Int64: ", int64SubZeroTest.ToString(), "-------");
			result.AddResult("Salar.Bois  ", BoisRunner.GetPackedSize(int64SubZeroTest));
			result.AddResult("MessagePack", MessagePackRunner.GetPackedSize(int64SubZeroTest));
			result.AddResult("MicrosoftAvro", MsAvroRunner.GetPackedSize(int64SubZeroTest));
			result.AddResult("ProtocolBuff", ProtocolBuffRunner.GetPackedSize(int64SubZeroTest));
			//-------------------------------
			var uint64Test = (ulong.MaxValue - 1);
			result.AddResult("------UInt64: ", uint64Test.ToString(), "-------");
			result.AddResult("Salar.Bois  ", BoisRunner.GetPackedSize(uint64Test));
			result.AddResult("MessagePack", MessagePackRunner.GetPackedSize(uint64Test));
			result.AddResult("MicrosoftAvro", MsAvroRunner.GetPackedSize(uint64Test));
			result.AddResult("ProtocolBuff", ProtocolBuffRunner.GetPackedSize(uint64Test));
			//-------------------------------
			var uint64ZeroTest = (ulong)0;
			result.AddResult("------UInt64: ", uint64ZeroTest.ToString(), "-------");
			result.AddResult("Salar.Bois  ", BoisRunner.GetPackedSize(uint64ZeroTest));
			result.AddResult("MessagePack", MessagePackRunner.GetPackedSize(uint64ZeroTest));
			result.AddResult("MicrosoftAvro", MsAvroRunner.GetPackedSize(uint64ZeroTest));
			result.AddResult("ProtocolBuff", ProtocolBuffRunner.GetPackedSize(uint64ZeroTest));

			//-------------------------------
			var floatTest = (float.MaxValue - 1);
			result.AddResult("------Float: ", floatTest.ToString(), "-------");
			result.AddResult("Salar.Bois  ", BoisRunner.GetPackedSize(floatTest));
			result.AddResult("MessagePack", MessagePackRunner.GetPackedSize(floatTest));
			result.AddResult("MicrosoftAvro", MsAvroRunner.GetPackedSize(floatTest));
			result.AddResult("ProtocolBuff", ProtocolBuffRunner.GetPackedSize(floatTest));
			//-------------------------------
			var floatZeroTest = (float)0;
			result.AddResult("------Float: ", floatZeroTest.ToString(), "-------");
			result.AddResult("Salar.Bois  ", BoisRunner.GetPackedSize(floatZeroTest));
			result.AddResult("MessagePack", MessagePackRunner.GetPackedSize(floatZeroTest));
			result.AddResult("MicrosoftAvro", MsAvroRunner.GetPackedSize(floatZeroTest));
			result.AddResult("ProtocolBuff", ProtocolBuffRunner.GetPackedSize(floatZeroTest));
			//-------------------------------
			var floatSubZeroTest = (float.MinValue + 1);
			result.AddResult("------Float: ", floatSubZeroTest.ToString(), "-------");
			result.AddResult("Salar.Bois  ", BoisRunner.GetPackedSize(floatSubZeroTest));
			result.AddResult("MessagePack", MessagePackRunner.GetPackedSize(floatSubZeroTest));
			result.AddResult("MicrosoftAvro", MsAvroRunner.GetPackedSize(floatSubZeroTest));
			result.AddResult("ProtocolBuff", ProtocolBuffRunner.GetPackedSize(floatSubZeroTest));

			//-------------------------------
			var decimalTest = (decimal.MaxValue - 1);
			result.AddResult("------Decimal: ", decimalTest.ToString(), "-------");
			result.AddResult("Salar.Bois  ", BoisRunner.GetPackedSize(decimalTest));
			result.AddResult("MessagePack", MessagePackRunner.GetPackedSize(decimalTest));
			result.AddResult("MicrosoftAvro", MsAvroRunner.GetPackedSize(decimalTest));
			result.AddResult("ProtocolBuff", ProtocolBuffRunner.GetPackedSize(decimalTest));
			//-------------------------------
			var decimalZeroTest = (decimal)0;
			result.AddResult("------Decimal: ", decimalZeroTest.ToString(), "-------");
			result.AddResult("Salar.Bois  ", BoisRunner.GetPackedSize(decimalZeroTest));
			result.AddResult("MessagePack", MessagePackRunner.GetPackedSize(decimalZeroTest));
			result.AddResult("MicrosoftAvro", MsAvroRunner.GetPackedSize(decimalZeroTest));
			result.AddResult("ProtocolBuff", ProtocolBuffRunner.GetPackedSize(decimalZeroTest));
			//-------------------------------
			var decimalSubZeroTest = (decimal.MinValue + 1);
			result.AddResult("------Decimal: ", decimalSubZeroTest.ToString(), "-------");
			result.AddResult("Salar.Bois  ", BoisRunner.GetPackedSize(decimalSubZeroTest));
			result.AddResult("MessagePack", MessagePackRunner.GetPackedSize(decimalSubZeroTest));
			result.AddResult("MicrosoftAvro", MsAvroRunner.GetPackedSize(decimalSubZeroTest));
			result.AddResult("ProtocolBuff", ProtocolBuffRunner.GetPackedSize(decimalSubZeroTest));

			//-------------------------------
			var doubleTest = (double.MaxValue - 1);
			result.AddResult("------Double: ", doubleTest.ToString(), "-------");
			result.AddResult("Salar.Bois  ", BoisRunner.GetPackedSize(doubleTest));
			result.AddResult("MessagePack", MessagePackRunner.GetPackedSize(doubleTest));
			result.AddResult("MicrosoftAvro", MsAvroRunner.GetPackedSize(doubleTest));
			result.AddResult("ProtocolBuff", ProtocolBuffRunner.GetPackedSize(doubleTest));
			//-------------------------------
			var doubleZeroTest = (double)0;
			result.AddResult("------Double: ", doubleZeroTest.ToString(), "-------");
			result.AddResult("Salar.Bois  ", BoisRunner.GetPackedSize(doubleZeroTest));
			result.AddResult("MessagePack", MessagePackRunner.GetPackedSize(doubleZeroTest));
			result.AddResult("MicrosoftAvro", MsAvroRunner.GetPackedSize(doubleZeroTest));
			result.AddResult("ProtocolBuff", ProtocolBuffRunner.GetPackedSize(doubleZeroTest));
			//-------------------------------
			var doubleSubZeroTest = (double.MinValue + 1);
			result.AddResult("------Double: ", doubleSubZeroTest.ToString(), "-------");
			result.AddResult("Salar.Bois  ", BoisRunner.GetPackedSize(doubleSubZeroTest));
			result.AddResult("MessagePack", MessagePackRunner.GetPackedSize(doubleSubZeroTest));
			result.AddResult("MicrosoftAvro", MsAvroRunner.GetPackedSize(doubleSubZeroTest));
			result.AddResult("ProtocolBuff", ProtocolBuffRunner.GetPackedSize(doubleSubZeroTest));

			return result;
		}
	}


	public static class ExtentsionsStudyRunner
	{
		public static void AddResult(this List<string> list, string key, object value)
		{
			if (value == (object) (-1))
				list.Add(string.Format("{0}\t\t: {1}", key, "Failed"));
			else
				list.Add(string.Format("{0}\t\t: {1}", key, value));
		}
		public static void AddResult(this List<string> list, params string[] items)
		{

			list.Add(string.Join("", items));
		}
	}
}
