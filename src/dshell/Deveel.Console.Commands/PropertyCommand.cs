using System;
using System.Collections.Generic;

namespace Deveel.Console.Commands {
	abstract class PropertyCommand : Command {
		protected virtual PropertyRegistry Properties {
			get {
				IPropertyHandler handler = Application as IPropertyHandler;
				return handler == null ? null : handler.Properties;
			}
		}

		public override IEnumerator<string> Complete(CommandDispatcher dispatcher, string partialCommand, string lastWord) {
			PropertyRegistry properties = Properties;
			return properties == null ? null : properties.Complete(Name, partialCommand, lastWord);
		}
	}
}