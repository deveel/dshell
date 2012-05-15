using System;
using System.IO;
using System.Text;

namespace Deveel.Console {
	public sealed class TextWriterOutputDevice : OutputDevice {
		private readonly TextWriter writer;

		public TextWriterOutputDevice(TextWriter writer) {
			this.writer = writer;
		}

		public override int LineMaxWidth {
			get { return -1; }
		}

		public override Encoding Encoding {
			get { return writer.Encoding; }
		}
		
		protected override TextWriter Output {
			get { return writer; }
		}

		public override void Flush() {
			writer.Flush();
		}

		public override void Close() {
			writer.Close();
		}
	}
}