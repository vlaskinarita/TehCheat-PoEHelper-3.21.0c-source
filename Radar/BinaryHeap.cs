using System.Collections.Generic;

namespace Radar;

public class BinaryHeap<TKey, TValue>
{
	private readonly List<KeyValuePair<TKey, TValue>> _storage = new List<KeyValuePair<TKey, TValue>>();

	private void SieveUp(int startIndex)
	{
		int index = startIndex;
		int nextIndex = (index - 1) / 2;
		while (index != nextIndex && Compare(index, nextIndex) < 0)
		{
			Swap(index, nextIndex);
			index = nextIndex;
			nextIndex = (index - 1) / 2;
		}
	}

	private void SieveDown(int startIndex)
	{
		int index = startIndex;
		while (index * 2 + 1 < _storage.Count)
		{
			int child1 = index * 2 + 1;
			int child2 = index * 2 + 2;
			int nextIndex = ((child2 >= _storage.Count) ? ((Compare(index, child1) > 0) ? child1 : index) : ((Compare(index, child1) <= 0) ? ((Compare(index, child2) > 0) ? child2 : index) : ((Compare(index, child2) <= 0) ? child1 : ((Compare(child1, child2) > 0) ? child2 : child1))));
			if (nextIndex == index)
			{
				break;
			}
			Swap(index, nextIndex);
			index = nextIndex;
		}
	}

	private int Compare(int i1, int i2)
	{
		return Comparer<TKey>.Default.Compare(_storage[i1].Key, _storage[i2].Key);
	}

	private void Swap(int i1, int i2)
	{
		List<KeyValuePair<TKey, TValue>> storage = _storage;
		List<KeyValuePair<TKey, TValue>> storage2 = _storage;
		KeyValuePair<TKey, TValue> value = _storage[i2];
		KeyValuePair<TKey, TValue> value2 = _storage[i1];
		storage[i1] = value;
		storage2[i2] = value2;
	}

	public void Add(TKey key, TValue value)
	{
		_storage.Add(new KeyValuePair<TKey, TValue>(key, value));
		SieveUp(_storage.Count - 1);
	}

	public bool TryRemoveTop(out KeyValuePair<TKey, TValue> value)
	{
		if (_storage.Count == 0)
		{
			value = default(KeyValuePair<TKey, TValue>);
			return false;
		}
		value = _storage[0];
		List<KeyValuePair<TKey, TValue>> storage = _storage;
		List<KeyValuePair<TKey, TValue>> storage2 = _storage;
		storage[0] = storage2[storage2.Count - 1];
		_storage.RemoveAt(_storage.Count - 1);
		SieveDown(0);
		return true;
	}
}
