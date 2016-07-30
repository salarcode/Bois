using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace SerializationPerformanceTest.Testers
{
    class BinarySerializationTester<TTestObject> : SerializationTester<TTestObject>
    {
        private readonly IFormatter formatter;

        public BinarySerializationTester(TTestObject testObject)
            : base(testObject)
        {
            formatter = new BinaryFormatter();
        }

        protected override TTestObject Deserialize()
        {
            base.MemoryStream.Seek(0, 0);
            TTestObject deserialize = (TTestObject)formatter.Deserialize(base.MemoryStream);
            return deserialize;
        }


        protected override MemoryStream Serialize()
        {
            var stream = new MemoryStream();
            formatter.Serialize(stream, base.TestObject);

            return stream;
        }
    }
}