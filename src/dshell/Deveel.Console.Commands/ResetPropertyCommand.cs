using System;

namespace Deveel.Console.Commands {
	class ResetPropertyCommand : PropertyCommand {
		public override string Name {
			get { return "reset-property"; }
		}

		public override string GroupName {
			get { return "variables"; }
		}

		public override string ShortDescription {
			get { return "resets a registered application property"; }
		}

		public override string LongDescription {
			get {
				return "Reset the given global application property "
				       + "to its default value ";
			}
		}

		public override string[] Synopsis {
			get { return new string[] { "reset-property <property-name>" }; }
		}

		public override CommandResultCode Execute(IExecutionContext context, CommandArguments args) {
			if (args.Count != 1)
				return CommandResultCode.SyntaxError;

			PropertyRegistry properties = Properties;
			if (properties == null) {
				Application.Error.WriteLine("the current context does not support properties.");
				return CommandResultCode.ExecutionFailed;
			}

			if (!args.MoveNext())
				return CommandResultCode.SyntaxError;

			String name = args.Current;
			PropertyHolder holder = properties.GetProperty(name);
			if (holder == null)
				return CommandResultCode.ExecutionFailed;

			string defaultValue = holder.DefaultValue;

			try {
				properties.SetProperty(name, defaultValue);
			} catch (Exception) {
				Application.Error.WriteLine("setting to default '" + defaultValue + "' failed.");
				return CommandResultCode.ExecutionFailed;
			}
			return CommandResultCode.Success;
		}
	}
}