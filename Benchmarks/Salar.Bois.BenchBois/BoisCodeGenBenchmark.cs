using BenchmarkDotNet.Attributes;
using Salar.Bois.BenchmarksBase;
using Salar.Bois.BenchmarksObjects.TestObjects;
using System.IO;

namespace Salar.Bois.BenchBois;

public class BoisCodeGenBenchmark_Big : BenchmarkBase<Test1_Arrays_Big>
{
	[Params("BoisCodeGen")]
	public override string TestName { get; set; }

	[Benchmark(Description = "Serialize")]
	[BenchmarkCategory("BoisCodeGen")]
	public override void Serialize()
	{
		for (int i = 0; i < IterationCount; i++)
		{
			Reset();
			CodeGenBois_Big.Write(TestObject, TestStream);
		}
	}

	[Benchmark(Description = "Deserialize")]
	[BenchmarkCategory("BoisCodeGen")]
	public override void Deserialize()
	{
		for (int i = 0; i < IterationCount; i++)
		{
			Reset();
			CodeGenBois_Big.Read(TestStream);
		}
	}
}

public class BoisCodeGenBenchmark_Small : BenchmarkBase<Test1_Arrays_Small>
{
	[Params("BoisCodeGen")]
	public override string TestName { get; set; }

	[Benchmark(Description = "Serialize")]
	[BenchmarkCategory("BoisCodeGen")]
	public override void Serialize()
	{
		for (int i = 0; i < IterationCount; i++)
		{
			Reset();
			CodeGenBois_Small.Write(TestObject, TestStream);
		}
	}

	[Benchmark(Description = "Deserialize")]
	[BenchmarkCategory("BoisCodeGen")]
	public override void Deserialize()
	{
		for (int i = 0; i < IterationCount; i++)
		{
			Reset();
			CodeGenBois_Small.Read(TestStream);
		}
	}
}

public static partial class CodeGenBois_Big
{
	[CodeGen.BoisReader]
	public static partial Test1_Arrays_Big? Read(Stream source);

	[CodeGen.BoisWriter]
	public static partial void Write(Test1_Arrays_Big? model, Stream output);
}

public static partial class CodeGenBois_Small
{
	[CodeGen.BoisReader]
	public static partial Test1_Arrays_Small? Read(Stream source);

	[CodeGen.BoisWriter]
	public static partial void Write(Test1_Arrays_Small? model, Stream output);
}
