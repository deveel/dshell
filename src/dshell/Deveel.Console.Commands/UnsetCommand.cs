using System;

namespace Deveel.Console.Commands {
	internal class UnsetCommand : Command {
		public override string Name {
			get { return "unset"; }
		}

		public override string GroupName {
			get { return "variables"; }
		}

		public override string ShortDescription {
			get { return "unsets previously set variables"; }
		}

		public override string[] Synopsis {
			get { return new string[] { "unset <varname> [ <varname> ... ]" }; }
		}

		private bool UnsetVariable(string varName, ApplicationSettings settings) {
			if (!settings.HasVariable(varName)) {
				Application.Error.WriteLine("unknown variable {0}", varName);
				return false;
			}

			settings.RemoveVariable(varName);
			return true;
		}

		public override CommandResultCode Execute(IExecutionContext context, CommandArguments args) {
			ISettingsHandler handler = Application as ISettingsHandler;
			if (handler == null) {
				Error.WriteLine("The application doesn't support settings.");
				return CommandResultCode.ExecutionFailed;
			}

			if (!args.MoveNext())
				return CommandResultCode.SyntaxError;

			string varName = args.Current;
			bool success = UnsetVariable(varName, handler.Settings);

			while (args.MoveNext()) {
				success |= UnsetVariable(args.Current, handler.Settings);
			}

			return success ? CommandResultCode.Success : CommandResultCode.ExecutionFailed;
		}
	}
}