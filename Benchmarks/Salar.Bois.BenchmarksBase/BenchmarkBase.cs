using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using Salar.Bois.BenchmarksObjects;
using System;
using System.IO;

namespace Salar.Bois.BenchmarksBase;

[HideColumns(Column.Namespace)]
public abstract class BenchmarkBase<T> : IDisposable, IBenchmark
	where T : class, IBenchmarkTestObject, new()
{
	public int IterationCount = 1000;

	protected MemoryStream TestStream { get; } = new MemoryStream();

	protected T TestObject { get; private set; }

	protected byte[] TestBuffer { get; set; } = Array.Empty<byte>();

	public abstract void Serialize();

	public abstract void Deserialize();

	public abstract string TestName { get; set; }

	[ParamsSource(nameof(DataSizeValues))]
	public int DataSize
	{
		get => TestBuffer.Length;
		set { }
	}

	public int[] DataSizeValues()
	{
		GlobalSetup();
		return new[] { TestBuffer.Length };
	}

	[GlobalSetup]
	public virtual void GlobalSetup()
	{
		TestObject = new T();
		Serialize();
		if (TestStream.Length > 0)
			TestBuffer = TestStream.ToArray();
	}

	[GlobalCleanup]
	public void GlobalCleanup()
	{
		if (TestStream is not null)
		{
			TestStream.Position = 0;
			TestStream.Dispose();
		}
		TestBuffer = Array.Empty<byte>();
	}

	[IterationSetup]
	public virtual void Reset()
	{
		TestStream.Position = 0;
	}

	public void Dispose()
	{
		TestStream?.Dispose();
	}


}
