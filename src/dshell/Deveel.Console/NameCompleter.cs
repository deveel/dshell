using System;
using System.Collections;
using System.Collections.Generic;

using Deveel.Collections;

namespace Deveel.Console {
	public class NameCompleter {
		#region ctor
		public NameCompleter() {
			nameSet = new SortedList<string, string>();
			canonicalNames = new SortedDictionary<string, string>();
		}

		public NameCompleter(IEnumerator<string> names) :
			this() {
			while (names.MoveNext()) {
				AddName(names.Current);
			}
		}

		public NameCompleter(ICollection<string> c) :
			this(c.GetEnumerator()) {
		}

		public NameCompleter(string[] names) :
			this() {
			for (int i = 0; i < names.Length; ++i) {
				AddName(names[i]);
			}
		}
		#endregion

		#region Fields
		private readonly SortedList<string, string> nameSet;
		private readonly SortedDictionary<string, string> canonicalNames;
		#endregion

		#region Public Methods
		public void AddName(string name) {
			nameSet.Add(name, String.Empty);
			canonicalNames.Add(name.ToLower(), name);
		}

		public IEnumerator<string> GetNamesEnumerator() {
			return nameSet.Keys.GetEnumerator();
		}

		public ICollection<string> GetNames() {
			return nameSet.Keys;
		}

		public IEnumerator<string> GetAlternatives(string partialName) {
			// first test, if we find the name directly
			IEnumerator<string> testIt = SubsetCollection<string>.Tail(nameSet, partialName).GetEnumerator();
			string testMatch = (testIt.MoveNext()) ? (string)testIt.Current : null;
			if (testMatch == null || !testMatch.StartsWith(partialName)) {
				string canonical = partialName.ToLower();
				testIt = SubsetDictionary<string, string>.Tail(canonicalNames, canonical).Keys.GetEnumerator();
				testMatch = (testIt.MoveNext()) ? (string)testIt.Current : null;
				if (testMatch == null || !testMatch.StartsWith(canonical))
					return null; // nope.
				string foundName = (string)canonicalNames[testMatch];
				partialName = foundName.Substring(0, partialName.Length);
			}

			return new NameEnumerator(this, partialName);
		}
		#endregion

		#region NameEnumerator
		class NameEnumerator : IEnumerator<string> {
			#region ctor
			public NameEnumerator(NameCompleter completer, string partialName) {
				enumerator = SubsetCollection<string>.Tail(completer.nameSet, partialName).GetEnumerator();
				namePattern = partialName;
			}
			#endregion

			#region Fields
			private IEnumerator<string> enumerator;
			private string namePattern;
			private string current;
			#endregion

			#region IEnumerator Members

			public string Current {
				get { return current; }
			}
			
			object IEnumerator.Current {
				get { return Current; }
			}

			public bool MoveNext() {
				if (enumerator.MoveNext()) {
					current = (string)enumerator.Current;
					if (current.StartsWith(namePattern))
						return true;
				}
				return false;
			}

			public void Reset() {
			}
			
			public void Dispose() {
			}

			#endregion
		}
		#endregion
	}
}