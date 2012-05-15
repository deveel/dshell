using System;
using System.Text;

namespace Deveel.Console {
	public sealed class ConsoleInputDevice : InputDevice {
		private string prompt;
		private string emptyPrompt;
		private string currentPrompt;
		private int lineCount = -1;
				
		public string Prompt {
			get { return prompt; }
			set {
				prompt = value;
				if (!String.IsNullOrEmpty(prompt)) {
					StringBuilder emptyBuilder = new StringBuilder();
					for (int i = 0; i < prompt.Length; i++) {
						emptyBuilder.Append(' ');
					}
					emptyPrompt = emptyBuilder.ToString();
					currentPrompt = prompt;
				}
			}
		}
		
		public override void CompleteLine() {
			lineCount = -1;
			currentPrompt = prompt;
		}
		
		public override string ReadLine() {
			string line = Readline.ReadLine(currentPrompt);
			if (++lineCount > 0)
				currentPrompt = emptyPrompt;
			
			return line;
		}
		
		public override int Read() {
			throw new NotSupportedException();
		}
		
		public override int Read(char[] buffer, int index, int count) {
			throw new NotSupportedException();
		}
	}
}