using BenchmarkDotNet.Attributes;
using MessagePack;
using Salar.Bois.BenchmarksBase;
using Salar.Bois.BenchmarksObjects;

namespace Salar.Bois.BenchMessagePack;

public class MessagePackLz4Benchmark<T> : BenchmarkBase<T>
	where T : class, IBenchmarkTestObject, new()
{
	[Params("MessagePackLz4")]
	public override string TestName { get; set; }
	
	[Benchmark(Description = "Serialize")]
	[BenchmarkCategory("MessagePackLz4")]
	public override void Serialize()
	{
		for (int i = 0; i < IterationCount; i++)
		{
			Reset();

			var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
			MessagePackSerializer.Serialize(TestStream, TestObject, lz4Options);
		}
	}

	[Benchmark(Description = "Deserialize")]
	[BenchmarkCategory("MessagePackLz4")]
	public override void Deserialize()
	{
		for (int i = 0; i < IterationCount; i++)
		{
			Reset();
			var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
			MessagePackSerializer.Deserialize<T>(TestStream, lz4Options);
		}
	}
}