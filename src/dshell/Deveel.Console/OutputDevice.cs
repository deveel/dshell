using System;
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
		
		public override void Write(char[] buffer, int index, int count) {
			int lineMaxWidth = LineMaxWidth;
			if (lineMaxWidth > 0) {
				if (count + lineWidth > lineMaxWidth) {
					int lines = (int) Math.Round((double)count/lineMaxWidth, MidpointRounding.ToEven);

					char[] lineTerminator = NewLine.ToCharArray();
					char[] toAdd = OnNewLine();
					char[] newBuffer = new char[lines*(lineTerminator.Length + toAdd.Length + lineMaxWidth)];

					int srcOffset = 0;
					int destOffset = 0;
					for (int i = 0; i < lines; i++) {
						int endLine = lineMaxWidth - lineTerminator.Length;
						Array.Copy(buffer, srcOffset, newBuffer, destOffset, endLine);
						Array.Copy(lineTerminator, 0, newBuffer, endLine, lineTerminator.Length);
						Array.Copy(toAdd, 0, newBuffer, endLine + lineTerminator.Length, toAdd.Length);
						srcOffset += endLine;
						destOffset += lineMaxWidth;
						lineWidth += lineMaxWidth;
					}

					buffer = newBuffer;
					count = buffer.Length;
					lineWidth = 0;
				} else {
					char[] lineTerminator = NewLine.ToCharArray();
					char[] test = new char[lineTerminator.Length];
					bool hasEOL = true;
					if (count < lineTerminator.Length) {
						hasEOL = false;
					} else {
						Array.Copy(buffer, count - lineTerminator.Length, test, 0, lineTerminator.Length);
						for (int i = 0; i < lineTerminator.Length; i++) {
							if (test[i] != lineTerminator[i]) {
								hasEOL = false;
								break;
							}
						}
					}
					
					if (hasEOL) {
						lineWidth = 0;
					} else {
						lineWidth += count;
					}
				}
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