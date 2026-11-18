using BenchmarkDotNet.Attributes;
using Salar.Bois.BenchmarksBase;
using Salar.Bois.BenchmarksObjects;

namespace Salar.Bois.BenchBois;

/// <summary>
/// Benchmarks the byte[] overloads, where the serializer picks the writer and reader.
/// </summary>
public class BoisArrayBenchmark<T> : BenchmarkBase<T>
	where T : class, IBenchmarkTestObject, new()
{
	private readonly byte[] _writeBuffer = new byte[1024 * 1024];
	private readonly BoisSerializer _bois = new BoisSerializer();

	[Params("Bois.Array")]
	public override string TestName { get; set; }

	public override void GlobalSetup()
	{
		TestObject = new T();
		_bois.Serialize(TestObject, _writeBuffer, 0, _writeBuffer.Length);
		TestBuffer = _writeBuffer;
	}

	public override void Reset()
	{
	}

	[Benchmark(Description = "Serialize")]
	[BenchmarkCategory("Bois.Array")]
	public override void Serialize()
	{
		for (int i = 0; i < IterationCount; i++)
			_bois.Serialize(TestObject, _writeBuffer, 0, _writeBuffer.Length);
	}

	[Benchmark(Description = "Deserialize")]
	[BenchmarkCategory("Bois.Array")]
	public override void Deserialize()
	{
		for (int i = 0; i < IterationCount; i++)
			_bois.Deserialize<T>(TestBuffer, 0, TestBuffer.Length);
	}
}
