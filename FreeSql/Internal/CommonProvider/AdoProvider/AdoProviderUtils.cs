using System;
using System.Text.RegularExpressions;

namespace FreeSql.Internal.CommonProvider {
	partial class AdoProvider {
		public abstract object AddslashesProcessParam(object param, Type mapType);
		public string Addslashes(string filter, params object[] parms) {
			if (filter == null || parms == null) return string.Empty;
			if (parms.Length == 0) return filter;
			var nparms = new object[parms.Length];
			for (int a = 0; a < parms.Length; a++) {
				if (parms[a] == null)
					filter = Regex.Replace(filter, @"\s*(=|IN)\s*\{" + a + @"\}", " IS {" + a + "}", RegexOptions.IgnoreCase);
				nparms[a] = AddslashesProcessParam(parms[a], null);
			}
			try { string ret = string.Format(filter, nparms); return ret; } catch { return filter; }
		}
	}
}
