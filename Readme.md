Salar.Bois is the most compact, extermly fast binary serializer for .Net Standard, .NET Code and .NET Framework.

* No compression is done, the high compact ratio is the result of Bois format.
* No configuration.
* No change to your classes.
* Compression is provided as a seperate package.

## Why Salar.Bois?
* Because payload size matters. Bois serializer generates the smallest payload size.
* Because speed matters. Both serialization and deserialization are extremely fast.
* Easy to use, `Serialize<T>` and `Deserialize<T>` are all you need.
* No configuration required. No separate schema required. 

## [NuGet Package](https://www.nuget.org/packages/Salar.Bois)
```
PM> Install-Package Salar.Bois
```

[LZ4 Compression wrapper package](https://www.nuget.org/packages/Salar.Bois.LZ4)
```
PM> Install-Package Salar.Bois.LZ4
```

> Minimum frameworks supported are .Net Core 2.0, .Net Standard 2.1 and .Net Framework 4.5

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
	public string Description { get; set; }
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
			Version = new Version(3, 0, 0, 0),
			Description = "Salar.Bois is a compact, fast and powerful binary serializer for .NET Framework."
		};

		var boisSerializer = new BoisSerializer();

		using (var file = new FileStream("output.data", FileMode.Create))
		{
			boisSerializer.Serialize(projectInstance, file);
		}

		// All done.

		// ...
		// if you want to have more compression using LZ4 wrapper

		var boisLz4Serializer = new BoisLz4Serializer();

		using (var file = new FileStream("output-compressed.data", FileMode.Create))
		{
			boisLz4Serializer.Pickle(projectInstance, file);
		}

	}
}
```
### How to deserialize an object:
```csharp
var boisSerializer = new BoisSerializer();
return boisSerializer.Deserialize<Project>(dataStream);

