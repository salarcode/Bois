using Salar.Bois.NetFx.Tests.CodeGenFixtures;
using Salar.Bois.CodeGen;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Salar.Bois.NetFx.Tests.Tests;

[CollectionDefinition("UntrustedLimitSettings", DisableParallelization = true)]
public class UntrustedLimitSettingsCollection;

/// <summary>
/// The unverified count limit is a static setting, so these tests must not run in parallel with others.
/// </summary>
[Collection("UntrustedLimitSettings")]
public class Test_UntrustedLimitSettings
{
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

	[Fact]
	public void CountOverTheLimitMustFitInTheDataLeft()
	{
		var model = Enumerable.Range(0, 5000).ToArray();
		var serializer = new BoisSerializer();

		using var runtime = new MemoryStream();
		serializer.Serialize(model, runtime);
		var runtimeBytes = runtime.ToArray();

		using var generated = new MemoryStream();
		SourceGeneratorScenariosBois.WriteInt32Array(model, generated);
		var generatedBytes = generated.ToArray();

		var oldRuntimeLimit = BoisSerializer.MaxUnverifiedCount;
		var oldGeneratedLimit = BoisCodeGen.MaxUnverifiedCount;
		try
		{
			BoisSerializer.MaxUnverifiedCount = 1024;
			BoisCodeGen.MaxUnverifiedCount = 1024;

			// the readers which know the data left prove the count
			Assert.Equal(model, serializer.Deserialize<int[]>(runtimeBytes, 0, runtimeBytes.Length));
			Assert.Equal(model, serializer.Deserialize<int[]>(new MemoryStream(runtimeBytes)));
			Assert.Equal(model, SourceGeneratorScenariosBois.ReadInt32Array(new MemoryStream(generatedBytes)));

			// non-seekable streams can't, so the limit applies
			var runtimeEx = Assert.Throws<InvalidDataException>(() => serializer.Deserialize<int[]>(new NonSeekableStream(runtimeBytes)));
			Assert.Contains(nameof(BoisSerializer.MaxUnverifiedCount), runtimeEx.Message);
			var generatedEx = Assert.Throws<InvalidDataException>(() => SourceGeneratorScenariosBois.ReadInt32Array(new NonSeekableStream(generatedBytes)));
			Assert.Contains(nameof(BoisCodeGen.MaxUnverifiedCount), generatedEx.Message);

			// raising the limit allows them
			BoisSerializer.MaxUnverifiedCount = 5000;
			BoisCodeGen.MaxUnverifiedCount = 5000;
			Assert.Equal(model, serializer.Deserialize<int[]>(new NonSeekableStream(runtimeBytes)));
			Assert.Equal(model, SourceGeneratorScenariosBois.ReadInt32Array(new NonSeekableStream(generatedBytes)));
		}
		finally
		{
			BoisSerializer.MaxUnverifiedCount = oldRuntimeLimit;
			BoisCodeGen.MaxUnverifiedCount = oldGeneratedLimit;
		}
	}

	[Fact]
	public void ByteLengthOverTheLimitMustFitInTheDataLeft()
	{
		var model = new string('a', 100_000);
		var serializer = new BoisSerializer();

		using var runtime = new MemoryStream();
		serializer.Serialize(model, runtime);
		var bytes = runtime.ToArray();

		var oldLimit = BoisSerializer.MaxUnverifiedByteLength;
		try
		{
			BoisSerializer.MaxUnverifiedByteLength = 70_000;

			Assert.Equal(model, serializer.Deserialize<string>(new MemoryStream(bytes)));
			var ex = Assert.Throws<InvalidDataException>(() => serializer.Deserialize<string>(new NonSeekableStream(bytes)));
			Assert.Contains(nameof(BoisSerializer.MaxUnverifiedByteLength), ex.Message);

			BoisSerializer.MaxUnverifiedByteLength = 100_000;
			Assert.Equal(model, serializer.Deserialize<string>(new NonSeekableStream(bytes)));
		}
		finally
		{
			BoisSerializer.MaxUnverifiedByteLength = oldLimit;
		}
	}
}
