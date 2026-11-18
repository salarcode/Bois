using Salar.Bois.BenchmarksObjects;
using System.Collections.Generic;

namespace Salar.Bois.BenchBois;

/// <summary>
/// Deep nesting of big objects: a chain of 20 nodes, each bigger than 63 bytes. Versioned, every level needs the
/// long size prefix and its bytes move once when it ends, so this is the worst case for the size patch.
/// <see cref="Test3_ChainVersioned"/> is the same data, versioned.
/// </summary>
public class Test3_Chain : IBenchmarkTestObject
{
    public string Name { get; set; }
    public Test3_Node Head { get; set; }

    public Test3_Chain()
    {
        Name = "chain";
        for (var i = 20; i > 0; i--)
            Head = new Test3_Node { Text = new string((char)('a' + i), 100), Values = new List<int> { i, i * 10, i * 100 }, Next = Head };
    }
}

public class Test3_Node
{
    public string Text { get; set; }
    public List<int> Values { get; set; }
    public Test3_Node Next { get; set; }
}

[BoisContract(Versioned = true)]
public class Test3_ChainVersioned : IBenchmarkTestObject
{
    [BoisMember(1)] 
    public string Name { get; set; }
    [BoisMember(2)] 
    public Test3_NodeVersioned Head { get; set; }

    public Test3_ChainVersioned()
    {
        Name = "chain";
        for (var i = 20; i > 0; i--)
            Head = new Test3_NodeVersioned { Text = new string((char)('a' + i), 100), Values = new List<int> { i, i * 10, i * 100 }, Next = Head };
    }
}

[BoisContract(Versioned = true)]
public class Test3_NodeVersioned
{
    [BoisMember(1)]
    public string Text { get; set; }
    [BoisMember(2)]
    public List<int> Values { get; set; }
    [BoisMember(3)]
    public Test3_NodeVersioned Next { get; set; }
}
