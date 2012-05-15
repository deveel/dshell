using System;

namespace Deveel.Console.Commands {
	internal class ResetContextPropertyCommand : ResetPropertyCommand {
		public override string GroupName {
			get { return "variables"; }
		}

		public override bool RequiresContext {
			get { return true; }
		}

		public override string Name {
			get { return "reset-context-property"; }
		}

		public override string[] Synopsis {
			get { return new string[] { "reset-context-property <property-name>" }; }
		}

		public override string ShortDescription {
			get { return "resets a registered property in a context"; }
		}

		protected override PropertyRegistry Properties {
			get {
				IPropertyHandler handler = Application.ActiveContext as IPropertyHandler;
				return handler == null ? null : handler.Properties;
			}
		}
	}
}