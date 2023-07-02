using MessagePack;
using ProtoBuf;
using System;
using System.Runtime.Serialization;

namespace Salar.Bois.BenchmarksObjects.TestObjects;

#if NET
[MemoryPack.MemoryPackable]
#endif
[ProtoContract]
[MessagePackObject(keyAsPropertyName: true)]
[Serializable]
public partial class Test1_Arrays_Big : Test1_Arrays_Small
{
	public Test1_Arrays_Big()
	{
		Floats = new float[] { float.MaxValue - 1, 27, 17, float.MaxValue - 1 };
		Doubles = new double[] { 30000, 270000, 17000000, 79990, 270000, 17000000, 79990, 270000, 17000000, 79990, 270000, 17000000, 79990 };
		Ints = new int[] { 300000, 270000, 1700000, 700000 };
		Strings = new string[] { "https://github.com/salarcode/bois", "https://www.nuget.org/packages/Salar.Bois/", "https://www.nuget.org/packages/Salar.Bois.LZ4" };
		Decimals = new[] { 10000.00m, decimal.MinValue, decimal.MaxValue, 10000.04m, 10000.03m, 10000.02m, 10000.01m };

		Floats = new float[byte.MaxValue];
		for (int i = 0; i < byte.MaxValue; i++)
		{
			Floats[i] = i;
		}

		Ints = new int[byte.MaxValue];
		for (int i = 0; i < byte.MaxValue; i++)
		{
			Ints[i] = i;
		}
	}
}

#if NET
[MemoryPack.MemoryPackable]
#endif
[ProtoContract]
[MessagePackObject(keyAsPropertyName: true)]
[Serializable]
public partial class Test1_Arrays_Small : IBenchmarkTestObject
{
	[ProtoMember(1)]
	[DataMember]
	//[Index(0)]
	public string[] Strings { get; set; }

	[ProtoMember(2)]
	[DataMember]
	//[Index(1)]
	public int[] Ints { get; set; }

	[ProtoMember(3)]
	[DataMember]
	//[Index(2)]
	public float[] Floats { get; set; }

	[ProtoMember(4)]
	[DataMember]
	//[Index(3)]
	public double[] Doubles { get; set; }

	[ProtoMember(5)]
	[DataMember]
	//[Index(4)]
	public decimal[] Decimals { get; set; }

	public Test1_Arrays_Small()
	{
		Floats = new float[] { 30, 27, 17, 70 };
		Doubles = new double[] { 30, 27, 17, 70 };
		Ints = new int[] { 30, 27, 17, 70 };
		Strings = new string[] { "Salar", "BOIS", "Github" };
		Decimals = new[] { 10000.00m, decimal.MaxValue };
	}
}