// and the compressed data
var boisLz4Serializer = new BoisLz4Serializer();
return boisLz4Serializer.Unpickle<Project>(dataStream);
```

## Defining objects
Nothing special is really required. Just these small easy rules.

* Having parameter-less public constructor.
* Collections/Lists should be generic and implement one of ICollection<>, IList<> or IDictionary<>

## Compression
The `BoisLz4Serializer` class is in a separate package called `Salar.Bois.LZ4`. It is provided for anyone looking for more compact serialization. To use it just create a new instance of `BoisLz4Serializer` and to serialize and compress the objects call `Pickle` and to deserialize and uncompress call `Unpickle`.
Please note that `BoisLz4Serializer` -for now- is implemented as a wrapper around [LZ4](https://github.com/MiloszKrajewski/K4os.Compression.LZ4).


## Bois Format
If you are interested to know how Salar.Bois has gain this amazing compact format check out its Bois format wiki page.

[Bois Format Schema](https://github.com/salarcode/Bois/wiki/Bois-Schema-Specs).

## Benchmarks

The benchmarks sourcecode is available. Every elapsed time is calculated for 5000 iteration (v3.0).

Please note that in these tests no configuration required for Bois versus attributes required for Avro, Zero, MsPack and ProtoZBuff. Also Zero required the properties to be virtual.

-Serialization against: ArrayTypes with small data

Serializer | 	Payload Size (bytes)  | Serialization | Deserialization | Format | Note
------------ | ------------ | ------------ | ------------ | ------------ | ------------
Salar.Bois	|	**58**	|	6 ms	|	5 ms	|	Binary 
Salar.Bois.LZ4	|	**59**	|	25 ms	|	6 ms	|	Binary 
Microsoft.Avro	|	87	|	9 ms	|	10 ms	|	Binary 
MessagePack	|	127	|	4 ms	|	4 ms	|	Binary 
MessagePackLZ4	|	125	|	18 ms	|	5 ms	|	Binary 
protobuf-net	|	92	|	7 ms	|	12 ms	|	Binary 
BinaryFormatter	|	458	|	59 ms	|	48 ms	|	Binary 
ZeroFormatter	|	153	|	3 ms	|	1 ms	|	Binary 


-Serialization against: ArrayTypes with big data

Serializer | 	Payload Size (bytes)  | Serialization | Deserialization | Format | Note
------------ | ------------ | ------------ | ------------ | ------------ | ------------
Salar.Bois	|	**1400**	|	70 ms	|	73 ms	|	Binary 
Salar.Bois.LZ4	|	**1342**	|	117 ms	|	75 ms	|	Binary 
Microsoft.Avro	|	1709	|	89 ms	|	119 ms	|	Binary 
MessagePack	|	1951	|	19 ms	|	28 ms	|	Binary 
MessagePackLZ4	|	1593	|	57 ms	|	41 ms	|	Binary 
protobuf-net	|	2161	|	58 ms	|	79 ms	|	Binary 
BinaryFormatter	|	2641	|	52 ms	|	50 ms	|	Binary 
ZeroFormatter	|	2336	|	11 ms	|	1 ms	|	Binary 


-Serialization against: PrimitiveTypes with small data

Serializer | 	Payload Size (bytes)  | Serialization | Deserialization | Format | Note
------------ | ------------ | ------------ | ------------ | ------------ | ------------
Salar.Bois	|	**74**	|	3 ms	|	4 ms	|	Binary 
Salar.Bois.LZ4	|	**75**	|	19 ms	|	5 ms	|	Binary 
Microsoft.Avro	|	77	|	4 ms	|	3 ms	|	Binary 
MessagePack	|	161	|	7 ms	|	3 ms	|	Binary 
MessagePackLZ4	|	173	|	22 ms	|	3 ms	|	Binary 
protobuf-net	|	91	|	6 ms	|	7 ms	|	Binary 
BinaryFormatter	|	671	|	72 ms	|	79 ms	|	Binary 
ZeroFormatter	|	134	|	7 ms	|	0 ms	|	Binary 


-Serialization against: ComplexCollections with small data

Serializer | 	Payload Size (bytes)  | Serialization | Deserialization | Format | Note
------------ | ------------ | ------------ | ------------ | ------------ | ------------
Salar.Bois	|	**82**	|	14 ms	|	23 ms	|	Binary 
Salar.Bois.LZ4	|	**81**	|	30 ms	|	24 ms	|	Binary 
Microsoft.Avro |	- |	- |	- |	Binary 	| Error
MessagePack	|	171	|	27 ms	|	19 ms	|	Binary 
MessagePackLZ4	|	153	|	42 ms	|	21 ms	|	Binary 
protobuf-net	|	150	|	17 ms	|	34 ms	|	Binary 
BinaryFormatter	|	10852	|	636 ms	|	612 ms	|	Binary 
ZeroFormatter |	- |	- |	- |	Binary |    Not supported


-Serialization against: ComplexCollections with big data

Serializer | 	Payload Size (bytes)  | Serialization | Deserialization | Format | Note
------------ | ------------ | ------------ | ------------ | ------------ | ------------
Salar.Bois	|	**10414**	|	356 ms	|	411 ms	|	Binary 
Salar.Bois.LZ4	|	**7628**	|	837 ms	|	483 ms	|	Binary 
Microsoft.Avro |	- |	- |	- |	Binary 	 |   Error
MessagePack	|	11013	|	796 ms	|	301 ms	|	Binary 
MessagePackLZ4	|	7636	|	28 ms	|	390 ms	|	Binary 
protobuf-net	|	14865	|	7 ms	|	265 ms	|	Binary 
BinaryFormatter	|	34751	|	961 ms	|	488 ms	|	Binary 
ZeroFormatter |	- |	- |	- |	Binary |    Not supported

-Serialization against: ComplexContainer Collections

Serializer | 	Payload Size (bytes)  | Serialization | Deserialization | Format | Note
------------ | ------------ | ------------ | ------------ | ------------ | ------------
Salar.Bois	|	**120**	|	9 ms	|	11 ms	|	Binary 
Salar.Bois.LZ4	|	**114**	|	27 ms	|	12 ms	|	Binary 
Microsoft.Avro | - |	- |	- |	Binary | Not supported
MessagePack |	- |	- |	- |	Binary |    Not supported
protobuf-net	|	171	|	14 ms	|	24 ms	|	Binary 
BinaryFormatter	|	7396	|	379 ms	|	475 ms	|	Binary 
ZeroFormatter | - |	- |	- |	Binary |    Not supported


-Serialization against: SpecializedCollections

Serializer | 	Payload Size (bytes)  | Serialization | Deserialization | Format | Note
------------ | ------------ | ------------ | ------------ | ------------ | ------------
Salar.Bois	|	**28**	|	9 ms	|	17 ms	|	Binary 
Salar.Bois.LZ4	|	**29**	|	22 ms	|	17 ms	|	Binary 
Microsoft.Avro | - | - | - | Binary | Invalid Result
MessagePack | - | - | - | Binary | Not supported
MessagePackLZ4 | - | - | - | Binary | Not supported
protobuf-net| - | - | - |	Binary | Not supported
BinaryFormatter	|	2515	|	763 ms	|	285 ms	|	Binary 
ZeroFormatter | - | - | - |	Binary | Not supported
