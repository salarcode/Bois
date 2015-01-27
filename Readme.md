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

~~* Use compacted serialize method for more primitive types.~~
* An option to control depth of object serialization.
~~* Embed cache object inside the serializer.~~
* Support ISet<> for .Net4

##Benchmarks

The benchmarks sourcecode is available. Every elapsed time is calculated for 5000 iteration (v1.6).

-Serialization against: BasicTypes

Serializer | 	Serialized Data Size (byte)  | Serialize Time (ms) | Deserialize Time (ms) | Format | Note
------------ | ------------ | ------------ | ------------ | ------------ | ------------
Salar.Bois |	106 |	13 ms |	18 ms |	Binary 	 
protobuf-net| 	115 |	6 ms |	10 ms |	Binary 	 
NetSerialize |	107 |	9 ms |	9 ms |	Binary 	 
SharpSerializer| 	432 |	146 ms |	211 ms |	Binary 	|SizeOptimized option used
BinaryFormatter |	638 |	127 ms| 	154 ms |	Binary 	 
BSON by Json.NET |	218 |	50 ms |	60 ms |	Binary 	 
Json.NET 	|216| 	48 ms| 	5 ms |	JSON 	 


-Serialization against: HierarchyObject

Serializer | 	Serialized Data Size (byte)  | Serialize Time (ms) | Deserialize Time (ms) | Format | Note
------------ | ------------ | ------------ | ------------ | ------------ | ------------
Salar.Bois |	326 |	26 ms |	29 ms |	Binary 	 
protobuf-net |	341 |	10 ms |	19 ms |	Binary 	 
NetSerialize |	356 |	27 ms |	22 ms |	Binary 	 
SharpSerializer |	844 |	208 ms |	225 ms |	Binary |	SizeOptimized option used
BinaryFormatter |	1657 |	283 ms |	315 ms |	Binary 	 
BSON by Json.NET |	652 |	106 ms |	140 ms |	Binary 	 
Json.NET |	686 |	105 ms |	4 ms |	JSON 	 


 -Serialization against: Collections
 
Serializer | 	Serialized Data Size (byte)  | Serialize Time (ms) | Deserialize Time (ms) | Format | Note
------------ | ------------ | ------------ | ------------ | ------------ | ------------
Salar.Bois |	72 |	40 ms |	46 ms 	Binary 	 
protobuf-net |	93 |	9 ms |	20 ms |	Binary 	 
NetSerialize |	79 |	18 ms |	14 ms |	Binary 	 
SharpSerializer |	854 |	244 ms |	322 ms |	Binary |	SizeOptimized option used
BinaryFormatter |	4552 |	431 ms |	464 ms |	Binary 	 
BSON by Json.NET| 	237 |	89 ms |	140 ms |	Binary 	 
Json.NET |	179 |	103 ms |	6 ms |	JSON 	 


  -Serialization against: SpecialCollections
  
Serializer | 	Serialized Data Size (byte)  | Serialize Time (ms) | Deserialize Time (ms) | Format | Note
------------ | ------------ | ------------ | ------------ | ------------ | ------------
Salar.Bois |	30 |	16 ms |	30 ms |	Binary 	 
protobuf-net |	- |	- |	- |	Binary |	Failed
NetSerialize |	- |	- |	- |	Binary 	| Failed
SharpSerializer |	- |	- |	- |	Binary 	| 	Failed
BinaryFormatter |	2497 |	901 ms 	| 	463 ms 	| 	Binary 	 
BSON by Json.NET |	95 |	48 ms 	| 	Failed 	| 	Binary 	| 	deserialization failed.
Json.NET |	57 |	24 ms |	Failed 	| 	JSON 	| 	 deserialization failed.
