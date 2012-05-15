using System;

namespace Deveel.Console.Commands {
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public sealed class CommandSynopsisAttribute : Attribute, ICommandAttribute {
		public CommandSynopsisAttribute(string text) {
			this.text = text;
		}

		private string text;

		public string Text {
			get { return text; }
			set { text = value; }
		}
	}
}