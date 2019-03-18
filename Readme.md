Salar.Bois is the most compact, extermly fast binary serializer for .NET Code and .NET Framework.

* No compression is done, the high compact ratio is the result of Bois format.
* No configuration.
* No change to your classes.

## Why Salar.Bois?
* Because payload size matters. Bois serializer generates the smallest payload size.
* Because speed matters. Both serialization and deserialization are extermly fast.
* Easy to use, `Serialize<T>` and `Deserialize<T>` are all you need.
* No configuration required. No sperate schema required. 

## [NuGet Package](https://www.nuget.org/packages/Salar.Bois)
```
PM> Install-Package Salar.Bois
```

### Getting Started:
It is easy to use , just add the package to your project and voila, you can now use it.

All you need to do is to call `Serialize` method.
```csharp
BoisSerializer.Initialize<Person>();
var boisSerializer = new BoisSerializer();

using (var mem = new MemoryStream())
{
	boisSerializer.Serialize(personInstance, mem);

	return mem.ToArray();
}
```
Note: Calling `BoisSerializer.Initialize` is not required at all, but if the performance of application is important to you, it is better to do it at the begining of your application.

Here is the complete example:
```csharp
public class Project
{
	public int ID { get; set; }
	public string Name { get; set; }
	public string ProjectUrl { get; set; }
	public Version Version { get; set; }
}

class Program
{
	static void Main()
	{
		// Initialize is optional, but recommended for better performance
		BoisSerializer.Initialize<Project>();

		var projectInstance = new Project()
		{
			ID = 1,
			Name = "Salar.Bois",
			ProjectUrl = "https://github.com/salarcode/Bois",
			Version = new Version(3, 0, 0, 0)
		};

		var boisSerializer = new BoisSerializer();

		using (var file = new FileStream("output.data", FileMode.CreateNew))
		{
			boisSerializer.Serialize(projectInstance, file);
		}
	}
}
```
### How to deserialize an object:
```csharp
var boisSerializer = new BoisSerializer();
return boisSerializer.Deserialize<Project>(dataStream);
```

## Defining objects
Nothing special is really required. Just these small easy rules.

* Having parameter-less public constructor.
* Collections/Lists should be generic and implement one of ICollection<>, IList<> or IDictionary<>

## Bois Format
If you are interested to know how Salar.Bois has gain this amazing compact format check out its Bois format wiki page.

[Bois Format Schema](https://github.com/salarcode/Bois/wiki/Bois-Schema-Specs).

## Benchmarks

The benchmarks sourcecode is available. Every elapsed time is calculated for 5000 iteration (v3.0).

Please note that in these tests no configuration required for Bois versus attributes required for Avro, Zero, MsPack and ProtoZBuff. Also Zero required the properties to be virtual.

-Serialization against: ArrayTypes with small data

Serializer | 	Payload Size (bytes)  | Serialization | Deserialization | Format | Note
------------ | ------------ | ------------ | ------------ | ------------ | ------------
Salar.Bois |	**58** |	10 ms |	10 ms |	Binary 	 
Microsoft.Avro |	87 |	8 ms |	10 ms |	Binary 	 
MessagePack |	127 |	4 ms |	5 ms |	Binary 	 
protobuf-net| 	92 |	6 ms |	12 ms |	Binary 	 
BinaryFormatter |	458 |	53 ms| 	48 ms |	Binary 	 
ZeroFormatter |	153 |	4 ms |	2 ms |	Binary 	 


-Serialization against: ArrayTypes with big data

Serializer | 	Payload Size (bytes)  | Serialization | Deserialization | Format | Note
------------ | ------------ | ------------ | ------------ | ------------ | ------------
Salar.Bois |	**1244** |	152 ms |	184 ms |	Binary 	 
Microsoft.Avro |	1534 |	86 ms |	121 ms |	Binary 	 
MessagePack |	1764 |	21 ms |	29 ms |	Binary 	 
protobuf-net| 	1977 |	62 ms |	79 ms |	Binary 	 
BinaryFormatter |	2466 |	49 ms| 	52 ms |	Binary 	 
ZeroFormatter |	2161 |	8 ms |	2 ms |	Binary 	 


-Serialization against: PrimitiveTypes with small data

Serializer | 	Payload Size (bytes)  | Serialization | Deserialization | Format | Note
------------ | ------------ | ------------ | ------------ | ------------ | ------------
Salar.Bois |	**76** |	6 ms |	8 ms |	Binary 	 
Microsoft.Avro |	77 |	4 ms |	4 ms |	Binary 	 
MessagePack |	161 |	8 ms |	6 ms |	Binary 	 
protobuf-net| 	91 |	6 ms |	8 ms |	Binary 	 
BinaryFormatter |	671 |	72 ms| 	83 ms |	Binary 	 
ZeroFormatter |	134 |	9 ms |	1 ms |	Binary 


-Serialization against: ComplexCollections with small data

Serializer | 	Payload Size (bytes)  | Serialization | Deserialization | Format | Note
------------ | ------------ | ------------ | ------------ | ------------ | ------------
Salar.Bois |	**82** |	23 ms |	30 ms |	Binary 	 
Microsoft.Avro |	- |	- |	- |	Binary 	| Error
MessagePack |	171 |	28 ms |	21 ms |	Binary 	 
protobuf-net| 	150 |	19 ms |	35 ms |	Binary 	 
BinaryFormatter |	10852 |	614 ms| 	590 ms |	Binary 	 
NetSerialize |	- |	- |	- |	Binary |    Error


-Serialization against: ComplexCollections with big data

Serializer | 	Payload Size (bytes)  | Serialization | Deserialization | Format | Note
------------ | ------------ | ------------ | ------------ | ------------ | ------------
Salar.Bois |	**10613** |	1s, 527 ms |	1s, 935 ms |	Binary 	 
Microsoft.Avro |	- |	- |	- |	Binary 	 |   Error
MessagePack |	11013 |	1s, 792 ms |  1s, 292 ms |	Binary 	 
protobuf-net| 	14865 |	1s, 142 ms |	2s, 392 ms |	Binary 	 
BinaryFormatter |	34751 |	14s, 580 ms| 	8s 307 ms |	Binary 	 
NetSerialize |	- |	- |	- |	Binary |    Failed

-Serialization against: ComplexContainer Collections

Serializer | 	Payload Size (bytes)  | Serialization | Deserialization | Format | Note
------------ | ------------ | ------------ | ------------ | ------------ | ------------
Salar.Bois |	**123** |	15 ms |	21 ms |	Binary 	 
Microsoft.Avro | - |	- |	- |	Binary | Error
MessagePack |	- |	- |	- |	Binary |    Error
protobuf-net| 171 |	18 ms |	28 ms |	Binary 
BinaryFormatter |	7396 |	384  ms| 	459  ms |	Binary 	 
NetSerialize | - |	- |	- |	Binary |    Error


-Serialization against: SpecializedCollections

Serializer | 	Payload Size (bytes)  | Serialization | Deserialization | Format | Note
------------ | ------------ | ------------ | ------------ | ------------ | ------------
Salar.Bois |	**28** |	10  ms |	22 ms |	Binary 	 
Microsoft.Avro | - | - | - | Binary | Invalid Result
MessagePack | - | - | - | Binary | Error
protobuf-net| - | - | - |	Binary | Error
BinaryFormatter | 2515 | 776 ms | 284 ms |	Binary 	 
NetSerialize | - | - | - |	Binary | Error

