using Salar.BinaryBuffers.Compatibility;
using Salar.Bois.NetFx.Tests.CodeGenFixtures;
using Salar.Bois.CodeGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Salar.Bois.NetFx.Tests.Tests;

/// <summary>
/// A length/count read from the data is untrusted. A forged huge length must fail without allocating
/// anything close to the declared size, whatever reader (byte[], seekable stream, non-seekable stream) is used.
/// </summary>
public class Test_UntrustedLengths(ITestOutputHelper output)
{
	public class ArrayHolder
	{
		public int[] Ints { get; set; }
		public string[] Strings { get; set; }
		public ushort[] Ushorts { get; set; }
		public byte[] Bytes { get; set; }
		public string Text { get; set; }
		public Guid? Id { get; set; }
		public List<int> List { get; set; }
		public Dictionary<int, int> Dict { get; set; }
	}

	private const long AllowedAllocation = 5_000_000;

	private static byte[] ForgedLength(uint length)
	{
		using var ms = new MemoryStream();
		var writer = new StreamBufferWriter(ms);
		BoisNumericSerializers.WriteUIntNullableMemberCount(ref writer, length);
		ms.WriteByte(1);
		return ms.ToArray();
	}

	private sealed class NonSeekableStream(byte[] data) : Stream
	{
		private readonly MemoryStream _inner = new(data);
		public override bool CanRead => true;
		public override bool CanSeek => false;
		public override bool CanWrite => false;
		public override long Length => throw new NotSupportedException();
		public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
		public override void Flush() { }
		public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
		public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
		public override void SetLength(long value) => throw new NotSupportedException();
		public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
	}

	/// <summary>
	/// Runs the action which is expected to fail on the forged data and returns the bytes allocated by it.
	/// An OutOfMemoryException means the declared size was allocated (or attempted), which is the bug.
	/// </summary>
	private static long Allocated(Action action)
	{
		AssertFailsWithoutOutOfMemory(action);
		var before = GC.GetAllocatedBytesForCurrentThread();
		AssertFailsWithoutOutOfMemory(action);
		return GC.GetAllocatedBytesForCurrentThread() - before;
	}

	internal static void AssertFailsWithoutOutOfMemory(Action action)
	{
		var ex = Record.Exception(action);
		Assert.NotNull(ex);
		Assert.False(ex is OutOfMemoryException, "The declared length was allocated: " + ex);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(1)]
	[InlineData(1024)]
	[InlineData(1025)]
	[InlineData(5000)]
	[InlineData(70000)]
	public void ValidLargeDataRoundTripsOnAllReaders(int length)
	{
		var serializer = new BoisSerializer();
		var model = new ArrayHolder
		{
			Ints = Enumerable.Range(0, length).ToArray(),
			Ushorts = Enumerable.Range(0, length).Select(x => (ushort)x).ToArray(),
			Strings = Enumerable.Range(0, length).Select(x => "s" + x).ToArray(),
			Bytes = Enumerable.Range(0, length * 3).Select(x => (byte)x).ToArray(),
			Text = new string('x', length * 3),
			Id = Guid.NewGuid(),
		};

		using var ms = new MemoryStream();
		serializer.Serialize(model, ms);
		var bytes = ms.ToArray();

		foreach (var actual in new[]
		{
			serializer.Deserialize<ArrayHolder>(bytes, 0, bytes.Length),
			serializer.Deserialize<ArrayHolder>(new MemoryStream(bytes)),
			serializer.Deserialize<ArrayHolder>(new NonSeekableStream(bytes)),
		})
		{
			Assert.Equal(model.Ints, actual.Ints);
			Assert.Equal(model.Ushorts, actual.Ushorts);
			Assert.Equal(model.Strings, actual.Strings);
			Assert.Equal(model.Bytes, actual.Bytes);
			Assert.Equal(model.Text, actual.Text);
			Assert.Equal(model.Id, actual.Id);
		}

		// the generated code path, int[] root
		var ints = Enumerable.Range(0, length).ToArray();
		using var intsStream = new MemoryStream();
		SourceGeneratorScenariosBois.WriteInt32Array(ints, intsStream);
		var intsBytes = intsStream.ToArray();
		Assert.Equal(ints, SourceGeneratorScenariosBois.ReadInt32Array(new MemoryStream(intsBytes)));
		Assert.Equal(ints, SourceGeneratorScenariosBois.ReadInt32Array(new NonSeekableStream(intsBytes)));
	}

