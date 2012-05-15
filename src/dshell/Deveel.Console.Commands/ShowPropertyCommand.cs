using System;
using System.Collections.Generic;

namespace Deveel.Console.Commands {
	class ShowPropertyCommand : PropertyCommand {
		private static ColumnDesign[] ProperiesColumns;

		static ShowPropertyCommand() {
			ProperiesColumns = new ColumnDesign[3];
			ProperiesColumns[0] = new ColumnDesign("Name");
			ProperiesColumns[1] = new ColumnDesign("Value");
			ProperiesColumns[2] = new ColumnDesign("Description");
		}

		public override string Name {
			get { return "show-property"; }
		}

		public override string GroupName {
			get { return "properties"; }
		}

		public override string ShortDescription {
			get { return "lists all the properties"; }
		}

		public override string[] Synopsis {
			get { return new string[] { "show-property [ <property-name> ]" }; }
		}

		public override CommandResultCode Execute(IExecutionContext context, CommandArguments args) {
			PropertyRegistry properties = Properties;
			if (properties == null) {
				Application.Error.WriteLine("the current context does not support properties.");
				return CommandResultCode.ExecutionFailed;
			}

			if (args.MoveNext()) {
				string name = args.Current;
				PropertyHolder holder = properties.GetProperty(name);
				if (holder == null)
					return CommandResultCode.ExecutionFailed;

				PrintDescription(name, holder, Application.Error);
				return CommandResultCode.Success;
			}

			ProperiesColumns[0].ResetWidth();
			ProperiesColumns[1].ResetWidth();
			TableRenderer table = new TableRenderer(ProperiesColumns, Application.Out);
			foreach(KeyValuePair<string, PropertyHolder> entry in properties) {
				ColumnValue[] row = new ColumnValue[3];
				PropertyHolder holder = entry.Value;
				row[0] = new ColumnValue(entry.Key);
				row[1] = new ColumnValue(holder.Value);
				row[2] = new ColumnValue(holder.ShortDescription);
				table.AddRow(row);
			}
			table.CloseTable();
			return CommandResultCode.Success;
		}

		private static void PrintDescription(String propName, PropertyHolder prop, OutputDevice output) {
			if (prop.ShortDescription != null) {
				output.WriteBold("PROPERTY");
				output.WriteLine();
				output.WriteLine("\t" + propName + " : " + prop.ShortDescription);
				output.WriteLine();
			}

			String desc = prop.LongDescription;
			if (desc != null) {
				output.WriteBold("DESCRIPTION");
				output.WriteLine();
				output.WriteLine(desc);
			} else {
				output.WriteLine("no detailed help for '" + propName + "'");
			}
		}
	}
}