using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Deveel.Collections;
using Deveel.Console.Commands;

namespace Deveel.Console {
	public sealed class ApplicationSettings : IEnumerable<KeyValuePair<string, string>> {
		public ApplicationSettings(IApplicationContext application) {
			this.application = application;
			specialVariables = new List<string>();
			specialVariables.Add(SpecialLastCommand);
			variables = new SortedDictionary<string, string>();
			application.Commands.CommandExecuted += CommandExecuted;
		}

		void CommandExecuted(object sender, Commands.CommandEventArgs e) {
			SetVariable(SpecialLastCommand, e.CommandText.Trim());
		}

		private readonly IApplicationContext application;
		private readonly List<string> specialVariables;
		private readonly SortedDictionary<string, string> variables;

		internal const string SpecialLastCommand = "_SHELLAPP_LAST_COMMAND";
		
		internal IList<string> SpecialVariables {
			get { return specialVariables; }
		}
		
		internal IDictionary<string, string> Variables {
			get { return variables; }
		}

		public void SetVariable(string name, string value) {
			variables[name] = value;
		}

		public void RemoveVariable(string name) {
			variables.Remove(name);
		}

		public bool HasVariable(string name) {
			return variables.ContainsKey(name);
		}

		public IEnumerator<string> CompleteUserVariable(string variable) {
			if (!variable.StartsWith("$"))
				return null; // strange, shouldn't happen.

			bool hasBrace = variable.StartsWith("${");
			string prefix = (hasBrace ? "${" : "$");
			string postfix = (hasBrace ? "}" : "");
			string name = variable.Substring(prefix.Length);

			SortedMatchEnumerator en = new SortedMatchEnumerator(name, variables.Keys.GetEnumerator());
			en.Prefix = prefix;
			en.Suffix = postfix;
			return en;
		}

		public string Substitute(string input) {
			int pos = 0;
			int endpos = 0;
			int startVar = 0;
			StringBuilder result = new StringBuilder();
			string varname;
			bool hasBrace = false;
			bool knownVar = false;

			if (input == null)
				return null;

			if (variables == null)
				return input;

			while ((pos = input.IndexOf('$', pos)) >= 0) {
				startVar = pos;
				if (input[pos + 1] == '$') {
					// quoting '$'
					result.Append(input.Substring(endpos, pos - endpos));

					endpos = pos + 1;
					pos += 2;
					continue;
				}

				hasBrace = (input[pos + 1] == '{');

				// text between last variable and here
				result.Append(input.Substring(endpos, pos - endpos));

				if (hasBrace)
					pos++;

				endpos = pos + 1;
				while (endpos < input.Length &&
				       Char.IsLetterOrDigit(input[endpos]))
					endpos++;

				varname = input.Substring(pos + 1, endpos - (pos + 1));

				if (hasBrace) {
					while (endpos < input.Length && input[endpos] != '}')
						++endpos;

					++endpos;
				}
				if (endpos > input.Length) {
					if (variables.ContainsKey(varname))
						System.Console.Error.WriteLine("warning: missing '}' for variable '" + varname + "'.");

					result.Append(input.Substring(startVar));
					break;
				}

				if (variables.ContainsKey(varname)) {
					result.Append(variables[varname]);
				} else {
					System.Console.Error.WriteLine("warning: variable '" + varname + "' not set.");
					result.Append(input.Substring(startVar, endpos - startVar));
				}

				pos = endpos;
			}

			if (endpos < input.Length)
				result.Append(input.Substring(endpos));

			return result.ToString();
		}

		public IEnumerator<KeyValuePair<string, string>> GetEnumerator() {
			return variables.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		internal IEnumerator<string> Complete(string partialLine, string lastWord) {
			string[] st = partialLine.Split(' ');
			string cmd = st[0];
			int argc = st.Length;

			Command setCommand = application.Commands.GetCommand(typeof(SetCommand));
			Command unsetCommand = application.Commands.GetCommand(typeof(UnsetCommand));

			List<string> alreadyGiven = new List<string>();
			if (setCommand != null && cmd.Equals(setCommand.Name)) {
				if (argc > (lastWord.Equals(String.Empty) ? 0 : 1))
					return null;
			} else if (unsetCommand != null && cmd.Equals(unsetCommand.Name)) { // 'unset'
				// remember all variables, that have already been given on
				// the commandline and exclude from completion..
				for (int i = 1; i < st.Length; i++) {
					alreadyGiven.Add(st[i]);
				}
			}

			return new SetSortedMatchEnumerator(alreadyGiven, lastWord, variables);
		}

		class SetSortedMatchEnumerator : SortedMatchEnumerator {
			public SetSortedMatchEnumerator(List<string> alreadyGiven, string lastWord, SortedDictionary<string, string> variables)
				: base(lastWord, variables) {
				this.alreadyGiven = alreadyGiven;
			}

			private readonly List<string> alreadyGiven;

			protected override bool Exclude(string current) {
				return alreadyGiven.Contains(current);
			}
		}
	}
}