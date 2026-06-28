using Salar.BinaryBuffers;
using Salar.Bois;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;

namespace Salar.Bois.NetFx.Tests.CodeGenFixtures;

public enum ScenarioStatus
{
	Unknown,
	Active,
	Archived,
}

public struct ScenarioStruct
{
	public int X { get; set; }
	public int Y { get; set; }
}

public sealed class EmptyScenario
{
}

public sealed class PrimitiveScenario
{
	public string Text { get; set; } = string.Empty;
	public string? NullableText { get; set; }
	public bool BoolValue { get; set; }
	public bool? NullableBool { get; set; }
	public char CharValue { get; set; }
	public char? NullableChar { get; set; }
	public short Int16Value { get; set; }
	public short? NullableInt16 { get; set; }
	public int Int32Value { get; set; }
	public int? NullableInt32 { get; set; }
	public long Int64Value { get; set; }
	public long? NullableInt64 { get; set; }
	public ushort UInt16Value { get; set; }
	public ushort? NullableUInt16 { get; set; }
	public uint UInt32Value { get; set; }
	public uint? NullableUInt32 { get; set; }
	public ulong UInt64Value { get; set; }
	public ulong? NullableUInt64 { get; set; }
	public float SingleValue { get; set; }
	public float? NullableSingle { get; set; }
	public double DoubleValue { get; set; }
	public double? NullableDouble { get; set; }
	public decimal DecimalValue { get; set; }
	public decimal? NullableDecimal { get; set; }
	public byte ByteValue { get; set; }
	public byte? NullableByte { get; set; }
	public sbyte SByteValue { get; set; }
	public sbyte? NullableSByte { get; set; }
	public DateTime DateTimeValue { get; set; }
	public DateTime? NullableDateTime { get; set; }
	public DateTimeOffset DateTimeOffsetValue { get; set; }
	public DateTimeOffset? NullableDateTimeOffset { get; set; }
	public DateOnly DateOnlyValue { get; set; }
	public DateOnly? NullableDateOnly { get; set; }
	public TimeOnly TimeOnlyValue { get; set; }
	public TimeOnly? NullableTimeOnly { get; set; }
	public TimeSpan TimeSpanValue { get; set; }
	public TimeSpan? NullableTimeSpan { get; set; }
	public Guid GuidValue { get; set; }
	public Guid? NullableGuid { get; set; }
	public Color ColorValue { get; set; }
	public Color? NullableColor { get; set; }
	public Uri UriValue { get; set; } = new("https://example.com/");
	public Version VersionValue { get; set; } = new(1, 0);
	public ScenarioStatus Status { get; set; }
	public ScenarioStatus? NullableStatus { get; set; }
	public byte[] Bytes { get; set; } = [];
	public int[] KnownTypeArray { get; set; } = [];
}

public sealed class CollectionScenario
{
	public List<string> Names { get; set; } = [];
	public HashSet<int> Numbers { get; set; } = [];
	public Dictionary<string, int> Scores { get; set; } = [];
	public NameValueCollection Headers { get; set; } = [];
}

public sealed class NestedScenario
{
	public PrimitiveScenario Primitive { get; set; } = new();
	public PrimitiveScenario? NullablePrimitive { get; set; }
	public ScenarioStruct Location { get; set; }
	public EmptyScenario Empty { get; set; } = new();
}

public sealed class ReusedNestedScenario
{
	public PrimitiveScenario First { get; set; } = new();
	public PrimitiveScenario Second { get; set; } = new();
	public PrimitiveScenario? Optional { get; set; }
}

public sealed class SameTypeCastingScenario
{
	public int FirstInt32 { get; set; }
	public int SecondInt32 { get; set; }
	public int? OptionalInt32 { get; set; }
	public string FirstText { get; set; } = string.Empty;
	public string SecondText { get; set; } = string.Empty;
	public string? OptionalText1 { get; set; }
	public string? OptionalText2 { get; set; }
	public ScenarioStatus FirstStatus { get; set; }
	public ScenarioStatus SecondStatus { get; set; }
	public ScenarioStatus ThirdStatus { get; set; }
	public ScenarioStatus? OptionalStatus1 { get; set; }
	public ScenarioStatus? OptionalStatus2 { get; set; }
	public ScenarioStatus? OptionalStatus3 { get; set; }
	public ScenarioStruct FirstLocation { get; set; }
	public ScenarioStruct SecondLocation { get; set; }
	public PrimitiveScenario FirstPrimitive { get; set; } = new();
	public PrimitiveScenario SecondPrimitive { get; set; } = new();
	public PrimitiveScenario? OptionalPrimitive1 { get; set; }
	public PrimitiveScenario? OptionalPrimitive2 { get; set; }
	public List<ScenarioStatus> StatusHistory1 { get; set; } = [];
	public List<ScenarioStatus> StatusHistory2 { get; set; } = [];
	public Dictionary<ScenarioStatus, PrimitiveScenario> StatusValues1 { get; set; } = [];
	public Dictionary<ScenarioStatus, PrimitiveScenario> StatusValues2 { get; set; } = [];
}

