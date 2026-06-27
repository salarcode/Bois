using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Salar.Bois;
using Salar.Bois.CodeGen;
using Salar.Bois.Generator;
using Salar.BinaryBuffers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Salar.Bois.NetFx.Tests.Tests;

public class Test_CodeGenGenerator
{
	[Fact]
	public void GeneratedSourceForRepeatedNullableEnumsCompiles()
	{
		var source = """
			using Salar.Bois;
			using System.IO;

			namespace GeneratorCompileSample;

			public enum SampleStatus
			{
				Unknown,
				Active,
				Archived,
			}

			public sealed class RepeatedEnumModel
			{
				public SampleStatus FirstStatus { get; set; }
				public SampleStatus SecondStatus { get; set; }
				public SampleStatus? OptionalStatus1 { get; set; }
				public SampleStatus? OptionalStatus2 { get; set; }
				public SampleStatus? OptionalStatus3 { get; set; }
			}

			public static partial class RepeatedEnumModelBois
			{
				[BoisReader]
				public static partial RepeatedEnumModel? Read(Stream source);

				[BoisWriter]
				public static partial void Write(RepeatedEnumModel? model, Stream output);
			}
			""";

		var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest));
		var compilation = CSharpCompilation.Create(
			"GeneratorCompileSample",
			[syntaxTree],
			GetMetadataReferences(),
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		GeneratorDriver driver = CSharpGeneratorDriver.Create(new BoisSourceGenerator());
		driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generatorDiagnostics);
		var runResult = driver.GetRunResult();

		Assert.Empty(generatorDiagnostics.Where(static x => x.Severity == DiagnosticSeverity.Error));
		Assert.DoesNotContain("enumValue", string.Concat(runResult.GeneratedTrees.Select(static x => x.GetText().ToString())));
		Assert.Empty(outputCompilation.GetDiagnostics().Where(static x => x.Severity == DiagnosticSeverity.Error));
	}

	[Fact]
	public void GeneratedReaderAndWriterRoundTripRepeatedSameTypeValues()
	{
		var init = new CodeGenSameTypeRuntimeModel
		{
			FirstInt32 = 10,
			SecondInt32 = 20,
			OptionalInt32 = 30,
			FirstText = "first",
			SecondText = "second",
			OptionalText1 = "optional-1",
			OptionalText2 = null,
			FirstStatus = CodeGenRuntimeStatus.Active,
			SecondStatus = CodeGenRuntimeStatus.Archived,
			ThirdStatus = CodeGenRuntimeStatus.Unknown,
			OptionalStatus1 = CodeGenRuntimeStatus.Active,
			OptionalStatus2 = null,
			OptionalStatus3 = CodeGenRuntimeStatus.Archived,
			FirstNested = new CodeGenNestedRuntimeModel { Id = 1, Name = "one" },
			SecondNested = new CodeGenNestedRuntimeModel { Id = 2, Name = "two" },
			StatusHistory1 = [CodeGenRuntimeStatus.Unknown, CodeGenRuntimeStatus.Active],
			StatusHistory2 = [CodeGenRuntimeStatus.Archived, CodeGenRuntimeStatus.Active],
			StatusValues1 =
			{
				[CodeGenRuntimeStatus.Active] = new CodeGenNestedRuntimeModel { Id = 3, Name = "three" },
			},
			StatusValues2 =
			{
				[CodeGenRuntimeStatus.Archived] = new CodeGenNestedRuntimeModel { Id = 4, Name = "four" },
			},
		};

		using var stream = new MemoryStream();

		CodeGenSameTypeRuntimeModelBois.Write(init, stream);
		stream.Position = 0;
		var final = CodeGenSameTypeRuntimeModelBois.Read(stream);

		Assert.NotNull(final);
		Assert.Equal(init.FirstInt32, final.FirstInt32);
		Assert.Equal(init.SecondInt32, final.SecondInt32);
		Assert.Equal(init.OptionalInt32, final.OptionalInt32);
		Assert.Equal(init.FirstText, final.FirstText);
		Assert.Equal(init.SecondText, final.SecondText);
		Assert.Equal(init.OptionalText1, final.OptionalText1);
		Assert.Equal(init.OptionalText2, final.OptionalText2);
		Assert.Equal(init.FirstStatus, final.FirstStatus);
		Assert.Equal(init.SecondStatus, final.SecondStatus);
		Assert.Equal(init.ThirdStatus, final.ThirdStatus);
		Assert.Equal(init.OptionalStatus1, final.OptionalStatus1);
		Assert.Equal(init.OptionalStatus2, final.OptionalStatus2);
		Assert.Equal(init.OptionalStatus3, final.OptionalStatus3);
		AssertNested(init.FirstNested, final.FirstNested);
		AssertNested(init.SecondNested, final.SecondNested);
		Assert.Equal(init.StatusHistory1, final.StatusHistory1);
		Assert.Equal(init.StatusHistory2, final.StatusHistory2);
		AssertNested(init.StatusValues1[CodeGenRuntimeStatus.Active], final.StatusValues1[CodeGenRuntimeStatus.Active]);
		AssertNested(init.StatusValues2[CodeGenRuntimeStatus.Archived], final.StatusValues2[CodeGenRuntimeStatus.Archived]);
	}

	private static IReadOnlyList<MetadataReference> GetMetadataReferences()
	{
		var references = new Dictionary<string, MetadataReference>(StringComparer.OrdinalIgnoreCase);
		var trustedAssemblies = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))?.Split(Path.PathSeparator) ?? [];

		foreach (var path in trustedAssemblies)
			AddReference(references, path);

		AddReference(references, typeof(BoisReaderAttribute).Assembly.Location);
		AddReference(references, typeof(BufferReaderBase).Assembly.Location);
		AddReference(references, typeof(BoisCodeGen).Assembly.Location);

		return references.Values.ToArray();
	}

	private static void AddReference(Dictionary<string, MetadataReference> references, string? path)
	{
		if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
			references.TryAdd(path, MetadataReference.CreateFromFile(path));
	}

	private static void AssertNested(CodeGenNestedRuntimeModel expected, CodeGenNestedRuntimeModel actual)
	{
		Assert.Equal(expected.Id, actual.Id);
		Assert.Equal(expected.Name, actual.Name);
	}
}

