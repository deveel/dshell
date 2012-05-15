using System;

namespace Deveel.Console {
	/// <summary>
	/// Allows to write a string to the screen and cancel it 
	/// afterwards (with Backspaces).
	/// </summary>
	public sealed class CancelWriter {
		private const string BACKSPACE = "\b";

		private readonly OutputDevice _out;
		private readonly bool _doWrite;

		/** the string that has been written. 'null', if
		 * nothing has been written or it is cancelled 
		 */
		private String _writtenString;

		public CancelWriter(OutputDevice output) {
			_out = output;
			_doWrite = _out.IsTerminal;
			_writtenString = null;
		}


		/// <summary>
		/// Gets a value indicating wether this cancel writer will 
		/// print anything.
		/// </summary>
		public bool IsPrinting {
			get { return _doWrite; }
		}

		/// <summary>
		/// Gets a value indicating if this cancel writer has any 
		/// cancellable output.
		/// </summary>
		public bool HasCancellableOutput {
			get { return _writtenString != null; }
		}

		/// <summary>
		/// Print message to screen and cancel out any 
		/// previous message.
		/// </summary>
		/// <param name="str">The string to write out.</param>
		public void Write(String str) {
			if (!_doWrite)
				return;

			int charCount = Cancel(false);
			_out.Write(str);
			_writtenString = str;

			/* wipe the difference between the previous
			 * message and this one */
			int lenDiff = charCount - str.Length;
			if (lenDiff > 0) {
				writeChars(lenDiff, " ");
				writeChars(lenDiff, BACKSPACE);
			}
			_out.Flush();
		}

		/// <summary>
		/// Cancels out the written string and wipe it 
		/// with spaces.
		/// </summary>
		/// <returns></returns>
		public int Cancel() {
			return Cancel(true);
		}

		/// <summary>
		/// Cancels the output of the underlying
		/// <see cref="OutputDevice">output</see>.
		/// </summary>
		/// <param name="wipeOut">Indicates if the written characters 
		/// should be wiped out with spaces. Otherwise, the cursor is 
		/// placed at the beginning of the string without wiping.</param>
		/// <returns>
		/// Returns number of characters cancelled.
		/// </returns>
		public int Cancel(bool wipeOut) {
			if (_writtenString == null)
				return 0;
			int backspaceCount = _writtenString.Length;
			writeChars(backspaceCount, BACKSPACE);
			if (wipeOut) {
				writeChars(backspaceCount, " ");
				writeChars(backspaceCount, BACKSPACE);
				_out.Flush();
			}
			_writtenString = null;
			return backspaceCount;
		}

		private void writeChars(int count, String str) {
			for (int i = 0; i < count; ++i) {
				_out.Write(str);
			}
		}
	}
}