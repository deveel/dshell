using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Deveel.Console.Commands;

namespace Deveel.Console {
	public sealed class ApplicationPlugins : IEnumerable<KeyValuePair<string, Command>> {
		public ApplicationPlugins(IApplicationContext application) {
			this.application = application;
			plugins = new SortedDictionary<string, Command>();
		}

		private readonly IApplicationContext application;
		private readonly SortedDictionary<string, Command> plugins;

		internal Command LoadPlugin(string typeName) {
			Type pluginType;

			try {
				pluginType = Type.GetType(typeName, true, true);
			} catch(Exception) {
				throw new ApplicationException("Unable to find the plugin type '" + typeName + "'.");
			}

			return application.Commands.Register(pluginType);
		}

		internal void Add(string typeName, Command command) {
			plugins.Add(typeName, command);
		}

		public bool HasPlugin(string typeName) {
			return plugins.ContainsKey(typeName);
		}

		public bool Unregister(string typeName) {
			Command c;
			if (!plugins.TryGetValue(typeName, out c))
				return false;

			application.Commands.Unregister(c);
			return true;
		}
		
		public IEnumerator<KeyValuePair<string, Command>> GetEnumerator() {
			return plugins.GetEnumerator();
		}
		
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}