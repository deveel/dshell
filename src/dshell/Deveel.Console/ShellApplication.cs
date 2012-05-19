using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Deveel.Console.Commands;
using Deveel.Configuration;

namespace Deveel.Console {
	public abstract class ShellApplication : IApplicationContext {
		private OutputDevice output;
		private OutputDevice message;
		private InputDevice input;
		private readonly CommandDispatcher dispatcher;
		private readonly PropertyRegistry properties;
		private ConfigurationFile history;
		private readonly ApplicationSettings settings;
		private readonly ApplicationPlugins plugins;
		private readonly ApplicationInterruptionHandler interruptHandler;

		private ICommandSeparator separator;

		private bool running;
		private bool terminated;
		private bool shutdownCalled;
		private bool initialized;
		private volatile bool interrupted;
		private StringBuilder historyLine;

		private TextReader fileReader;
		//TODO: define a strategy to set this ...
		private bool hasTerminal = true;

		private string prompt;
//		private string emptyPrompt;

		private Options appOptions;

		private IExecutionContext activeContext;

		public event EventHandler Interrupted;
		public event EventHandler BeforeExit;

		protected ShellApplication() {
			output = new ConsoleOutputDevice(System.Console.Out, true);
			message = new ConsoleOutputDevice(System.Console.Error, false);
			input = new ConsoleInputDevice();
			dispatcher = new CommandDispatcher(this);
			if (this is ISettingsHandler)
				settings = new ApplicationSettings(this);
			if (this is IPropertyHandler)
				properties = new PropertyRegistry(this as IPropertyHandler);
			if (this is IPluginHandler)
				plugins = new ApplicationPlugins(this);
			interruptHandler = new ApplicationInterruptionHandler(this);
		}

		public void Dispose() {
			Exit(0);
		}
		
		public ApplicationInterruptionHandler Interruption {
			get { return interruptHandler; }
		}

		public ApplicationPlugins Plugins {
			get { return plugins; }
		}

		bool IExecutionContext.IsIsolated {
			get { return false; }
		}

		public bool IsRunning {
			get { return running; }
		}


		public IExecutionContext ActiveContext {
			get { return activeContext ?? this; }
		}

		public string PartialLine {
			get {
				StringBuilder line = new StringBuilder();
				if (historyLine != null)
					line.Append(historyLine.ToString());
				line.Append(Readline.LineBuffer);
				return line.ToString();
			}
		}
				
		public bool HadnlesPlugins {
			get { return this is IPluginHandler; }
		}

		public PropertyRegistry Properties {
			get { return properties; }
		}

		public bool IsInBatch {
			get { return dispatcher.IsInBatch; }
		}

		public bool HandlesProperties {
			get { return this is IPropertyHandler; }
		}

		public OutputDevice Out {
			get { return output; }
		}

		public OutputDevice Error {
			get { return message; }
		}
		
		public InputDevice Input {
			get { return input; }
		}

		public CommandDispatcher Commands {
			get { return dispatcher; }
		}

		protected bool HasPrompt {
			get { return !String.IsNullOrEmpty(prompt); }
		}

		protected string Prompt {
			get { return prompt; }
		}

		private bool IsInterruptable {
			get { return this is IInterruptable; }
		}

		public ApplicationSettings Settings {
			get { return settings; }
		}
		
		public bool HandlesSettings {
			get { return this is ISettingsHandler; }
		}

		private ICommandSeparator GetSeparator() {
			if (separator == null)
				separator = CreateSeparator();
			return separator;
		}

		public void SetPrompt(string text) {
			prompt = text;
//			StringBuilder tmp = new StringBuilder();
//			int emptyLength = prompt.Length;
//			for (int i = emptyLength; i > 0; --i) {
//				tmp.Append(' ');
//			}
//			emptyPrompt = tmp.ToString();

			if (output == null)
				output = new ConsoleOutputDevice(System.Console.Out, true);
			if (message == null)
				message = new ConsoleOutputDevice(System.Console.Error, false);
			if (input == null)
				input = new ConsoleInputDevice();

			if (output is ConsoleOutputDevice)
				(output as ConsoleOutputDevice).Prompt = prompt;
			if (input is ConsoleInputDevice)
				(input as ConsoleInputDevice).Prompt = prompt;
		}

		public void SetInputDevice(InputDevice device) {
			if (device == null) 
				throw new ArgumentNullException("device");

			input = device;
		}

		public void SetOutDevice(OutputDevice device) {
			if (device == null)
				throw new ArgumentNullException("device");
			if (device is ConsoleOutputDevice)
				(device as ConsoleOutputDevice).Prompt = prompt;

			output = device;
		}

		public void SetErrorDevice(OutputDevice device) {
			if (device == null)
				throw new ArgumentNullException("device");

			message = device;
		}
		
