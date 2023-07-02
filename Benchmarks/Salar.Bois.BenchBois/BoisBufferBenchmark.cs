using BenchmarkDotNet.Attributes;
using Salar.BinaryBuffers;
using Salar.Bois.BenchmarksBase;
using Salar.Bois.BenchmarksObjects;

namespace Salar.Bois.BenchBois;

public class BoisBufferBenchmark<T> : BenchmarkBase<T>
	where T : class, IBenchmarkTestObject, new()
{
	private readonly BoisSerializer _serializer;
	private readonly BinaryBufferWriter _binaryBufferWriter;
	private readonly BinaryBufferReader _binaryBufferReader;

	[Params("Bois.Buffer")]
	public override string TestName { get; set; }

	public BoisBufferBenchmark()
	{
		TestBuffer = new byte[1024 * 1024];
		_serializer = new BoisSerializer();
		_binaryBufferWriter = new BinaryBufferWriter(TestBuffer);
		_binaryBufferReader = new BinaryBufferReader(TestBuffer);
	}

	public override void GlobalSetup()
	{
		base.GlobalSetup();
		TestBuffer = _binaryBufferWriter.ToArray();
	}

	public override void Reset()
	{
		_binaryBufferWriter.Position = 0;
		_binaryBufferReader.Position = 0;
	}

	[Benchmark(Description = "Serialize")]
	[BenchmarkCategory("Bois.Buffer")]
	public override void Serialize()
	{
		for (int i = 0; i < IterationCount; i++)
		{
			_binaryBufferWriter.Position = 0;
			_serializer.Serialize(TestObject, _binaryBufferWriter);
		}
	}

	[Benchmark(Description = "Deserialize")]
	[BenchmarkCategory("Bois.Buffer")]
	public override void Deserialize()
	{
		for (int i = 0; i < IterationCount; i++)
		{
			_binaryBufferReader.Position = 0;
			_serializer.Deserialize<T>(_binaryBufferReader);
		}
	}
}