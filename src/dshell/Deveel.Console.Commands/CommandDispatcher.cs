using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using Deveel.Collections;
using Deveel.Configuration;

namespace Deveel.Console.Commands {
	public sealed class CommandDispatcher {
		#region ctor

		public CommandDispatcher(IApplicationContext application) {
			this.application = application;
			aliases = new CommandAliases(this);
			commandMap = new SortedDictionary<string, Command>();
			commands = new List<CommandInfo>();
			batchCount = 0;

			Readline.TabComplete += OnTabComplete;
		}

		#endregion

		#region Fields
		private readonly IApplicationContext application;
		private readonly CommandAliases aliases;
		private readonly List<CommandInfo> commands; // commands in seq. of addition.
		private List<Command> regCommands;
		private readonly SortedDictionary<string, Command> commandMap;
		private int batchCount;
		private char commandSeparator;

		#endregion

		#region Events

		/// <summary>
		/// An event fired during the execution of a command in the
		/// method <see cref="ExecuteCommand"/>.
		/// </summary>
		public event CommandEventHandler CommandExecuting;

		/// <summary>
		/// An event fired after the execution of a command in the
		/// method <see cref="ExecuteCommand"/>.
		/// </summary>
		public event CommandEventHandler CommandExecuted;

		#endregion

		#region Properties

		public bool IsInBatch {
			get { return batchCount > 0; }
		}

		public ICollection<Command> RegisteredCommands {
			get {
				if (regCommands == null) {
					regCommands = new List<Command>();
					foreach (CommandInfo commandInfo in commands)
						regCommands.Add(commandInfo.Command);
				}
				return regCommands;
			}
		}

		public ICollection RegisteredCommandNames {
			get { return commandMap.Keys; }
		}

		public IApplicationContext Application {
			get { return application; }
		}

		public char CommandSeparator {
			get { return commandSeparator; }
			set { commandSeparator = value; }
		}

		public CommandAliases Aliases {
			get { return aliases; }
		}

		#endregion

		#region Private Methods

		private IEnumerator possibleValues;
		private String variablePrefix;

		private void OnTabComplete(object sender, TabCompleteEventArgs e) {
			try {
				string completeCommandString = application.PartialLine.Trim();
				bool variableExpansion = false;

				// ok, do we have a variable expansion ?
				int pos = e.Text.Length - 1;
				while (pos > 0 && (e.Text[pos] != '$') &&
				       Char.IsLetter(e.Text[pos])) {
					--pos;
				}
				// either $... or ${...
				if ((pos >= 0 && e.Text[pos] == '$')) {
					variableExpansion = true;
				} else if ((pos >= 1) && e.Text[pos - 1] == '$' &&
				           e.Text[pos] == '{') {
					variableExpansion = true;
					--pos;
				}

				if (variableExpansion) {
					if (application is ISettingsHandler) {
						ApplicationSettings settings = ((ISettingsHandler) application).Settings;
						if (e.State == 0) {
							variablePrefix = e.Text.Substring(0, pos);
							String varname = e.Text.Substring(pos);
							possibleValues = settings.CompleteUserVariable(varname);
						}

						if (possibleValues.MoveNext()) {
							e.Output = variablePrefix + ((String) possibleValues.Current);
						} else {
							possibleValues.Reset();
						}
					}
				}
					// the first word.. the command.
				else if (completeCommandString.Equals(e.Text)) {
					string text = e.Text.ToLower();
					if (e.State == 0) {
						possibleValues = GetRegisteredCommandNames(text).GetEnumerator();
					}

					while (HasNext(possibleValues)) {
						String nextKey = (String) possibleValues.Current;
						if (nextKey == null || nextKey.Length == 0) // don't complete the 'empty' thing.
							continue;
						if (text.Length < 1) {
							Command c = (Command) commandMap[nextKey];
							if (!c.CommandCompletion)
								continue;
							if (c.RequiresContext &&
							    (application.ActiveContext == null ||
							     !application.ActiveContext.IsIsolated)) {
								continue;
							}
						}
						if (nextKey.StartsWith(text)) {
							e.Output = nextKey;
							break;
						}
					}
				}
					// .. otherwise get completion from the specific command.
				else {
					string text = e.Text.ToLower();
					if (e.State == 0) {
						Command cmd = GetCommand(completeCommandString);
						if (cmd != null)
							possibleValues = cmd.Complete(this, completeCommandString, text);
					}

					while (HasNext(possibleValues)) {
						string key = (string) possibleValues.Current;
						if (key.ToLower().StartsWith(text)) {
							e.Output = key;
							break;
						}
					}
				}
			} catch(Exception ex) {
				Application.Error.WriteLine("An error occurred while TAB-completing: {0}", ex.Message);
				e.Error = true;
				possibleValues = null;
				throw;
			}
		}
		
		private static bool HasNext(IEnumerator en) {
			if (en == null)
				return false;
			bool hasNext = en.MoveNext();
			if (!hasNext)
				en.Reset();
			return hasNext;
		}

