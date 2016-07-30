using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ProtoBuf;

namespace SerializationPerformanceTest.TestData.BelgianBeer
{
 
    [Serializable, ProtoContract, DataContract]
    public class Beer
    {
        [ProtoMember(1), DataMember]
        public string Brand { get; set; }

        [ProtoMember(2), DataMember]
        public List<String> Sort { get; set; }

        [ProtoMember(3), DataMember]
        public float Alcohol { get; set; }

        [ProtoMember(4), DataMember]
        public string Brewery { get; set; }
    }
}