using System;
using System.Collections.Generic;

namespace Deveel.Console {
	public sealed class Question {
		private readonly string text;
		private object[] options;
		private int defaultOption;
		private int maxResponse = 1;
		
		public Question(string text, object[] options, int defaultOption) {
			this.text = text;
			this.options = options;
			this.defaultOption = defaultOption;
		}
		
		public string Text {
			get { return text; }
		}
		
		public object[] Options {
			get { return options; }
		}
		
		public int DefaultOption {
			get { return defaultOption; }
			set {
				if (value < 0 || value > options.Length)
					throw new ArgumentOutOfRangeException("value");

				defaultOption = value;
			}
		}
		
		public int MaxSelected {
			get { return maxResponse; }
			set {
				if (value > options.Length)
					throw new ArgumentException();
				maxResponse = value;
			}
		}
		
		public bool SingleOption {
			get { return maxResponse == 1; }
			set { maxResponse = value ? 1 : options.Length; }
		}
		
		public Answer Answer(int[] selected) {
			if (selected.Length == 0 || selected.Length > maxResponse)
				throw new ArgumentOutOfRangeException();
			
			if (selected.Length > options.Length)
				throw new ArgumentOutOfRangeException();
			
			List<int> check = new List<int>(selected.Length);
			for (int i = 0; i < selected.Length; i++) {
				if (selected[i] < 0 || selected[i] >= options.Length)
					throw new ArgumentOutOfRangeException();
				if (check.Contains(selected[i]))
					throw new ArgumentException();
				
				check.Add(selected[i]);
			}
			
			return new Answer(this, selected, true);
		}
		
		public Answer Answer(int selected) {
			return Answer(new int[] { selected } );
		}
		
		public Answer Ask(IApplicationContext context, bool verticalAlign) {
			context.Out.Write(text);
			
			if (verticalAlign) {
				context.Out.WriteLine();
			} else {
				context.Out.Write(" [");
			}
			
			for(int i = 0; i < options.Length; i++) {
				if (verticalAlign) {
					context.Out.Write("  ");
					
					context.Out.Write(i + 1);
					context.Out.Write(". ");
					
					if (defaultOption != -1 &&
						maxResponse == 1) {
						context.Out.Write("[");
						if (defaultOption == i) {
							context.Out.Write("x");
						} else {
							context.Out.Write(" ");
						}
						context.Out.Write("] ");
					}
					
					context.Out.WriteLine(options[i]);
				} else {
					if (defaultOption != -1 &&
					    maxResponse == 1 &&
					    defaultOption == i)
						context.Out.Write("*");
					
					context.Out.Write(options[0]);
					if (i < options.Length - 1)
						context.Out.Write(", ");
				}
			}
			
			if (!verticalAlign)
				context.Out.WriteLine("] : ");
			
			string input = context.Input.ReadLine();
			if (String.IsNullOrEmpty(input))
				return new Answer(this, null, false);
			
			string[] sp = input.Split(',');
			
			if (sp.Length > options.Length ||
			    sp.Length > maxResponse)
				return new Answer(this, null, false);
			
			List<int> selected = new List<int>();
			for (int i = 0; i < sp.Length; i++) {
				string s = sp[i].Trim();
				bool found = false;
				int offset = -1;
				
				if (verticalAlign && Int32.TryParse(s, out offset)) {
					offset = offset -1;
					found = true;
				} else {
					for (int j = 0; j < options.Length; j++) {
						if (String.Compare(s, options[j].ToString(), true) == 0) {
							offset = j;
							found = true;
							break;
						}
					}
				}
				
				if (!found)
					return new Answer(this, null, false);

				selected.Add(offset);
			}
			
			return new Answer(this, selected.ToArray(), true);
		}
	}
}