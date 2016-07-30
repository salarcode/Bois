using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace SerializationPerformanceTest.Testers
{
    class DataContractSerializationTester<TTestObject> : SerializationTester<TTestObject>
    {
        private readonly DataContractSerializer serializer;

        public DataContractSerializationTester(TTestObject testObject)
            : base(testObject)
        {
            serializer = new DataContractSerializer(typeof(TTestObject));
        }

        protected override TTestObject Deserialize()
        {
            base.MemoryStream.Seek(0, 0);
            return (TTestObject)serializer.ReadObject(base.MemoryStream);
        }
        
        protected override MemoryStream Serialize()
        {
            var stream = new MemoryStream();

            serializer.WriteObject(stream, base.TestObject);

            return stream;
        }
    }
}