using System;

namespace Deveel.Console.Commands {
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class CommandAttribute : Attribute, ICommandAttribute {
		public CommandAttribute(string name) {
			if (name == null)
				throw new ArgumentNullException("name");

			this.name = name;
		}

		private readonly string name;
		private bool requiresContext;
		private string shortDescription;

		public string CommandName {
			get { return name; }
		}

		public bool RequiresContext {
			get { return requiresContext; }
			set { requiresContext = value; }
		}

		public string ShortDescription {
			get { return shortDescription; }
			set { shortDescription = value; }
		}
	}
}