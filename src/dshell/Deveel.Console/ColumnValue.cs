using System;

namespace Deveel.Console {
	public sealed class ColumnValue {
		#region ctor
		public ColumnValue(string value) {
			if (value == null) {
				width = NullLength;
				columnValues = null;
			} else {
				width = 0;
				//TODO: Check if is better {newline}
				string[] tok = value.Split('\n');
				columnValues = new string[tok.Length];
				for (int i = 0; i < columnValues.Length; ++i) {
					string line = (string)tok[i];
					int lWidth = line.Length;
					columnValues[i] = line;
					if (lWidth > width) {
						width = lWidth;
					}
				}
			}
			pos = 0;
		}

		public ColumnValue(int value)
			: this(value.ToString()) {
		}
		#endregion

		#region Fields
		internal readonly static string NullText = "[NULL]";
		internal readonly static int NullLength = NullText.Length;

		private string[] columnValues; // multi-rows
		private int width;
		private int pos;
		#endregion

		#region Properties
		public int Width {
			get { return width; }
		}

		public bool IsNull {
			get { return (columnValues == null); }
		}
		#endregion

		#region Public Methods
		public bool HasNextLine() {
			return (columnValues != null && pos < columnValues.Length);
		}

		public string GetNextLine() {
			string result = "";
			if (columnValues == null) {
				if (pos == 0)
					result = NullText;
			} else if (pos < columnValues.Length) {
				result = columnValues[pos];
			}
			++pos;
			return result;
		}
		#endregion
	}
}