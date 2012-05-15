using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Deveel.Collections;

namespace Deveel.Console.Commands {
	public class HelpCommand : Command {
		public override string Name {
			get { return "help"; }
		}

		public override string[] Aliases {
			get { return new string[] { "?" }; }
		}

		public override string[] Synopsis {
			get { return new string[] { "help [ <command-name> ]", "? [ <command-name> ]" }; }
		}

		public override string ShortDescription {
			get { return "provides help for commands"; }
		}

		#region Private Methods
		private void WriteDescription(Command c) {
			string desc = c.LongDescription;
			if (desc == null) {
				if (c.ShortDescription != null) {
					desc = "\t[short description]: " + c.ShortDescription;
				}
			}

			string[] synopsis = c.Synopsis;

			if (synopsis != null && synopsis.Length > 0) {
				Application.Error.WriteBold("SYNOPSIS");
				Application.Error.WriteLine();

				for (int i = 0; i < synopsis.Length; i++) {
					Application.Error.WriteLine("\t" + synopsis[i]);
				}
				
				Application.Error.WriteLine();
			}
			if (desc != null) {
				Application.Error.WriteBold("DESCRIPTION");
				Application.Error.WriteLine();

				StringReader reader = new StringReader(desc);
				string line;
				while ((line = reader.ReadLine()) != null) {
					Application.Error.Write("\t");
					Application.Error.WriteLine(line);
				}
				if (c.RequiresContext) {
					Application.Error.WriteLine("\t[Requires valid context]");
				}
			}
			if (desc == null && synopsis == null) {
				Application.Error.WriteLine("no detailed help for '" + c.Name + "'");
			}
		}
		#endregion

		private class CommandHelp {
			public string Name;
			public string Description;
		}

		#region Public Methods

		public override CommandResultCode Execute(IExecutionContext context, CommandArguments args) {
			if (args.Count > 1)
				return CommandResultCode.SyntaxError;

			string commandName = null;
			if (args.MoveNext())
				commandName = args.Current;

			// nothing given: provide generic help.
			
			Application.Error.WriteLine();

			int maxPad = 0;
			if (commandName == null) {
				ICollection<Command> commands = Application.Commands.RegisteredCommands;

				// process the command groups first...
				Dictionary<string, List<CommandHelp>> groups = new Dictionary<string, List<CommandHelp>>();
				foreach (Command command in commands) {
					string groupName = command.GroupName;
					if (groupName == null || groupName.Length == 0)
						groupName = "commands";

					List<CommandHelp> list;
					if (!groups.TryGetValue(groupName, out list)) {
						list = new List<CommandHelp>();
						groups[groupName] = list;
					}

					CommandHelp commandHelp = new CommandHelp();

					StringBuilder cmdPrint = new StringBuilder(" ");
					string[] aliases = command.Aliases;

					cmdPrint.Append(command.Name);

					if (aliases != null && aliases.Length > 0) {
						cmdPrint.Append(" | ");
						for (int i = 0; i < aliases.Length; i++) {
							if (i != 0)
								cmdPrint.Append(" | ");
							cmdPrint.Append(aliases[i]);
						}
					}

					commandHelp.Name = cmdPrint.ToString();

					string description = command.ShortDescription;
					if (description == null) {
						// no description ... try to get the groups description...
					}

					commandHelp.Description = description;

					maxPad = Math.Max(maxPad, cmdPrint.Length);

					list.Add(commandHelp);
				}

				foreach (KeyValuePair<string, List<CommandHelp>> entry in groups) {
					string groupName = entry.Key;
					Application.Error.Write(groupName);
					Application.Error.Write(":");
					Application.Error.WriteLine();

					List<CommandHelp> commandList = entry.Value;
					foreach (CommandHelp command in commandList) {
						Application.Error.Write("  ");
						Application.Error.Write(command.Name);

						if (command.Description != null) {
							for (int i = 0; i < maxPad - command.Name.Length; ++i)
								Application.Error.Write(" ");

							Application.Error.Write(" : ");
							Application.Error.Write(command.Description);
						}

						Application.Error.WriteLine();
					}
				}
			} else {
				CommandDispatcher disp = Application.Commands;

				string cmdString = disp.CompleteCommandName(commandName);
				Command c = disp.GetCommand(cmdString);
				if (c == null) {
					Application.Error.WriteLine("Help: unknown command '" + cmdString + "'");
					Application.Error.WriteLine();
					return CommandResultCode.ExecutionFailed;
				}

				WriteDescription(c);
			}

			Application.Error.WriteLine();
			return CommandResultCode.Success;
		}

		public override string LongDescription {
			get {
				return "Provides help for the given command.   If invoked without a " +
				       "command name as parameter, a list of all available commands" +
				       "is shown.";
			}
		}

		public override IEnumerator<string> Complete(CommandDispatcher dispatcher, string partialCommand, string lastWord) {
			// if we already have one arguemnt and try to expand the next: no.
			int argc = partialCommand.Split(' ').Length;
	if (argc > 2 || (argc == 2 && lastWord.Length == 0)) {
	    return null;
	}

			IEnumerator<string> it = Application.Commands.GetRegisteredCommandNames(lastWord).GetEnumerator();
        return new SortedMatchEnumerator(lastWord, it);
		}
		#endregion
		
		#region HelpSortedMatchEnumerator
		
		class HelpSortedMatchEnumerator : SortedMatchEnumerator {
			private readonly HelpCommand command;
			
			public HelpSortedMatchEnumerator(HelpCommand command, string lastWord, IEnumerator<string> en)
				: base(lastWord, en) {
				this.command = command;
			}
			
			protected override bool Exclude(string current) {
				Command cmd = command.Application.Commands.GetCommand(current);
				return (cmd.LongDescription == null);
			}
		}
		
		#endregion
	}
}