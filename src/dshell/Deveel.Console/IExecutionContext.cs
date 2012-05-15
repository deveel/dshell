using System;

namespace Deveel.Console {
	public interface IExecutionContext : IDisposable {
		bool IsIsolated { get; }
	}
}