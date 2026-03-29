using BenchmarkDotNet.Attributes;
using Salar.Bois.BenchmarksBase;
using Salar.Bois.BenchmarksObjects.TestObjects;
using System.IO;

namespace Salar.Bois.BenchBois;

public class BoisBenchmark_Test1_Arrays_Big : BenchmarkBase<Test1_Arrays_Big>
{
	[Params("Bois")]
	public override string TestName { get; set; }


	[Benchmark(Description = "Serialize")]
	[BenchmarkCategory("Bois")]
	public override void Serialize()
	{
		for (int i = 0; i < IterationCount; i++)
		{
			Reset();
			Bois_Test1_Arrays_Big.WriteCompanyModel(TestObject, TestStream);
		}
	}

	[Benchmark(Description = "Deserialize")]
	[BenchmarkCategory("Bois")]
	public override void Deserialize()
	{
		for (int i = 0; i < IterationCount; i++)
		{
			Reset();
			Bois_Test1_Arrays_Big.ReadCompanyModel(TestStream);
		}
	}
}

public class BoisBenchmark_Test1_Arrays_Small : BenchmarkBase<Test1_Arrays_Small>
{
	[Params("Bois")]
	public override string TestName { get; set; }


	[Benchmark(Description = "Serialize")]
	[BenchmarkCategory("Bois")]
	public override void Serialize()
	{
		for (int i = 0; i < IterationCount; i++)
		{
			Reset();
			Bois_Test1_Arrays_Small.WriteCompanyModel(TestObject, TestStream);
		}
	}

	[Benchmark(Description = "Deserialize")]
	[BenchmarkCategory("Bois")]
	public override void Deserialize()
	{
		for (int i = 0; i < IterationCount; i++)
		{
			Reset();
			Bois_Test1_Arrays_Small.ReadCompanyModel(TestStream);
		}
	}
}

public static partial class Bois_Test1_Arrays_Big
{
	[BoisReader]
	public static partial Test1_Arrays_Big? ReadCompanyModel(Stream source);

	[BoisWriter]
	public static partial void WriteCompanyModel(Test1_Arrays_Big? model, Stream output);
}

public static partial class Bois_Test1_Arrays_Small
{
	[BoisReader]
	public static partial Test1_Arrays_Small? ReadCompanyModel(Stream source);

	[BoisWriter]
	public static partial void WriteCompanyModel(Test1_Arrays_Small? model, Stream output);
}