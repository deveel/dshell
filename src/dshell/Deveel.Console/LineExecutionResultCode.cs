using System;

namespace Deveel.Console {
	/// <summary>
	/// Defines the result code for the execution of a single
	/// input line.
	/// </summary>
	public enum LineExecutionResultCode {
		/// <summary>
		/// The line parsed has been executed successfully.
		/// </summary>
		Executed = 1,

		/// <summary>
		/// The line inserted was empty (zero-length).
		/// </summary>
		Empty = 2,

		/// <summary>
		/// The line parsed is incomplete.
		/// </summary>
		Incomplete = 3
	}
}