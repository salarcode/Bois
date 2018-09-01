using System.Collections;
using System.Collections.Generic;

namespace Salar.Bois.NetFx.Tests.TestObjects
{
	public class TestObjectListImplementedInt : TestObjectListImplemented<int>
	{

	}
	public class TestObjectListImplementedStr : TestObjectListImplemented<string>
	{

	}

	public abstract class TestObjectListImplemented<T> : IList<T>
	{
		private readonly List<T> _internalList = new List<T>();

		public IEnumerator<T> GetEnumerator()
		{
			return _internalList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _internalList.GetEnumerator();
		}

		public void Add(T item)
		{
			_internalList.Add(item);
		}

		public void Clear()
		{
			_internalList.Clear();
		}

		public bool Contains(T item)
		{
			return _internalList.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			_internalList.CopyTo(array, arrayIndex);
		}

		public bool Remove(T item)
		{
			return _internalList.Remove(item);
		}

		public int Count => _internalList.Count;

		public bool IsReadOnly => false;

		public int IndexOf(T item)
		{
			return _internalList.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			_internalList.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			_internalList.RemoveAt(index);
		}

		public T this[int index]
		{
			get => _internalList[index];
			set => _internalList[index] = value;
		}
	}
}
