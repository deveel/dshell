using System;
using System.Collections.Generic;

namespace Deveel.Console.Commands {
	internal class ShowCommand : Command {
		public override string Name {
			get { return "show"; }
		}

		public override string ShortDescription {
			get { return "shows information about a given set."; }
		}

		public override bool CommandCompletion {
			get { return true; }
		}

		public override CommandResultCode Execute(IExecutionContext context, CommandArguments args) {
			IInformationProvider provider = Application as IInformationProvider;
			if (provider == null) {
				Error.WriteLine("The current context does not support information.");
				return CommandResultCode.ExecutionFailed;
			}

			if (!args.MoveNext())
				return CommandResultCode.SyntaxError;

			string infoName = args.Current;
			if (!provider.IsInfoSupported(infoName)) {
				Error.WriteLine("Information " + infoName + " is not supported by the current context.");
				return CommandResultCode.ExecutionFailed;
			}

			ColumnDesign[] columns = provider.GetColumns(infoName);
			for (int i = 0; i < columns.Length; i++)
				columns[i].ResetWidth();

			TableRenderer renderer = new TableRenderer(columns, Out);
			// TODO: make it configurable ...
			renderer.EnableHeader = true;
			renderer.EnableFooter = true;

			IList<ColumnValue[]> values = provider.GetValues(infoName);
			for (int i = 0; i < values.Count; i++) {
				ColumnValue[] rowValues = values[i];
				renderer.AddRow(rowValues);
			}

			renderer.Flush();
			renderer.CloseTable();
			return CommandResultCode.Success;
		}

		public override IEnumerator<string> Complete(CommandDispatcher dispatcher, string partialCommand, string lastWord) {
			return base.Complete(dispatcher, partialCommand, lastWord);
		}
	}
}