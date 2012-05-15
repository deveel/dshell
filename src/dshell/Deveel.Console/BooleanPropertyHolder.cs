using System;

namespace Deveel.Console {
	public abstract class BooleanPropertyHolder : EnumeratedPropertyHolder {
		#region ctor

		protected BooleanPropertyHolder() :
			base(BoolValues) {
		}

		protected BooleanPropertyHolder(bool initialValue) :
			this() {
			Value = initialValue ? "true" : "false";
		}

		#endregion

		#region Fields
		private readonly static string[] BoolValues = { "0", "off", "false", "1", "on", "true" };
		#endregion

		#region Protected Methods
		protected override void OnEnumeratedPropertyChanged(int index, string value) {
			OnBooleanPropertyChanged(index >= (BoolValues.Length / 2));
		}
		#endregion

		#region Public Methods
		public abstract void OnBooleanPropertyChanged(bool value);
		#endregion
	}
}