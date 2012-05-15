using System;
using System.Collections;
using System.Collections.Generic;

namespace Deveel.Collections {
	public sealed class SubsetCollection<T> : ICollection<T> {
		private readonly SortedList<T, T> subset;
		
		
		public int Count {
			get { return subset.Count; }
		}
		
		public bool IsReadOnly {
			get { return true; }
		}
		
		void ICollection<T>.Add(T item) {
			throw new NotSupportedException();
		}
		
		void ICollection<T>.Clear() {
			throw new NotSupportedException();
		}
		
		public bool Contains(T item) {
			return subset.ContainsKey(item);
		}
		
		public void CopyTo(T[] array, int arrayIndex) {
			subset.Keys.CopyTo(array, arrayIndex);
		}
		
		bool ICollection<T>.Remove(T item) {
			throw new NotSupportedException();
		}
		
		public IEnumerator<T> GetEnumerator() {
			return subset.Keys.GetEnumerator();
		}
		
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
		public static readonly T NullKey = default(T);
		
		public SubsetCollection(SortedList<T, T> list, T startKey, T endKey) {
			subset = new SortedList<T, T>();
			foreach(KeyValuePair<T, T> pair in list) {
				int c1 = 0;
				if (!ReferenceEquals(startKey, NullKey))
					c1 = list.Comparer.Compare(pair.Key, startKey);
				int c2 = 0;
				if (!ReferenceEquals(endKey, NullKey))
					c2 = list.Comparer.Compare(pair.Key, endKey);
				if (c1 >= 0 && c2 <= 0)
					subset.Add(pair.Key, pair.Key);
			}

		}
		
		public SubsetCollection(ICollection<T> list, IComparer<T> comparer, T startKey, T endKey) {
			subset = new SortedList<T, T>();
			foreach(T value in list) {
				int c1 = 0;
				if (!ReferenceEquals(startKey, NullKey))
					c1 = comparer.Compare(value, startKey);
				int c2 = 0;
				if (!ReferenceEquals(endKey, NullKey))
					c2 = comparer.Compare(value, endKey);
				if (c1 >= 0 && c2 <= 0)
					subset.Add(value, value);
			}

		}
		
		public static SubsetCollection<T> Tail(SortedList<T, T> list, T startKey) {
			return new SubsetCollection<T>(list, startKey, NullKey);
		}
		
		public static SubsetCollection<T> Tail(ICollection<T> c, IComparer<T> comparer, T startKey) {
			return new SubsetCollection<T>(c, comparer, startKey, NullKey);
		}
	}
}