using System;

namespace Deveel.Console.Commands {
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class CommandCompletionAttribute : Attribute, ICommandAttribute {
		public CommandCompletionAttribute(bool completeCommand) {
			this.completeCommand = completeCommand;
		}

		private readonly bool completeCommand;

		public bool CompleteCommand {
			get { return completeCommand; }
		}
	}
}