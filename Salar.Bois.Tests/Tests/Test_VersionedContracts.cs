using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Salar.Bois.Generator;
using Salar.Bois.NetFx.Tests.CodeGenFixtures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Salar.Bois.NetFx.Tests.Tests;

[BoisContract(Versioned = true)]
[BoisReserved(2, typeof(string))]
[BoisReserved(4, typeof(BoisSkip.Contract))]
public class VersionedOrder
{
    [BoisMember(1)] public int Id { get; set; }
    [BoisMember(3)] public decimal? Discount { get; set; }
    [BoisMember(5)] public List<VersionedOrderLine>? Lines { get; set; }
    [BoisMember(false)] public string? DisplayText { get; set; }
    public int LineCount => Lines?.Count ?? 0;
}

[BoisContract(Versioned = true)]
public class VersionedOrderLine
{
    [BoisMember(1)]
    public string? Name { get; set; }
}

[BoisReserved(2, typeof(Guid?))]
public class VersionedBase
{
    [BoisMember(1)]
    public int BaseValue { get; set; }
}

[BoisContract(Versioned = true)]
public class VersionedDerived : VersionedBase
{
    [BoisMember(3)]
    public int DerivedValue;
    [BoisMember(4)]
    public Dictionary<string, int[]>? Values { get; set; }
}

[BoisContract(Versioned = true)]
public struct VersionedStruct
{
    [BoisMember(1)]
    public int X { get; set; }
}

public class Test_VersionedContracts
{
    [Fact]
    public void DescribeListsTheSlots()
    {
        Assert.Equal(
            "VersionedOrder (versioned)\n" +
            "1: Id int\n" +
            "2: reserved string\n" +
            "3: Discount decimal?\n" +
            "4: reserved BoisSkip.Contract\n" +
            "5: Lines List<VersionedOrderLine>",
            BoisContractInfo.Describe(typeof(VersionedOrder)));
    }

    [Fact]
    public void DescribeReadsBaseClassesAndFields()
    {
        Assert.Equal(
            "VersionedDerived (versioned)\n" +
            "1: BaseValue int\n" +
            "2: reserved Guid?\n" +
            "3: DerivedValue int\n" +
            "4: Values Dictionary<string, int[]>",
            BoisContractInfo.Describe(typeof(VersionedDerived)));

        Assert.Equal("VersionedStruct (versioned)\n1: X int", BoisContractInfo.Describe(typeof(VersionedStruct)));
    }

    [Fact]
    public void DescribeListsTheCurrentOrderOfOtherTypes()
    {
        // [BoisMember(index)] is an ordering hint here
        Assert.Equal(
            "ContractScenario (not versioned)\n" +
            "1: First int\n" +
            "2: Second int\n" +
            "3: Third int",
            BoisContractInfo.Describe(typeof(ContractScenario)));
    }

    [Fact]
    public void DescribeRejectsTypesWithoutMembers()
    {
        Assert.Throws<ArgumentException>(() => BoisContractInfo.Describe(typeof(int)));
        Assert.Throws<ArgumentException>(() => BoisContractInfo.Describe(typeof(List<int>)));
        Assert.Throws<ArgumentNullException>(() => BoisContractInfo.Describe(null!));
    }

    /// <summary>
    /// Every rule, checked with the same source in both serializers.
    /// </summary>
    private const string InvalidTypes = """
		[BoisContract(Versioned = true)]
		public class Gap
		{
			[BoisMember(1)] public int A { get; set; }
			[BoisMember(3)] public int C { get; set; }
		}

		[BoisContract(Versioned = true)]
		[BoisReserved(2, typeof(int))]
		public class DuplicateSlots
		{
			[BoisMember(1)] public int A { get; set; }
			[BoisMember(1)] public int B { get; set; }
			[BoisMember(2)] public int C { get; set; }
		}

		[BoisContract(Versioned = true)]
		[BoisReserved(1, typeof(int))]
		[BoisReserved(1, typeof(string))]
		public class DuplicateReservations
		{
			[BoisMember(2)] public int A { get; set; }
		}

		[BoisContract(Versioned = true)]
		public class MissingSlots
		{
			public int NoAttribute { get; set; }
			[BoisMember] public int NoIndex { get; set; }
			[BoisMember(0)] public int Zero { get; set; }
			[BoisMember(1)] public int A { get; set; }
			[BoisMember(false)] public int Excluded { get; set; }
			public int Computed => 1;
		}

		public class UnmarkedBase
		{
			public int BaseValue { get; set; }
		}

		[BoisContract(Versioned = true)]
		public class UnmarkedBaseChild : UnmarkedBase
		{
			[BoisMember(1)] public int A { get; set; }
		}

		[BoisContract(Versioned = true)]
		[BoisReserved(2, null)]
		[BoisReserved(3, typeof(object))]
		[BoisReserved(0, typeof(int))]
		public class BadReservations
		{
			[BoisMember(1)] public int A { get; set; }
		}

		[BoisReserved(1, typeof(int))]
		public class ReservedNotVersioned
		{
			public int A { get; set; }
		}

		[BoisContract(Versioned = true)]
		public class NotWritable
		{
			[BoisMember(1)] public int PrivateSet { get; private set; }
			[BoisMember(2)] public readonly int ReadOnlyField;
			[BoisMember(3)] public int InitOnly { get; init; }
			[BoisMember(4)] public int Getter => 1;
		}

		[BoisContract(Versioned = true)]
		[BoisReserved(1, typeof(int))]
		public class NoMembers
		{
		}
		""";

