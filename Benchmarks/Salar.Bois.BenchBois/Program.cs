using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Salar.Bois.BenchmarksObjects.TestObjects;
using System;
using System.Linq;

namespace Salar.Bois.BenchBois;

internal static class Program
{
	private static void Main(string[] args)
	{
		IConfig config = DefaultConfig.Instance.AddDiagnoser(MemoryDiagnoser.Default);

		// use --job on the command line to pick another job, e.g. --job medium
		if (!args.Contains("--job", StringComparer.OrdinalIgnoreCase) && !args.Contains("-j"))
			config = config.AddJob(Job.ShortRun);

		BenchmarkSwitcher
			.FromTypes([
				typeof(BoisBenchmark<Test1_Arrays_Small>),
				typeof(BoisBenchmark<Test1_Arrays_Big>),
				typeof(BoisBenchmark<Test2_Objects>),
				typeof(BoisBufferBenchmark<Test1_Arrays_Small>),
				typeof(BoisBufferBenchmark<Test1_Arrays_Big>),
				typeof(BoisBufferBenchmark<Test2_Objects>),
#if NET9_0_OR_GREATER
				typeof(BoisSpanBufferBenchmark<Test1_Arrays_Small>),
				typeof(BoisSpanBufferBenchmark<Test1_Arrays_Big>),
				typeof(BoisSpanBufferBenchmark<Test2_Objects>),
#endif
				typeof(BoisCodeGenBenchmark_Small),
				typeof(BoisCodeGenBenchmark_Big),
				typeof(BoisCodeGenBenchmark_Objects),
			])
			.Run(args, config);
	}
}
