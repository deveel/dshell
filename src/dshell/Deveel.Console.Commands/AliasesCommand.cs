using System;
using System.Collections.Generic;

namespace Deveel.Console.Commands {
	internal class AliasesCommand : Command {
		private static readonly ColumnDesign[] Columns;

		static AliasesCommand() {
			Columns = new ColumnDesign[2];
			Columns[0] = new ColumnDesign("Alias");
			Columns[1] = new ColumnDesign("Execute Command");
		}

		public override string Name {
			get { return "aliases"; }
		}

		public override string GroupName {
			get { return "aliases"; }
		}

		public override string ShortDescription {
			get { return "lists all the aliases"; }
		}

		public override CommandResultCode Execute(IExecutionContext context, CommandArguments args) {
			Columns[0].ResetWidth();
			Columns[1].ResetWidth();
			TableRenderer table = new TableRenderer(Columns, Out);
			foreach(KeyValuePair<string, string> alias in Application.Commands.Aliases) {
				ColumnValue[] row = new ColumnValue[2];
				row[0] = new ColumnValue(alias.Key);
				row[1] = new ColumnValue(alias.Value);
				table.AddRow(row);
			}
			table.CloseTable();
			return CommandResultCode.Success;
		}
	}
}