using System;

namespace Deveel.Console.Commands {
	class ExitCommand : Command {
		public override string Name {
			get { return "exit"; }
		}

		public override string[] Synopsis {
			get { return new string[] { "exit [ <code> ]" }; }
		}

		public override string[] Aliases {
			get { return new string[] { "quit" }; }
		}

		public override string ShortDescription {
			get { return "exits the application"; }
		}

		public override CommandResultCode Execute(IExecutionContext context, CommandArguments args) {
			int exitCode = 0;
			if (args.Count == 1)
				exitCode = Int32.Parse(args.Peek(0));

			Application.Exit(exitCode);
			return CommandResultCode.Success;
		}
	}
}