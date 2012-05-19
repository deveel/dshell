using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Deveel.Console {
	class TestOutputDevice : OutputDevice {
		private readonly TextWriter outputSink;
		private readonly List<string> stringList;

		public TestOutputDevice() {
			outputSink = new OutputSink(this);
			stringList = new List<string>();
		}

		public event StringWriteEventHandler StringWritten;

		public override Encoding Encoding {
			get { return Encoding.ASCII; }
		}

		protected override TextWriter Output {
			get { return outputSink; }
		}

		public IEnumerable<string> Strings {
			get { return stringList.AsReadOnly(); }
		}

		protected void OnStringWritten(string s) {
			if (StringWritten != null)
				StringWritten(this, new StringWriteEventArgs(s));

			stringList.Add(s);
		}

		#region OutputSink

		class OutputSink : TextWriter {
			private readonly TestOutputDevice outputDevice;

			public OutputSink(TestOutputDevice outputDevice) {
				this.outputDevice = outputDevice;
			}

			public override Encoding Encoding {
				get { return outputDevice.Encoding; }
			}

			public override void Write(char[] buffer, int index, int count) {
				outputDevice.OnStringWritten(new string(buffer, index, count));
			}
		}

		#endregion
	}
}
