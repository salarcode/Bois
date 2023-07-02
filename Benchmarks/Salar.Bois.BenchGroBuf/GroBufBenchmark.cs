using BenchmarkDotNet.Attributes;
using GroBuf.DataMembersExtracters;
using GroBuf;
using MessagePack;
using Salar.Bois.BenchmarksBase;
using Salar.Bois.BenchmarksObjects;

namespace Salar.Bois.BenchMessagePack;

public class GroBufBenchmark<T> : BenchmarkBase<T>
	where T : class, IBenchmarkTestObject, new()
{
	private readonly Serializer _serializer;

	[Params("GroBuf")]
	public override string TestName { get; set; } = string.Empty;

	public GroBufBenchmark()
	{
		_serializer = new Serializer(new PropertiesExtractor(), options: GroBufOptions.WriteEmptyObjects);
	}

	[Benchmark(Description = "Serialize")]
	[BenchmarkCategory("GroBuf")]
	public override void Serialize()
	{
		for (int i = 0; i < IterationCount; i++)
		{
			Reset();
			TestBuffer = _serializer.Serialize(TestObject);
		}
	}

	[Benchmark(Description = "Deserialize")]
	[BenchmarkCategory("GroBuf")]
	public override void Deserialize()
	{
		for (int i = 0; i < IterationCount; i++)
		{
			Reset();
			_serializer.Deserialize<T>(TestBuffer);
		}
	}
}