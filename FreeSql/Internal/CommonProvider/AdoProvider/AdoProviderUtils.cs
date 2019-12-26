using FreeSql.Internal.Model;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.Internal.CommonProvider
{
    partial class AdoProvider
    {
        public abstract object AddslashesProcessParam(object param, Type mapType, ColumnInfo mapColumn);
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
                nparms[a] = AddslashesProcessParam(parms[a], null, null);
            }
            try { string ret = string.Format(filter, nparms); return ret; } catch { return filter; }
        }
        static ConcurrentDictionary<int, Regex> _dicAddslashesReplaceIsNull = new ConcurrentDictionary<int, Regex>();

        protected string AddslashesIEnumerable(object param, Type mapType, ColumnInfo mapColumn)
        {
            var sb = new StringBuilder();
            var ie = param as IEnumerable;
            var idx = 0;
            foreach (var z in ie)
            {
                sb.Append(",");
                if (++idx > 500)
                {
                    sb.Append("   \r\n    \r\n"); //500元素分割, 3空格\r\n4空格
                    idx = 1;
                }
                sb.Append(AddslashesProcessParam(z, mapType, mapColumn));
            }

            return sb.Length == 0 ? "(NULL)" : sb.Remove(0, 1).Insert(0, "(").Append(")").ToString();
        }
    }
}
