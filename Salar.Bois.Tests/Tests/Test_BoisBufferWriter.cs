using Salar.Bois.NetFx.Tests.Base;
using Salar.Bois.Types;
using System.IO;
using Xunit;

namespace Salar.Bois.NetFx.Tests.Tests
{
	public class Test_BoisBufferWriter : TestBase
	{
		[Fact]
		public void ManualStreamWriterFlushPushesBufferedBytesToOutput()
		{
			using var stream = new MemoryStream();
			var writer = new BoisBufferWriter(stream);

			writer.Write(new byte[] { 1, 2, 3, 4 });

			Assert.Equal(0, stream.Length);

			writer.Flush();

			Assert.Equal(new byte[] { 1, 2, 3, 4 }, stream.ToArray());
		}

		[Fact]
		public void SerializeLargeByteArrayToBufferedStreamOutput()
		{
			var init = CreatePayload(BoisBufferWriter.InternalCacheSize + 257);
			using var stream = new MemoryStream();

			Bois.Serialize(init, stream);

			Assert.True(stream.Length > 0);

			stream.Position = 0;
			var final = Bois.Deserialize<byte[]>(stream);

			Assert.Equal(init, final);
		}

		[Fact]
		public void SerializeLargeByteArrayToBufferedArrayOutput()
		{
			var init = CreatePayload(BoisBufferWriter.InternalCacheSize + 257);
			var output = new byte[init.Length + 64];

			Bois.Serialize(init, output, 7, output.Length - 7);

			var final = Bois.Deserialize<byte[]>(output, 7, output.Length - 7);

			Assert.Equal(init, final);
		}

		private static byte[] CreatePayload(int length)
		{
			var data = new byte[length];
			for (var i = 0; i < data.Length; i++)
				data[i] = unchecked((byte)i);
			return data;
		}
	}
}
