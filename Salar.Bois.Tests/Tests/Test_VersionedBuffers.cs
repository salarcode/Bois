using Salar.BinaryBuffers;
using Salar.BinaryBuffers.Compatibility;
using Salar.Bois.NetFx.Tests.CodeGenFixtures;
using Salar.Bois.Serializers;
using Salar.Bois.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Salar.Bois.NetFx.Tests.Tests;

/// <summary>
/// The size prefix of versioned objects: writing it with each writer, reading it, and jumping over data.
/// </summary>
public class Test_VersionedBuffers
{
	/// <summary>
	/// A versioned object for the tests: raw bytes and nested objects.
	/// </summary>
	public sealed class Node(params object[] parts)
	{
		public object[] Parts { get; } = parts;
	}

	private sealed class NonSeekableStream(Stream inner) : Stream
	{
		public NonSeekableStream(byte[] data) : this(new MemoryStream(data)) { }
		public Stream Inner => inner;
		public override bool CanRead => inner.CanRead;
		public override bool CanSeek => false;
		public override bool CanWrite => inner.CanWrite;
		public override long Length => throw new NotSupportedException();
		public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
		public override void Flush() => inner.Flush();
		public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);
		public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
		public override void SetLength(long value) => throw new NotSupportedException();
		public override void Write(byte[] buffer, int offset, int count) => inner.Write(buffer, offset, count);
	}

	private static byte[] Bytes(int length, byte seed = 1)
		=> Enumerable.Range(0, length).Select(i => (byte)(i * 7 + seed)).ToArray();

	/// <summary>
	/// The expected bytes, built independently from the writer code.
	/// </summary>
	private static byte[] Expected(Node node)
	{
		var body = node.Parts.SelectMany(static part => part is Node child ? Expected(child) : (byte[])part).ToArray();
		var prefix = new List<byte>();
		if (body.Length <= 63)
		{
			prefix.Add((byte)(0xC0 | body.Length));
		}
		else
		{
			prefix.Add(0x01);
			for (var value = (uint)body.Length; ; value >>= 7)
			{
				if (value < 0x80)
				{
					prefix.Add((byte)value);
					break;
				}
				prefix.Add((byte)(value & 0x7F | 0x80));
			}
		}
		return prefix.Concat(body).ToArray();
	}

	private static void Write<TWriter>(ref TWriter writer, Node node)
		where TWriter : IBufferWriter, allows ref struct
	{
		var start = VersionedWriter.Begin(ref writer);
		foreach (var part in node.Parts)
		{
			if (part is Node child)
				Write(ref writer, child);
			else
				writer.Write((byte[])part);
		}
		VersionedWriter.End(ref writer, start);
	}

	public static IEnumerable<object[]> Nodes()
	{
		foreach (var length in new[] { 0, 1, 63, 64, 127, 128, 16_383, 16_384, 70_000 })
			yield return [$"body {length}", new Node(Bytes(length))];

		yield return ["nested small in large", new Node(Bytes(100), new Node(Bytes(10)), Bytes(5))];
		yield return ["nested large in large", new Node(Bytes(3), new Node(Bytes(200, 9), new Node(Bytes(20_000, 4))), Bytes(70))];
		yield return ["nested large in small parent", new Node(new Node(Bytes(64)))];
		yield return ["many siblings", new Node(Enumerable.Range(0, 50).Select(i => (object)new Node(Bytes(i * 3))).ToArray())];

		// big objects inside big objects wait for the outermost one, then move once
		yield return ["deep mixed nesting", DeepNode(8)];
		yield return ["many small and big siblings", new Node(Enumerable.Range(0, 40).Select(i => (object)new Node(Bytes(55 + i * 5, (byte)i))).ToArray())];
		yield return ["big siblings over the stream cache", new Node(Bytes(100), new Node(Bytes(9_000, 2)), new Node(Bytes(5)),
			new Node(Bytes(70), new Node(Bytes(64, 3)), Bytes(1)), Bytes(20_000, 5), DeepNode(3))];
	}

	private static Node DeepNode(int depth)
		=> depth == 0
			? new Node(Bytes(80))
			: new Node(Bytes(depth * 30, (byte)depth), new Node(Bytes(10)), DeepNode(depth - 1), new Node(Bytes(70 + depth)), Bytes(3));

	[Theory]
	[MemberData(nameof(Nodes))]
	public void BoisBufferWriterOverArray(string name, Node node)
	{
		var expected = Expected(node);
		var buffer = new byte[expected.Length + 20];
		var writer = new BoisBufferWriter(buffer, 7, expected.Length + 10);
		writer.Write((byte)0xAB);
		Write(ref writer, node);

		Assert.True(buffer.AsSpan(8, expected.Length).SequenceEqual(expected), name);
		Assert.Equal(expected.Length + 1, writer.WrittenLength);
	}

	[Theory]
	[MemberData(nameof(Nodes))]
	public void BoisBufferWriterOverStream(string name, Node node)
	{
		var expected = Expected(node);
		foreach (var before in new[] { 0, 100, BoisBufferWriter.InternalCacheSize - 2, BoisBufferWriter.InternalCacheSize + 300 })
		{
			foreach (var seekable in new[] { true, false })
			{
				var output = new MemoryStream();
				var writer = new BoisBufferWriter(seekable ? output : new NonSeekableStream(output));
				var leading = Bytes(before, 3);
				writer.Write(leading);
				Write(ref writer, node);
				writer.Write((byte)0xEE);
				writer.Flush();

				var actual = output.ToArray();
				Assert.True(leading.Concat(expected).Append((byte)0xEE).SequenceEqual(actual), $"{name}, {before} bytes before, seekable {seekable}");
			}
		}
	}

	[Theory]
	[MemberData(nameof(Nodes))]
	public void BinaryBufferWriter(string name, Node node)
	{
		var expected = Expected(node);

		var buffer = new byte[expected.Length + 20];
		var writer = new BinaryBufferWriter(buffer, 5, expected.Length + 10);
		writer.Write((byte)0xAB);
		Write(ref writer, node);
		Assert.True(new byte[] { 0xAB }.Concat(expected).SequenceEqual(writer.ToArray()), name + " as BinaryBufferWriter");

		// the runtime serializer often uses the base class as the writer type
		BufferWriterBase baseWriter = new BinaryBufferWriter(new byte[expected.Length + 3]);
		baseWriter.Write((byte)0xAB);
		Write(ref baseWriter, node);
		Assert.True(new byte[] { 0xAB }.Concat(expected).SequenceEqual(((BinaryBufferWriter)baseWriter).ToArray()), name + " as BufferWriterBase");
	}

	[Theory]
	[MemberData(nameof(Nodes))]
	public void BinarySpanBufferWriter(string name, Node node)
	{
		var expected = Expected(node);
		Span<byte> buffer = new byte[expected.Length + 20];
		var writer = new BinarySpanBufferWriter(buffer.Slice(3));
		writer.Write((byte)0xAB);
		Write(ref writer, node);

		Assert.True(new byte[] { 0xAB }.Concat(expected).SequenceEqual(writer.ToArray()), name);
	}

	[Fact]
	public void WritersThatCantFillInTheSizeAreRejected()
	{
		var writer = new StreamBufferWriter(new MemoryStream());
		var ex = Assert.Throws<NotSupportedException>(() => Write(ref writer, new Node(Bytes(3))));
		Assert.Contains("StreamBufferWriter", ex.Message);
	}

	[Fact]
	public void WriterFailsWhenTheBufferIsFull()
	{
		// a size above 63 needs more bytes than the placeholder
		var buffer = new byte[1 + 64];
		var boisWriter = new BoisBufferWriter(buffer);
		Assert.Throws<EndOfStreamException>(() => Write(ref boisWriter, new Node(Bytes(64))));

		var binaryWriter = new BinaryBufferWriter(new byte[1 + 64]);
		Assert.ThrowsAny<Exception>(() => Write(ref binaryWriter, new Node(Bytes(64))));
	}

	[Fact]
	public void BoisBufferWriterOverStreamFreesTheGrownCacheOnlyWithTheWriter()
	{
		// an object bigger than the cache is kept until its size is known, then sent
		var output = new MemoryStream();
		var writer = new BoisBufferWriter(new NonSeekableStream(output));
		Write(ref writer, new Node(Bytes(BoisBufferWriter.InternalCacheSize * 3)));
		Assert.Equal(Expected(new Node(Bytes(BoisBufferWriter.InternalCacheSize * 3))).Length, output.Length);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(63)]
	[InlineData(64)]
	[InlineData(300)]
	[InlineData(70_000)]
	public void OldReadersReadTheSizeAsAMemberCount(int length)
	{
		// old readers decode the prefix as a member count and only check it for null
		var prefix = Expected(new Node(Bytes(length))).Take(length <= 63 ? 1 : 1 + VersionedFormat.LongLengthExtraBytes(length)).ToArray();
		var reader = new BinaryBufferReader(prefix);
		Assert.NotNull(NumericSerializers.ReadVarUInt32Nullable(ref reader));
		Assert.Equal(prefix.Length, reader.Position);
	}

	[Fact]
	public void OldReadersReadAClassWithASizePrefix()
	{
		// today's bytes of InheritedMemberScenario: member count 2, then DerivedValue 10 and BaseValue 20
		var current = new byte[] { 0x82, 0x14, 0x28 };
		var serializer = new BoisSerializer();
		foreach (var data in new[]
		{
			current,
			new byte[] { 0xC2, 0x14, 0x28 },       // size 2
			new byte[] { 0x01, 0x02, 0x14, 0x28 }, // size 2, long form
		})
		{
			var runtime = serializer.Deserialize<InheritedMemberScenario>(data, 0, data.Length);
			var generated = SourceGeneratorScenariosBois.ReadInheritedMemberScenario(new MemoryStream(data))!;
			foreach (var actual in new[] { runtime, generated })
			{
				Assert.Equal(10, actual.DerivedValue);
				Assert.Equal(20, actual.BaseValue);
			}
		}
	}

	[Fact]
	public void ReadLengthReadsBothForms()
	{
		var shortForm = new BinaryBufferReader([0xC5]);
		Assert.Equal(5, VersionedReader.ReadLength(ref shortForm, shortForm.ReadByte(), "T"));

		var longData = Expected(new Node(Bytes(300)));
		var longForm = new BinaryBufferReader(longData);
		Assert.Equal(300, VersionedReader.ReadLength(ref longForm, longForm.ReadByte(), "T"));
		Assert.Equal(3, longForm.Position);
	}

	[Theory]
	[InlineData(0x02)]
	[InlineData(0x3F)]
	[InlineData(0x41)]
	[InlineData(0x80)]
	[InlineData(0xBF)]
	public void ReadLengthRejectsOtherPrefixes(byte first)
	{
		var reader = new BinaryBufferReader([0x00, 0x00]);
		var ex = Assert.Throws<InvalidDataException>(() => VersionedReader.ReadLength(ref reader, first, "OrderDto"));
		Assert.Contains("OrderDto", ex.Message);
	}

	[Fact]
	public void ReadLengthRejectsASizeLargerThanTheDataLeft()
	{
		// 100,000 declared, 10 bytes present
		var data = Expected(new Node(Bytes(100_000))).Take(10).ToArray();
		var reader = new BinaryBufferReader(data);
		Assert.Throws<InvalidDataException>(() => VersionedReader.ReadLength(ref reader, reader.ReadByte(), "T"));

		// a non-seekable stream can't tell, the skip fails at the end of the data instead
		var streamReader = new BoisStreamReader(new NonSeekableStream(data));
		Assert.Equal(100_000, VersionedReader.ReadLength(ref streamReader, streamReader.ReadByte(), "T"));
		Assert.Throws<EndOfStreamException>(() => VersionedReader.SkipTo(ref streamReader, streamReader.Position + 100_000, "T"));
	}

	public static IEnumerable<object[]> Readers()
	{
		yield return ["byte[]"];
		yield return ["span"];
		yield return ["stream"];
		yield return ["non-seekable stream"];
	}

	[Theory]
	[MemberData(nameof(Readers))]
	public void SkipToJumpsToTheEndOfTheObject(string readerKind)
	{
		// header byte, a 50,000 byte object, then a marker
		var data = new byte[] { 0x11 }.Concat(Bytes(50_000)).Append((byte)0x99).ToArray();
		const int end = 1 + 50_000;

		switch (readerKind)
		{
			case "byte[]":
				var arrayReader = new BinaryBufferReader(data);
				arrayReader.ReadByte();
				VersionedReader.SkipTo(ref arrayReader, end, "T");
				Assert.Equal(0x99, arrayReader.ReadByte());
				break;
			case "span":
				var spanReader = new BinarySpanBufferReader(data);
				spanReader.ReadByte();
				VersionedReader.SkipTo(ref spanReader, end, "T");
				Assert.Equal(0x99, spanReader.ReadByte());
				break;
			default:
				var streamReader = new BoisStreamReader(readerKind == "stream" ? new MemoryStream(data) : new NonSeekableStream(data));
				streamReader.ReadByte();
				VersionedReader.SkipTo(ref streamReader, end, "T");
				Assert.Equal(0x99, streamReader.ReadByte());
				break;
		}
	}

	[Fact]
	public void SkipToRejectsReadingPastTheEnd()
	{
		var reader = new BinaryBufferReader(Bytes(10));
		reader.ReadSpan(6);
		var ex = Assert.Throws<InvalidDataException>(() => VersionedReader.SkipTo(ref reader, 5, "OrderDto"));
		Assert.Contains("OrderDto", ex.Message);
	}

	[Fact]
	public void SkippingDoesNotAllocate()
	{
		var data = Bytes(1_000_000);
		var arrayReader = new BinaryBufferReader(data);
		var streamReader = new BoisStreamReader(new NonSeekableStream(data));

		// warm up: the stream reader creates its small per-thread buffer once
		VersionedReader.Skip(ref arrayReader, 10_000);
		VersionedReader.Skip(ref streamReader, 10_000);

		var before = GC.GetAllocatedBytesForCurrentThread();
		VersionedReader.Skip(ref arrayReader, 900_000);
		VersionedReader.Skip(ref streamReader, 900_000);
		var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

		Assert.True(allocated < 1024, $"{allocated} bytes allocated");
	}

	[Fact]
	public void BoisStreamReaderReadsNormalTypes()
	{
		var serializer = new BoisSerializer();
		var model = Test_CodeGenGenerator.CreateCollectionScenario();
		using var ms = new MemoryStream();
		serializer.Serialize(model, ms);
		var data = ms.ToArray().Append((byte)0x99).ToArray();

		var stream = new NonSeekableStream(data);
		BufferReaderBase reader = new BoisStreamReader(stream);
		var actual = serializer.Deserialize<CollectionScenario, BufferReaderBase>(ref reader);

		Assert.Equal(model.Names, actual.Names);
		Assert.Equal(model.Scores, actual.Scores);
		Assert.Equal(data.Length - 1, reader.Position);

		// the reader doesn't read ahead, the stream is left right after the data
		Assert.Equal(0x99, stream.ReadByte());
	}
}

/// <summary>
/// <see cref="BoisSerializer.MaxVersionedObjectLength"/> is static, so this doesn't run in parallel with other tests.
/// </summary>
[Collection("UntrustedLimitSettings")]
public class Test_VersionedLengthLimit
{
	[Fact]
	public void SizeAboveTheLimitIsRejected()
	{
		var old = BoisSerializer.MaxVersionedObjectLength;
		try
		{
			BoisSerializer.MaxVersionedObjectLength = 100;
			var reader = new BinaryBufferReader([0x01, 0xC8, 0x01]); // size 200
			var ex = Assert.Throws<InvalidDataException>(() => VersionedReader.ReadLength(ref reader, reader.ReadByte(), "T"));
			Assert.Contains(nameof(BoisSerializer.MaxVersionedObjectLength), ex.Message);

			var small = new BinaryBufferReader([0xC5, 0, 0, 0, 0, 0]);
			Assert.Equal(5, VersionedReader.ReadLength(ref small, small.ReadByte(), "T"));
		}
		finally
		{
			BoisSerializer.MaxVersionedObjectLength = old;
		}
	}
}
