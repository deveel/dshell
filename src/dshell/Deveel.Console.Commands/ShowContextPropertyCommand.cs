using System;

namespace Deveel.Console.Commands {
	internal class ShowContextPropertyCommand : ShowPropertyCommand {
		public override string GroupName {
			get { return "properties"; }
		}

		public override string Name {
			get { return "show-context-property"; }
		}

		public override string ShortDescription {
			get { return "lists all the properties in a context"; }
		}

		public override string[] Synopsis {
			get { return new string[] { "show-context-property [ <property-name> ]" }; }
		}

		public override bool RequiresContext {
			get { return true; }
		}

		protected override PropertyRegistry Properties {
			get {
				IPropertyHandler handler = Application.ActiveContext as IPropertyHandler;
				return handler == null ? null : handler.Properties;
			}
		}
	}
}