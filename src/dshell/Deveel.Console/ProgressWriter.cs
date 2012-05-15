using System;

namespace Deveel.Console {
	/// <summary>
	/// A utility class that can write the progress of an operation
	/// to the screen.
	/// </summary>
	public class ProgressWriter {
		private const int DefaultScreenWidth = 65;

		/** min time before presenting an eta */
		private const long MinEtaRunningTime = 30 * 1000L;
		/** min time between two eta updates */
		private const long MinEtaDiffTime = (1 * 1000L);

		private readonly long _expectedTargetValue;
		private readonly OutputDevice _out;
		private readonly DateTime _startTime;
		private readonly CancelWriter _etaWriter;

		private DateTime _lastEtaUpdate;

		private int _progressDots;
		private int _screenWidth;

		public ProgressWriter(long expectedTargetValue, OutputDevice output) {
			_expectedTargetValue = expectedTargetValue;
			_out = output;
			_progressDots = 0;
			_startTime = DateTime.Now;
			_lastEtaUpdate = DateTime.MinValue;
			_etaWriter = new CancelWriter(_out);
			ScreenWidth = DefaultScreenWidth;
		}

		public int ScreenWidth {
			set { _screenWidth = value; }
			get { return _screenWidth; }
		}

		public void Update(long value) {
			if (_expectedTargetValue > 0 && value <= _expectedTargetValue) {
				long newDots = (_screenWidth * value) / _expectedTargetValue;
				if (newDots > _progressDots) {
					_etaWriter.Cancel(false);
					while (_progressDots < newDots) {
						_out.Write(".");
						++_progressDots;
					}
					_out.Flush();
				}
				WriteEta(value);
			}
		}

		public void Finish() {
			_etaWriter.Cancel();
		}

		private void WriteEta(long value) {
			if (!_etaWriter.IsPrinting)
				return;

			DateTime now = DateTime.Now;
			long runningTime = (long)(now - _startTime).TotalMilliseconds;
			if (runningTime < MinEtaRunningTime)
				return;

			long lastUpdateDiff = (long)(now - _lastEtaUpdate).TotalMilliseconds;
			if (!_etaWriter.HasCancellableOutput || lastUpdateDiff > MinEtaDiffTime) {
				long etaTime = _expectedTargetValue * runningTime / value;
				long rest = etaTime - runningTime;
				_etaWriter.Write("ETA: " + TimeRenderer.RenderTime(rest));
				_lastEtaUpdate = now;
			}
		}
	}
}