	public class EmptyHolder
	{
		public EmptyStructScenario[] Array { get; set; }
		public List<EmptyStructScenario> List { get; set; }
	}

	public class OneEmptyArray { public EmptyStructScenario[] V { get; set; } }
	public class OneEmptyList { public List<EmptyStructScenario> V { get; set; } }
	public class OneEmptySet { public HashSet<EmptyStructScenario> V { get; set; } }

	[Theory]
	[InlineData(0)]
	[InlineData(1024)]
	[InlineData(1025)]
	[InlineData(5000)]
	[InlineData(70000)]
	public void ValidEmptyStructsRoundTripOnAllReaders(int length)
	{
		// custom structs without members take no space in the data, so their count can't be validated
		// against the data left but valid data must still be readable
		var serializer = new BoisSerializer();
		var model = new EmptyHolder
		{
			Array = new EmptyStructScenario[length],
			List = new List<EmptyStructScenario>(new EmptyStructScenario[length]),
		};

		using var ms = new MemoryStream();
		serializer.Serialize(model, ms);
		var bytes = ms.ToArray();

		foreach (var actual in new[]
		{
			serializer.Deserialize<EmptyHolder>(bytes, 0, bytes.Length),
			serializer.Deserialize<EmptyHolder>(new MemoryStream(bytes)),
			serializer.Deserialize<EmptyHolder>(new NonSeekableStream(bytes)),
		})
		{
			Assert.Equal(length, actual.Array.Length);
			Assert.Equal(length, actual.List.Count);
		}

		// the generated code path
		using var arrayStream = new MemoryStream();
		SourceGeneratorScenariosBois.WriteEmptyStructArray(new EmptyStructScenario[length], arrayStream);
		using var listStream = new MemoryStream();
		SourceGeneratorScenariosBois.WriteEmptyStructList(new List<EmptyStructScenario>(new EmptyStructScenario[length]), listStream);

		foreach (var stream in new Func<byte[], Stream>[] { b => new MemoryStream(b), b => new NonSeekableStream(b) })
		{
			Assert.Equal(length, SourceGeneratorScenariosBois.ReadEmptyStructArray(stream(arrayStream.ToArray()))!.Length);
			Assert.Equal(length, SourceGeneratorScenariosBois.ReadEmptyStructList(stream(listStream.ToArray()))!.Count);
		}
	}

	[Theory]
	[InlineData(200_000_000u)]
	[InlineData(2_000_000u)]
	public void ForgedEmptyStructCountIsRejected(uint forgedLength)
	{
		var serializer = new BoisSerializer();
		const int sampleLength = 3;
		var cases = new List<(string Name, byte[] Forged, Action<Stream> Read)>
		{
			("runtime array", ForgeMember(new OneEmptyArray { V = new EmptyStructScenario[sampleLength] }, sampleLength, forgedLength), s => serializer.Deserialize<OneEmptyArray>(s)),
			("runtime list", ForgeMember(new OneEmptyList { V = new List<EmptyStructScenario>(new EmptyStructScenario[sampleLength]) }, sampleLength, forgedLength), s => serializer.Deserialize<OneEmptyList>(s)),
			("runtime set", ForgeMember(new OneEmptySet { V = new HashSet<EmptyStructScenario> { default } }, 1, forgedLength), s => serializer.Deserialize<OneEmptySet>(s)),
			("generated array", ForgedLength(forgedLength)[..^1], s => SourceGeneratorScenariosBois.ReadEmptyStructArray(s)),
			("generated list", ForgedLength(forgedLength)[..^1], s => SourceGeneratorScenariosBois.ReadEmptyStructList(s)),
		};

		foreach (var (name, forged, read) in cases)
		{
			foreach (var (kind, create) in new (string, Func<Stream>)[]
			{
				("MemoryStream", () => new MemoryStream(forged)),
				("NonSeekable", () => new NonSeekableStream(forged)),
			})
			{
				var allocated = Allocated(() => read(create()));
				output.WriteLine($"{name} {kind} {forgedLength}: {allocated:N0}");
				Assert.True(allocated < AllowedAllocation, $"{name} {kind} {forgedLength}: {allocated:N0} bytes");
			}
		}
	}

