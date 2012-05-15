using System;

namespace Deveel.Console.Commands {
	public sealed class EchoCommand : Command {
		public override string ShortDescription {
			get { return "prompts the given arguments"; }
		}

		public override string Name {
			get { return "echo"; }
		}

		public override string[] Synopsis {
			get { return new string[] { "echo <whatever>" };}
		}

		public override string[] Aliases {
			get { return new string[] { "prompt" }; }
		}

		public override CommandResultCode Execute(IExecutionContext context, CommandArguments args) {
			String outStr = args.ToString();
			Application.Out.WriteLine(StripQuotes(outStr));
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

		public override String LongDescription {
			get { return "just echo the string given."; }
		}
	}
}