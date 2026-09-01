using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Salar.Bois.BenchmarksObjects.TestObjects;

namespace Salar.Bois.BenchBois;

internal static class Program
{
	private static void Main(string[] args)
	{
		var config = DefaultConfig.Instance.AddJob(Job.ShortRun);
		BenchmarkSwitcher
			.FromTypes([
				typeof(BoisBufferBenchmark<Test1_Arrays_Small>),
#if NET9_0_OR_GREATER
				typeof(BoisSpanBufferBenchmark<Test1_Arrays_Small>),
#endif
			])
			.Run(args, config);
	}
}
