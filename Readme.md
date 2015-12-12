Salar.Bois is a high compact ratio, fast and powerful binary serializer for .NET Framework.
With Bois you can serialize your existing objects with almost no change.

BOIS can serialize almost anything as long as they satisfy these conditions.

* Having parameter-less public constructor.
* Polymorphous properties have limited support, which only base type mentioned in the property will be serialized/deserialized.
* Collections/Lists should be generic and implement either of IList<> or IDictionary<>

Some classes have special support, which are listed below:

* NameValueCollection
* Version, Color and Guid

##NuGet Package 
```
PM> Install-Package Salar.Bois
```

It is easy to use , just add the reference to your project and voila, you can now use it.

###How to serialize an object:
```csharp
var boisSerializer = new BoisSerializer();
using (var mem = new MemoryStream())
{
	boisSerializer.Serialize(this, mem);

	return mem.ToArray();
}
```
###How to deserialize an object:
```csharp
var boisSerializer = new BoisSerializer();
return boisSerializer.Deserialize<SampleObject>(dataStream);
```
##Features in progress 

Some features are in progress and will be available as soon as they're completed:

* Support multidimensional array of simple type
* Support Multidimensional array of generic object with polymorphic attribute
* An option to control depth of object serialization.
* ~~Use compacted serialize method for all the primitive types.~~
* ~~Embed cache object inside the serializer.~~

##Benchmarks

The benchmarks sourcecode is available. Every elapsed time is calculated for 5000 iteration (v2.2).

-Serialization against: PrimitiveTypes with small data

Serializer | 	Serialized Data Size (byte)  | Serialization | Deserialization | Format | Note
------------ | ------------ | ------------ | ------------ | ------------ | ------------
Salar.Bois |	**73** |	4 ms |	6 ms |	Binary 	 
Microsoft.Avro |	77 |	2 ms |	3 ms |	Binary 	 
MessagePack |	90 |	8 ms |	14 ms |	Binary 	 
protobuf-net| 	91 |	4 ms |	12 ms |	Binary 	 
BinaryFormatter |	651 |	85 ms| 	106 ms |	Binary 	 
NetSerialize |	85 |	4 ms |	6 ms |	Binary 	 


-Serialization against: PrimitiveTypes with big data for 100 iteration

Serializer | 	Serialized Data Size (byte)  | Serialization | Deserialization | Format | Note
------------ | ------------ | ------------ | ------------ | ------------ | ------------
Salar.Bois |	91 |	4 ms |	7 ms |	Binary 	 
Microsoft.Avro |	88 |	2 ms |	3 ms |	Binary 	 
MessagePack |	98 |	8 ms |	14 ms |	Binary 	 
protobuf-net| 	101 |	4 ms |	12 ms |	Binary 	 
BinaryFormatter |	651 |	85 ms| 	106 ms |	Binary 	 
NetSerialize |	95 |	4 ms |	6 ms |	Binary 	 


-Serialization against: ArrayTypes with small data

Serializer | 	Serialized Data Size (byte)  | Serialization | Deserialization | Format | Note
------------ | ------------ | ------------ | ------------ | ------------ | ------------
Salar.Bois |	**58** |	14 ms |	16 ms |	Binary 	 
Microsoft.Avro |	81 |	4 ms |	6 ms |	Binary 	 
MessagePack |	85 |	4 ms |	12 ms |	Binary 	 
protobuf-net| 	87 |	4 ms |	15 ms |	Binary 	 
BinaryFormatter |	414 |	57 ms| 	54 ms |	Binary 	 
NetSerialize |	89 |	4 ms |	6 ms |	Binary 


-Serialization against: ArrayTypes with big data for 100 iteration

Serializer | 	Serialized Data Size (byte)  | Serialization | Deserialization | Format | Note
------------ | ------------ | ------------ | ------------ | ------------ | ------------
Salar.Bois |	**129948** |	464 ms |	476 ms |	Binary 	 
Microsoft.Avro |	131135 |	126  ms |	146 ms |	Binary 	 
MessagePack |	163902 |	109 ms |	202 ms |	Binary 	 
protobuf-net| 	163902 |	72 ms |	72 ms |	Binary 	 
BinaryFormatter |	131466 |	12 ms| 	6 ms |	Binary 	 
NetSerialize |	163902 |	107 ms |	80 ms |	Binary 


-Serialization against: SimpleCollections with small data

Serializer | 	Serialized Data Size (byte)  | Serialization | Deserialization | Format | Note
------------ | ------------ | ------------ | ------------ | ------------ | ------------
Salar.Bois |	89 |	17 ms |	16 ms |	Binary 	 
Microsoft.Avro |	89 |	4 ms |	6 ms |	Binary 	 
MessagePack |	83 |	4 ms |	12 ms |	Binary 	 
protobuf-net| 	122 |	4 ms |	15 ms |	Binary 	 
BinaryFormatter |	6703 |	57 ms| 	54 ms |	Binary 	 
NetSerialize |	- |	- |	- |	Binary |    Failed


-Serialization against: SimpleCollections with big data for 100 iteration

Serializer | 	Serialized Data Size (byte)  | Serialization | Deserialization | Format | Note
------------ | ------------ | ------------ | ------------ | ------------ | ------------
Salar.Bois |	**41543** |	353 ms |	305 ms |	Binary 	 
Microsoft.Avro |	**41543** |	80 ms |	97 ms |	Binary 	 
MessagePack |	42380 |	51 ms |	200 ms |	Binary 	 
protobuf-net| 	77585 |	86 ms |	133 ms |	Binary 	 
BinaryFormatter |	159756 |	196 ms| 	205 ms |	Binary 	 
NetSerialize |	- |	- |	- |	Binary |    Failed


-Serialization against: ComplexCollections

Serializer | 	Serialized Data Size (byte)  | Serialization | Deserialization | Format | Note
------------ | ------------ | ------------ | ------------ | ------------ | ------------
Salar.Bois |	**93** |	46 ms |	94 ms |	Binary 	 
Microsoft.Avro | - |	- |	- |	Binary |    Failed
MessagePack |	- |	- |	- |	Binary |    Failed
protobuf-net| 	150 |	16 ms |	38 ms |	Binary 	 
BinaryFormatter |	10807 |	885 ms| 783 ms |	Binary 	 
NetSerialize | - |	- |	- |	Binary |    Failed


-Serialization against: SpecializedCollections

Serializer | 	Serialized Data Size (byte)  | Serialization | Deserialization | Format | Note
------------ | ------------ | ------------ | ------------ | ------------ | ------------
Salar.Bois |	**29** |	13 ms |	36 ms |	Binary 	 
Microsoft.Avro | - |	- |	- |	Binary | Failed with invalid result
MessagePack |	- |	- |	- |	Binary |    Failed
protobuf-net| - |	- |	- |	Binary |    Failed
BinaryFormatter |	2505 |	768  ms| 	379  ms |	Binary 	 
NetSerialize | - |	- |	- |	Binary |    Failed


-Serialization against: ComplexContainer object

Serializer | 	Serialized Data Size (byte)  | Serialization | Deserialization | Format | Note
------------ | ------------ | ------------ | ------------ | ------------ | ------------
Salar.Bois |	**1292** |	386  ms |	944 ms |	Binary 	 
Microsoft.Avro | - | - | - | Binary | Failed
MessagePack | - | - | - | Binary | Failed
protobuf-net| 1550 | 204 ms | 341 ms |	Binary
BinaryFormatter | 18910 | 3s,297 ms | 3s,595 ms |	Binary 	 
NetSerialize | 1482 | 143 ms | 189 ms |	Binary

