using BenchmarkDotNet.Attributes;
using MessagePack;
using Salar.Bois.BenchmarksBase;
using Salar.Bois.BenchmarksObjects;

namespace Salar.Bois.BenchMessagePack;

public class MessagePackBenchmark<T> : BenchmarkBase<T>
	where T : class, IBenchmarkTestObject, new()
{
	[Params("MessagePack")]
	public override string TestName { get; set; }

	[Benchmark(Description = "Serialize")]
	[BenchmarkCategory("MessagePack")]
	public override void Serialize()
	{
		for (int i = 0; i < IterationCount; i++)
		{
			Reset();
			MessagePackSerializer.Serialize(TestStream, TestObject);
		}
	}

	[Benchmark(Description = "Deserialize")]
	[BenchmarkCategory("MessagePack")]
	public override void Deserialize()
	{
		for (int i = 0; i < IterationCount; i++)
		{
			Reset();
			MessagePackSerializer.Deserialize<T>(TestStream);
		}
	}
}