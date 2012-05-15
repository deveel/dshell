using System;
using System.Collections.Generic;

namespace Deveel.Console.Commands {
	internal sealed class SetCommand : Command {
		public override string Name {
			get { return "set"; }
		}

		public override string ShortDescription {
			get { return "sets the value of a variable"; }
		}

		public override string GroupName {
			get { return "variables"; }
		}

		public override string[] Synopsis {
			get { return new string[] { "set <varname> <value>" }; }
		}

		public override CommandResultCode Execute(IExecutionContext context, CommandArguments args) {
			if (!(Application is ISettingsHandler)) {
				Error.WriteLine("The application doesn't support settings.");
				return CommandResultCode.ExecutionFailed;
			}

			ApplicationSettings settings = ((ISettingsHandler) Application).Settings;

			int argc = args.Count;
			if (argc < 2)
				return CommandResultCode.SyntaxError;

			if (!args.MoveNext())
				return CommandResultCode.SyntaxError;

			string varname = args.Current;

			if (!args.MoveNext())
				return CommandResultCode.SyntaxError;

			string value = args.Current;

			if (value.StartsWith("\"") && value.EndsWith("\"")) {
				value = value.Substring(1, value.Length - 2);
			} else if (value.StartsWith("\'") && value.EndsWith("\'")) {
				value = value.Substring(1, value.Length - 2);
			}

			settings.SetVariable(varname, value);

			Out.WriteLine();
			Out.WriteLine("variable {0} set to {1}", varname, value);
			Out.WriteLine();

			return CommandResultCode.Success;
		}

		public override IEnumerator<string> Complete(CommandDispatcher dispatcher, string partialCommand, string lastWord) {
			ISettingsHandler handler = Application as ISettingsHandler;
			return handler == null ? null : handler.Settings.Complete(partialCommand, lastWord);
		}

		public override string LongDescription {
			get {
				return "Sets variable with name <varname> to <value>. "
				       + "Variables are  expanded in any  command you issue on the "
				       + "commandline.  Variable expansion works like on the shell "
				       + "with the dollarsign. Both forms, $VARNAME and ${VARNAME}, "
				       + "are supported.  If the variable is  _not_  set, then the "
				       + "text is  left untouched.  So  if  there  is  no variable "
				       + "$VARNAME, then it is not replaced by an empty string but "
				       + "stays '$VARNAME'. This is because some scripts use wierd "
				       + "identifiers  containting  dollars  (esp. Oracle scripts) "
				       + "If you want to quote the dollarsign explicitly, write "
				       + "two dollars: $$FOO means $FOO";
			}
		}
	}
}