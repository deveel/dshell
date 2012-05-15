using System;
using System.Collections.Generic;

namespace Deveel.Collections {
	public sealed class SubsetDictionary<TKey, TValue> : IDictionary<TKey, TValue> {
		private SortedDictionary<TKey, TValue> subset;
		
		private static TKey NullKey = default(TKey);
		
		public SubsetDictionary(SortedDictionary<TKey, TValue> dictionary, TKey startKey, TKey endKey) {
			subset = new SortedDictionary<TKey, TValue>(dictionary.Comparer);
			foreach(KeyValuePair<TKey, TValue> pair in dictionary) {
				int c1 = 0;
				if (!ReferenceEquals(startKey, NullKey))
					c1 = dictionary.Comparer.Compare(pair.Key, startKey);
				int c2 = 0;
				if (!ReferenceEquals(endKey, NullKey))
					c2 = dictionary.Comparer.Compare(pair.Key, endKey);
				if (c1 >= 0 && c2 <= 0)
					subset.Add(pair.Key, pair.Value);
			}
		}
		
		public TValue this[TKey key] {
			get { return subset[key]; }
		}
		
		TValue IDictionary<TKey, TValue>.this[TKey key] {
			get { return this[key]; }
			set { throw new NotSupportedException(); }
		}
		
		public ICollection<TKey> Keys {
			get { return subset.Keys; }
		}
		
		public ICollection<TValue> Values {
			get { return subset.Values; }
		}
		
		public int Count {
			get { return subset.Count; }
		}
		
		public bool IsReadOnly {
			get { return true;}
		}
		
		public bool ContainsKey(TKey key) {
			return subset.ContainsKey(key);
		}
		
		void IDictionary<TKey, TValue>.Add(TKey key, TValue value) {
			throw new NotSupportedException();
		}
		
		bool IDictionary<TKey, TValue>.Remove(TKey key) {
			throw new NotSupportedException();
		}
		
		public bool TryGetValue(TKey key, out TValue value)
		{
			return subset.TryGetValue(key, out value);
		}
		
		void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) {
			throw new NotSupportedException();
		}
		
		void ICollection<KeyValuePair<TKey, TValue>>.Clear() {
			throw new NotSupportedException();
		}
		
		public bool Contains(KeyValuePair<TKey, TValue> item) {
			return ((ICollection<KeyValuePair<TKey, TValue>>) subset).Contains(item);
		}
		
		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
			subset.CopyTo(array, arrayIndex);
		}
		
		bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) {
			throw new NotSupportedException();
		}
		
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return subset.GetEnumerator();
		}
		
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
		
		public static SubsetDictionary<TKey, TValue> Tail(SortedDictionary<TKey, TValue> dictionary, TKey startKey) {
			return new SubsetDictionary<TKey, TValue>(dictionary, startKey, NullKey);
		}
		
		public static SubsetDictionary<TKey, TValue> Head(SortedDictionary<TKey, TValue> dictionary, TKey endKey) {
			return new SubsetDictionary<TKey, TValue>(dictionary, NullKey, endKey);
		}
	}
}