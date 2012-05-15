using System;

namespace Deveel.Console {
	public interface IPluginHandler {
		ApplicationPlugins Plugins { get; }
	}
}