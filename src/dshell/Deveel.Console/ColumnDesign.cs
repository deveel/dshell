using System;

namespace Deveel.Console {
	public sealed class ColumnDesign {
		#region ctor
		public ColumnDesign(string label, ColumnAlignment alignment, int autoWrap) {
			this.label = label;
			this.alignment = alignment;
			initialWidth = label.Length;
			width = initialWidth;
			display = true;
			autoWrapCol = autoWrap;
		}

		public ColumnDesign(string label, ColumnAlignment alignment)
			: this(label, alignment, -1) {
		}

		public ColumnDesign(string label)
			: this(label, ColumnAlignment.Left) {
		}
		#endregion

		#region Fields
		private readonly ColumnAlignment alignment;
		private readonly string label;
		private int initialWidth;
		private int width;
		private bool display;
		// wrap columns automatically at this column; -1 = disabled
		private int autoWrapCol;
		#endregion

		#region Properties
		public bool Display {
			get { return display; }
			set { display = value; }
		}

		public int Width {
			get { return width; }
			set {
				if (value > width)
					width = value;
			}
		}

		public string Label {
			get { return label; }
		}

		public ColumnAlignment Alignment {
			get { return alignment; }
		}

		public int AutoWrap {
			get { return autoWrapCol; }
			set { autoWrapCol = value; }
		}
		#endregion

		#region Public Methods
		public void ResetWidth() {
			width = initialWidth;
		}
		#endregion
	}
}