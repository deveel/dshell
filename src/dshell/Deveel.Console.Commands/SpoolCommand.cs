using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Deveel.Console.Commands {
	public sealed class SpoolCommand : Command {
		private Stack<OutputDevice> outStack;
		private Stack<OutputDevice> msgStack;

		public override string Name {
			get { return "spool"; }
		}

		public override string ShortDescription {
			get { return "logs the output to a file"; }
		}

		public override string[] Synopsis {
			get { return new string[] { "spool <filename> | off" }; }
		}

		#region Private Methods
		private static OutputDevice OpenStackedDevice(Stack<OutputDevice> stack, OutputDevice newOut) {
			OutputDevice origOut = stack.Peek();
			OutputDevice outDevice = new StackedDevice(origOut, newOut);
			stack.Push(outDevice);
			return outDevice;
		}

		private static OutputDevice CloseStackedDevice(Stack<OutputDevice> stack) {
			OutputDevice output = stack.Pop();
			output.Close();
			return stack.Peek();
		}

		private void OpenSpool(string filename) {
			// open file
			OutputDevice spool = new TextWriterOutputDevice(new StreamWriter(filename));
			Application.SetOutDevice(OpenStackedDevice(outStack, spool));
			Application.SetErrorDevice(OpenStackedDevice(msgStack, spool));
			Application.Error.WriteLine("-- open spool at " + DateTime.Now);
		}

		private bool CloseSpool() {
			if (outStack.Count == 1) {
				Application.Error.WriteLine("no open spool.");
				return false;
			}

			Application.Error.WriteLine("-- close spool at " + DateTime.Now);
			Application.SetOutDevice(CloseStackedDevice(outStack));
			Application.SetErrorDevice(CloseStackedDevice(msgStack));
			return true;
		}
		#endregion

		internal override void SetApplicationContext(IApplicationContext app) {
			outStack = new Stack<OutputDevice>();
			msgStack = new Stack<OutputDevice>();
			outStack.Push(app.Out);
			msgStack.Push(app.Error);
			base.SetApplicationContext(app);
		}

		#region Public Methods

		public override CommandResultCode Execute(IExecutionContext context, CommandArguments args) {
			if (!args.MoveNext())
				return CommandResultCode.SyntaxError;

			try {
				string arg = args.Current;
				if (arg.ToLower().Equals("off")) {
					CloseSpool();
				} else if (arg.Length > 0) {
					OpenSpool(arg);
				} else {
					return CommandResultCode.SyntaxError;
				}
			} catch (Exception e) {
				System.Console.Error.Write(e.StackTrace);
				return CommandResultCode.ExecutionFailed;
			}
			return CommandResultCode.Success;
		}

		public override string LongDescription {
			get {
				return "\tIf command is followed by a filename, opens a file\n"
				       + "\tand writes all subsequent output not only to the terminal\n"
				       + "\tbut as well to this file. With\n"
				       + "\t spool off\n"
				       + "\tspooling is stopped and the file is closed. The spool\n"
				       + "\tcommand works recursivly, i.e. you can open more than one \n"
				       + "\tfile, and you have to close each of them with 'spool off'\n";
			}
		}
		#endregion

		#region StackedDevice
		class StackedDevice : OutputDevice {
			public StackedDevice(OutputDevice a, OutputDevice b) {
				_a = a;
				_b = b;
				
				writer = new StackedWriter(a, b);
			}

			private OutputDevice _a;
			private OutputDevice _b;
			
			private readonly StackedWriter writer;
			
			protected override TextWriter Output {
				get { return writer; }
			}

			public override Encoding Encoding {
				get { return _a.Encoding; }
			}

			public override bool IsTerminal {
				get { return _a.IsTerminal && _b.IsTerminal; }
			}
			
			#region StackedWriter
			
			class StackedWriter : TextWriter {
				private readonly OutputDevice a;
				private readonly OutputDevice b;
				
				public StackedWriter(OutputDevice a, OutputDevice b) {
					this.a = a;
					this.b = b;
				}
				
				public override Encoding Encoding {
					get { return a.Encoding; }
				}
				
				public override void Write(char[] buffer, int index, int count) {
					a.Write(buffer, index, count);
					b.Write(buffer, index, count);
				}
				
				public override void Flush() {
					a.Flush();
					b.Flush();
				}
				
				public override void Close() {
					b.Close();
				}
			}
			
			#endregion
		}
		#endregion
	}
}