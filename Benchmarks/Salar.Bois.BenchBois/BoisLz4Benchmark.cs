using BenchmarkDotNet.Attributes;
using Salar.Bois.BenchmarksBase;
using Salar.Bois.BenchmarksObjects;
using Salar.Bois.LZ4;

namespace Salar.Bois.BenchBois;

public class BoisLz4Benchmark<T> : BenchmarkBase<T>
	where T : class, IBenchmarkTestObject, new()
{
	private readonly BoisLz4Serializer _serializer;

	[Params("BoisLz4")]
	public override string TestName { get; set; }
	
	public BoisLz4Benchmark()
	{
		_serializer = new BoisLz4Serializer();
	}

	[Benchmark(Description = "Serialize")]
	[BenchmarkCategory("BoisLz4")]
	public override void Serialize()
	{
		for (int i = 0; i < IterationCount; i++)
		{
			Reset();
			_serializer.Pickle(TestObject, TestStream);
		}
	}

	[Benchmark(Description = "Deserialize")]
	[BenchmarkCategory("BoisLz4")]
	public override void Deserialize()
	{
		for (int i = 0; i < IterationCount; i++)
		{
			Reset();
			_serializer.Unpickle<T>(TestStream);
		}
	}
}