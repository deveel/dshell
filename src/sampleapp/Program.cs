using System;
using System.Collections.Generic;

using Deveel.Configuration;
using Deveel.Console.Commands;

namespace Deveel.Console {
	public sealed class TestApplication : ShellApplication, IInterruptable {
		private string appDir;
		
		private void Configure() {
		}
		
		[STAThread]
		static void Main(string[] args) {
			TestApplication application = new TestApplication();
			application.Commands.Register(new CompletionExampleCommand());
			application.Commands.Register(typeof(PlainAskCommand));
			application.Commands.Register(typeof(VerticalAskCommand));
			application.Commands.Register(typeof (LongDescriptiveCommand));

			application.HandleCommandLine(args);
			application.SetPrompt("testapp> ");
			application.Run();
		}

		protected override Options CreateOptions() {
			Options options = new Options();
			options.AddOption("file", false, "Indicates the application will store data into files.");
			options.AddOption("dir", true, "The application directory.");
			return options;
		}
		
		protected override void RegisterCommands() {
			Commands.Register(typeof(ConfigureCommand));
		}
		
		private class ConfigureCommand : Command {
			public override string Name {
				get { return "configure"; }
			}
			
			public override CommandResultCode Execute(IExecutionContext context, CommandArguments args) {
				if (args.MoveNext())
					((TestApplication)Application).appDir = args.Current;
				
				TestApplication app = (TestApplication)Application;
				app.Configure();
				return CommandResultCode.Success;
			}

			public override void RegisterOptions(Options options) {
				options.AddOption("f", "file", true, "The file");
				options.AddOption("d", "dir", true, "The directory.");
			}
			
			public override bool HandleCommandLine(CommandLine commandLine) {
				if (commandLine.HasOption("file")) {
					string dir = commandLine.GetOptionValue("dir");
					if (String.IsNullOrEmpty(dir))
						dir = Environment.CurrentDirectory;
					
					((TestApplication)Application).appDir = dir;
					return true;
				}
				
				return false;
			}
		}
		
		private class CompletionExampleCommand : Command {
			private static readonly string[] alternatives = new string[] { "the", "quick", "brown", "fox", 
				"jumped", "over", "the", "lazy", "dog" };
			
			public override string Name {
				get { return "complete";}
			}
			
			public override bool CommandCompletion {
				get { return true; }
			}
						
			public override IEnumerator<string> Complete(CommandDispatcher dispatcher, string partialCommand, string lastWord) {
				List<string> list = new List<string>();
				for(int i = 0; i < alternatives.Length; i++) {
					string s = alternatives[i];
					if (s.StartsWith(lastWord))
						list.Add(s);
				}
				
				return list.GetEnumerator();
			}
			
			public override CommandResultCode Execute(IExecutionContext context, CommandArguments args) {
				return CommandResultCode.Success;
			}
		}

		private class PlainAskCommand : Command {
			public override string Name {
				get { return "ask"; }
			}

			private bool Ask(Question question, out bool valid) {
				Answer answer = question.Ask(Application, false);
				if (!answer.IsValid) {
					valid = false;
					return false;
				}

				valid = true;

				if ((string)answer.SelectedValue == "yes")
					return true;

				return false;
			}

			public override CommandResultCode Execute(IExecutionContext context, CommandArguments args) {
				Question question = new Question("Do you want to continue?", new object[] { "yes", "no", "maybe" }, 2);
				bool valid;
				while (Ask(question, out valid))
					continue;

				if (!valid)
					return CommandResultCode.ExecutionFailed;

				return CommandResultCode.Success;
			}
		}

		private class VerticalAskCommand : Command {
			public override string Name {
				get { return "vask"; }
			}

			private bool Ask(Question question, out bool valid) {
				Answer answer = question.Ask(Application, true);
				if (!answer.IsValid) {
					valid = false;
					return false;
				}

				valid = true;

				if ((string)answer.SelectedValue == "yes")
					return true;

				return false;
			}

			public override CommandResultCode Execute(IExecutionContext context, CommandArguments args) {
				Question question = new Question("Do you want to continue?", new object[] { "yes", "no", "maybe" }, 2);
				bool valid;
				while (Ask(question, out valid))
					continue;

				if (!valid)
					return CommandResultCode.ExecutionFailed;

				return CommandResultCode.Success;
			}
		}

		private class LongDescriptiveCommand : Command {
			public override string Name {
				get { return "long-description"; }
			}

			public override string LongDescription {
				get {
					return
						"This command has a long description that is used to test the maximum line settings of the output device. " +
						"We go on for some more text, to check that the help command really folds the description, accordingly to " +
						"the maximum allowed line\n" +
						"Additionally, let's test some carriage return to see if this is supported well by the system.";
				}
			}

			public override CommandResultCode Execute(IExecutionContext context, CommandArguments args) {
				return CommandResultCode.Success;
			}
		}
	}
}