using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Salar.Bois.NetFx.Tests.TestObjects
{
	public class TestObjectCollectionsPrimitive : IEnumerable<object[]>
	{
		public List<int> ListInt { get; set; }

		public List<int?> ListIntField;

		public Collection<int?> CollInt { get; set; }

		public Collection<int> CollIntField;

		public HashSet<int> HashSet { get; set; }

		public HashSet<int?> HashSetField;
		
		public BlockingCollection<int> ConcurrentBlockingCollection { get; set; }
		public BlockingCollection<int?> ConcurrentBlockingCollectionField;
		
		public ConcurrentBag<int> ConcurrentBag { get; set; }
		public ConcurrentBag<int?> ConcurrentBagField;

		public IEnumerator<object[]> GetEnumerator()
		{
			yield return new object[]
			{
				new TestObjectCollectionsPrimitive()
				{
					ListInt = new List<int>() {0, 30, 100, short.MaxValue},
					ListIntField = new List<int?>() {0, 30, 100, short.MaxValue},

					CollInt = new Collection<int?>() {0, 30, 100, short.MaxValue},
					CollIntField = new Collection<int>() {0, 30, 100, short.MaxValue},

					HashSet = new HashSet<int>(){0, 30, 100, short.MaxValue},
					HashSetField = new HashSet<int?>(){0, 30, 100, short.MaxValue},

					ConcurrentBlockingCollection = new BlockingCollection<int>(){0, 30, 100, short.MaxValue},
					ConcurrentBlockingCollectionField = new BlockingCollection<int?>(){0, 30, 100, short.MaxValue},

					ConcurrentBag = new ConcurrentBag<int>(){0, 30, 100, short.MaxValue},
					ConcurrentBagField = new ConcurrentBag<int?>(){0, 30, 100, short.MaxValue}
				}
			};
			yield return new object[]
			{
				new TestObjectCollectionsPrimitive()
				{
					ListInt = new List<int>() {0, 30, 100, short.MaxValue},
					ListIntField = new List<int?>() {0, null, 30, 100, short.MaxValue},

					CollInt = new Collection<int?>() {0, null, 30, 100, short.MaxValue},
					CollIntField = new Collection<int>() {0, 30, 100, short.MaxValue},

					HashSet = new HashSet<int>(){0, 30, 100, short.MaxValue},
					HashSetField = new HashSet<int?>(){0, 30, 100, short.MaxValue},

					ConcurrentBlockingCollection = new BlockingCollection<int>(){0, 30, 100, short.MaxValue},
					ConcurrentBlockingCollectionField = new BlockingCollection<int?>(){0, 30, null, 100, short.MaxValue},

					ConcurrentBag = new ConcurrentBag<int>(){0, 30, 100, short.MaxValue},
					ConcurrentBagField = new ConcurrentBag<int?>(){0, 30, null, 100, short.MaxValue}
				}
			};
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
