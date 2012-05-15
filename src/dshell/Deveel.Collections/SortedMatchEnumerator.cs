using System;
using System.Collections;
using System.Collections.Generic;

namespace Deveel.Collections {
	/// <summary>
	/// An <see cref="IEnumerator">enumerator</see> returning end-truncated 
	/// matching values from a sorted list.
	/// </summary>
	/// <remarks>
	/// This IEnumerator is initialized with a sorted ISet, sorted 
	/// IDictionary or another IEnumerator that must be placed at the 
	/// beginning of the matching area of a sorted set.
	/// <para>
	/// This IEnumerator is commonly used for TAB-completion.
	/// </para>
	/// </remarks>
	public class SortedMatchEnumerator : IEnumerator<string> {
		#region ctor
		public SortedMatchEnumerator(string partialMatch, IEnumerator<String> en) {
			this.partialMatch = partialMatch;
			this.en = en;
		}
		
		public SortedMatchEnumerator(string partialMatch, ICollection<string> c, IComparer<string> comparer)
			: this(partialMatch, SubsetCollection<string>.Tail(c, comparer, partialMatch).GetEnumerator()) {
		}

		public SortedMatchEnumerator(string partialMatch, SortedList<string, string> list)
			: this(partialMatch, SubsetCollection<string>.Tail(list, partialMatch).GetEnumerator()) {
		}

		public SortedMatchEnumerator(string partialMatch, SortedDictionary<string, string> dictionary)
			: this(partialMatch, SubsetDictionary<string, string>.Tail(dictionary, partialMatch).Keys.GetEnumerator()) {
		}
		#endregion

		#region Fields
		private readonly IEnumerator<string> en;
		private readonly string partialMatch;

		private string prefix;
		private string suffix;

		// the current match
		private string current;
		#endregion

		#region Properties
		public string Prefix {
			get { return prefix; }
			set { prefix = value; }
		}

		public string Suffix {
			get { return suffix; }
			set { suffix = value; }
		}

		public string Current {
			get {
				string result = current;
				if (prefix != null)
					result = prefix + result;
				if (suffix != null)
					result = result + suffix;
				return result;
			}
		}
		
		object IEnumerator.Current {
			get { return Current; }
		}
		
		#endregion

		#region Protected Methods
		protected virtual bool Exclude(string current) {
			return false;
		}
		#endregion

		#region Public Methods
		public bool MoveNext() {
			while (en.MoveNext()) {
				current = (string)en.Current;
				if (current.Length == 0)
					continue;
				if (!current.StartsWith(partialMatch))
					return false;
				if (Exclude(current))
					continue;
				return true;
			}
			return false;
		}

		public void Reset() {
			current = null;
			en.Reset();
		}
		
		public void Dispose() {
		}
		
		#endregion
	}
}