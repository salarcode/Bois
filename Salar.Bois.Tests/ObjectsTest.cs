using System;
using System.Threading;
using System.Threading.Tasks;
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
		public void BasicTypesDateTimeTest_XmlSerializer()
		{
			var serial = new System.Xml.Serialization.XmlSerializer(typeof(BasicTypesDateTime));
			var init = new BasicTypesDateTime();
			init.Initialize();
			BasicTypesDateTime final;

			using (var mem = new MemoryStream())
			using (var r = new StreamReader(mem))
			{
				serial.Serialize(mem, init);

				mem.Seek(0, SeekOrigin.Begin);
				var xml = r.ReadToEnd();

				mem.Seek(0, SeekOrigin.Begin);
				final = (BasicTypesDateTime)serial.Deserialize(mem);
			}
			AssertionHelper.AssertMembersAreEqual(init, final);
		}

		[TestMethod]
		public void BasicTypesDateTimeTest()
		{
			var init = new BasicTypesDateTime();
			init.Initialize();
			BasicTypesDateTime final;

			using (var mem = new MemoryStream())
			{
				_bois.Serialize(init, mem);
				mem.Seek(0, SeekOrigin.Begin);
				final = _bois.Deserialize<BasicTypesDateTime>(mem);
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
			var init = StructType1.InitializeThis();
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
		public void StructType1_NullableVariable()
		{
			StructType1? init = StructType1.InitializeThis();
			StructType1? final;

			using (var mem = new MemoryStream())
			{
				_bois.Serialize(init, mem);
				mem.Seek(0, SeekOrigin.Begin);
				final = _bois.Deserialize<StructType1?>(mem);
			}

			Assert.IsNotNull(final);
			AssertionHelper.AssertMembersAreEqual(init.Value, final.Value);
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
			Assert.IsNull(final.STypeNull);
			AssertionHelper.AssertMembersAreEqual(init.SType, final.SType);
			AssertionHelper.AssertMembersAreEqual(init.STypeNullable.Value, final.STypeNullable.Value);
			init.LastName.Should().Be.EqualTo(final.LastName);
			init.AcceptableAges.Should().Have.SameSequenceAs(final.AcceptableAges);
		}
		[TestMethod]
		public void HierarchyWithStruct2_NormalTest()
		{
			var init = new HierarchyWithStruct2();
			init.Initialize();
			HierarchyWithStruct2 final;

			using (var mem = new MemoryStream())
			{
				_bois.Serialize(init, mem);
				mem.Seek(0, SeekOrigin.Begin);
				final = _bois.Deserialize<HierarchyWithStruct2>(mem);
			}
			init.CEnc.Should().Be.EqualTo(final.CEnc);
			init.CLen.Should().Be.EqualTo(final.CLen);
			init.CType.Should().Be.EqualTo(final.CType);
			init.SCode.Should().Be.EqualTo(final.SCode);
			init.SDesc.Should().Be.EqualTo(final.SDesc);
			init.Ver.Should().Be.EqualTo(final.Ver);
			init.ChSet.Should().Be.EqualTo(final.ChSet);
			init.HDR.Should().Have.SameSequenceAs(final.HDR);
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



		[TestMethod]
		public void ThreadSafe_NormalTest()
		{
			var boisThreaded = new BoisSerializer();
			var init = new PrimitiveTypes1();
			init.Initialize();
			PrimitiveTypes1 final;

			using (var iniToDeserialMem = new MemoryStream())
			{
				boisThreaded.Serialize(init, iniToDeserialMem);
				iniToDeserialMem.Seek(0, SeekOrigin.Begin);

				int done = 0;
				var tasks = new Thread[500];
				for (int i = 0; i < tasks.Length; i++)
				{
					var th = new Thread(
						() =>
						{
							Thread.Sleep(50);
							using (var mem = new MemoryStream())
							{
								boisThreaded.Serialize(init, mem);
								mem.Seek(0, SeekOrigin.Begin);
								final = boisThreaded.Deserialize<PrimitiveTypes1>(mem);
							}
							Interlocked.Increment(ref done);
							AssertionHelper.AssertMembersAreEqual(init, final);
						});
					th.IsBackground = true;
					th.Name = "ThreadSafe_Test_" + i;
					tasks[i] = th;
				}
				foreach (var task in tasks)
				{
					task.Start();
				}

				while (done < tasks.Length)
				{
					Thread.Sleep(10);
				}
			}
		}


		[TestMethod]
		public void CommonListChildObject_BasicTest()
		{
			var init = new CommonListChildObject();
			init.AddRange(new[] { "Item1", "Item2", "Item3" });
			init.SyncDate = DateTime.Now;
			init.ListName = "Test";
			CommonListChildObject final;

			using (var mem = new MemoryStream())
			{
				_bois.Serialize(init, mem);
				mem.Seek(0, SeekOrigin.Begin);
				final = _bois.Deserialize<CommonListChildObject>(mem);
			}

			// test for the list members
			final.Should().Have.SameSequenceAs(init);

			// WILL FAIL ANYWAY, properties for contained collections are not supported
			//AssertionHelper.AssertMembersAreEqual(init, final);
		}


		[TestMethod]
		public void ArraySingleType1_NormalTest()
		{
			var init = new ArraySingleType1();
			init.Initialize();
			ArraySingleType1 final;

			using (var mem = new MemoryStream())
			{
				_bois.Serialize(init, mem);
				mem.Seek(0, SeekOrigin.Begin);
				final = _bois.Deserialize<ArraySingleType1>(mem);
			}
			AssertionHelper.AssertMembersAreEqual(init, final);
		}

	}
}