public sealed class ContractScenario
{
	[BoisMember(0)]
	public int First { get; set; }

	[BoisMember(2)]
	public int Third { get; set; }

	[BoisMember(1)]
	public int Second { get; set; }

	[BoisMember(false)]
	public int Ignored { get; set; }
}

[BoisContract(fields: true, properties: false)]
public sealed class ContractFieldsOnlyScenario
{
	public int FieldValue;
	public int IgnoredProperty { get; set; }
}

[BoisContract(fields: false, properties: true)]
public sealed class ContractPropertiesOnlyScenario
{
	public int IgnoredField;
	public int PropertyValue { get; set; }
}

public sealed class DataScenario
{
	public DataTable Table { get; set; } = new();
	public DataSet Set { get; set; } = new();
}

public static partial class SourceGeneratorScenariosBois
{
	[BoisReader]
	public static partial PrimitiveScenario? ReadPrimitiveScenario(Stream source);

	[BoisReader]
	public static partial PrimitiveScenario? ReadPrimitiveScenario(BufferReaderBase reader);

	[BoisReader]
	public static partial PrimitiveScenario? ReadPrimitiveScenario(BufferReaderBase reader, Encoding encoding);

	[BoisReader]
	public static partial PrimitiveScenario? ReadPrimitiveScenario(byte[] buffer, int position, int length);

	[BoisWriter]
	public static partial void WritePrimitiveScenario(PrimitiveScenario? model, Stream output);

	[BoisWriter]
	public static partial void WritePrimitiveScenario(PrimitiveScenario? model, BufferWriterBase writer);

	[BoisWriter]
	public static partial void WritePrimitiveScenario(BufferWriterBase writer, PrimitiveScenario? model);

	[BoisWriter]
	public static partial void WritePrimitiveScenario(PrimitiveScenario? model, BufferWriterBase writer, Encoding encoding);

	[BoisWriter]
	public static partial void WritePrimitiveScenario(PrimitiveScenario? model, byte[] output, int position, int length);

	[BoisWriter]
	public static partial void WritePrimitiveScenario(byte[] output, int position, int length, PrimitiveScenario? model);

	[BoisReader]
	public static partial CollectionScenario? ReadCollectionScenario(Stream source);

	[BoisWriter]
	public static partial void WriteCollectionScenario(CollectionScenario? model, Stream output);

	[BoisReader]
	public static partial NestedScenario? ReadNestedScenario(Stream source);

	[BoisWriter]
	public static partial void WriteNestedScenario(NestedScenario? model, Stream output);

	[BoisReader]
	public static partial ReusedNestedScenario? ReadReusedNestedScenario(Stream source);

	[BoisWriter]
	public static partial void WriteReusedNestedScenario(ReusedNestedScenario? model, Stream output);

	[BoisReader]
	public static partial SameTypeCastingScenario? ReadSameTypeCastingScenario(Stream source);

	[BoisWriter]
	public static partial void WriteSameTypeCastingScenario(SameTypeCastingScenario? model, Stream output);

	[BoisReader]
	public static partial ContractScenario? ReadContractScenario(Stream source);

	[BoisWriter]
	public static partial void WriteContractScenario(ContractScenario? model, Stream output);

	[BoisReader]
	public static partial ContractFieldsOnlyScenario? ReadContractFieldsOnlyScenario(Stream source);

	[BoisWriter]
	public static partial void WriteContractFieldsOnlyScenario(ContractFieldsOnlyScenario? model, Stream output);

	[BoisReader]
	public static partial ContractPropertiesOnlyScenario? ReadContractPropertiesOnlyScenario(Stream source);

	[BoisWriter]
	public static partial void WriteContractPropertiesOnlyScenario(ContractPropertiesOnlyScenario? model, Stream output);

	[BoisReader]
	public static partial EmptyScenario? ReadEmptyScenario(Stream source);

	[BoisWriter]
	public static partial void WriteEmptyScenario(EmptyScenario? model, Stream output);

	[BoisReader]
	public static partial ScenarioStruct ReadScenarioStruct(Stream source);

	[BoisWriter]
	public static partial void WriteScenarioStruct(ScenarioStruct model, Stream output);

	[BoisReader]
	public static partial List<string>? ReadStringList(Stream source);

	[BoisWriter]
	public static partial void WriteStringList(List<string>? model, Stream output);

	[BoisReader]
	public static partial Dictionary<string, int>? ReadStringIntDictionary(Stream source);

	[BoisWriter]
	public static partial void WriteStringIntDictionary(Dictionary<string, int>? model, Stream output);

	[BoisReader]
	public static partial int[]? ReadInt32Array(Stream source);

	[BoisWriter]
	public static partial void WriteInt32Array(int[]? model, Stream output);

	[BoisReader]
	public static partial string? ReadString(Stream source, Encoding encoding);

	[BoisWriter]
	public static partial void WriteString(string? model, Stream output, Encoding encoding);

	[BoisReader]
	public static partial DataScenario? ReadDataScenario(Stream source);

	[BoisWriter]
	public static partial void WriteDataScenario(DataScenario? model, Stream output);
}
