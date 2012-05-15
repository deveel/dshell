using System;
using System.Collections.Generic;

namespace Deveel.Console {
	public abstract class PropertyHolder {
		#region ctor
		protected PropertyHolder()
			: this(null) {
		}

		protected PropertyHolder(string value) {
			this.value = value;
		}
		#endregion

		#region Fields
		private string value;
		#endregion

		#region Properties
		public string Value {
			get { return value; }
			set { this.value = value; }
		}

		public abstract string DefaultValue { get; }

		public virtual string ShortDescription {
			get { return null; }
		}

		public virtual string LongDescription {
			get { return null; }
		}
		#endregion

		#region Protected Methods
		protected abstract string OnValueChanged(string newValue);
		#endregion

		#region Public Methods
		public virtual IEnumerator<string> CompleteValue(String partialValue) {
			return null;
		}
		#endregion
	}
}