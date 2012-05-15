using System;

namespace Deveel.Console {
	public sealed class Answer {
		private readonly Question question;
		private readonly int[] selected;
		private readonly bool valid;
		
		internal Answer(Question question, int[] selected, bool valid) {
			this.question = question;
			this.selected = selected;
			this.valid = valid;
		}
				
		public Question Question {
			get { return question; }
		}
		
		public bool IsValid {
			get { return valid; }
		}
		
		public int SelectedOption {
			get { return selected.Length == 1 ? selected[0] : -1; }
		}
		
		public int[] SelectedOptions {
			get { return (int[])selected.Clone(); }
		}
		
		public object[] SelectedValues {
			get {
				object[] values = new object[selected.Length];
				for (int i = 0; i < selected.Length; i++) {
					values[i] = question.Options[selected[i]];
				}
				return values;
			}
		}
		
		public object SelectedValue {
			get {
				int selectedOption = SelectedOption;
				return selectedOption == -1 ? null : question.Options[selectedOption];
			}
		}
	}
}