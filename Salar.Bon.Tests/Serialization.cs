using System;
using Salar.Bon;
using SharpTestsEx;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Salar.Bion.Tests.Objects;

namespace Salar.Bion.Tests
{
	[TestClass]
	public class Serialization
	{
		BonSerializer _bon = new BonSerializer();

		[TestMethod]
		public void PrimitiveTypes1Test()
		{
			var init = new PrimitiveTypes1();
			init.Initialize();
			PrimitiveTypes1 final;

			using (var mem = new MemoryStream())
			{
				_bon.Serialize(init, mem);
				mem.Seek(0, SeekOrigin.Begin);
				final = _bon.Deserialize<PrimitiveTypes1>(mem);
			}
			AssertionHelper.AssertMembersAreEqual(init, final);
		}

		[TestMethod]
		public void PrimitivesNullableTest()
		{
			var init = new PrimitiveTypes1Nullable();
			init.Initialize();
			PrimitiveTypes1Nullable final;

			using (var mem = new MemoryStream())
			{
				_bon.Serialize(init, mem);

				mem.Seek(0, SeekOrigin.Begin);

				final = _bon.Deserialize<PrimitiveTypes1Nullable>(mem);
			}
			AssertionHelper.AssertMembersAreEqual(init, final);
		}

		[TestMethod]
		public void DbNullTest_Stream_Advancing()
		{
			var init = DBNull.Value;
			DBNull final;

			using (var mem = new MemoryStream())
			{
				_bon.Serialize(init, mem);
				mem.Seek(0, SeekOrigin.Begin);

				final = _bon.Deserialize<DBNull>(mem);

				// means deserialize should advance the position in stream 
				mem.Position.Should().Be.GreaterThan(0);
			}
			final.Should().Be(final);
		}

		[TestMethod]
		public void BasicTypes1Test()
		{
			var init = new BasicTypes1();
			init.Initialize();
			BasicTypes1 final;

			using (var mem = new MemoryStream())
			{
				_bon.Serialize(init, mem);
				mem.Seek(0, SeekOrigin.Begin);
				final = _bon.Deserialize<BasicTypes1>(mem);
			}
			AssertionHelper.AssertMembersAreEqual(init, final);
		}

		[TestMethod]
		public void BasicTypes1NullableTest()
		{
			var init = new BasicTypes1Nullable();
			init.Initialize();
			BasicTypes1Nullable final;

			using (var mem = new MemoryStream())
			{
				_bon.Serialize(init, mem);
				mem.Seek(0, SeekOrigin.Begin);
				final = _bon.Deserialize<BasicTypes1Nullable>(mem);
			}
			AssertionHelper.AssertMembersAreEqual(init, final);
		}
	}
}
