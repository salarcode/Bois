using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Salar.Bois.BenchBois;
using Salar.Bois.BenchmarksObjects.TestObjects;
using Salar.Bois.BenchMessagePack;
using Salar.Bois.BenchProtobufNet;
using System.Linq;

namespace Salar.Bois.BenchmarksRun;

public class BenchRunner
{
	private readonly BenchEngine _engine;

	public BenchRunner()
	{
		_engine = new BenchEngine();
		SetupBenchmarks();
	}

	private void SetupBenchmarks()
	{
		// benchmark
		_engine.AddBenchmark(typeof(BoisBenchmark<>));
		_engine.AddBenchmark(typeof(BoisBufferBenchmark<>));
		_engine.AddBenchmark(typeof(BoisLz4Benchmark<>));
		_engine.AddBenchmark(typeof(MessagePackBenchmark<>));
		_engine.AddBenchmark(typeof(MessagePackLz4Benchmark<>));
		_engine.AddBenchmark(typeof(ProtobufNetBenchmark<>));
#if NET
		_engine.AddBenchmark(typeof(BenchMemoryPack.MemoryPackBenchmark<>));
#endif
		_engine.AddBenchmark(typeof(GroBufBenchmark<>));

		// test objects
		_engine.AddTestObject<Test1_Arrays_Small>();
		_engine.AddTestObject<Test1_Arrays_Big>();
	}

	public void RunAll()
	{
		var benchmarks = _engine.GetBenchmarkable();

		BenchmarkSwitcher.FromTypes(benchmarks.ToArray())
			.With(typeof(BenchRunner).Assembly)
			.RunAllJoined(new RunConfig());
	}

	public void RunSwitcher()
	{
		var benchmarks = _engine.GetBenchmarkable();

		BenchmarkSwitcher.FromTypes(benchmarks.ToArray())
			.With(typeof(BenchRunner).Assembly)
			.Run();
	}

	class RunConfig : ManualConfig
	{
		public RunConfig()
		{
			//Orderer = new DefaultOrderer(SummaryOrderPolicy.Declared);
			AddExporter(DefaultConfig.Instance.GetExporters().ToArray());
			AddAnalyser(DefaultConfig.Instance.GetAnalysers().ToArray());
			AddColumnProvider(DefaultConfig.Instance.GetColumnProviders().ToArray());
			AddLogger(DefaultConfig.Instance.GetLoggers().ToArray());
			AddValidator(DefaultConfig.Instance.GetValidators().ToArray());
			AddJob(DefaultConfig.Instance.GetJobs().ToArray());
			AddDiagnoser(DefaultConfig.Instance.GetDiagnosers().ToArray());
			AddHardwareCounters(DefaultConfig.Instance.GetHardwareCounters().ToArray());
			AddFilter(DefaultConfig.Instance.GetFilters().ToArray());
		}
	}
}
