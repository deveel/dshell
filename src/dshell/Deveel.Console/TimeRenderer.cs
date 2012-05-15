using System;
using System.IO;
using System.Text;

namespace Deveel.Console {
	public class TimeRenderer {
		private const long SecondMillis = 1000;
		private const long MinuteMillis = 60 * SecondMillis;
		private const long HourMillis = 60 * MinuteMillis;

		public static void PrintFraction(long execTime, long number, OutputDevice output) {
			if (number == 0) {
				output.Write(" -- ");
				return;
			}
			long milli = execTime / number;
			long micro = (execTime - number * milli) * 1000 / number;
			PrintTime(milli, micro, output);
		}

		/** render time as string */
		public static String RenderTime(long execTimeInMs) {
			return RenderTime(execTimeInMs, 0);
		}

		/** render time as string */
		public static String RenderTime(long execTimeInMs, long usec) {
			StringBuilder result = new StringBuilder();
			PrintTime(execTimeInMs, usec, new StringBuilderOutputDevice(result));
			return result.ToString();
		}

		private class StringBuilderOutputDevice : OutputDevice {
			public StringBuilderOutputDevice(StringBuilder builder) {
				writer = new StringWriter(builder);
			}

			private readonly StringWriter writer;
			
			protected override TextWriter Output {
				get { return writer; }
			}

			public override Encoding Encoding {
				get { return Encoding.UTF8; }
			}
		}

		/** print time to output device */
		public static void PrintTime(long execTimeInMs, OutputDevice output) {
			PrintTime(execTimeInMs, 0, output);
		}

		/** print time to output device */
		public static void PrintTime(long execTimeInMs, long usec, OutputDevice output) {
			long totalTime = execTimeInMs;

			bool hourPrinted = false;
			bool minutePrinted = false;

			if (execTimeInMs > HourMillis) {
				output.Write(Convert.ToString(execTimeInMs / HourMillis));
				output.Write("h ");
				execTimeInMs %= HourMillis;
				hourPrinted = true;
			}

			if (hourPrinted || execTimeInMs > MinuteMillis) {
				long minute = execTimeInMs / 60000;
				if (hourPrinted && minute < 10) {
					output.Write("0"); // need padding.
				}
				output.Write(minute.ToString());
				output.Write("m ");
				execTimeInMs %= MinuteMillis;
				minutePrinted = true;
			}

			if (minutePrinted || execTimeInMs >= SecondMillis) {
				long seconds = execTimeInMs / SecondMillis;
				if (minutePrinted && seconds < 10) {
					output.Write("0"); // need padding.
				}
				output.Write(seconds.ToString());
				output.Write(".");
				execTimeInMs %= SecondMillis;
				// milliseconds
				if (execTimeInMs < 100)
					output.Write("0");
				if (execTimeInMs < 10)
					output.Write("0");
				output.Write(execTimeInMs.ToString());
			} else if (execTimeInMs > 0) {
				output.Write(execTimeInMs.ToString());
			}

			if (usec > 0) {
				if (totalTime > 0) {  // need delimiter and padding.
					output.Write(".");
					if (usec < 100)
						output.Write("0");
					if (usec < 10)
						output.Write("0");
				}
				output.Write(usec.ToString());
			} else if (execTimeInMs == 0) {
				output.Write("0 ");
			}

			if (totalTime > MinuteMillis) {
				output.Write("s");
			} else if (totalTime >= SecondMillis) 
				output.Write(" ");
			else if (totalTime > 0 && totalTime < SecondMillis) 
				output.Write(" m");
			else if (totalTime == 0 && usec > 0) 
				output.Write(" µ");
			else
				output.Write("sec");
		}
	}
}