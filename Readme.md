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

The benchmarks sourcecode is available. Every elapsed time is calculated for 5000 iteration (v2.2).

-Serialization against: PrimitiveTypes with small data

Serializer | 	Payload Size (bytes)  | Serialization | Deserialization | Format | Note
------------ | ------------ | ------------ | ------------ | ------------ | ------------
Salar.Bois |	**73** |	4 ms |	6 ms |	Binary 	 
Microsoft.Avro |	77 |	2 ms |	3 ms |	Binary 	 
MessagePack |	90 |	8 ms |	14 ms |	Binary 	 
protobuf-net| 	91 |	4 ms |	12 ms |	Binary 	 
BinaryFormatter |	651 |	85 ms| 	106 ms |	Binary 	 
NetSerialize |	85 |	4 ms |	6 ms |	Binary 	 


-Serialization against: PrimitiveTypes with big data for 100 iteration

Serializer | 	Payload Size (bytes)  | Serialization | Deserialization | Format | Note
------------ | ------------ | ------------ | ------------ | ------------ | ------------
Salar.Bois |	**91** |	4 ms |	7 ms |	Binary 	 
Microsoft.Avro |	88 |	2 ms |	3 ms |	Binary 	 
MessagePack |	98 |	8 ms |	14 ms |	Binary 	 
protobuf-net| 	101 |	4 ms |	12 ms |	Binary 	 
BinaryFormatter |	651 |	85 ms| 	106 ms |	Binary 	 
NetSerialize |	95 |	4 ms |	6 ms |	Binary 	 


-Serialization against: ArrayTypes with small data

Serializer | 	Payload Size (bytes)  | Serialization | Deserialization | Format | Note
------------ | ------------ | ------------ | ------------ | ------------ | ------------
Salar.Bois |	**58** |	14 ms |	16 ms |	Binary 	 
Microsoft.Avro |	81 |	4 ms |	6 ms |	Binary 	 
MessagePack |	85 |	4 ms |	12 ms |	Binary 	 
protobuf-net| 	87 |	4 ms |	15 ms |	Binary 	 
BinaryFormatter |	414 |	57 ms| 	54 ms |	Binary 	 
NetSerialize |	89 |	4 ms |	6 ms |	Binary 


-Serialization against: ArrayTypes with big data for 100 iteration

Serializer | 	Payload Size (bytes)  | Serialization | Deserialization | Format | Note
------------ | ------------ | ------------ | ------------ | ------------ | ------------
Salar.Bois |	**129948** |	464 ms |	476 ms |	Binary 	 
Microsoft.Avro |	131135 |	126  ms |	146 ms |	Binary 	 
MessagePack |	163902 |	109 ms |	202 ms |	Binary 	 
protobuf-net| 	163902 |	72 ms |	72 ms |	Binary 	 
BinaryFormatter |	131466 |	12 ms| 	6 ms |	Binary 	 
NetSerialize |	163902 |	107 ms |	80 ms |	Binary 


-Serialization against: SimpleCollections with small data

Serializer | 	Payload Size (bytes)  | Serialization | Deserialization | Format | Note
------------ | ------------ | ------------ | ------------ | ------------ | ------------
Salar.Bois |	89 |	17 ms |	16 ms |	Binary 	 
Microsoft.Avro |	89 |	4 ms |	6 ms |	Binary 	 
MessagePack |	83 |	4 ms |	12 ms |	Binary 	 
protobuf-net| 	122 |	4 ms |	15 ms |	Binary 	 
BinaryFormatter |	6703 |	57 ms| 	54 ms |	Binary 	 
NetSerialize |	- |	- |	- |	Binary |    Failed


-Serialization against: SimpleCollections with big data for 100 iteration

Serializer | 	Payload Size (bytes)  | Serialization | Deserialization | Format | Note
------------ | ------------ | ------------ | ------------ | ------------ | ------------
Salar.Bois |	**41543** |	353 ms |	305 ms |	Binary 	 
Microsoft.Avro |	**41543** |	80 ms |	97 ms |	Binary 	 
MessagePack |	42380 |	51 ms |	200 ms |	Binary 	 
protobuf-net| 	77585 |	86 ms |	133 ms |	Binary 	 
BinaryFormatter |	159756 |	196 ms| 	205 ms |	Binary 	 
NetSerialize |	- |	- |	- |	Binary |    Failed


-Serialization against: ComplexCollections

Serializer | 	Payload Size (bytes)  | Serialization | Deserialization | Format | Note
------------ | ------------ | ------------ | ------------ | ------------ | ------------
Salar.Bois |	**93** |	46 ms |	94 ms |	Binary 	 
Microsoft.Avro | - |	- |	- |	Binary |    Failed
MessagePack |	- |	- |	- |	Binary |    Failed
protobuf-net| 	150 |	16 ms |	38 ms |	Binary 	 
BinaryFormatter |	10807 |	885 ms| 783 ms |	Binary 	 
NetSerialize | - |	- |	- |	Binary |    Failed


-Serialization against: SpecializedCollections

Serializer | 	Payload Size (bytes)  | Serialization | Deserialization | Format | Note
------------ | ------------ | ------------ | ------------ | ------------ | ------------
Salar.Bois |	**29** |	13 ms |	36 ms |	Binary 	 
Microsoft.Avro | - |	- |	- |	Binary | Failed with invalid result
MessagePack |	- |	- |	- |	Binary |    Failed
protobuf-net| - |	- |	- |	Binary |    Failed
BinaryFormatter |	2505 |	768  ms| 	379  ms |	Binary 	 
NetSerialize | - |	- |	- |	Binary |    Failed


-Serialization against: ComplexContainer object

Serializer | 	Payload Size (bytes)  | Serialization | Deserialization | Format | Note
------------ | ------------ | ------------ | ------------ | ------------ | ------------
Salar.Bois |	**1292** |	386  ms |	944 ms |	Binary 	 
Microsoft.Avro | - | - | - | Binary | Failed
MessagePack | - | - | - | Binary | Failed
protobuf-net| 1550 | 204 ms | 341 ms |	Binary
BinaryFormatter | 18910 | 3s,297 ms | 3s,595 ms |	Binary 	 
NetSerialize | 1482 | 143 ms | 189 ms |	Binary