public enum CodeGenRuntimeStatus
{
	Unknown,
	Active,
	Archived,
}

public sealed class CodeGenNestedRuntimeModel
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
}

public sealed class CodeGenSameTypeRuntimeModel
{
	public int FirstInt32 { get; set; }
	public int SecondInt32 { get; set; }
	public int? OptionalInt32 { get; set; }
	public string FirstText { get; set; } = string.Empty;
	public string SecondText { get; set; } = string.Empty;
	public string? OptionalText1 { get; set; }
	public string? OptionalText2 { get; set; }
	public CodeGenRuntimeStatus FirstStatus { get; set; }
	public CodeGenRuntimeStatus SecondStatus { get; set; }
	public CodeGenRuntimeStatus ThirdStatus { get; set; }
	public CodeGenRuntimeStatus? OptionalStatus1 { get; set; }
	public CodeGenRuntimeStatus? OptionalStatus2 { get; set; }
	public CodeGenRuntimeStatus? OptionalStatus3 { get; set; }
	public CodeGenNestedRuntimeModel FirstNested { get; set; } = new();
	public CodeGenNestedRuntimeModel SecondNested { get; set; } = new();
	public List<CodeGenRuntimeStatus> StatusHistory1 { get; set; } = [];
	public List<CodeGenRuntimeStatus> StatusHistory2 { get; set; } = [];
	public Dictionary<CodeGenRuntimeStatus, CodeGenNestedRuntimeModel> StatusValues1 { get; set; } = [];
	public Dictionary<CodeGenRuntimeStatus, CodeGenNestedRuntimeModel> StatusValues2 { get; set; } = [];
}


public static partial class CodeGenSameTypeRuntimeModelBois
{
	[BoisReader]
	public static partial CodeGenSameTypeRuntimeModel? Read(Stream source);

	[BoisWriter]
	public static partial void Write(CodeGenSameTypeRuntimeModel? model, Stream output);
}