		protected void LoadHistory(Stream inputStream) {
			StreamReader reader = new StreamReader(inputStream);
			StringBuilder line = new StringBuilder();
			int c;
			do {
				while ((c = reader.Read()) >= 0 && c != '\n') {
					char ch = (char)c;
					if (ch == '\r')
						continue;

					if (ch == '\\') {
						line.Append((char)reader.Read());
					} else {
						line.Append(ch);
					}
				}
				if (line.Length > 0) {
					History.AddHistory(line.ToString());
					line.Length = 0;
				}
			} while (c >= 0);
			reader.Close();
		}

		protected void WriteHistory(Stream outputStream) {
			StreamWriter writer = new StreamWriter(outputStream, Encoding.UTF8);
			int len = History.Count;
			for (int i = 0; i < len; ++i) {
				String line = History.GetHistory(i);
				if (line == null)
					continue;

				line = EscapeHistoryLine(line);
				writer.WriteLine(line);
			}
			writer.Close();
		}

		protected void CreateHistoryFile(string fileName) {		
			history = new ConfigurationFile(fileName);
		}

		protected void WriteHistoryToFile() {
			if (history != null)
				history.Write(WriteHistory);
		}

		private static String EscapeHistoryLine(String s) {
			if (s.IndexOf('\\') >= 0 || s.IndexOf('\n') >= 0) {
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < s.Length; ++i) {
					char c = s[i];
					switch (c) {
						case '\\': 
							sb.Append("\\\\"); 
							break;
						case '\n': 
							sb.Append("\\\n"); 
							break;
						default: 
							sb.Append(c); 
							break;
					}
				}
				return sb.ToString();
			}

			return s;
		}

		private string previousHistoryLine;

		private void StoreHistoryLine() {
			string line = historyLine.ToString().Trim();
			if (!line.Equals(String.Empty) &&
				!line.Equals(previousHistoryLine)) {
				History.AddHistory(line);
				previousHistoryLine = line;
			}
		}

		private string ReadLineFromFile() {
			if (fileReader == null)
				fileReader = new StreamReader(System.Console.OpenStandardInput());

			string line = fileReader.ReadLine();
			if (line == null)
				throw new EndOfStreamException();

			return (line.Length == 0) ? null : line;
		}

		private void RegisterDefaults() {
			if (this is IPropertyHandler) {
				if (!Properties.HasProperty("echo-commands"))
					Properties.RegisterProperty("echo-commands", new EchoCommandProperty(this, dispatcher));
			}

			Commands.Register(typeof(ExitCommand));
			Commands.Register(typeof(HelpCommand));
			Commands.Register(typeof(SpoolCommand));
			Commands.Register(typeof(EchoCommand));

			// Commands.Register(typeof(AliasCommand));

			Commands.Register(typeof(SetCommand));
			Commands.Register(typeof(UnsetCommand));
			Commands.Register(typeof(VariablesCommand));

			if (properties != null) {
				Commands.Register(typeof(SetPropertyCommand));
				Commands.Register(typeof(ResetPropertyCommand));
				Commands.Register(typeof(SetContextPropertyCommand));
				Commands.Register(typeof(ResetContextPropertyCommand));
				Commands.Register(typeof(ShowPropertyCommand));
				Commands.Register(typeof(ShowContextPropertyCommand));
			}
		}

		protected virtual void OnRunning() {
		}
		
		private void Readline_Interrupt(object sender, EventArgs args) {
			Interruption.Interrupt();
		}

		protected ApplicationPlugins LoadPlugins(string  fileName) {
			if (this is IPluginHandler && File.Exists(fileName)) {
				StreamReader reader = new StreamReader(fileName);
				string line;
				while ((line = reader.ReadLine()) != null) {
					line = line.Trim();
					if (String.IsNullOrEmpty(line))
						continue;

					Command command;

					try {
						command = plugins.LoadPlugin(line);
					} catch(Exception) {
						Error.WriteLine("Cannot load the plugin '" + line + "'.");
						continue;
					}

					plugins.Add(line, command);
				}
			}

			return plugins;
		}
		
		protected void SavePlugins(string fileName) {
			if (this is IPluginHandler) {
				if (!File.Exists(fileName))
					File.Create(fileName);
				
				ConfigurationFile configFile = new ConfigurationFile(fileName);
				foreach(KeyValuePair<string, Command> plugin in plugins) {
					configFile.SetValue(plugin.Key, plugin.Value.GetType().FullName);
				}

				configFile.Save("Plugins");
			}
		}


		protected PropertyRegistry LoadProperties(string fileName) {
			if (this is IPropertyHandler && File.Exists(fileName)) {
				ConfigurationFile confFile = new ConfigurationFile(fileName);
				foreach(KeyValuePair<string, string> property in confFile.Properties) {
					properties.SetProperty(property.Key, property.Value);
				}
			}

			return properties;
		}

