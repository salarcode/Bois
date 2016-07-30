using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SerializationPerformanceTest.Testers
{
    class ProtobufSerializationTester<TTestObject> : SerializationTester<TTestObject>
    {
        public ProtobufSerializationTester(TTestObject testObject)
            : base(testObject)
        {
        }

        protected override TTestObject Deserialize()
        {
            base.MemoryStream.Seek(0, 0);
            return ProtoBuf.Serializer.Deserialize<TTestObject>(base.MemoryStream);
        }
        
        protected override MemoryStream Serialize()
        {
            var stream = new MemoryStream();
            ProtoBuf.Serializer.Serialize(stream, base.TestObject);
            return stream;
        }
    }
}