		private void OnExecuting(IExecutionContext session, string commandText) {
			if (CommandExecuting != null)
				CommandExecuting(session, new CommandEventArgs(commandText));
		}

		private void OnExecuted(IExecutionContext session, string commandText, CommandResultCode resultCode) {
			if (CommandExecuted != null)
				CommandExecuted(session, new CommandEventArgs(commandText, resultCode));
		}

		private Command GetCommandFromCooked(string completeCmd) {
			if (String.IsNullOrEmpty(completeCmd))
				return null;

			Command c;
			if (!commandMap.TryGetValue(completeCmd, out  c))
				return null;

			return c;
		}
		#endregion

		internal void RegisterAdditionalCommand(string commandName, Command command) {
			commandMap.Add(commandName, command);
		}

		internal void UnregisterAdditionalCommand(string commandName) {
			commandMap.Remove(commandName);
		}

		#region Public Methods

		public void StartBatch() {
			++batchCount;
		}

		public void EndBatch() {
			--batchCount;
		}

		public void Shutdown() {
			int i = 0;
			while (i < commands.Count) {
				CommandInfo cmdInfo = (CommandInfo)commands[i];

				try {
					cmdInfo.Command.OnShutdown();
				} catch (Exception e) {
#if DEBUG
					System.Console.Error.Write(e.StackTrace);
#endif
				}
				i++;
			}
		}

		public void Register(Command command) {
			if (Application.IsRunning)
				throw new InvalidOperationException("The application is running.");

			if (command == null)
				throw new ArgumentNullException("command");

			try {
				command.Init();
			} catch(Exception e) {
				throw new ArgumentException("An error occurred while initializing the command: " + e.Message);
			}

			if (command is IInterruptable)
				Application.Interruption.Push(command as IInterruptable);

			if (command.Application != null && 
				command.Application != application)
				throw new ArgumentException("The command instance is already registered by another application.");

			command.SetApplicationContext(application);

			CommandInfo commandInfo = new CommandInfo(command.GetType(), command);
			commands.Add(commandInfo);
			string name = commandInfo.Command.Name;
			commandMap.Add(name, commandInfo.Command);
			if (commandInfo.Command.HasAliases) {
				string[] cmdAliases = commandInfo.Command.Aliases;
				for (int i = 0; i < cmdAliases.Length; ++i) {
					if (commandMap.ContainsKey(cmdAliases[i]))
						throw new ArgumentException("attempt to register command '" + cmdAliases[i] + "', that is already used");

					commandMap.Add(cmdAliases[i], commandInfo.Command);
				}
			}
		}

		public Command Register(Type commandType, params object[] args) {
			if (!typeof(Command).IsAssignableFrom(commandType))
				throw new ArgumentException("The type '" + commandType + "' is not assignable from ICommand.");

			Command command;
			try {
				command = (Command)Activator.CreateInstance(commandType, args);
			} catch (Exception e) {
				if (e is TargetInvocationException)
					e = e.InnerException;

				throw new ArgumentException(e.Message);
			}

			Register(command);
			return command;
		}

		public Command Register(Type commandType) {
			return Register(commandType, null);
		}

		public void Unregister(Type commandType) {
			bool found = false;
			string[] removedCommandNames = null;
			for (int i = commands.Count - 1; i >= 0; i--) {
				CommandInfo ci = commands[i];
				if (ci.CommandType == commandType) {
					found = true;
					ArrayList list = new ArrayList();
					list.Add(ci.Command.Name);
					if (ci.Command.Aliases != null && ci.Command.Aliases.Length > 0)
						list.AddRange(ci.Command.Aliases);

					removedCommandNames = (string[]) list.ToArray(typeof(string));
					commands.RemoveAt(i);
					break;
				}
			}

			if (!found)
				throw new ApplicationException("Attempt to unregister the command type '" + commandType + "' not registered.");

			if (removedCommandNames.Length > 0) {
				for (int i = 0; i < removedCommandNames.Length; i++)
					commandMap.Remove(removedCommandNames[i]);
			}
		}

		public void Unregister(Command command) {
			if (command == null)
				throw new ArgumentNullException("command");

			Unregister(command.GetType());
		}

		public bool Rename(string commandName, string newName) {
			if (String.IsNullOrEmpty(commandName))
				throw new ArgumentNullException("commandName");
			if (String.IsNullOrEmpty(newName))
				throw new ArgumentNullException("newName");

			if (commandMap.ContainsKey(newName))
				return false;

			Command command;
			if (!commandMap.TryGetValue(commandName, out command))
				return false;

			if (!commandMap.Remove(commandName))
				return false;

			commandMap[newName] = command;
			return true;
		}

		public void RegisterOptions(Options options) {
			foreach(Command command in commandMap.Values) {
				command.RegisterOptions(options);
			}
		}

		public bool ContainsCommand(string commandText) {
			return commandMap.ContainsKey(commandText);
		}

