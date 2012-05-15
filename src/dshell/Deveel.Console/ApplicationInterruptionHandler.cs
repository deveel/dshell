using System;
using System.Collections.Generic;

namespace Deveel.Console {
	public sealed class ApplicationInterruptionHandler {
		private readonly IApplicationContext application;
		
		public ApplicationInterruptionHandler(IApplicationContext application) {
			this.application = application;
			once = false;
			interruptStack = new Stack<IInterruptable>();
		}
		
		private bool once;
		private readonly Stack<IInterruptable> interruptStack;

		public IApplicationContext Application {
			get { return application; }
		}
		
		internal void Interrupt() {
			if (interruptStack.Count > 0) {
				IInterruptable toInterrupt = interruptStack.Peek();
				toInterrupt.Interrupt();
			} else {
				if (application is ShellApplication)
					((ShellApplication) application).OnInterrupt();
				System.Console.Error.WriteLine("[Ctrl-C ; interrupted]");
				Environment.Exit(1);
			}
		}
		
		internal void Push(IInterruptable interruptable) {
			interruptStack.Push(interruptable);
		}

		internal void Pop() {
			once = false;
			interruptStack.Pop();
		}

		internal void Reset() {
			once = true;
			interruptStack.Clear();
		}
	}
}