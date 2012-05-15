using System;

namespace Deveel.Console.Commands {
	public delegate void CommandEventHandler(object sender, CommandEventArgs e);

	public sealed class CommandEventArgs : EventArgs {
		internal CommandEventArgs(string commandText) {
			this.commandText = commandText;
		}

		internal CommandEventArgs(string commandText, CommandResultCode resultcode) {
			this.commandText = commandText;
			this.resultcode = resultcode;
		}

		private readonly string commandText;
		private readonly CommandResultCode resultcode;

		public string CommandText {
			get { return commandText; }
		}

		public CommandResultCode ResultCode {
			get { return resultcode; }
		}
	}
}