    private static readonly string[] InvalidTypeNames =
    [
        "Gap", "DuplicateSlots", "DuplicateReservations", "MissingSlots", "UnmarkedBaseChild",
        "BadReservations", "ReservedNotVersioned", "NotWritable", "NoMembers",
    ];

    private static readonly Dictionary<string, string[]> ExpectedProblems = new()
    {
        ["Gap"] =
        [
            "Slot 2 is missing. If a member was removed, add [BoisReserved(2, typeof(<removed member's type>))].",
        ],
        ["DuplicateSlots"] =
        [
            "Slot 1 is used by both member 'A' and member 'B'. Each slot can be used once.",
            "Slot 2 is used by both member 'C' and a reservation. Each slot can be used once; never reuse a reserved slot.",
        ],
        ["DuplicateReservations"] =
        [
            "Slot 1 is used by both a reservation and a reservation. Each slot can be used once; never reuse a reserved slot.",
        ],
        ["MissingSlots"] =
        [
            "Member 'NoAttribute' needs a slot: add [BoisMember(slot)] with a slot of 1 or higher, or [BoisMember(false)] to exclude it.",
            "Member 'NoIndex' needs a slot: add [BoisMember(slot)] with a slot of 1 or higher, or [BoisMember(false)] to exclude it.",
            "Member 'Zero' needs a slot: add [BoisMember(slot)] with a slot of 1 or higher, or [BoisMember(false)] to exclude it.",
        ],
        ["UnmarkedBaseChild"] =
        [
            "Member 'BaseValue' needs a slot: add [BoisMember(slot)] with a slot of 1 or higher, or [BoisMember(false)] to exclude it.",
        ],
        ["BadReservations"] =
        [
            "Reserved slot 2 needs the removed member's type, e.g. [BoisReserved(2, typeof(string))].",
            "Reserved slot 3 has type 'object', which can't be serialized. Use the removed member's exact type, or typeof(BoisSkip.Contract).",
            "Reserved slot 0 is invalid. Slots start at 1.",
        ],
        ["ReservedNotVersioned"] =
        [
            "Type 'ReservedNotVersioned' has [BoisReserved] but is not versioned. Add [BoisContract(Versioned = true)] or remove [BoisReserved].",
        ],
        ["NotWritable"] =
        [
            "Member 'PrivateSet' in slot 1 can't be written. Use a field that is not readonly, or a property with a public setter.",
            "Member 'ReadOnlyField' in slot 2 can't be written. Use a field that is not readonly, or a property with a public setter.",
            "Member 'InitOnly' in slot 3 can't be written. Use a field that is not readonly, or a property with a public setter.",
            "Member 'Getter' in slot 4 can't be written. Use a field that is not readonly, or a property with a public setter.",
            "Type 'NotWritable' needs at least one member with [BoisMember(slot)].",
        ],
        ["NoMembers"] =
        [
            "Type 'NoMembers' needs at least one member with [BoisMember(slot)].",
        ],
    };

