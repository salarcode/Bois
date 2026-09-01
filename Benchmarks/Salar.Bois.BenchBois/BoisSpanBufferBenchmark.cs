#if NET9_0_OR_GREATER
using BenchmarkDotNet.Attributes;
using Salar.BinaryBuffers;
using Salar.Bois.BenchmarksBase;
using Salar.Bois.BenchmarksObjects;
using System;

namespace Salar.Bois.BenchBois;

/// <summary>
/// Benchmarks the span based buffers. These are ref structs, so the emitted serializers are
/// specialized for them and the buffer never leaves the stack.
/// Only available where generics may be instantiated with ref structs.
/// </summary>
public class BoisSpanBufferBenchmark<T> : BenchmarkBase<T>
	where T : class, IBenchmarkTestObject, new()
{
	private readonly byte[] _writeBuffer = new byte[1024 * 1024];
	private readonly BoisSerializer _bois = new BoisSerializer();

	[Params("Bois.SpanBuffer")]
	public override string TestName { get; set; }

	public override void GlobalSetup()
	{
		TestObject = new T();

		// produce the payload through the span writer so Deserialize has valid data
		var writer = new BinarySpanBufferWriter(_writeBuffer.AsSpan());
		_bois.Serialize(TestObject, ref writer);

		TestBuffer = _writeBuffer;
	}

	public override void Reset()
	{
	}

	[Benchmark(Description = "Serialize")]
	[BenchmarkCategory("Bois.SpanBuffer")]
	public override void Serialize()
	{
		for (int i = 0; i < IterationCount; i++)
		{
			// the writer is a ref struct, it must be created within this scope
			var writer = new BinarySpanBufferWriter(_writeBuffer.AsSpan());
			_bois.Serialize(TestObject, ref writer);
		}
	}

	[Benchmark(Description = "Deserialize")]
	[BenchmarkCategory("Bois.SpanBuffer")]
	public override void Deserialize()
	{
		for (int i = 0; i < IterationCount; i++)
		{
			var reader = new BinarySpanBufferReader(TestBuffer.AsSpan());
			_bois.Deserialize<T, BinarySpanBufferReader>(ref reader);
		}
	}
}
#endif