		protected ApplicationSettings LoadSettings(string fileName) {
			if (HandlesSettings && File.Exists(fileName)) {
				ConfigurationFile confFile = new ConfigurationFile(fileName);
				foreach(KeyValuePair<string, string> pair in confFile.Properties) {
					settings.SetVariable(pair.Key, pair.Value);
				}
			}
			return settings;
		}
		
		protected void SaveSettings(string fileName) {
			if (!HandlesSettings)
				return;
			
			if (!File.Exists(fileName))
				File.Create(fileName);
			
			IDictionary<string, string> toWrite = new Dictionary<string, string>();
			foreach (KeyValuePair<string, string> entry in settings.Variables)
				toWrite.Add(entry.Key, entry.Value);
			foreach (string variable in settings.SpecialVariables)
				toWrite.Remove(variable);

			ConfigurationFile configFile = new ConfigurationFile(fileName);
			foreach (KeyValuePair<string, string> entry in toWrite)
				configFile.SetValue(entry.Key, entry.Value);
			configFile.Save("Settings");
		}

		protected virtual bool OnTerminated() {
			return true;
		}

		protected virtual void OnInterrupted() {

		}

		protected virtual ICommandSeparator CreateSeparator() {
			return null;
		}

		protected virtual LineExecutionResultCode ExecuteLine(string line) {
			ICommandSeparator commandSeparator = GetSeparator();
			if (commandSeparator != null) {
				StringBuilder lineBuilder = new StringBuilder(line);
				lineBuilder.Append(Environment.NewLine);
				commandSeparator.Append(lineBuilder.ToString());

				LineExecutionResultCode resultCode = LineExecutionResultCode.Incomplete;

				while (commandSeparator.MoveNext()) {
					string completeCommand = commandSeparator.Current;
					if (HandlesSettings)
						completeCommand = Settings.Substitute(completeCommand);

					Command c = Commands.GetCommand(completeCommand);

					if (c == null) {
						commandSeparator.Consumed();
						// do not shadow successful executions with the 'line-empty'
						// message. Background is: when we consumed a command, that
						// is complete with a trailing ';', then the following newline
						// would be considered as empty command. So return only the
						// 'Empty', if we haven't got a succesfully executed line.
						if (resultCode != LineExecutionResultCode.Executed)
							resultCode = LineExecutionResultCode.Empty;
					} else if (!c.IsComplete(completeCommand)) {
						commandSeparator.Cont();
						resultCode = LineExecutionResultCode.Incomplete;
					} else {
						Execute(ActiveContext, completeCommand.Trim());

						commandSeparator.Consumed();

						resultCode = LineExecutionResultCode.Executed;
					}
				}

				return resultCode;
			}
			
			// if we don't have any separator, we assume it's a complete command
			Execute(ActiveContext, line);
			return LineExecutionResultCode.Executed;
		}

		public void Interrupt() {
			if (!IsInterruptable)
				throw new InvalidOperationException("The application is not interruptable.");

			interrupted = true;
		}

		public void Execute(IExecutionContext context, string commandText) {
			dispatcher.ExecuteCommand(context, commandText);
		}

		protected IExecutionContext SetActiveContext(IExecutionContext context) {
			activeContext = context;
			return ActiveContext;
		}

		protected virtual Options CreateOptions() {
			return null;
		}

		protected virtual void RegisterCommands() {
		}

		public void RegisterOptions(Options options) {
			if (running)
				throw new InvalidOperationException("The application is running.");

			Initialize();

			appOptions = options;

			foreach(Command command in dispatcher.RegisteredCommands) {
				command.RegisterOptions(options);
			}
		}

		public void HandleCommandLine(CommandLine commandLine) {
			if (running)
				throw new InvalidOperationException("The application is running.");

			Initialize();

			foreach(Command command in dispatcher.RegisteredCommands) {
				if (command.HandleCommandLine(commandLine))
					return;
			}
		}

		public void HandleCommandLine(string [] args, ICommandLineParser parser) {
			if (parser == null)
				throw new ArgumentNullException("parser");

			Initialize();

			parser.Options = appOptions;
			CommandLine commandLine = parser.Parse(args);
			if (commandLine.HasParsed)
				HandleCommandLine(commandLine);
		}

		public void HandleCommandLine(string[] args, ParseStyle parseStyle) {
			ICommandLineParser parser = Parser.Create(parseStyle);
			HandleCommandLine(args, parser);
		}

		public void HandleCommandLine(string[] args) {
			HandleCommandLine(args, ParseStyle.Gnu);
		}

		private void Initialize() {
			if (!initialized) {
				Readline.ControlCInterrupts = true;
				Readline.Interrupt += Readline_Interrupt;
				RegisterDefaults();
				RegisterCommands();

				appOptions = CreateOptions();

				initialized = true;
			}
		}