	[Fact]
	public void ForgedDataTableRowCountWithoutColumnsIsRejected()
	{
		var serializer = new BoisSerializer();
		using var real = new MemoryStream();
		serializer.Serialize(new System.Data.DataTable("t"), real);
		var bytes = real.ToArray();

		// the row count is the last value and zero; replace it with a huge one
		using var forgedStream = new MemoryStream();
		forgedStream.Write(bytes, 0, bytes.Length - 1);
		var writer = new StreamBufferWriter(forgedStream);
		Salar.Bois.CodeGen.BoisNumericSerializers.WriteVarInt(ref writer, 200_000_000);
		var forged = forgedStream.ToArray();

		var allocated = Allocated(() => serializer.Deserialize<System.Data.DataTable>(new MemoryStream(forged)));
		Assert.True(allocated < AllowedAllocation, $"{allocated:N0} bytes");
	}

	public class OneInts { public int[] V { get; set; } }
	public class OneUshorts { public ushort[] V { get; set; } }
	public class OneStrings { public string[] V { get; set; } }
	public class OneBytes { public byte[] V { get; set; } }
	public class OneText { public string V { get; set; } }
	public class OneGuid { public Guid? V { get; set; } }
	public class OneObjects { public OneText[] V { get; set; } }
	public class OneList { public List<int> V { get; set; } }

	/// <summary>
	/// Serializes a real sample to learn the layout, then swaps the length of the member for a forged one.
	/// </summary>
	private static byte[] ForgeMember<T>(T sample, int sampleLength, uint forgedLength)
	{
		var serializer = new BoisSerializer();
		using var real = new MemoryStream();
		serializer.Serialize(sample, real);
		var realBytes = real.ToArray();
		var sampleHeader = ForgedLength((uint)sampleLength);
		var header = sampleHeader[..^1]; // without the trailing marker byte

		var at = realBytes.AsSpan().IndexOf(header);
		Assert.True(at >= 0, "sample layout not recognized");

		var forgedHeader = ForgedLength(forgedLength)[..^1];
		var result = new List<byte>();
		result.AddRange(realBytes.AsSpan(0, at).ToArray());
		result.AddRange(forgedHeader);
		result.Add(1); // some trailing data
		return result.ToArray();
	}

	public static IEnumerable<object[]> MemberScenarios()
	{
		foreach (var length in new uint[] { int.MaxValue, 500_000_000, 100_000_000 })
		{
			yield return new object[] { "int[] member", length };
			yield return new object[] { "ushort[] member", length };
			yield return new object[] { "string[] member", length };
			yield return new object[] { "byte[] member", length };
			yield return new object[] { "string member", length };
			yield return new object[] { "Guid? member", length };
			yield return new object[] { "object[] member", length };
			yield return new object[] { "List<int> member", length };
		}
	}

