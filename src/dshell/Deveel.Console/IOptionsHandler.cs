using System;

using Deveel.Configuration;

namespace Deveel.Console {
	public interface IOptionsHandler {
		void RegisterOptions(Options options);

		bool HandleCommandLine(CommandLine commandLine);
	}
}