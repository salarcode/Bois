#if NET
using BenchmarkDotNet.Attributes;
using MemoryPack;
using MessagePack;
using Salar.Bois.BenchmarksBase;
using Salar.Bois.BenchmarksObjects;
using System;

namespace Salar.Bois.BenchMemoryPack;

public class MemoryPackBenchmark<T> : BenchmarkBase<T>
	where T : class, IBenchmarkTestObject, new()
{

	[Params("MemoryPack")]
	public override string TestName { get; set; }
	
	[Benchmark(Description = "Serialize")]
	[BenchmarkCategory("MemoryPack")]
	public override void Serialize()
	{
		for (int i = 0; i < IterationCount; i++)
		{
			Reset();
			TestBuffer = MemoryPackSerializer.Serialize(TestObject);
		}
	}

	[Benchmark(Description = "Deserialize")]
	[BenchmarkCategory("MemoryPack")]
	public override void Deserialize()
	{
		for (int i = 0; i < IterationCount; i++)
		{
			Reset();
			MemoryPackSerializer.Deserialize<T>(TestBuffer);
		}
	}
}
#endif