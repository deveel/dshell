using System;

namespace Deveel.Console {
	public delegate void StringWriteEventHandler(object sender, StringWriteEventArgs args);

	public sealed class StringWriteEventArgs : EventArgs {
		public readonly string s;

		public StringWriteEventArgs(string s) {
			this.s = s;
		}

		public string Value {
			get { return s; }
		}
	}
}