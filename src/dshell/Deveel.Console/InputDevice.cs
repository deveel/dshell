using System;
using System.IO;

namespace Deveel.Console {
	public abstract class InputDevice : TextReader {
		public virtual bool IsTerminal {
			get { return false; }
		}
		
		public virtual void CompleteLine() {
		}
	}
}