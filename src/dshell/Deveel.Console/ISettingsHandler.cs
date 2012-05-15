using System;

namespace Deveel.Console {
	public interface ISettingsHandler {
		ApplicationSettings Settings { get; }
	}
}