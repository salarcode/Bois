using System;
using Salar.Bon;
using Salar.Bon.Tests.Objects;
using SharpTestsEx;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

		[TestMethod]
		public void HierarchyObjects1Test()
		{
			var init = new HierarchyObjects1();
			init.Initialize();
			HierarchyObjects1 final;

			using (var mem = new MemoryStream())
			{
				_bon.Serialize(init, mem);
				mem.Seek(0, SeekOrigin.Begin);
				final = _bon.Deserialize<HierarchyObjects1>(mem);
			}

			AssertionHelper.AssertMembersAreEqual(init.Child1, final.Child1);
			AssertionHelper.AssertMembersAreEqual(init.Child2, final.Child2);
			init.Name.Should().Be.EqualTo(final.Name);
			init.AcceptableAges.Should().Have.SameSequenceAs(final.AcceptableAges);
			init.LastName.Should().Be.EqualTo(final.LastName);
		}

		[TestMethod]
		public void HierarchyObjects1_ChildTest()
		{
			var init = new HierarchyObjects1.TheChild();
			init.Initialize();
			HierarchyObjects1.TheChild final;

			using (var mem = new MemoryStream())
			{
				_bon.Serialize(init, mem);
				mem.Seek(0, SeekOrigin.Begin);
				final = _bon.Deserialize<HierarchyObjects1.TheChild>(mem);
			}

			AssertionHelper.AssertMembersAreEqual(init, final);
		}

		[TestMethod]
		public void StructType1_NormalTest()
		{
			var init = new StructType1();
			init.Initialize();
			StructType1 final;

			using (var mem = new MemoryStream())
			{
				_bon.Serialize(init, mem);
				mem.Seek(0, SeekOrigin.Begin);
				final = _bon.Deserialize<StructType1>(mem);
			}

			AssertionHelper.AssertMembersAreEqual(init, final);
		}

		[TestMethod]
		public void HierarchyWithStruct_NormalTest()
		{
			var init = new HierarchyWithStruct();
			init.Initialize();
			HierarchyWithStruct final;

			using (var mem = new MemoryStream())
			{
				_bon.Serialize(init, mem);
				mem.Seek(0, SeekOrigin.Begin);
				final = _bon.Deserialize<HierarchyWithStruct>(mem);
			}

			AssertionHelper.AssertMembersAreEqual(init, final);
		}


	}
}
