using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace FreeSql.Internal.CommonProvider
{
    partial class AdoProvider
    {
        public abstract object AddslashesProcessParam(object param, Type mapType);
        public string Addslashes(string filter, params object[] parms)
        {
            if (filter == null || parms == null) return string.Empty;
            if (parms.Length == 0) return filter;
            var nparms = new object[parms.Length];
            for (int a = 0; a < parms.Length; a++)
            {
                if (parms[a] == null)
                    filter = _dicAddslashesReplaceIsNull.GetOrAdd(a, b => new Regex(@"\s*(=|IN)\s*\{" + b + @"\}", RegexOptions.IgnoreCase | RegexOptions.Compiled))
                        .Replace(filter, $" IS {{{a}}}");
                nparms[a] = AddslashesProcessParam(parms[a], null);
            }
            try { string ret = string.Format(filter, nparms); return ret; } catch { return filter; }
        }
        static ConcurrentDictionary<int, Regex> _dicAddslashesReplaceIsNull = new ConcurrentDictionary<int, Regex>();
    }
}
