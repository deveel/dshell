using System;

namespace Deveel.Console.Commands {
	internal class SetContextPropertyCommand : SetPropertyCommand {
		public override string Name {
			get { return "set-context-property"; }
		}

		public override bool RequiresContext {
			get { return true; }
		}

		public override string GroupName {
			get { return "variables"; }
		}

		public override string[] Synopsis {
			get { return new string[] {"set-context-property <propert-name> <value>"}; }
		}

		public override string ShortDescription {
			get { return "sets the value of a property"; }
		}
	}
}