		public void Run(string[] args) {
			HandleCommandLine(args);
			Run();
		}

		public void Run() {
			Initialize();

			OnRunning();

			running = true;

			string cmdLine = null;
//			string displayPrompt = prompt;

			historyLine = new StringBuilder();

			try {
				while (!terminated) {
					interrupted = false;

					// a CTRL-C will not interrupt the current reading
					// thus it does not make much sense here to interrupt.
					// WORKAROUND: Write message in the Interrupt() method.
					// TODO: find out, if we can do something that behaves
					//       like a shell. This requires, that CTRL-C makes
					//       Readline.ReadLine() return.
					if (IsInterruptable)
						Interruption.Push(this as IInterruptable);

					try {
//						cmdLine = (hasTerminal) ? Readline.ReadLine(displayPrompt) : ReadLineFromFile();
						cmdLine = Input.ReadLine();
					} catch(EndOfStreamException) {
						// EOF on CTRL-D
						if (OnTerminated()) {
//							displayPrompt = prompt;
							Input.CompleteLine();
							continue;
						}

						break;
					} catch(Exception e) {
#if DEBUG
						System.Console.Error.WriteLine(e.Message);
						System.Console.Error.WriteLine(e.StackTrace);
#endif
					}

					if (IsInterruptable)
						Interruption.Reset();

					// anyone pressed CTRL-C
					if (interrupted) {
						if ((cmdLine == null || cmdLine.Trim().Length == 0) &&
						    historyLine.Length == 0) {
							terminated = true; // terminate if we press CTRL on empty line.
						}

						historyLine.Length = 0;

//						displayPrompt = prompt;
						Input.CompleteLine();
						continue;
					}

					if (cmdLine == null) {
						continue;
					}

					// if there is already some line in the history, then add
					// newline. But if the only thing we added was a delimiter (';'),
					// then this would be annoying.

					if (historyLine.Length > 0 &&
					    !cmdLine.Trim().Equals(dispatcher.CommandSeparator.ToString())) {
						historyLine.Append(Environment.NewLine);
					}

					historyLine.Append(cmdLine);

					LineExecutionResultCode lineExecState = ExecuteLine(cmdLine);						
					
					// displayPrompt = lineExecState == LineExecutionResultCode.Incomplete ? emptyPrompt : prompt;

					if (lineExecState != LineExecutionResultCode.Incomplete) {
						Input.CompleteLine();
						StoreHistoryLine();
						historyLine.Length = 0;
					}
				}

				if (IsInterruptable)
					Interruption.Reset();
			} catch(Exception e) {
#if DEBUG
				System.Console.Error.WriteLine(e.Message);
				System.Console.Error.WriteLine(e.StackTrace);
#endif
				Exit(1);
			} finally {
				running = false;
			}
		}

		protected virtual void OnShutdown() {
		}

		public void Shutdown() {
			Shutdown(true);
		}

		public void Shutdown(bool exit) {
			if (shutdownCalled)
				return;

			Interruption.Reset();

			try {
				if (dispatcher != null)
					dispatcher.Shutdown();

				WriteHistoryToFile();

				OnShutdown();
			} finally {
				shutdownCalled = true;
			}

			GC.Collect();
			GC.Collect();

			if (exit)
				Exit(0);
		}

		protected virtual void OnExit() {
		}

		private void OnBeforeExit() {
			if (BeforeExit != null)
				BeforeExit(this, EventArgs.Empty);
		}

		public void Exit(int code) {
			terminated = true;

			OnBeforeExit();
			OnExit();
			
			Environment.Exit(code);
		}

		#region EchoCommandProperty

		class EchoCommandProperty
			: BooleanPropertyHolder {
			private readonly ShellApplication app;
			private readonly CommandDispatcher dispatcher;
			private readonly CommandEventHandler handler;

			public EchoCommandProperty(ShellApplication app, CommandDispatcher dispatcher)
				: base(false) {
				this.app = app;
				this.dispatcher = dispatcher;
				handler = new CommandEventHandler(OnDispatcherExecuting);
			}

			public override string DefaultValue {
				get { return "off"; }
			}

			public override void OnBooleanPropertyChanged(bool echoCommands) {
				if (echoCommands) {
					dispatcher.CommandExecuting += handler;
				} else {
					dispatcher.CommandExecuting -= handler;
				}
			}

			void OnDispatcherExecuting(object sender, CommandEventArgs e) {
				app.Error.WriteLine(e.CommandText.Trim());
			}

			public override String ShortDescription {
				get { return "echo commands prior to execution."; }
			}
		}

		#endregion

		internal void OnInterrupt() {
			if (Interrupted != null)
				Interrupted(this, EventArgs.Empty);

			OnInterrupted();
		}
	}
}