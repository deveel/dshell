using System;
using System.Collections.Generic;

namespace Deveel.Console.Commands {
	internal class VariablesCommand : Command {
		private static readonly ColumnDesign[] VarColumns;

		static VariablesCommand() {
			VarColumns = new ColumnDesign[2];
			VarColumns[0] = new ColumnDesign("Variable");
			VarColumns[1] = new ColumnDesign("Value");
		}

		public override string Name {
			get { return "variables"; }
		}

		public override string GroupName {
			get { return "variables"; }
		}

		public override string ShortDescription {
			get { return "shows a lit of all variables registered in the application."; }
		}

		public override CommandResultCode Execute(IExecutionContext context, CommandArguments args) {
			ISettingsHandler handler = Application as ISettingsHandler;
			if (handler == null) {
				Error.WriteLine("The application doesn't support settings.");
				return CommandResultCode.ExecutionFailed;
			}

			if (args.MoveNext())
				return CommandResultCode.SyntaxError;

			VarColumns[0].ResetWidth();
			VarColumns[1].ResetWidth();

			TableRenderer table = new TableRenderer(VarColumns, Out);
			table.EnableHeader = true;
			table.EnableFooter = true;
			foreach(KeyValuePair<string, string> setting in handler.Settings) {
				if (setting.Key == ApplicationSettings.SpecialLastCommand)
					continue;

				ColumnValue[] row = new ColumnValue[4];
				row[0] = new ColumnValue(setting.Key);
				row[1] = new ColumnValue(setting.Value);
				table.AddRow(row);
			}

			table.CloseTable();
			Error.WriteLine();

			return CommandResultCode.Success;
		}
	}
}