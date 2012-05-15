using System;

namespace Deveel.Console.Commands {
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class CommandGroupAttribute : Attribute, ICommandAttribute {
		private readonly string groupName;

		public CommandGroupAttribute(string groupName) {
			this.groupName = groupName;
		}

		public string GroupName {
			get { return groupName; }
		}
	}
}