	[Theory]
	[MemberData(nameof(MemberScenarios))]
	public void ForgedMemberLengthDoesNotAllocate(string scenario, uint forgedLength)
	{
		var serializer = new BoisSerializer();
		const int sampleLength = 3;
		var (forged, read) = scenario switch
		{
			"int[] member" => (ForgeMember(new OneInts { V = new int[sampleLength] }, sampleLength, forgedLength), (Func<Stream, object>)(s => serializer.Deserialize<OneInts>(s))),
			"ushort[] member" => (ForgeMember(new OneUshorts { V = new ushort[sampleLength] }, sampleLength, forgedLength), s => serializer.Deserialize<OneUshorts>(s)),
			"string[] member" => (ForgeMember(new OneStrings { V = new string[sampleLength] }, sampleLength, forgedLength), s => serializer.Deserialize<OneStrings>(s)),
			"byte[] member" => (ForgeMember(new OneBytes { V = new byte[sampleLength] }, sampleLength, forgedLength), s => serializer.Deserialize<OneBytes>(s)),
			"string member" => (ForgeMember(new OneText { V = new string('a', sampleLength) }, sampleLength, forgedLength), s => serializer.Deserialize<OneText>(s)),
			"Guid? member" => (ForgeMember(new OneGuid { V = Guid.NewGuid() }, 16, forgedLength), s => serializer.Deserialize<OneGuid>(s)),
			"object[] member" => (ForgeMember(new OneObjects { V = new[] { new OneText { V = "a" }, new OneText { V = "b" }, new OneText { V = "c" } } }, sampleLength, forgedLength), s => serializer.Deserialize<OneObjects>(s)),
			_ => (ForgeMember(new OneList { V = new List<int>(new int[sampleLength]) }, sampleLength, forgedLength), s => serializer.Deserialize<OneList>(s)),
		};

		var results = new List<string>();
		var worst = 0L;
		foreach (var (kind, action) in new (string, Action)[]
		{
			("byte[]", () => serializer.Deserialize(new Salar.BinaryBuffers.BinaryBufferReader(forged, 0, forged.Length), TypeOf(scenario))),
			("MemoryStream", () => read(new MemoryStream(forged))),
			("NonSeekable", () => read(new NonSeekableStream(forged))),
		})
		{
			var allocated = Allocated(action);
			worst = Math.Max(worst, allocated);
			results.Add($"{kind}={allocated:N0}");
		}
		output.WriteLine($"{scenario} {forgedLength}: {string.Join(", ", results)}");
		Assert.True(worst < AllowedAllocation, $"{scenario} {forgedLength}: {string.Join(", ", results)}");
	}

	private static Type TypeOf(string scenario) => scenario switch
	{
		"int[] member" => typeof(OneInts),
		"ushort[] member" => typeof(OneUshorts),
		"string[] member" => typeof(OneStrings),
		"byte[] member" => typeof(OneBytes),
		"string member" => typeof(OneText),
		"Guid? member" => typeof(OneGuid),
		"object[] member" => typeof(OneObjects),
		_ => typeof(OneList),
	};

	[Theory]
	[InlineData("int[] root")]
	[InlineData("byte[] root")]
	[InlineData("string root")]
	[InlineData("string[] root")]
	[InlineData("Guid? root")]
	public void ForgedRootLengthDoesNotAllocate(string scenario)
	{
		var forged = ForgedLength(500_000_000);
		var serializer = new BoisSerializer();

		Action Make(Func<byte[], Stream> stream, bool useBuffer) => scenario switch
		{
			"int[] root" => useBuffer ? () => serializer.Deserialize<int[]>(forged, 0, forged.Length) : () => serializer.Deserialize<int[]>(stream(forged)),
			"byte[] root" => useBuffer ? () => serializer.Deserialize<byte[]>(forged, 0, forged.Length) : () => serializer.Deserialize<byte[]>(stream(forged)),
			"string root" => useBuffer ? () => serializer.Deserialize<string>(forged, 0, forged.Length) : () => serializer.Deserialize<string>(stream(forged)),
			"string[] root" => useBuffer ? () => serializer.Deserialize<string[]>(forged, 0, forged.Length) : () => serializer.Deserialize<string[]>(stream(forged)),
			_ => useBuffer ? () => serializer.Deserialize<Guid?>(forged, 0, forged.Length) : () => serializer.Deserialize<Guid?>(stream(forged)),
		};

		var results = new List<string>();
		var worst = 0L;
		foreach (var (kind, action) in new (string, Action)[]
		{
			("byte[]", Make(null, true)),
			("MemoryStream", Make(b => new MemoryStream(b), false)),
			("NonSeekable", Make(b => new NonSeekableStream(b), false)),
		})
		{
			var allocated = Allocated(action);
			worst = Math.Max(worst, allocated);
			results.Add($"{kind}={allocated:N0}");
		}
		output.WriteLine($"{scenario}: {string.Join(", ", results)}");
		Assert.True(worst < AllowedAllocation, $"{scenario}: {string.Join(", ", results)}");
	}
}
