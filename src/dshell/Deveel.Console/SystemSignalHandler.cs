using System;
using System.Collections;

#if MONO
using Mono.Unix.Native;
#else
using System.Runtime.InteropServices;
#endif

namespace Deveel.Console {
	public abstract class SystemSignalHandler {
		#region ctor
		protected SystemSignalHandler(Signals signal) {
			this.signal = signal;
			signum = GetSignum(signal);

#if MONO
			signalHandler = new SignalHandler(OnSignal);
#else
			signalHandler = new EventHandler(OnSignal);
#endif
		}

		static SystemSignalHandler() {
			installedHandlers = new Hashtable();
		}
		#endregion

		#region Fields

		private readonly Signals signal;
#if MONO
		private readonly SignalHandler signalHandler;
		private readonly Signum signum;
#else
		private readonly EventHandler signalHandler;
		private readonly CtrlType signum;
#endif

		private static readonly Hashtable installedHandlers;

		#endregion

		#region Properties
		public Signals Signal {
			get { return signal; }
		}
		#endregion

		#region Protected Methods

		protected virtual void OnInstall() {
		}

		protected abstract void OnSignal();

		protected virtual void OnUnintall() {
		}

		#endregion

		#region Private Methods

#if !MONO
		[DllImport("Kernel32")]
		private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);
#endif

		private void Install() {
#if MONO
			Stdlib.Signal(signum, signalHandler);
#else
			SetConsoleCtrlHandler(signalHandler, true);
#endif
			OnInstall();
		}

		private void Uninstall() {
#if MONO
			//TODO:
#else
			if (SetConsoleCtrlHandler(signalHandler, false))
				OnUnintall();
#endif
		}

#if MONO
		private void OnSignal(int signal) {
			if (Stdlib.ToSignum(signal) == (Signum)signum)
				OnSignal();
		}
#else
		private bool OnSignal(CtrlType sig) {
			if (sig == signum) {
				try {
					OnSignal();
				} catch(Exception) {
					return false;
				}
			}

			return true;
		}
#endif
		#endregion

		public static void Install(Type type) {
			Install(type, new object[0]);
		}

		public static void Install(Type type, params object[] args) {
			if (type == null)
				throw new ArgumentNullException("type");

			if (!typeof(SystemSignalHandler).IsAssignableFrom(type))
				throw new ArgumentException("The specified type '" + type + "' is not assignable from SystemSignalHandler.");

			SystemSignalHandler handler = (SystemSignalHandler)Activator.CreateInstance(type, args);

			Install(handler);
		}

		public static void Install(SystemSignalHandler handler) {
			if (handler == null)
				throw new ArgumentNullException("handler");

			Signals signum = handler.Signal;
			if (installedHandlers.ContainsKey(signum))
				throw new InvalidOperationException("An handler for the system signal " + signum + " is already installed.");

			handler.Install();

			installedHandlers.Add(signum, handler);
		}

		public static SystemSignalHandler Install(System.EventHandler handler, Signals signum) {
			DelegateSignalHandler signalHandler = new DelegateSignalHandler(signum, handler);
			Install(signalHandler);
			return signalHandler;
		}

		public static SystemSignalHandler GetInstalledHandler(Signals signum) {
			return installedHandlers[signum] as SystemSignalHandler;
		}

		public static bool Uninstall(SystemSignalHandler handler) {
			if (handler == null)
				throw new ArgumentNullException("handler");

			return Uninstall(handler.Signal);
		}

		public static bool Uninstall(Signals signum) {
			SystemSignalHandler handler = installedHandlers[signum] as SystemSignalHandler;
			if (handler == null)
				return false;

			handler.Uninstall();
			return true;
		}

#if MONO
		private static Signum GetSignum(Signals signal) {
			//TODO:
		}
#else
		private static CtrlType GetSignum(Signals signal) {
			switch (signal) {
				case Signals.CtrlC:
					return CtrlType.CTRL_C_EVENT;
				case Signals.CtrlBreak:
					return CtrlType.CTRL_BREAK_EVENT;
				case Signals.CtrlShutdown:
					return CtrlType.CTRL_CLOSE_EVENT;
				default:
					throw new NotSupportedException();
			}
		}
#endif

#if !MONO
		private delegate bool EventHandler(CtrlType sig);

		enum CtrlType {
			CTRL_C_EVENT = 0,
			CTRL_BREAK_EVENT = 1,
			CTRL_CLOSE_EVENT = 2,
			CTRL_LOGOFF_EVENT = 5,
			CTRL_SHUTDOWN_EVENT = 6
		}
#endif

		#region DelegateSignalHandler

		private class DelegateSignalHandler : SystemSignalHandler {
			public DelegateSignalHandler(Signals signal, System.EventHandler handler) 
				: base(signal) {
				this.handler = handler;
			}

			private readonly System.EventHandler handler;

			protected override void OnSignal() {
				if (handler != null)
					handler(this, EventArgs.Empty);
			}
		}

		#endregion
	}
}