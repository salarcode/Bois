using System;
using System.Drawing;
using ReflectionMagic;
using Salar.Bois;
using Salar.Bois.Tests.Objects;
using SharpTestsEx;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Salar.Bois.Tests
{
	[TestClass]
	public class AttributeTests
	{
		private BoisSerializer _bois;
		private dynamic bion;
		private MemoryStream bionStream;
		private BinaryReader bionReader;
		private BinaryWriter bionWriter;

		[TestInitialize]
		public void Initialize()
		{
			_bois = new BoisSerializer();
			bion = _bois.AsDynamic();
			bionStream = new MemoryStream();
			bionReader = new BinaryReader(bionStream);
			bionWriter = new BinaryWriter(bionStream);
		}

		private void EchoStreamSize()
		{
			Console.WriteLine("DataStream size: " + bionStream.Length);
		}

		void ResetStream()
		{
			bionStream.Seek(0, SeekOrigin.Begin);
		}
		[TestMethod]
		public void ObjectWithAttibute_Normal_Test()
		{
			var init = new ObjectWithAttribute();
			init.Initialize();
			ObjectWithAttribute final;

			using (var mem = new MemoryStream())
			{
				_bois.Serialize(init, mem);

				mem.Seek(0, SeekOrigin.Begin);

				final = _bois.Deserialize<ObjectWithAttribute>(mem);
			}
			init.AcceptChar.Should().Be(final.AcceptChar);

			AssertionHelper.AssertMembersAreEqual(final.ForeColor, new Color());

			init.Language.Should().Be(final.Language);
			final.PassedTimeSpan.Should().Be(new TimeSpan());
			init.TestDate.Should().Be(final.TestDate);
			init.TestGuid.Should().Be(final.TestGuid);
			final.Text1.Should().Be(null);
			init.TextField2.Should().Be(final.TextField2);
		}

	}
}
