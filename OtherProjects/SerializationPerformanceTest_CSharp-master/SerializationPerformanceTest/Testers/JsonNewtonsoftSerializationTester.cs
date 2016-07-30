using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace SerializationPerformanceTest.Testers
{
    class JsonNewtonsoftSerializationTester<TTestObject> : SerializationTester<TTestObject>
    {
        private readonly JsonSerializer jsonSerializer;
        private StreamReader streamReader;


        public JsonNewtonsoftSerializationTester(TTestObject testObject)
            : base(testObject)
        {
            jsonSerializer = new JsonSerializer();
        }

        protected override void Init()
        {
            base.Init();
            streamReader = new StreamReader(this.MemoryStream);
        }

        protected override TTestObject Deserialize()
        {
            base.MemoryStream.Position = 0;
            var jsonTextReader = new JsonTextReader(streamReader) { CloseInput = false };

            return jsonSerializer.Deserialize<TTestObject>(jsonTextReader);
        }

        protected override MemoryStream Serialize()
        {
            var stream = new MemoryStream();
            var streamWriter = new StreamWriter(stream);
            jsonSerializer.Serialize(streamWriter, base.TestObject);
            streamWriter.Flush();

            return stream;
        }

        public override void Dispose()
        {
            streamReader.Dispose();
            base.Dispose();
        }
    }
}