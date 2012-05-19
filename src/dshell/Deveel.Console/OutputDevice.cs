using System;
using System.Collections.Generic;
using System.IO;

namespace Deveel.Console {
	public abstract class OutputDevice : TextWriter {
		private int lineWidth;

		public virtual bool IsTerminal {
			get { return false; }
		}

		public virtual int LineMaxWidth {
			get { return -1; }
		}
		
		protected abstract TextWriter Output { get; }

		protected virtual void AttributeBold() {
		}

		protected virtual void AttributeGrey() {
		}

		protected virtual void AttributeReset() {
		}

		protected virtual char [] OnNewLine() {
			return new char[0];
		}

		public virtual void WriteBold(string message) {
			AttributeBold();
			Write(message);
			AttributeReset();
		}

		public virtual void WriteGrey(string value) {
			AttributeGrey();
			Write(value);
			AttributeReset();
		}

		private static bool AreEqual(char[] a, char[] b) {
			if (a.Length != b.Length)
				return false;

			for (int i = 0; i < a.Length; i++) {
				if (!a[i].Equals(b[i]))
					return false;
			}

			return true;
		}
		
		public override void Write(char[] buffer, int index, int count) {
			int lineMaxWidth = LineMaxWidth;
			if (lineMaxWidth > 0) {
				char[] lineTerminator = NewLine.ToCharArray();
				char[] test = new char[lineTerminator.Length];

				List<char[]> lines = new List<char[]>();
				List<char> flush = new List<char>();
				for (int i = 0; i < count; i++) {
					if (i + lineTerminator.Length <= count) {
						Array.Copy(buffer, i, test, 0, lineTerminator.Length);

						bool hasEOL = AreEqual(lineTerminator, test);

						if (hasEOL) {
							flush.AddRange(lineTerminator);
							lines.Add(flush.ToArray());
							flush.Clear();
							i += (lineTerminator.Length - 1);
						} else {
							flush.Add(buffer[i]);
						}
					} else {
						flush.Add(buffer[i]);
					}
				}

				if (flush.Count > 0)
					lines.Add(flush.ToArray());

				List<char> newBuffer = new List<char>();
				for (int i = 0; i < lines.Count; i++) {
					char[] line = lines[i];
					char[] toAdd = OnNewLine();

					while (line.Length + lineWidth + toAdd.Length > lineMaxWidth) {
						int lineDiff = lineMaxWidth - lineWidth;
						int toCopy = lineDiff - lineTerminator.Length;
						char[] temp = new char[toCopy + toAdd.Length + lineTerminator.Length];
						Array.Copy(toAdd, 0, temp, 0, toAdd.Length);
						Array.Copy(line, 0, temp, toAdd.Length, toCopy);
						Array.Copy(lineTerminator, 0, temp, toCopy + toAdd.Length, lineTerminator.Length);

						newBuffer.AddRange(temp);

						char[] newLine = new char[line.Length - toCopy];
						Array.Copy(line, toCopy, newLine, 0, newLine.Length);

						line = newLine;
						
						if (lineWidth > 0)
							lineWidth = 0;
					}

					if (AreEqual(lineTerminator, line)) {
						newBuffer.AddRange(toAdd);
						newBuffer.AddRange(lineTerminator);
						lineWidth = 0;
					} else {
						newBuffer.AddRange(line);
						lineWidth += line.Length;
					}
				}

				buffer = newBuffer.ToArray();
				count = newBuffer.Count;
			}

			Output.Write(buffer, index, count);
		}
		
		public override void Flush() {
			Output.Flush();
		}
		
		public override void Close() {
			Output.Close();
		}
	}
}