		public ICollection<string> GetRegisteredCommandNames(string key) {
			// return commandMap.TailDictionary(key).Keys;
			return SubsetDictionary<string, Command>.Tail(commandMap, key).Keys;
		}

		public string CompleteCommandName(string command) {
			if (command == null || command.Length == 0)
				return null;

			string cmd = command.ToLower();
			string startChar = cmd.Substring(0, 1);

			ICollection<string> commandNames = GetRegisteredCommandNames(startChar);
			string longestMatch = null;

			foreach (string testMatch in commandNames) {
				if (cmd.StartsWith(testMatch)) {
					longestMatch = testMatch;
				} else if (!testMatch.StartsWith(startChar)) {
					break; // ok, thats it.
				}
			}

			// ok, fallback: grab the first whitespace delimited part.
			if (longestMatch == null) {
				string[] tok = command.Split(new char[] { ' ', ';', '\t', '\n', '\r', '\f' });
				if (tok.Length > 0)
					return tok[0];
			}

			return longestMatch;
		}

		public void ExecuteCommand(IExecutionContext context, string commandText) {
			if (String.IsNullOrEmpty(commandText))
				return;

			// remove trailing command separator and whitespaces.
			StringBuilder cmdBuilder = new StringBuilder(commandText.Trim());
			int i;
			for (i = cmdBuilder.Length - 1; i > 0; --i) {
				char c = cmdBuilder[i];
				if (c != CommandSeparator && !Char.IsWhiteSpace(c))
					break;
			}
			if (i < 0)
				return;

			cmdBuilder.Length = i + 1;
			string cmd = cmdBuilder.ToString();

			string cmdName = CompleteCommandName(cmd);
			Command command = GetCommandFromCooked(cmdName);

			if (command != null) {
				try {
					string[] args = new string[0];
					string parameters = cmd.Substring(cmdName.Length);
					if (parameters.Length > 0) {
						parameters = parameters.Trim();
						args = parameters.Split(' ');
						ArrayList argsList = new ArrayList();
						for (int j = 0; j < args.Length; j++) {
							args[j] = args[j].Trim();
							if (args[j].Length > 0)
								argsList.Add(args[j]);
						}
						args = (string[]) argsList.ToArray(typeof (string));
					}

					if (command.RequiresContext && (context == null || !context.IsIsolated)) {
						Application.Error.WriteLine(cmdName + " requires a valid isolated context.");
						return;
					}

					OnExecuting(context, commandText);
					CommandResultCode result = command.Execute(context, new CommandArguments(args));
					OnExecuted(context, commandText, result);

					switch (result) {
						case CommandResultCode.SyntaxError: {
								string[] synopsis = command.Synopsis;
								if (synopsis != null && synopsis.Length > 0) {
									Application.Error.WriteLine(cmdName + " usage: ");
									for (int j = 0; j < synopsis.Length; j++) {
										Application.Error.Write("  ");
										Application.Error.WriteLine(synopsis[j]);
									}
								} else {
									Application.Error.WriteLine(cmdName + " syntax error.");
								}
								break;
							}
						case CommandResultCode.ExecutionFailed: {
								// if we are in batch mode, then no message is written
								// to the screen by default. Thus we don't know, _what_
								// command actually failed. So in this case, write out
								// the offending command.

								if (IsInBatch) {
									Application.Error.WriteLine("-- failed command: ");
									Application.Error.WriteLine(commandText);
								}
								break;
							}
					}
				} catch (Exception e) {
#if DEBUG
					System.Console.Error.WriteLine(e.Message);
					System.Console.Error.WriteLine(e.StackTrace);
#endif
					Application.Error.WriteLine(e.ToString());
					OnExecuted(context, commandText, CommandResultCode.ExecutionFailed);
				}
			}
		}

		public Command GetCommand(Type commandType) {
			for (int i = 0; i < commands.Count; i++) {
				CommandInfo cmdInfo = commands[i];

				if (cmdInfo.CommandType == commandType)
					return cmdInfo.Command;
			}

			return null;
		}
		
		public string[] GetCommandNames(Type commandType) {
			string baseName = null;
			foreach(KeyValuePair<string, Command> pair in commandMap) {
				if (commandType.IsInstanceOfType(pair.Value)) {
					baseName = pair.Key;
					break;
				}
			}
			
			if (String.IsNullOrEmpty(baseName))
				return new string[0];
			
			List<string> names = new List<string>();
			names.Add(baseName);
			
			/*
			string[] commandAliases = aliases.GetCommandAliases(baseName);
			for (int i = 0; i < names.Count; i++) {
				names.Add(names[i]);
			}
			*/
			
			return names.ToArray();
		}

		public Command GetCommand(string commandText) {
			return GetCommandFromCooked(CompleteCommandName(commandText));
		}
		
		#endregion

		#region CommandInfo
		class CommandInfo {
			public CommandInfo(Type cmdType, Command command) {
				CommandType = cmdType;
				Command = command;
			}

			public readonly Type CommandType;
			public readonly Command Command;
		}
		#endregion
	}
}