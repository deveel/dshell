using System;

namespace Deveel.Console.Commands {
	class SetPropertyCommand : PropertyCommand {
		public override string Name {
			get { return "set-property"; }
		}

		public override string GroupName {
			get { return "properties"; }
		}

		public override string ShortDescription {
			get { return "sets the value of a property"; }
		}

		public override string[] Synopsis {
			get { return new string[] { "set-property <propert-name> <value>" }; }
		}

		public override CommandResultCode Execute(IExecutionContext context, CommandArguments args) {
			if (!args.MoveNext())
				return CommandResultCode.SyntaxError;

			string varname = args.Current;
			string[] newArgs = new string[args.Count-1];
			while (args.MoveNext()) {
				newArgs[args.CurrentIndex - 1] = args.Current;
			}
			
			string param = String.Join(" ", newArgs);
			int pos = 0;
			int paramLength = param.Length;
			// skip whitespace after 'set'
			while (pos < paramLength
				   && Char.IsWhiteSpace(param[pos])) {
				++pos;
			}
			// skip non-whitespace after 'set  ': variable name
			while (pos < paramLength
				   && !Char.IsWhiteSpace(param[pos])) {
				++pos;
			}
			// skip whitespace before vlue..
			while (pos < paramLength
				   && Char.IsWhiteSpace(param[pos])) {
				++pos;
			}
			String value = param.Substring(pos);
			if (value.StartsWith("\"") && value.EndsWith("\"")) {
				value = value.Substring(1, value.Length - 2);
			} else if (value.StartsWith("\'") && value.EndsWith("\'")) {
				value = value.Substring(1, value.Length - 2);
			}

			try {
				PropertyRegistry properties = Properties;
				if (properties == null)
					throw new Exception("The current context doesn't support properties.");

				properties.SetProperty(varname, value);
			} catch (Exception e) {
				Application.Error.WriteLine(e.Message);
				return CommandResultCode.ExecutionFailed;
			}
			return CommandResultCode.Success;
		}
	}
}