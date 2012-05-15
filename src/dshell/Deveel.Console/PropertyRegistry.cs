using System;
using System.Collections;
using System.Collections.Generic;

using Deveel.Collections;

namespace Deveel.Console {
	public class PropertyRegistry : IEnumerable<KeyValuePair<string, PropertyHolder>> {
		#region ctor
		public PropertyRegistry(IPropertyHandler context) {
			this.context = context;
			namedProperties = new SortedDictionary<string, PropertyHolder>();
		}
		#endregion

		#region Fields
		private readonly IPropertyHandler context;
		private readonly SortedDictionary<string, PropertyHolder> namedProperties;
		#endregion

		#region Properties

		protected SortedDictionary<string, PropertyHolder> Properties {
			get { return namedProperties; }
		}

		public IPropertyHandler Context {
			get { return context; }
		}

		#endregion

		internal IEnumerator<string> Complete(string sourceCommand, string partialCommand, string lastWord) {
			partialCommand = partialCommand.Trim();
			if (!String.IsNullOrEmpty(partialCommand)) {
				string[] st = partialCommand.Split(' ');
				String cmd = st[0];
				int argc = st.Length;

				if (argc > 1) {
					// one arg given
					if (sourceCommand.Equals(cmd)) {
						String name = st[1];
						PropertyHolder holder = GetProperty(name);
						if (holder == null) {
							return null;
						}
						return holder.CompleteValue(lastWord);
					}
				}
			}

			return new SortedMatchEnumerator(lastWord, Properties.Keys, Properties.Comparer);
		}

		#region Public Methods

		public virtual bool HasProperty(string name) {
			return namedProperties.ContainsKey(name);
		}

		public virtual void RegisterProperty(string name, PropertyHolder holder) {
			if (namedProperties.ContainsKey(name))
				throw new ArgumentException("Property named '" + name + "' already exists");

			namedProperties.Add(name, holder);
		}

		public virtual void UnregisterProperty(string name) {
			namedProperties.Remove(name);
		}

		public virtual void SetProperty(string name, string value) {
			PropertyHolder holder = (PropertyHolder)namedProperties[name];
			if (holder == null)
				throw new ArgumentException("unknown property '" + name + "'");

			holder.Value = value;
		}

		public PropertyHolder GetProperty(string name) {
			return (PropertyHolder) namedProperties[name];
		}

		public IEnumerator<KeyValuePair<string, PropertyHolder>> GetEnumerator() {
			return namedProperties.GetEnumerator();
		}
		
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		#endregion
	}
}