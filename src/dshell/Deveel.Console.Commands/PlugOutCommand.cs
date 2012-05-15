using System;

namespace Deveel.Console.Commands {
	internal class PlugOutCommand : Command {
		public override string GroupName {
			get { return "plugins"; }
		}

		public override string Name {
			get { return "plug-out"; }
		}

		public override string[] Synopsis {
			get { return new string[] { "plug-out <type-or-name>" }; }
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
			if (!plugins.HasPlugin(pluginType)) {
				Application.Error.WriteLine("unknown plugin '" + pluginType + "'");
				return CommandResultCode.ExecutionFailed;
			}
			if (!plugins.Unregister(pluginType))
				return CommandResultCode.ExecutionFailed;
			return CommandResultCode.Success;
		}
	}
}