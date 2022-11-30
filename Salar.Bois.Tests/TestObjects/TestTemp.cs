using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Salar.Bois.NetFx.Tests.TestObjects
{
	public class TestTemp
	{
		public ArraySegment<int> ArraySegmentInt { get; set; }

		public Queue<int> QueueInt { get; set; }

		public Stack<int> StackInt { get; set; }

		public ILookup<int,string> Lookup { get; set; }

		public BigInteger BigInteger { get; set; }


	}
}
