using System;

namespace Deveel.Console.Commands {
	public interface IExecutable {
		CommandResultCode Execute(IExecutionContext context, CommandArguments args);
	}
}