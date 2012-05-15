using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Deveel.Console {
	internal class ConsoleOutputDevice : OutputDevice {
		public ConsoleOutputDevice(TextWriter std, bool isOut) {
			this.std = std;
#if !MONO
			if (isOut) {
				stdHandle = GetStdHandle(-11);
			}
#endif
		}

		private string prompt;
		private readonly TextWriter std;
#if !MONO
		private IntPtr stdHandle;
#endif

		public override Encoding Encoding {
			get { return std.Encoding; }
		}
		
		protected override TextWriter Output {
			get { return std; }
		}

		public override int LineMaxWidth {
			get { return System.Console.WindowWidth; }
		}

		protected override void AttributeBold() {
#if !MONO
			SetConsoleTextAttribute(stdHandle, CharacterAttributes.FOREGROUND_INTENSITY);
#endif
		}

		protected override void AttributeGrey() {
			System.Console.ForegroundColor = ConsoleColor.Gray;
		}

		protected override void AttributeReset() {
			System.Console.ResetColor();
		}

		protected override char[] OnNewLine() {
			return !String.IsNullOrEmpty(prompt) ? prompt.ToCharArray() : new char[0];
		}

		internal string Prompt {
			get { return prompt; }
			set { prompt = value; }
		}

		//TODO:
		public override bool IsTerminal {
			get { return true; }
		}

#if !MONO
		[DllImport("kernel32.dll")]
		private static extern IntPtr GetStdHandle(int nStdHandle);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool SetConsoleTextAttribute(IntPtr hConsoleOutput, CharacterAttributes wAttributes);

		private enum CharacterAttributes {
			FOREGROUND_BLUE = 0x0001,
			FOREGROUND_GREEN = 0x0002,
			FOREGROUND_RED = 0x0004,
			FOREGROUND_INTENSITY = 0x0008,
			BACKGROUND_BLUE = 0x0010,
			BACKGROUND_GREEN = 0x0020,
			BACKGROUND_RED = 0x0040,
			BACKGROUND_INTENSITY = 0x0080,
			COMMON_LVB_LEADING_BYTE = 0x0100,
			COMMON_LVB_TRAILING_BYTE = 0x0200,
			COMMON_LVB_GRID_HORIZONTAL = 0x0400,
			COMMON_LVB_GRID_LVERTICAL = 0x0800,
			COMMON_LVB_GRID_RVERTICAL = 0x1000,
			COMMON_LVB_REVERSE_VIDEO = 0x4000,
			COMMON_LVB_UNDERSCORE = 0x8000
		}
#endif
	}
}