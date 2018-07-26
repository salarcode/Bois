using System;
using System.Threading;

namespace Salar.Bois.Types
{
	internal class BoisComputedTypeHashtable<T>
	{
		Entry[] _buckets;
		int _size; // only use in writer lock

		readonly object _writerLock = new object();
		readonly float _loadFactor;


		internal BoisComputedTypeHashtable(int capacity = 4, float loadFactor = 0.75f)
		{
			var tableSize = CalculateCapacity(capacity, loadFactor);
			this._buckets = new Entry[tableSize];
			this._loadFactor = loadFactor;
		}

		public int KeyGetHashCode(Type key)
		{
			if (key == null) return 0;
			return key.GetHashCode();
		}

		public bool KeyEquals(Type key1, Type key2)
		{
			return key1 == key2;
		}

		public bool TryAdd(Type key, T value)
		{
			return TryAdd(key, ignore => value);
		}

		public bool TryAdd(Type key, Func<Type, T> valueFactory)
		{
			T _;
			return TryAddInternal(key, valueFactory, out _);
		}

		bool TryAddInternal(Type key, Func<Type, T> valueFactory, out T resultingValue)
		{
			lock (_writerLock)
			{
				var nextCapacity = CalculateCapacity(_size + 1, _loadFactor);

				if (_buckets.Length < nextCapacity)
				{
					// rehash
					var nextBucket = new Entry[nextCapacity];
					for (int i = 0; i < _buckets.Length; i++)
					{
						var e = _buckets[i];
						while (e != null)
						{
							var newEntry = new Entry { Key = e.Key, Value = e.Value, Hash = e.Hash };
							AddToBuckets(nextBucket, key, newEntry, null, out resultingValue);
							e = e.Next;
						}
					}

					// add entry(if failed to add, only do resize)
					var successAdd = AddToBuckets(nextBucket, key, null, valueFactory, out resultingValue);

					// replace field(threadsafe for read)
					Volatile.Write(ref _buckets, nextBucket);

					if (successAdd) _size++;
					return successAdd;
				}
				else
				{
					// add entry(insert last is thread safe for read)
					var successAdd = AddToBuckets(_buckets, key, null, valueFactory, out resultingValue);
					if (successAdd) _size++;
					return successAdd;
				}
			}
		}

		bool AddToBuckets(Entry[] buckets, Type newKey, Entry newEntryOrNull, Func<Type, T> valueFactory, out T resultingValue)
		{
			var h = (newEntryOrNull != null) ? newEntryOrNull.Hash : KeyGetHashCode(newKey);
			if (buckets[h & (buckets.Length - 1)] == null)
			{
				if (newEntryOrNull != null)
				{
					resultingValue = newEntryOrNull.Value;
					Volatile.Write(ref buckets[h & (buckets.Length - 1)], newEntryOrNull);
				}
				else
				{
					resultingValue = valueFactory(newKey);
					Volatile.Write(ref buckets[h & (buckets.Length - 1)], new Entry { Key = newKey, Value = resultingValue, Hash = h });
				}
			}
			else
			{
				var searchLastEntry = buckets[h & (buckets.Length - 1)];
				while (true)
				{
					if (KeyEquals(searchLastEntry.Key, newKey))
					{
						resultingValue = searchLastEntry.Value;
						return false;
					}

					if (searchLastEntry.Next == null)
					{
						if (newEntryOrNull != null)
						{
							resultingValue = newEntryOrNull.Value;
							Volatile.Write(ref searchLastEntry.Next, newEntryOrNull);
						}
						else
						{
							resultingValue = valueFactory(newKey);
							Volatile.Write(ref searchLastEntry.Next, new Entry { Key = newKey, Value = resultingValue, Hash = h });
						}
						break;
					}
					searchLastEntry = searchLastEntry.Next;
				}
			}

			return true;
		}

		public bool TryGetValue(Type key, out T value)
		{
			var table = _buckets;
			var hash = KeyGetHashCode(key);
			var entry = table[hash & table.Length - 1];

			if (entry == null) goto NOT_FOUND;

			if (KeyEquals(entry.Key, key))
			{
				value = entry.Value;
				return true;
			}

			var next = entry.Next;
			while (next != null)
			{
				if (KeyEquals(next.Key, key))
				{
					value = next.Value;
					return true;
				}
				next = next.Next;
			}

			NOT_FOUND:
			value = default(T);
			return false;
		}

		public T GetOrAdd(Type key, Func<Type, T> valueFactory)
		{
			T v;
			if (TryGetValue(key, out v))
			{
				return v;
			}

			TryAddInternal(key, valueFactory, out v);
			return v;
		}

		private static int CalculateCapacity(int collectionSize, float loadFactor)
		{
			var size = (int)(((float)collectionSize) / loadFactor);

			size--;
			size |= size >> 1;
			size |= size >> 2;
			size |= size >> 4;
			size |= size >> 8;
			size |= size >> 16;
			size += 1;

			if (size < 8)
			{
				size = 8;
			}
			return size;
		}

		private class Entry
		{
			public Type Key;
			public T Value;
			public int Hash;
			public Entry Next;

			// debug only
			public override string ToString()
			{
				return Key + "(" + Count() + ")";
			}

			int Count()
			{
				var count = 1;
				var n = this;
				while (n.Next != null)
				{
					count++;
					n = n.Next;
				}
				return count;
			}
		}
	}
}