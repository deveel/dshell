using System;

namespace Deveel.Console.Commands {
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public sealed class CommandAliasAttribute : Attribute, ICommandAttribute {
		private readonly string alias;

		public CommandAliasAttribute(string @alias) {
			this.alias = alias;
		}

		public string Alias {
			get { return alias; }
		}
	}
}