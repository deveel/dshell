using System;
using System.Collections.Generic;

namespace Deveel.Console {
	internal class ApplicationPropertyRegistry : PropertyRegistry {
		public ApplicationPropertyRegistry(ShellApplication context, ConfigurationFile config)
			: base(context) {
			this.config = config;
		}

		private bool dirty;
		private readonly ConfigurationFile config;

		public override void RegisterProperty(string name, PropertyHolder holder) {
			base.RegisterProperty(name, holder);
			dirty = true;
		}

		public override void SetProperty(string name, string value) {
			base.SetProperty(name, value);
			dirty = true;
		}

		public override void UnregisterProperty(string name) {
			base.UnregisterProperty(name);
			dirty = true;
		}

		internal void Save() {
			if (dirty) {
				config.ClearValues();

				foreach (KeyValuePair<string, PropertyHolder> entry in Properties)
					config.SetValue(entry.Key, entry.Value.Value);

				config.Save("Properties");
				
				dirty = false;
			}
		}
	}
}