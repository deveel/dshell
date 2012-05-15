using System;

using Deveel.Console.Commands;

namespace Deveel.Console {
	public interface IApplicationContext : IExecutionContext {
		OutputDevice Out { get; }

		OutputDevice Error { get; }
		
		InputDevice Input { get; }

		CommandDispatcher Commands { get; }
		
		ApplicationInterruptionHandler Interruption { get; }

		string PartialLine { get; }

		IExecutionContext ActiveContext { get; }

		bool IsRunning { get; }


		void SetOutDevice(OutputDevice device);

		void SetErrorDevice(OutputDevice device);

		void Execute(IExecutionContext context, string commandText);

		void Exit(int code);
	}
}