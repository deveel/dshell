using System;
using System.Collections;

namespace Deveel.Console {
	public sealed class SignalInterruptHandler : SystemSignalHandler {
		#region ctor
		public SignalInterruptHandler()
			: base(Signals.CtrlC) {
			once = false;
			interruptStack = new Stack();
		}
		#endregion

		#region Fields
		private bool once;
		private readonly Stack interruptStack;
		private bool ctrlCBackup;
		#endregion

		#region Properties

		public static SignalInterruptHandler Current {
			get { return GetInstalledHandler(Signals.CtrlC) as SignalInterruptHandler; }
		}

		#endregion

		#region Protected Methods

		protected override void OnInstall() {
//			ctrlCBackup = System.Console.TreatControlCAsInput;
//			System.Console.TreatControlCAsInput = false;
			//Readline.ControlCIsEOF = true;
		}

		protected override void OnSignal() {
			if (once)
				// got the interrupt more than once. May happen if you press
				// Ctrl-C multiple times .. or with broken thread lib on Linux.
				return;

			once = true;
			if (interruptStack.Count > 0) {
				IInterruptable toInterrupt = (IInterruptable)interruptStack.Peek();
				toInterrupt.Interrupt();
			} else {
				System.Console.Error.WriteLine("[Ctrl-C ; interrupted]");
				Environment.Exit(1);
			}
		}

		protected override void OnUnintall() {
//			System.Console.TreatControlCAsInput = ctrlCBackup;
		}

		#endregion

		#region Public Methods

		public void Push(IInterruptable interruptable) {
			interruptStack.Push(interruptable);
		}

		public void Pop() {
			once = false;
			interruptStack.Pop();
		}

		public void Reset() {
			once = true;
			interruptStack.Clear();
		}

		#endregion
	}
}