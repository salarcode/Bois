extern alias SalarBoisCodeGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Salar.Bois;
using Salar.Bois.Generator;
using Salar.Bois.NetFx.Tests.CodeGenFixtures;
using Salar.BinaryBuffers;
using Salar.BinaryBuffers.Compatibility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

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

	[Fact]
	public void MovedCompanyModelGeneratedMethodsRoundTripSupportedMembers()
	{
		var init = new CompanyModel
		{
			Id = Guid.Parse("3f67f0a3-6ef2-4649-8e6c-f41d1922b668"),
			Name = "Salar Code",
			Address = "Unit Test Street",
			Phone = "+1-555-0100",
			Founded = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc),
			Revenue = 123456.789m,
			IsActive = true,
		};
		init.AddEmployee("Alice");
		init.AddEmployee("Bob");

		var streamFinal = RoundTrip(init, CompanyModelBois.WriteCompanyModel, CompanyModelBois.ReadCompanyModel);
		AssertCompanySupportedMembers(init, streamFinal);

		var writerBuffer = new byte[4096];
		var writer = new BinaryBufferWriter(writerBuffer, 13, writerBuffer.Length - 13);
		CompanyModelBois.WriteCompanyModel(init, writer, Encoding.UTF8);
		var readerFinal = CompanyModelBois.ReadCompanyModel(new BinaryBufferReader(writerBuffer, 13, writerBuffer.Length - 13), Encoding.UTF8);
		AssertCompanySupportedMembers(init, readerFinal);

		var buffer = new byte[4096];
		CompanyModelBois.WriteCompanyModel(init, buffer, 11, buffer.Length - 11);
		var bufferFinal = CompanyModelBois.ReadCompanyModel(buffer, 11, buffer.Length - 11);
		AssertCompanySupportedMembers(init, bufferFinal);

		var wholeBuffer = new byte[4096];
		CompanyModelBois.WriteCompanyModel(init, wholeBuffer, 0, wholeBuffer.Length);
		var wholeBufferFinal = CompanyModelBois.ReadCompanyModel(wholeBuffer);
		AssertCompanySupportedMembers(init, wholeBufferFinal);

		var segmentBuffer = new byte[4096];
		CompanyModelBois.WriteCompanyModel(init, segmentBuffer, 17, segmentBuffer.Length - 17);
		var segment = new ArraySegment<byte>(segmentBuffer, 17, segmentBuffer.Length - 17);
		var segmentFinal = CompanyModelBois.ReadCompanyModel(segment);
		AssertCompanySupportedMembers(init, segmentFinal);

		var segmentInFinal = CompanyModelBois.ReadCompanyModelIn(in segment);
		AssertCompanySupportedMembers(init, segmentInFinal);

		var nestedFinal = RoundTrip(init, Holding.CompanyModelSerializer.WriteCompanyModel, Holding.CompanyModelSerializer.ReadCompanyModel);
		AssertCompanySupportedMembers(init, nestedFinal);
	}

	[Fact]
	public void MovedSourceGeneratorScenariosRoundTripPrimitiveAndNestedVariations()
	{
		var primitive = CreatePrimitiveScenario();
		var primitiveFinal = RoundTrip(primitive, SourceGeneratorScenariosBois.WritePrimitiveScenario, SourceGeneratorScenariosBois.ReadPrimitiveScenario);
		AssertPrimitiveScenario(primitive, primitiveFinal);

		var collection = new CollectionScenario
		{
			Names = ["one", "two"],
			Numbers = [1, 2, 3],
			Scores = { ["first"] = 10, ["second"] = 20 },
		};
		collection.Headers.Add("x-one", "1");
		collection.Headers.Add("x-two", "2");
		var collectionFinal = RoundTrip(collection, SourceGeneratorScenariosBois.WriteCollectionScenario, SourceGeneratorScenariosBois.ReadCollectionScenario);
		AssertCollectionScenario(collection, collectionFinal);

		var nested = new NestedScenario
		{
			Primitive = primitive,
			NullablePrimitive = CreatePrimitiveScenario("nullable"),
			Location = new ScenarioStruct { X = 7, Y = 9 },
			Empty = new EmptyScenario(),
		};
		var nestedFinal = RoundTrip(nested, SourceGeneratorScenariosBois.WriteNestedScenario, SourceGeneratorScenariosBois.ReadNestedScenario);
		AssertNestedScenario(nested, nestedFinal);

		var reused = new ReusedNestedScenario
		{
			First = CreatePrimitiveScenario("first"),
			Second = CreatePrimitiveScenario("second"),
			Optional = CreatePrimitiveScenario("optional"),
		};
		var reusedFinal = RoundTrip(reused, SourceGeneratorScenariosBois.WriteReusedNestedScenario, SourceGeneratorScenariosBois.ReadReusedNestedScenario);
		AssertPrimitiveScenario(reused.First, reusedFinal!.First);
		AssertPrimitiveScenario(reused.Second, reusedFinal.Second);
		AssertPrimitiveScenario(reused.Optional, reusedFinal.Optional);
	}

	[Fact]
	public void MovedSourceGeneratorScenariosRoundTripSameTypeAndRootKnownTypes()
	{
		var sameType = new SameTypeCastingScenario
		{
			FirstInt32 = 1,
			SecondInt32 = 2,
			OptionalInt32 = 3,
			FirstText = "first",
			SecondText = "second",
			OptionalText1 = "optional-1",
			OptionalText2 = null,
			FirstStatus = ScenarioStatus.Active,
			SecondStatus = ScenarioStatus.Archived,
			ThirdStatus = ScenarioStatus.Unknown,
			OptionalStatus1 = ScenarioStatus.Active,
			OptionalStatus2 = null,
			OptionalStatus3 = ScenarioStatus.Archived,
			FirstLocation = new ScenarioStruct { X = 10, Y = 11 },
			SecondLocation = new ScenarioStruct { X = 20, Y = 21 },
			FirstPrimitive = CreatePrimitiveScenario("first-primitive"),
			SecondPrimitive = CreatePrimitiveScenario("second-primitive"),
			OptionalPrimitive1 = CreatePrimitiveScenario("optional-primitive-1"),
			OptionalPrimitive2 = null,
			StatusHistory1 = [ScenarioStatus.Unknown, ScenarioStatus.Active],
			StatusHistory2 = [ScenarioStatus.Archived, ScenarioStatus.Active],
			StatusValues1 = { [ScenarioStatus.Active] = CreatePrimitiveScenario("active") },
			StatusValues2 = { [ScenarioStatus.Archived] = CreatePrimitiveScenario("archived") },
		};
		var sameTypeFinal = RoundTrip(sameType, SourceGeneratorScenariosBois.WriteSameTypeCastingScenario, SourceGeneratorScenariosBois.ReadSameTypeCastingScenario);
		AssertSameTypeCastingScenario(sameType, sameTypeFinal);

		var contract = new ContractScenario { First = 1, Second = 2, Third = 3, Ignored = 4 };
		var contractFinal = RoundTrip(contract, SourceGeneratorScenariosBois.WriteContractScenario, SourceGeneratorScenariosBois.ReadContractScenario);
		Assert.NotNull(contractFinal);
		Assert.Equal(contract.First, contractFinal.First);
		Assert.Equal(contract.Second, contractFinal.Second);
		Assert.Equal(contract.Third, contractFinal.Third);
		Assert.Equal(default, contractFinal.Ignored);

		var fieldsOnly = new ContractFieldsOnlyScenario { FieldValue = 10, IgnoredProperty = 20 };
		var fieldsOnlyFinal = RoundTrip(fieldsOnly, SourceGeneratorScenariosBois.WriteContractFieldsOnlyScenario, SourceGeneratorScenariosBois.ReadContractFieldsOnlyScenario);
		Assert.NotNull(fieldsOnlyFinal);
		Assert.Equal(fieldsOnly.FieldValue, fieldsOnlyFinal.FieldValue);
		Assert.Equal(default, fieldsOnlyFinal.IgnoredProperty);

		var propertiesOnly = new ContractPropertiesOnlyScenario { IgnoredField = 30, PropertyValue = 40 };
		var propertiesOnlyFinal = RoundTrip(propertiesOnly, SourceGeneratorScenariosBois.WriteContractPropertiesOnlyScenario, SourceGeneratorScenariosBois.ReadContractPropertiesOnlyScenario);
		Assert.NotNull(propertiesOnlyFinal);
		Assert.Equal(default, propertiesOnlyFinal.IgnoredField);
		Assert.Equal(propertiesOnly.PropertyValue, propertiesOnlyFinal.PropertyValue);

		var structValue = new ScenarioStruct { X = 42, Y = 43 };
		var structFinal = RoundTrip(structValue, SourceGeneratorScenariosBois.WriteScenarioStruct, SourceGeneratorScenariosBois.ReadScenarioStruct);
		Assert.Equal(structValue.X, structFinal.X);
		Assert.Equal(structValue.Y, structFinal.Y);

		Assert.Empty(RoundTrip(new EmptyScenario(), SourceGeneratorScenariosBois.WriteEmptyScenario, SourceGeneratorScenariosBois.ReadEmptyScenario)!.GetType().GetProperties());
		Assert.Equal(["a", "b"], RoundTrip(new List<string> { "a", "b" }, SourceGeneratorScenariosBois.WriteStringList, SourceGeneratorScenariosBois.ReadStringList));
		Assert.Equal(new Dictionary<string, int> { ["one"] = 1, ["two"] = 2 }, RoundTrip(new Dictionary<string, int> { ["one"] = 1, ["two"] = 2 }, SourceGeneratorScenariosBois.WriteStringIntDictionary, SourceGeneratorScenariosBois.ReadStringIntDictionary));
		Assert.Equal([1, 2, 3], RoundTrip(new[] { 1, 2, 3 }, SourceGeneratorScenariosBois.WriteInt32Array, SourceGeneratorScenariosBois.ReadInt32Array));
		Assert.Equal("hello", RoundTripString("hello"));
	}

	[Fact]
	public void GeneratedSourceForDifferentNestedTypesCompiles()
	{
		var source = """
			using Salar.Bois;
			using System.IO;

			namespace GeneratorCompileSample;

			public sealed class TypeA
			{
				public int Value { get; set; }
			}

			public sealed class TypeB
			{
				public string Text { get; set; } = "";
			}

			public sealed class DifferentNestedTypesModel
			{
				public TypeA First { get; set; } = new();
				public TypeB Second { get; set; } = new();
			}

			public static partial class DifferentNestedTypesModelBois
			{
				[BoisReader]
				public static partial DifferentNestedTypesModel? Read(Stream source);

				[BoisWriter]
				public static partial void Write(DifferentNestedTypesModel? model, Stream output);
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

		Assert.Empty(generatorDiagnostics.Where(static x => x.Severity == DiagnosticSeverity.Error));
		Assert.Empty(outputCompilation.GetDiagnostics().Where(static x => x.Severity == DiagnosticSeverity.Error));
	}

	[Fact]
	public void DifferentNestedTypesRoundTripReadAndWrite()
	{
		var init = new DifferentNestedTypesModel
		{
			First = new DifferentNestedTypeA { Value = 42 },
			Second = new DifferentNestedTypeB { Text = "hello" },
		};

		using var stream = new MemoryStream();
		DifferentNestedTypesModelBois.Write(init, stream);
		stream.Position = 0;
		var final = DifferentNestedTypesModelBois.Read(stream);

		Assert.NotNull(final);
		Assert.NotNull(final.First);
		Assert.Equal(init.First.Value, final.First.Value);
		Assert.NotNull(final.Second);
		Assert.Equal(init.Second.Text, final.Second.Text);
	}

	private static IReadOnlyList<MetadataReference> GetMetadataReferences()
	{
		var references = new Dictionary<string, MetadataReference>(StringComparer.OrdinalIgnoreCase);
		var trustedAssemblies = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))?.Split(Path.PathSeparator) ?? [];

		foreach (var path in trustedAssemblies)
			AddReference(references, path);

		AddReference(references, typeof(BoisReaderAttribute).Assembly.Location);
		AddReference(references, typeof(BufferReaderBase).Assembly.Location);
		AddReference(references, typeof(SalarBoisCodeGen::Salar.Bois.CodeGen.BoisCodeGen).Assembly.Location);

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

	private static T? RoundTrip<T>(T? init, Action<T?, Stream> write, Func<Stream, T?> read)
	{
		using var stream = new MemoryStream();
		write(init, stream);
		stream.Position = 0;
		return read(stream);
	}

	private static string? RoundTripString(string? init)
	{
		using var stream = new MemoryStream();
		SourceGeneratorScenariosBois.WriteString(init, stream, Encoding.UTF8);
		stream.Position = 0;
		return SourceGeneratorScenariosBois.ReadString(stream, Encoding.UTF8);
	}

	private static PrimitiveScenario CreatePrimitiveScenario(string suffix = "value")
	{
		return new PrimitiveScenario
		{
			Text = "text-" + suffix,
			NullableText = "nullable-" + suffix,
			BoolValue = true,
			NullableBool = false,
			CharValue = 'A',
			NullableChar = 'Z',
			Int16Value = -16,
			NullableInt16 = 16,
			Int32Value = -32,
			NullableInt32 = 32,
			Int64Value = -64,
			NullableInt64 = 64,
			UInt16Value = 16,
			NullableUInt16 = 17,
			UInt32Value = 32,
			NullableUInt32 = 33,
			UInt64Value = 64,
			NullableUInt64 = 65,
			SingleValue = 1.25f,
			NullableSingle = 2.5f,
			DoubleValue = 3.75,
			NullableDouble = 4.125,
			DecimalValue = 5.5m,
			NullableDecimal = 6.75m,
			ByteValue = 7,
			NullableByte = 8,
			SByteValue = -9,
			NullableSByte = 10,
			DateTimeValue = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc),
			NullableDateTime = new DateTime(2026, 2, 3, 4, 5, 6, DateTimeKind.Utc),
			DateTimeOffsetValue = new DateTimeOffset(2026, 3, 4, 5, 6, 7, TimeSpan.Zero),
			NullableDateTimeOffset = new DateTimeOffset(2026, 4, 5, 6, 7, 8, TimeSpan.Zero),
			DateOnlyValue = new DateOnly(2026, 5, 6),
			NullableDateOnly = new DateOnly(2026, 6, 7),
			TimeOnlyValue = new TimeOnly(8, 9, 10),
			NullableTimeOnly = new TimeOnly(11, 12, 13),
			TimeSpanValue = TimeSpan.FromSeconds(14),
			NullableTimeSpan = TimeSpan.FromSeconds(15),
			GuidValue = Guid.Parse("89d1f11d-ad46-47da-9747-e240ff17e6ac"),
			NullableGuid = Guid.Parse("1739e49f-56ee-49f1-a9d1-81a899ca0a8e"),
			ColorValue = Color.FromArgb(1, 2, 3),
			NullableColor = Color.FromArgb(4, 5, 6),
			UriValue = new Uri("https://example.com/" + suffix),
			VersionValue = new Version(1, 2, 3, 4),
			Status = ScenarioStatus.Active,
			NullableStatus = ScenarioStatus.Archived,
			Bytes = [1, 2, 3],
			KnownTypeArray = [4, 5, 6],
		};
	}

	private static void AssertCompanySupportedMembers(CompanyModel expected, CompanyModel? actual)
	{
		Assert.NotNull(actual);
		Assert.Equal(expected.Id, actual.Id);
		Assert.Equal(expected.Name, actual.Name);
		Assert.Equal(expected.Address, actual.Address);
		Assert.Equal(expected.Phone, actual.Phone);
		Assert.Equal(expected.Founded, actual.Founded);
		Assert.Equal(expected.Revenue, actual.Revenue);
		Assert.Equal(expected.IsActive, actual.IsActive);
	}

	private static void AssertPrimitiveScenario(PrimitiveScenario? expected, PrimitiveScenario? actual)
	{
		Assert.NotNull(expected);
		Assert.NotNull(actual);
		Assert.Equal(expected.Text, actual.Text);
		Assert.Equal(expected.NullableText, actual.NullableText);
		Assert.Equal(expected.BoolValue, actual.BoolValue);
		Assert.Equal(expected.NullableBool, actual.NullableBool);
		Assert.Equal(expected.CharValue, actual.CharValue);
		Assert.Equal(expected.NullableChar, actual.NullableChar);
		Assert.Equal(expected.Int16Value, actual.Int16Value);
		Assert.Equal(expected.NullableInt16, actual.NullableInt16);
		Assert.Equal(expected.Int32Value, actual.Int32Value);
		Assert.Equal(expected.NullableInt32, actual.NullableInt32);
		Assert.Equal(expected.Int64Value, actual.Int64Value);
		Assert.Equal(expected.NullableInt64, actual.NullableInt64);
		Assert.Equal(expected.UInt16Value, actual.UInt16Value);
		Assert.Equal(expected.NullableUInt16, actual.NullableUInt16);
		Assert.Equal(expected.UInt32Value, actual.UInt32Value);
		Assert.Equal(expected.NullableUInt32, actual.NullableUInt32);
		Assert.Equal(expected.UInt64Value, actual.UInt64Value);
		Assert.Equal(expected.NullableUInt64, actual.NullableUInt64);
		Assert.Equal(expected.SingleValue, actual.SingleValue);
		Assert.Equal(expected.NullableSingle, actual.NullableSingle);
		Assert.Equal(expected.DoubleValue, actual.DoubleValue);
		Assert.Equal(expected.NullableDouble, actual.NullableDouble);
		Assert.Equal(expected.DecimalValue, actual.DecimalValue);
		Assert.Equal(expected.NullableDecimal, actual.NullableDecimal);
		Assert.Equal(expected.ByteValue, actual.ByteValue);
		Assert.Equal(expected.NullableByte, actual.NullableByte);
		Assert.Equal(expected.SByteValue, actual.SByteValue);
		Assert.Equal(expected.NullableSByte, actual.NullableSByte);
		Assert.Equal(expected.DateTimeValue, actual.DateTimeValue);
		Assert.Equal(expected.NullableDateTime, actual.NullableDateTime);
		Assert.Equal(expected.DateTimeOffsetValue, actual.DateTimeOffsetValue);
		Assert.Equal(expected.NullableDateTimeOffset, actual.NullableDateTimeOffset);
		Assert.Equal(expected.DateOnlyValue, actual.DateOnlyValue);
		Assert.Equal(expected.NullableDateOnly, actual.NullableDateOnly);
		Assert.Equal(expected.TimeOnlyValue, actual.TimeOnlyValue);
		Assert.Equal(expected.NullableTimeOnly, actual.NullableTimeOnly);
		Assert.Equal(expected.TimeSpanValue, actual.TimeSpanValue);
		Assert.Equal(expected.NullableTimeSpan, actual.NullableTimeSpan);
		Assert.Equal(expected.GuidValue, actual.GuidValue);
		Assert.Equal(expected.NullableGuid, actual.NullableGuid);
		Assert.Equal(expected.ColorValue, actual.ColorValue);
		Assert.Equal(expected.NullableColor, actual.NullableColor);
		Assert.Equal(expected.UriValue, actual.UriValue);
		Assert.Equal(expected.VersionValue, actual.VersionValue);
		Assert.Equal(expected.Status, actual.Status);
		Assert.Equal(expected.NullableStatus, actual.NullableStatus);
		Assert.Equal(expected.Bytes, actual.Bytes);
		Assert.Equal(expected.KnownTypeArray, actual.KnownTypeArray);
	}

	private static void AssertCollectionScenario(CollectionScenario expected, CollectionScenario? actual)
	{
		Assert.NotNull(actual);
		Assert.Equal(expected.Names, actual.Names);
		Assert.Equal(expected.Numbers.OrderBy(static x => x), actual.Numbers.OrderBy(static x => x));
		Assert.Equal(expected.Scores, actual.Scores);
		Assert.Equal(expected.Headers.AllKeys, actual.Headers.AllKeys);
		foreach (var key in expected.Headers.AllKeys)
			Assert.Equal(expected.Headers[key], actual.Headers[key]);
	}

	private static void AssertNestedScenario(NestedScenario expected, NestedScenario? actual)
	{
		Assert.NotNull(actual);
		AssertPrimitiveScenario(expected.Primitive, actual.Primitive);
		AssertPrimitiveScenario(expected.NullablePrimitive, actual.NullablePrimitive);
		Assert.Equal(expected.Location.X, actual.Location.X);
		Assert.Equal(expected.Location.Y, actual.Location.Y);
		Assert.NotNull(actual.Empty);
	}

	private static void AssertSameTypeCastingScenario(SameTypeCastingScenario expected, SameTypeCastingScenario? actual)
	{
		Assert.NotNull(actual);
		Assert.Equal(expected.FirstInt32, actual.FirstInt32);
		Assert.Equal(expected.SecondInt32, actual.SecondInt32);
		Assert.Equal(expected.OptionalInt32, actual.OptionalInt32);
		Assert.Equal(expected.FirstText, actual.FirstText);
		Assert.Equal(expected.SecondText, actual.SecondText);
		Assert.Equal(expected.OptionalText1, actual.OptionalText1);
		Assert.Equal(expected.OptionalText2, actual.OptionalText2);
		Assert.Equal(expected.FirstStatus, actual.FirstStatus);
		Assert.Equal(expected.SecondStatus, actual.SecondStatus);
		Assert.Equal(expected.ThirdStatus, actual.ThirdStatus);
		Assert.Equal(expected.OptionalStatus1, actual.OptionalStatus1);
		Assert.Equal(expected.OptionalStatus2, actual.OptionalStatus2);
		Assert.Equal(expected.OptionalStatus3, actual.OptionalStatus3);
		Assert.Equal(expected.FirstLocation.X, actual.FirstLocation.X);
		Assert.Equal(expected.FirstLocation.Y, actual.FirstLocation.Y);
		Assert.Equal(expected.SecondLocation.X, actual.SecondLocation.X);
		Assert.Equal(expected.SecondLocation.Y, actual.SecondLocation.Y);
		AssertPrimitiveScenario(expected.FirstPrimitive, actual.FirstPrimitive);
		AssertPrimitiveScenario(expected.SecondPrimitive, actual.SecondPrimitive);
		AssertPrimitiveScenario(expected.OptionalPrimitive1, actual.OptionalPrimitive1);
		Assert.Null(actual.OptionalPrimitive2);
		Assert.Equal(expected.StatusHistory1, actual.StatusHistory1);
		Assert.Equal(expected.StatusHistory2, actual.StatusHistory2);
		AssertPrimitiveScenario(expected.StatusValues1[ScenarioStatus.Active], actual.StatusValues1[ScenarioStatus.Active]);
		AssertPrimitiveScenario(expected.StatusValues2[ScenarioStatus.Archived], actual.StatusValues2[ScenarioStatus.Archived]);
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

public sealed class DifferentNestedTypeA
{
	public int Value { get; set; }
}

public sealed class DifferentNestedTypeB
{
	public string Text { get; set; } = string.Empty;
}

public sealed class DifferentNestedTypesModel
{
	public DifferentNestedTypeA First { get; set; } = new();
	public DifferentNestedTypeB Second { get; set; } = new();
}

public static partial class DifferentNestedTypesModelBois
{
	[BoisReader]
	public static partial DifferentNestedTypesModel? Read(Stream source);

	[BoisWriter]
	public static partial void Write(DifferentNestedTypesModel? model, Stream output);
}
