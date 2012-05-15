using System;
using System.Collections.Generic;

namespace Deveel.Console {
	public interface IInformationProvider {
		bool IsInfoSupported(string name);

		ColumnDesign[] GetColumns(string name);

		IList<ColumnValue[]> GetValues(string name);
	}
}