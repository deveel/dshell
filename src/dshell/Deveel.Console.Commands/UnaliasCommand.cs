using System;
using System.Collections.Generic;

namespace Deveel.Console.Commands {
	internal sealed class UnaliasCommand : Command {
		public override string Name {
			get { return "unalias"; }
		}

		public override string GroupName {
			get { return "aliases"; }
		}

		public override string ShortDescription {
			get { return "removes an aliased command"; }
		}

		public override string[] Synopsis {
			get { return new string[] { "unalias <alias-name>" }; }
		}

		public override CommandResultCode Execute(IExecutionContext context, CommandArguments args) {
			if (args.Count < 1)
				return CommandResultCode.SyntaxError;

			int failedCount = 0;
			while (args.MoveBack()) {
				string alias = args.Current;
				if (!Application.Commands.Aliases.HasAlias(alias)) {
					Error.WriteLine("unknown alias '" + alias + "'");
					failedCount++;
				} else {
					Application.Commands.Aliases.RemoveAlias(alias);
				}
			}

			return failedCount < args.Count ? CommandResultCode.Success : CommandResultCode.ExecutionFailed;
		}

		public override IEnumerator<string> Complete(CommandDispatcher dispatcher, string partialCommand, string lastWord) {
			return Application.Commands.Aliases.Complete(partialCommand, lastWord);
		}
	}
}