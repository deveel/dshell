using System;
using System.Collections.Generic;

namespace Deveel.Console.Commands {
	sealed class AliasCommand : Command {
		public override string Name {
			get { return "alias"; }
		}

		public override CommandResultCode Execute(IExecutionContext context, CommandArguments args) {
			if (!args.MoveNext())
				return CommandResultCode.SyntaxError;

			string alias = args.Current;
			// no quoted aliases..
			if (alias.StartsWith("\"") || alias.StartsWith("'"))
				return CommandResultCode.SyntaxError;

			// unless we override an alias, moan, if this command already
			// exists.
			if (!Application.Commands.Aliases.HasAlias(alias) &&
				Application.Commands.ContainsCommand(alias)) {
				Error.WriteLine("cannot alias built-in command!");
				return CommandResultCode.ExecutionFailed;
			}

			if (!args.MoveNext())
				return CommandResultCode.SyntaxError;

			String value = StripQuotes(args.Current); // rest of values.
			Application.Commands.Aliases.AddAlias(alias, value);
			return CommandResultCode.Success;
		}


		private static String StripQuotes(String value) {
			if (value.StartsWith("\"") && value.EndsWith("\"")) {
				value = value.Substring(1, value.Length - 2);
			} else if (value.StartsWith("\'") && value.EndsWith("\'")) {
				value = value.Substring(1, value.Length - 2);
			}
			return value;
		}

		public override IEnumerator<string> Complete(CommandDispatcher disp, string partialCommand, string lastWord) {
			return Application.Commands.Aliases.Complete(partialCommand, lastWord);
		}

		public override String LongDescription {
			get {
				return "Add an alias for a command. This means, that you can\n" +
					   "give a short name for a command you often use.  This\n" +
					   "might be as simple as\n" +
					   "   alias ls tables\n" +
					   "to execute the tables command with a short 'ls'.\n\n" +
					   "For longer commands it is even more helpful:\n" +
					   "   alias size select count(*) from\n" +
					   "This command  needs a table  name as a  parameter to\n" +
					   "expand  to  a  complete command.  So 'size students'\n" +
					   "expands to 'select count(*) from students' and yields\n" +
					   "the expected result.\n\n" +
					   "To make life easier, the application tries to determine\n" +
					   "the command  to be executed so that the  tab-completion\n" +
					   "works even here; in this latter case it  would help\n" +
					   "complete table names.";
			}
		}
	}
}