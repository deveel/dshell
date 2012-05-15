using System;

namespace Deveel.Console.Commands {
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class CommandDesctiprionAttribute : Attribute, ICommandAttribute {
		public CommandDesctiprionAttribute(string value, DescriptionSource source) {
			this.value = value;
			this.source = source;
		}

		public CommandDesctiprionAttribute(string value)
			: this(value, DescriptionSource.Direct) {
		}

		private readonly string value;
		private DescriptionSource source;

		public DescriptionSource Source {
			get { return source; }
			set { source = value; }
		}

		public string Value {
			get { return value; }
		}
	}
}