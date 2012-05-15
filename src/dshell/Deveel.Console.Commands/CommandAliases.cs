using System;
using System.Collections;
using System.Collections.Generic;

using Deveel.Collections;

namespace Deveel.Console.Commands {
	public sealed class CommandAliases : IEnumerable<KeyValuePair<string, string>> {
		internal CommandAliases(CommandDispatcher dispatcher) {
			aliases = new SortedDictionary<string, string>();
			dispatcher = dispatcher;
			currentExecutedAliases = new List<string>();
		}

		private readonly SortedDictionary<string, string> aliases;
		private readonly CommandDispatcher dispatcher;
		private bool dirty;
		// to determine, if we got a recursion: one alias calls another
		// alias which in turn calls the first one ..
		private readonly List<string> currentExecutedAliases;
		private ConfigurationFile confFile;

		internal CommandDispatcher Dispatcher {
			get { return dispatcher; }
		}

		internal void AddAlias(string alias, string command) {
			aliases.Add(alias, command);
			dispatcher.RegisterAdditionalCommand(alias, new AliasedCommand(this, alias));
			dirty = true;
		}

		internal void RemoveAlias(string alias) {
			aliases.Remove(alias);
			dispatcher.UnregisterAdditionalCommand(alias);
			dirty = true;
		}

		internal IEnumerator<string> Complete(string partialCommand, string lastWord) {
			string[] st = partialCommand.Split(' ');
			String cmd = (String)st[0];
			int argc = st.Length;

			// 'aliases' command gets no names.
			string[] commandNames1 = dispatcher.GetCommandNames(typeof(AliasesCommand));
			if (Array.BinarySearch(commandNames1, cmd) >= 0)
				return null;

			// some completion within the alias/unalias commands.
			commandNames1 = dispatcher.GetCommandNames(typeof(AliasCommand));
			string[] commandNames2 = dispatcher.GetCommandNames(typeof(UnaliasCommand));
			if (Array.BinarySearch(commandNames1, cmd) >= 0 || 
			    Array.BinarySearch(commandNames2, cmd) >= 0) {
				List<string> alreadyGiven = new List<string>();

				if (Array.BinarySearch(commandNames1, cmd) >= 0) {
					// do not complete beyond first word.
					if (argc > ("".Equals(lastWord) ? 0 : 1)) {
						return null;
					}
				} else {
					/*
					 * remember all aliases, that have already been given on
					 * the commandline and exclude from completion..
					 * cool, isn't it ?
					 */
					for (int i = 1; i < st.Length; i++) {
						alreadyGiven.Add(st[i]);
					}
				}

				// ok, now return the list.
				return new AliasSortedMatchEnumerator(alreadyGiven, lastWord, aliases);
			}

			/* ok, someone tries to complete something that is a command.
			 * try to find the actual command and ask that command to do
			 * the completion.
			 */
			String toExecute = (String)aliases[cmd];
			if (toExecute != null) {
				Command c = dispatcher.GetCommand(toExecute);
				if (c != null) {
					int i = 0;
					String param = partialCommand;
					while (param.Length < i
						   && Char.IsWhiteSpace(param[i])) {
						++i;
					}
					while (param.Length < i
						   && !Char.IsWhiteSpace(param[i])) {
						++i;
					}
					return c.Complete(dispatcher, toExecute + param.Substring(i), lastWord);
				}
			}

			return null;
		}

		private class AliasSortedMatchEnumerator : SortedMatchEnumerator {
			public AliasSortedMatchEnumerator(List<string> alreadyGiven, string lastWord, SortedDictionary<string, string> aliases)
				: base(lastWord, aliases) {
				this.alreadyGiven = alreadyGiven;
			}

			private readonly List<string> alreadyGiven;

			protected override bool Exclude(String current) {
				return alreadyGiven.Contains(current);
			}
		}

		#region AliasedCommand
		
		private class AliasedCommand : Command {
			public AliasedCommand(CommandAliases command, string name) {
				parent = command;
				this.name = name;
			}

			private readonly CommandAliases parent;
			private readonly string name;

			public override string Name {
				get { return name; }
			}

			public override string LongDescription {
				get {
					string dsc = String.Empty;
					// not session-proof:
					if (parent.currentExecutedAliases.Contains(name)) {
						dsc = "\t[ this command cyclicly references itself ]";
					} else {
						parent.currentExecutedAliases.Add(name);
						dsc = "\tThis is an alias for the command\n" + "\t   " + parent.aliases[name];

						String actualCmdStr = (String)parent.aliases[name];
						if (actualCmdStr != null) {
							string[] st = actualCmdStr.Split(' ');
							actualCmdStr = st[0].Trim();
							Command c = parent.dispatcher.GetCommand(actualCmdStr);
							String longDesc = null;
							if (c != null && (longDesc = c.LongDescription) != null) {
								dsc += "\n\n\t..the following description could be determined for this";
								dsc += "\n\t------- [" + actualCmdStr + "] ---\n";
								dsc += longDesc;
							}
							parent.currentExecutedAliases.Clear();
						}
					}

					return dsc;
				}
			}

			public override CommandResultCode Execute(IExecutionContext context, CommandArguments args) {
				String toExecute = (String)parent.aliases[name];

				if (toExecute == null) {
					return CommandResultCode.ExecutionFailed;
				}
				// not session-proof:
				if (parent.currentExecutedAliases.Contains(name)) {
					parent.Dispatcher.Application.Error.WriteLine("Recursive call to aliases [" + name + "]. Stopping this senseless venture.");
					parent.currentExecutedAliases.Clear();
					return CommandResultCode.ExecutionFailed;
				}
				string commandText = args.ToString();
				commandText = toExecute + " " + commandText;
				parent.Dispatcher.Application.Error.WriteLine("execute alias: " + commandText);
				parent.currentExecutedAliases.Add(name);
				parent.Dispatcher.ExecuteCommand(context, commandText);
				parent.currentExecutedAliases.Clear();
				return CommandResultCode.Success;
			}
		}
		
		#endregion

		public bool HasAlias(string alias) {
			return aliases.ContainsKey(alias);
		}
		
		public bool IsAliasOf(string alias, string commandName) {
			string cname;
			if (!aliases.TryGetValue(alias, out cname))
				return false;
			
			return cname.Equals(commandName);
		}
		
		public string[] GetCommandAliases(string commandName) {
			List<string> commandAliases = new List<string>();
			foreach(KeyValuePair<string, string> alias in aliases) {
				if (alias.Value == commandName) {
					commandAliases.Add(alias.Key);
				}
			}
			
			return commandAliases.ToArray();
		}

		internal void LoadFromFile(ConfigurationFile file) {
			aliases.Clear();

			foreach(KeyValuePair<string, string> property in file.Properties) {
				aliases.Add(property.Key, property.Value);
			}
			confFile = file;
		}

		internal void Save() {
			if (confFile != null && dirty) {
				confFile.ClearValues();
				foreach(KeyValuePair<string, string> alias in aliases) {
					confFile.SetValue(alias.Key, alias.Value);
				}
				confFile.Save("Aliases");
				dirty = false;
			}
		}

		public IEnumerator<KeyValuePair<string, string>> GetEnumerator() {
			return aliases.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}