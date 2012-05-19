using System;

using Deveel.Console.Commands;

namespace Deveel.Console {
	/// <summary>
	/// Represents the context of a console application
	/// </summary>
	public interface IApplicationContext : IExecutionContext {
		/// <summary>
		/// Gets an instance of <see cref="OutputDevice"/> that is
		/// used to output standard messages
		/// </summary>
		OutputDevice Out { get; }

		/// <summary>
		/// Gets an instance of <see cref="OutputDevice"/> that is
		/// a safe sink for messages to be output.
		/// </summary>
		OutputDevice Error { get; }
		
		/// <summary>
		/// Gets the instance of <see cref="InputDevice"/> that is used
		/// by the application to read the user input.
		/// </summary>
		InputDevice Input { get; }

		/// <summary>
		/// Gets the instance of <see cref="CommandDispatcher"/> used by
		/// the application to retrieve and execute commands.
		/// </summary>
		CommandDispatcher Commands { get; }
		
		ApplicationInterruptionHandler Interruption { get; }

		string PartialLine { get; }

		IExecutionContext ActiveContext { get; }

		/// <summary>
		/// Gets a value indicating if the application is currently executing
		/// any command.
		/// </summary>
		bool IsRunning { get; }


		void SetOutDevice(OutputDevice device);

		void SetErrorDevice(OutputDevice device);

		void Execute(IExecutionContext context, string commandText);

		/// <summary>
		/// Exits the application with the given code.
		/// </summary>
		/// <param name="code">The code used to exit the application.</param>
		void Exit(int code);
	}
}