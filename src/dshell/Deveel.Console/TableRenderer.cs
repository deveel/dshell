using System;
using System.Collections;
using System.Text;

namespace Deveel.Console {
	public class TableRenderer {
		#region ctor
		public TableRenderer(ColumnDesign[] columns, OutputDevice output, string separator, bool enableHeader, bool enableFooter) {
			this.columns = columns;
			this.output = output;
			this.enableHeader = enableHeader;
			this.enableFooter = enableFooter;

			// we cache the rows in order to dynamically determine the
			// output width of each column.
			cacheRows = new ArrayList(MaxCacheElements);
			alreadyFlushed = false;
			writtenRows = 0;
			colSeparator = " " + separator;
			separatorWidth = separator.Length;
		}

		public TableRenderer(ColumnDesign[] columns, OutputDevice output)
			: this(columns, output, "|", true, true) {
		}

		#endregion

		#region Fields

		private const int MaxCacheElements = 500;

		private ArrayList cacheRows;
		private bool alreadyFlushed;
		private int writtenRows;
		private int separatorWidth;

		private bool enableHeader;
		private bool enableFooter;

		protected ColumnDesign[] columns;
		protected OutputDevice output;
		protected string colSeparator;

		#endregion

		#region Properties

		public ColumnDesign[] Columns {
			get { return columns; }
		}

		public bool EnableFooter {
			get { return enableFooter; }
			set { enableFooter = value; }
		}

		public bool EnableHeader {
			get { return enableHeader; }
			set { enableHeader = value; }
		}

		public string ColumnSeparator {
			get { return colSeparator; }
			set {
				colSeparator = " " + value;
				separatorWidth = value.Length;
			}
		}

		#endregion

		#region Private Methods
		private void WriteHorizontalLine() {
			for (int i = 0; i < columns.Length; ++i) {
				if (!columns[i].Display)
					continue;

				string txt = FormatString(String.Empty, '-', columns[i].Width + separatorWidth + 1, ColumnAlignment.Left);
				output.Write(txt);
				output.Write("+");
			}

			output.WriteLine();
		}

		private void WriteTableHeader() {
			WriteHorizontalLine();

			for (int i = 0; i < columns.Length; ++i) {
				if (!columns[i].Display)
					continue;

				string txt = FormatString(columns[i].Label, ' ', columns[i].Width + 1, ColumnAlignment.Center);
				output.WriteBold(txt);
				output.Write(colSeparator);
			}

			output.WriteLine();
			WriteHorizontalLine();
		}
		#endregion

		#region Protected Methods
		protected string FormatString(string text, char fillchar, int len, ColumnAlignment alignment) {
			// Console.Out.WriteLine("[formatString] len: " + len + ", text.Length: " + text.Length);
			// text = "hi";
			StringBuilder fillstr = new StringBuilder();

			if (len > 4000)
				len = 4000;

			if (text == null)
				text = ColumnValue.NullText;

			int slen = text.Length;

			if (alignment == ColumnAlignment.Left)
				fillstr.Append(text);

			int fillNumber = len - slen;
			int boundary = 0;
			if (alignment == ColumnAlignment.Center)
				boundary = fillNumber / 2;

			while (fillNumber > boundary) {
				fillstr.Append(fillchar);
				--fillNumber;
			}

			if (alignment != ColumnAlignment.Left)
				fillstr.Append(text);

			while (fillNumber > 0) {
				fillstr.Append(fillchar);
				--fillNumber;
			}

			return fillstr.ToString();
		}

		protected void AddRowToCache(ColumnValue[] row) {
			cacheRows.Add(row);
			if (cacheRows.Count >= MaxCacheElements) {
				Flush();
				cacheRows.Clear();
			}
		}

		protected virtual void UpdateColumnWidths(ColumnValue[] row) {
			for (int i = 0; i < columns.Length; ++i) {
				columns[i].Width = row[i].Width;
			}
		}

		protected bool WriteColumns(ColumnValue[] currentRow, bool hasMoreLines) {
			for (int i = 0; i < columns.Length; ++i) {
				if (!columns[i].Display)
					continue;

				hasMoreLines = WriteColumn(currentRow[i], hasMoreLines, i);
			}

			return hasMoreLines;
		}

		protected bool WriteColumn(ColumnValue column, bool hasMoreLines, int i) {
			output.Write(" ");

			string txt = FormatString(column.GetNextLine(), ' ', columns[i].Width, columns[i].Alignment);

			hasMoreLines |= column.HasNextLine();
			if (column.IsNull) {
				output.WriteGrey(txt);
			} else {
				output.Write(txt);
			}

			output.Write(colSeparator);
			return hasMoreLines;
		}
		#endregion

		#region Public Methods
		public void AddRow(ColumnValue[] row) {
			UpdateColumnWidths(row);
			AddRowToCache(row);
		}

		public void CloseTable() {
			Flush();
			if (writtenRows > 0 && enableFooter) {
				WriteHorizontalLine();
			}
		}

		public void Flush() {
			if (!alreadyFlushed) {
				if (enableHeader)
					WriteTableHeader();
				alreadyFlushed = true;
			}

			foreach (ColumnValue[] currentRow in cacheRows) {
				bool hasMoreLines;

				do {
					hasMoreLines = false;
					hasMoreLines = WriteColumns(currentRow, hasMoreLines);
					output.WriteLine();
				} while (hasMoreLines);

				++writtenRows;
			}
		}
		#endregion
	}
}