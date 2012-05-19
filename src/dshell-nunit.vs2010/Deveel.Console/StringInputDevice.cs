using System;
using System.IO;

namespace Deveel.Console {
	public sealed class StringInputDevice : InputDevice {
		private StringReader baseReader;

		public StringInputDevice() {
		}

		public StringInputDevice(string s) {
			baseReader = new StringReader(s);
		}

		public void SetInput(string s) {
			if (baseReader != null) {
				baseReader.Dispose();
				baseReader = null;
			}

			baseReader = new StringReader(s);
		}

		public override int Read(char[] buffer, int index, int count) {
			if (baseReader == null)
				return 0;

			return baseReader.Read(buffer, index, count);
		}

		protected override void Dispose(bool disposing) {
			if (disposing) {
				if (baseReader != null)
					baseReader.Dispose();
				baseReader = null;
			}

			base.Dispose(disposing);
		}
	}
}