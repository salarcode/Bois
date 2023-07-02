using BenchmarkDotNet.Attributes;
using MessagePack;
using Salar.Bois.BenchmarksBase;
using Salar.Bois.BenchmarksObjects;

namespace Salar.Bois.BenchProtobufNet;

public class ProtobufNetBenchmark<T> : BenchmarkBase<T>
	where T : class, IBenchmarkTestObject, new()
{
	[Params("protobuf-net")]
	public override string TestName { get; set; }

	[Benchmark(Description = "Serialize")]
	[BenchmarkCategory("protobuf-net")]
	public override void Serialize()
	{
		for (int i = 0; i < IterationCount; i++)
		{
			Reset();
			ProtoBuf.Serializer.Serialize(TestStream, TestObject);
		}
	}

	[Benchmark(Description = "Deserialize")]
	[BenchmarkCategory("protobuf-net")]
	public override void Deserialize()
	{
		for (int i = 0; i < IterationCount; i++)
		{
			Reset();
			ProtoBuf.Serializer.Deserialize<T>(TestStream);
		}
	}
}