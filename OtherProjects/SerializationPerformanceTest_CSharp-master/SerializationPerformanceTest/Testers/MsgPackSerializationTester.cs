using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MsgPack.Serialization;

namespace SerializationPerformanceTest.Testers
{
    class MsgPackSerializationTester<TTestObject> : SerializationTester<TTestObject>
    {
        private readonly MessagePackSerializer<TTestObject> serializer;


        public MsgPackSerializationTester(TTestObject testObject)
            : base(testObject)
        {
            serializer = MessagePackSerializer.Create<TTestObject>();            
        }


        protected override TTestObject Deserialize()
        {
            base.MemoryStream.Position = 0;
            return serializer.Unpack(base.MemoryStream);
        }
        
        protected override MemoryStream Serialize()
        {
            var stream = new MemoryStream();
            serializer.Pack(stream, base.TestObject);
            return stream;
        }

    }
    
}