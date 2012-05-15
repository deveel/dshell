using System;

namespace Deveel.Console.Commands {
	/// <summary>
	/// The code returned after the execution of a <see cref="Command">command</see>
	/// from the method <see cref="Command.Execute"/>.
	/// </summary>
	public enum CommandResultCode {
		/// <summary>
		/// This code is returned is the execution of a command was
		/// successfully.
		/// </summary>
		Success = 0,

		/// <summary>
		/// The code returned if the command could not be executed because 
		/// of an syntax error.
		/// </summary>
		/// <remarks>
		/// If this code is returned, the synopsis of the command is shown.
		/// </remarks>
		SyntaxError = 1,

		/// <summary>
		/// The code returned if the command could not be executed because 
		/// of some problem, that is not a syntax error.
		/// </summary>
		ExecutionFailed = 2,
	}
}