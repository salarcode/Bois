using Salar.Bois.BenchmarksObjects;
using System;
using System.Collections.Generic;

namespace Salar.Bois.BenchmarksRun;
public class BenchEngine
{
	private readonly List<Type> _benchmarks;
	private readonly List<Type> _testObjects;

	public bool OutputMarkdown { get; set; } = true;

	public BenchEngine()
	{
		_benchmarks = new List<Type>();
		_testObjects = new List<Type>();
	}

	public void AddBenchmark<T>()
	{
		_benchmarks.Add(typeof(T));
	}

	public void AddBenchmark(params Type[] testObjects)
	{
		_benchmarks.AddRange(testObjects);
	}

	public void AddTestObject<T>()
		where T : IBenchmarkTestObject
	{
		_testObjects.Add(typeof(T));
	}

	public void AddTestObject(params Type[] testObjects)
	{
		_testObjects.AddRange(testObjects);
	}

	public IEnumerable<Type> GetBenchmarkable()
	{
		foreach (var testObjectType in _testObjects)
		{
			foreach (var benchmarkType in _benchmarks)
			{
				yield return benchmarkType.MakeGenericType(testObjectType);
			}
		}
	}
}
