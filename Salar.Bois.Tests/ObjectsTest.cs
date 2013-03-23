using System;
using Salar.Bois;
using Salar.Bois.Tests.Objects;
using SharpTestsEx;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Salar.Bois.Tests
{
	[TestClass]
	public class ObjectsTest
	{
		BoisSerializer _bois = new BoisSerializer();

		[TestMethod]
		public void PrimitiveTypes1Test()
		{
			var init = new PrimitiveTypes1();
			init.Initialize();
			PrimitiveTypes1 final;

			using (var mem = new MemoryStream())
			{
				_bois.Serialize(init, mem);
				mem.Seek(0, SeekOrigin.Begin);
				final = _bois.Deserialize<PrimitiveTypes1>(mem);
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
				_bois.Serialize(init, mem);

				mem.Seek(0, SeekOrigin.Begin);

				final = _bois.Deserialize<PrimitiveTypes1Nullable>(mem);
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
				_bois.Serialize(init, mem);
				mem.Seek(0, SeekOrigin.Begin);

				final = _bois.Deserialize<DBNull>(mem);

				// means deserialize should advance the position in stream 
				mem.Position.Should().Be.GreaterThan(0);
			}
			final.Should().Be(final);
		}

		[TestMethod]
		public void Guid_Normal_Test()
		{
			var init = Guid.NewGuid();
			Guid final;

			using (var mem = new MemoryStream())
			{
				_bois.Serialize(init, mem);
				mem.Seek(0, SeekOrigin.Begin);
				final = _bois.Deserialize<Guid>(mem);
			}
			final.Should().Be.EqualTo(init);
		}

		[TestMethod]
		public void Guid_Empty_Test()
		{
			var init = Guid.Empty;
			Guid final;

			using (var mem = new MemoryStream())
			{
				_bois.Serialize(init, mem);
				mem.Seek(0, SeekOrigin.Begin);
				final = _bois.Deserialize<Guid>(mem);
			}
			final.Should().Be.EqualTo(init);
		}

		[TestMethod]
		public void BasicTypes1Test()
		{
			var init = new BasicTypes1();
			init.Initialize();
			BasicTypes1 final;

			using (var mem = new MemoryStream())
			{
				_bois.Serialize(init, mem);
				mem.Seek(0, SeekOrigin.Begin);
				final = _bois.Deserialize<BasicTypes1>(mem);
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
				_bois.Serialize(init, mem);
				mem.Seek(0, SeekOrigin.Begin);
				final = _bois.Deserialize<BasicTypes1Nullable>(mem);
			}
			AssertionHelper.AssertMembersAreEqual(init, final);
		}

		[TestMethod]
		public void BasicTypes2NullableTest()
		{
			var init = new BasicTypes2Nullable();
			init.Initialize();
			BasicTypes2Nullable final;

			using (var mem = new MemoryStream())
			{
				_bois.Serialize(init, mem);
				mem.Seek(0, SeekOrigin.Begin);
				final = _bois.Deserialize<BasicTypes2Nullable>(mem);
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
				_bois.Serialize(init, mem);
				mem.Seek(0, SeekOrigin.Begin);
				final = _bois.Deserialize<HierarchyObjects1>(mem);
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
				_bois.Serialize(init, mem);
				mem.Seek(0, SeekOrigin.Begin);
				final = _bois.Deserialize<HierarchyObjects1.TheChild>(mem);
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
				_bois.Serialize(init, mem);
				mem.Seek(0, SeekOrigin.Begin);
				final = _bois.Deserialize<StructType1>(mem);
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
				_bois.Serialize(init, mem);
				mem.Seek(0, SeekOrigin.Begin);
				final = _bois.Deserialize<HierarchyWithStruct>(mem);
			}

			AssertionHelper.AssertMembersAreEqual(init.SType, final.SType);
			init.LastName.Should().Be.EqualTo(final.LastName);
			init.AcceptableAges.Should().Have.SameSequenceAs(final.AcceptableAges);
		}

		[TestMethod]
		public void CollectionTypes1_NormalTest()
		{
			var init = new CollectionTypes1();
			init.Initialize();
			CollectionTypes1 final;

			using (var mem = new MemoryStream())
			{
				_bois.Serialize(init, mem);
				mem.Seek(0, SeekOrigin.Begin);
				final = _bois.Deserialize<CollectionTypes1>(mem);
			}
			AssertionHelper.AssertMembersAreEqual(init, final);
		}


	}
}
