using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Xml.Serialization;

namespace SerializationPerformanceTest.Testers
{
    class XmlSerializationTester<TTestObject> : SerializationTester<TTestObject>{
        private readonly XmlSerializer serializer;

        public XmlSerializationTester(TTestObject testObject)
            : base(testObject)
        {
            serializer = new XmlSerializer(typeof(TTestObject));
            
        }

        protected override TTestObject Deserialize()
        {
            base.MemoryStream.Seek(0, 0);
            var deserialize = (TTestObject)serializer.Deserialize(base.MemoryStream);
            return deserialize;
        }

        protected override MemoryStream Serialize()
        {
            var stream = new MemoryStream();
            serializer.Serialize(stream, base.TestObject);
            return stream;
        }
    }
}