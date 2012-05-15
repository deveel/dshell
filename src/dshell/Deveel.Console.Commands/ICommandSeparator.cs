using System;
using System.Collections.Generic;

namespace Deveel.Console.Commands {
	public interface ICommandSeparator : IEnumerator<string> {
		void Append(string line);

		void Cont();

		void Consumed();
	}
}