    [Fact]
    public void RuntimeSerializerRejectsInvalidTypes()
    {
        var assembly = CompileForRuntime(InvalidTypes);
        var serializer = new BoisSerializer();

        foreach (var name in InvalidTypeNames)
        {
            var type = assembly.GetType("VersionedSample." + name, throwOnError: true)!;

            // the problems are reported at first use, by serializing, deserializing or describing
            var describe = Assert.Throws<InvalidTypeException>(() => BoisContractInfo.Describe(type));
            var serialize = Assert.Throws<InvalidTypeException>(() => serializer.Serialize(Activator.CreateInstance(type)!, type, new MemoryStream()));
            var deserialize = Assert.Throws<InvalidTypeException>(() => serializer.Deserialize(new MemoryStream([0x80]), type));
            Assert.Equal(describe.Message, serialize.Message);
            Assert.Equal(describe.Message, deserialize.Message);

            var problems = describe.Message.Contains('\n')
                ? describe.Message.Split('\n').Skip(1).Select(static x => x.TrimEnd('\r').Substring(2)).ToArray()
                : [describe.Message];
            Assert.True(ExpectedProblems[name].OrderBy(static x => x).SequenceEqual(problems.OrderBy(static x => x)),
                $"{name}:{Environment.NewLine}{describe.Message}");
        }
    }

    [Fact]
    public void SourceGeneratorReportsTheSameProblems()
    {
        var methods = string.Concat(InvalidTypeNames.Select(static name => $$"""

				[BoisReader] public static partial {{name}}? Read{{name}}(Stream source);
				[BoisWriter] public static partial void Write{{name}}({{name}}? model, Stream output);
				[BoisWriter] public static partial void WriteAgain{{name}}({{name}}? model, Stream output);
			"""));
        var diagnostics = RunGenerator(InvalidTypes, methods);

        var versionedDiagnostics = diagnostics.Where(static x => x.Id == "BOISGEN003").ToArray();
        Assert.All(versionedDiagnostics, static x => Assert.Equal(DiagnosticSeverity.Error, x.Severity));
        Assert.All(versionedDiagnostics, static x => Assert.NotEqual(Location.None, x.Location));

        // each problem once, even though each type is used by three methods
        var actual = versionedDiagnostics.Select(static x => x.GetMessage()).OrderBy(static x => x).ToArray();
        var expected = ExpectedProblems.Values.SelectMany(static x => x).OrderBy(static x => x).ToArray();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void SourceGeneratorAcceptsValidVersionedTypes()
    {
        const string types = """
			[BoisContract(Versioned = true)]
			[BoisReserved(2, typeof(string))]
			[BoisReserved(4, typeof(BoisSkip.Contract))]
			public class Order
			{
				[BoisMember(1)] public int Id { get; set; }
				[BoisMember(3)] public decimal? Discount { get; set; }
				[BoisMember(5)] public List<Line>? Lines { get; set; }
				[BoisMember(false)] public string? DisplayText { get; set; }
				public int LineCount => Lines?.Count ?? 0;
			}

			[BoisContract(Versioned = true)]
			public class Line
			{
				[BoisMember(1)] public string? Name { get; set; }
			}
			""";
        const string methods = """
			[BoisReader] public static partial Order? ReadOrder(Stream source);
			[BoisWriter] public static partial void WriteOrder(Order? model, Stream output);
			""";

        var diagnostics = RunGenerator(types, methods);
        Assert.DoesNotContain(diagnostics, static x => x.Severity == DiagnosticSeverity.Error);
    }

    private static string Source(string usingNamespace, string types, string methods = "")
        => $$"""
			using {{usingNamespace}};
			using System;
			using System.Collections.Generic;
			using System.IO;

			namespace VersionedSample;

			{{types}}

			public static partial class VersionedSampleBois
			{
			{{methods}}
			}
			""";

    private static Assembly CompileForRuntime(string types)
    {
        var references = Test_CodeGenGenerator.GetMetadataReferences()
            .Append(MetadataReference.CreateFromFile(typeof(BoisSerializer).Assembly.Location));
        var compilation = CSharpCompilation.Create(
            "VersionedSampleRuntime" + Guid.NewGuid().ToString("N"),
            [CSharpSyntaxTree.ParseText(Source("Salar.Bois", types), CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest))],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        using var stream = new MemoryStream();
        var result = compilation.Emit(stream);
        Assert.True(result.Success, string.Join(Environment.NewLine, result.Diagnostics));
        return Assembly.Load(stream.ToArray());
    }

    private static IReadOnlyList<Diagnostic> RunGenerator(string types, string methods)
    {
        var compilation = CSharpCompilation.Create(
            "VersionedSampleGenerator",
            [CSharpSyntaxTree.ParseText(Source("Salar.Bois.CodeGen", types, methods), CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest))],
            Test_CodeGenGenerator.GetMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new BoisSourceGenerator());
        driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics);
        return diagnostics;
    }
}
