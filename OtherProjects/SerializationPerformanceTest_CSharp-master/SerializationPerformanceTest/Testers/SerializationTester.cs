using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace SerializationPerformanceTest.Testers
{
    abstract public class SerializationTester: IDisposable
    {
        protected MemoryStream MemoryStream { get; set; }

        public abstract void Test(int iterations = 100);

        public virtual void Dispose()
        {
            this.MemoryStream.Dispose();
        }
    }

    /// <summary>
    /// Base class for testing serialization formats/frameworks.
    /// </summary>
    /// <typeparam name="TTestObject">The object that will be tested to Serialize/Deserialize</typeparam>
    abstract public class SerializationTester<TTestObject> : SerializationTester
    {
        protected TTestObject TestObject { get; private set; }

        private bool isInit;

        protected SerializationTester(TTestObject testObject)
        {
            base.MemoryStream = new MemoryStream();
            this.TestObject = testObject;
        }

        /// <summary>
        /// Will do any preparations needed before serializing/deserializing
        /// </summary>
        protected virtual void Init()
        {
            isInit = true;
            this.MemoryStream = Serialize();
            Console.WriteLine("Size of serialized object : " + base.MemoryStream.Length.ToString("#,0"));
        }

        /// <summary>
        /// Will deserialize the TestObject to a .NET Object 
        /// </summary>
        /// <returns></returns>
        protected abstract TTestObject Deserialize();

        /// <summary>
        /// Will serialize the object to a MemoryStream
        /// </summary>
        /// <returns></returns>
        protected abstract MemoryStream Serialize();


        /// <summary>
        /// Will run the tests for Size/Speed of Serialization/Deserialization
        /// </summary>
        /// <param name="iterations"></param>
        public override void Test(int iterations = 100)
        {
            if (!isInit)
            {
                Init();
            }

            TimeSpan timeSpan;

            timeSpan = Measure<TTestObject>(this.Deserialize, iterations);
            Console.WriteLine(this.GetType().Name + "(Deserialize) : " + timeSpan.TotalMilliseconds / iterations);
            GC.Collect();

            timeSpan = Measure<MemoryStream>(this.Serialize, iterations);
            Console.WriteLine(this.GetType().Name + "(Serialize) : " + timeSpan.TotalMilliseconds / iterations);
            GC.Collect();
        }

        private TimeSpan Measure<TTestObject>(Func<TTestObject> testFunc, int iterations)
        {
            var list = new List<TTestObject>(iterations);

            //warm up lazy initialized classes
            TTestObject warmup = testFunc.Invoke();

            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                TTestObject obj = testFunc.Invoke();

                list.Add(obj);
            }

            sw.Stop();

            GC.KeepAlive(warmup);

            return sw.Elapsed;
        }


    }
}