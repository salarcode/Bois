using Salar.Bois.NetFx.Tests.Base;
using Salar.Bois.NetFx.Tests.TestObjects;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Salar.Bois.NetFx.Tests.Tests
{
	public class Test_Complex_Objects : TestBase
	{

		[Theory]
		[MemberData(nameof(TestObjectPrimitiveTypes.GetTestData), MemberType = typeof(TestObjectPrimitiveTypes))]
		public void TestingObjectPrimitiveTypes(TestObjectPrimitiveTypes init)
		{
			ResetBois();

			//BoisSerializer.Initialize<TestObjectPrimitiveTypes>();

			Bois.Serialize(init, TestStream);
			ResetStream();

			var final = Bois.Deserialize<TestObjectPrimitiveTypes>(TestStream);

			SerializeAreEqual(init, final);
		}

        /// <summary>
        /// Executes same tests as <see cref="TestingObjectPrimitiveTypes(TestObjectPrimitiveTypes)"/>, but
		/// triggers a StreamBufferReader to be used instead of a BinaryBufferReader in <see cref="BoisSerializer.Deserialize(BinaryBuffers.BufferReaderBase, Type)"/>
		/// by passing in a MemoryStream there TryGetBuffer() will return false.
		/// This will cause <see cref="PrimitiveReader.ReadByteArray(BinaryBuffers.BufferReaderBase)"/> cause to re-throw an
		/// <see cref="EndOfStreamException"/> when trying to read a zero-length byte array.
        /// </summary>
        /// <param name="init">Test data</param>
        [Theory]
        [MemberData(nameof(TestObjectPrimitiveTypes.GetTestData), MemberType = typeof(TestObjectPrimitiveTypes))]
        public void TestingObjectPrimitiveTypesTyped(TestObjectPrimitiveTypes init)
        {
            ResetBois();

            //BoisSerializer.Initialize<TestObjectPrimitiveTypes>();

            Bois.Serialize(init, TestStream);

            var type = init.GetType();
            var data = TestStream.ToArray();

            using (var stream = new MemoryStream(data))
            {
                var final = Bois.Deserialize(stream, type);

                SerializeAreEqual(init, final);
            }
        }
        [Theory]
		[MemberData(nameof(TestObjectSelfReferencing.GetTestData), MemberType = typeof(TestObjectSelfReferencing))]
		public void TestingObjectSelfReferencing(TestObjectSelfReferencing init)
		{
			ResetBois();

			Bois.Serialize(init, TestStream);
			ResetStream();

			var final = Bois.Deserialize<TestObjectSelfReferencing>(TestStream);

			SerializeAreEqual(init, final);
		}

		[Theory]
		[MemberData(nameof(TestObjectNullableProps.GetTestData), MemberType = typeof(TestObjectNullableProps))]
		public void TestingTestObjectNullableProps(TestObjectNullableProps init)
		{
			ResetBois();

			Bois.Serialize(init, TestStream);
			ResetStream();

			var final = Bois.Deserialize<TestObjectNullableProps>(TestStream);

			SerializeAreEqual(init, final);
		}
	}
}
