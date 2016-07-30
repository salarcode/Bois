using System.IO;
using ServiceStack.Text;

namespace SerializationPerformanceTest.Testers
{
    class JsonServiceStackSerializationTester<TTestObject> : SerializationTester<TTestObject>
    {
        private readonly TypeSerializer<TTestObject> serializer;
        private StreamReader streamReader;


        public JsonServiceStackSerializationTester(TTestObject testObject)
            : base(testObject)
        {
            serializer = new TypeSerializer<TTestObject>();
        }

        protected override void Init()
        {
            base.Init();

            streamReader = new StreamReader(this.MemoryStream);
        }

        protected override TTestObject Deserialize()
        {
            base.MemoryStream.Position = 0;
            return serializer.DeserializeFromReader(this.streamReader);
        }

        protected override MemoryStream Serialize()
        {
            var stream = new MemoryStream();
            var streamWriter = new StreamWriter(stream);
            serializer.SerializeToWriter(base.TestObject, streamWriter);
            streamWriter.Flush();

            return stream;
        }

        public override void Dispose()
        {
            this.streamReader.Dispose();
            base.Dispose();
        }
    }
}