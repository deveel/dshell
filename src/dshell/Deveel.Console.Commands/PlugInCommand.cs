using System;

namespace Deveel.Console.Commands {
	internal class PlugInCommand : Command {
		public override string Name {
			get { return "plug-in"; }
		}

		public override string[] Synopsis {
			get { return new string[] { "plug-in <type>" }; }
		}

		public override CommandResultCode Execute(IExecutionContext context, CommandArguments args) {
			if (!args.MoveNext()) 
				return CommandResultCode.SyntaxError;

			if (!(Application is IPluginHandler)) {
				Error.WriteLine("The application doesn't support plug-ins.");
				return CommandResultCode.ExecutionFailed;
			}

			ApplicationPlugins plugins = ((IPluginHandler) Application).Plugins;
			string pluginType = args.Current;
			if (plugins.HasPlugin(pluginType)) {
				Application.Error.WriteLine("plugin '" + pluginType + "' already loaded");
				return CommandResultCode.ExecutionFailed;
			}

			Command plugin;
			try {
				plugin = plugins.LoadPlugin(pluginType);
			} catch (Exception e) {
				Application.Error.WriteLine("couldn't load plugin: " + e.Message);
				return CommandResultCode.ExecutionFailed;
			}
			if (plugin != null) {
				plugins.Add(pluginType, plugin);
				Out.Write("adding command: ");
				Out.Write(plugin.Name);
				string[] aliases = plugin.Aliases;
				if (aliases.Length > 0) {
					Out.Write(" (");
					for (int i = 0; i < aliases.Length; ++i) {							
						Out.Write(aliases[i]);

						if (i < aliases.Length - 1)
							Out.Write(", ");
					}

					Out.Write(")");
				}

				Out.WriteLine();
			}

			return CommandResultCode.Success;
		}
	}
}