using BenchmarkDotNet.Attributes;
using Salar.Bois.BenchmarksBase;
using Salar.Bois.BenchmarksObjects;

namespace Salar.Bois.BenchBois;

public class BoisBenchmark<T> : BenchmarkBase<T>
	where T : class, IBenchmarkTestObject, new()
{
	private readonly BoisSerializer _serializer;

	[Params("Bois")]
	public override string TestName { get; set; }

	public BoisBenchmark()
	{
		_serializer = new BoisSerializer();
	}

	[Benchmark(Description = "Serialize")]
	[BenchmarkCategory("Bois")]
	public override void Serialize()
	{
		for (int i = 0; i < IterationCount; i++)
		{
			Reset();
			_serializer.Serialize(TestObject, TestStream);
		}
	}

	[Benchmark(Description = "Deserialize")]
	[BenchmarkCategory("Bois")]
	public override void Deserialize()
	{
		for (int i = 0; i < IterationCount; i++)
		{
			Reset();
			_serializer.Deserialize<T>(TestStream);
		}
	}
}