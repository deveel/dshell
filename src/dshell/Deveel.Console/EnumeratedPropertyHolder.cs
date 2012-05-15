using System;
using System.Collections.Generic;
using System.Text;

namespace Deveel.Console {
	public abstract class EnumeratedPropertyHolder : PropertyHolder {
		#region ctor
		public EnumeratedPropertyHolder(string[] enumeratedValues) {
			values = enumeratedValues;
			completer = new NameCompleter(enumeratedValues);
		}

		public EnumeratedPropertyHolder(ICollection<string> enumeratedValues) {
			values = (string[])(new List<string>(enumeratedValues)).ToArray();
			completer = new NameCompleter(enumeratedValues.GetEnumerator());
		}
		#endregion

		#region Fields
		private string[] values;
		private NameCompleter completer;
		#endregion

		#region Protected Methods
		protected override string OnValueChanged(string newValue) {
			if (newValue == null)
				throw new ArgumentNullException("newValue");

			newValue = newValue.Trim();

			IEnumerator<string> possibleValues = completer.GetAlternatives(newValue);
			if (possibleValues == null || !possibleValues.MoveNext()) {
				StringBuilder expected = new StringBuilder();
				for (int i = 0; i < values.Length; ++i) {
					if (i != 0)
						expected.Append(", ");
					expected.Append(values[i]);
				}
				throw new Exception("'" + newValue + "' does not match any of [" + expected.ToString() + "]");
			}

			string value = (string)possibleValues.Current;

			//CHECK: check this...
			if (possibleValues.MoveNext()) {
				StringBuilder matching = new StringBuilder(value);
				do {
					matching.Append(", ");
					matching.Append((string)possibleValues.Current);
				} while (possibleValues.MoveNext());

				throw new Exception("'" + newValue + "' ambiguous. Matches [" + matching.ToString() + "]");
			}

			int index = -1;
			// small size of array -- linear search acceptable
			for (int i = 0; i < values.Length; ++i) {
				if (value.Equals(values[i])) {
					index = i;
					break;
				}
			}

			OnEnumeratedPropertyChanged(index, value);
			return value;
		}

		protected abstract void OnEnumeratedPropertyChanged(int index, string value);
		#endregion

		#region Public Methods
		public override IEnumerator<string> CompleteValue(string partialValue) {
			return completer.GetAlternatives(partialValue);
		}
		#endregion
	}
}