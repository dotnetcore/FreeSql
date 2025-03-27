using FreeSql.Internal.Model;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.Internal.CommonProvider
{
    partial class AdoProvider
    {
        public object AddslashesTypeHandler(Type type, object param)
        {
            if (Utils.TypeHandlers.TryGetValue(type, out var typeHandler))
            {
                var result = typeHandler.Serialize(param);
                return AddslashesProcessParam(result, null, null);
            }
            return null;
        }

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
            try { string ret = string.Format(CultureInfo.InvariantCulture, filter, nparms); return ret; } catch { return filter; }
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
                sb.Append(string.Format(CultureInfo.InvariantCulture, "{0}", AddslashesProcessParam(z, mapType, mapColumn)));
            }

            return sb.Length == 0 ? "(NULL)" : sb.Remove(0, 1).Insert(0, "(").Append(")").ToString();
        }

        public static bool IsFromSlave(string cmdText, CommandType cmdType)
        {
            return cmdType == CommandType.StoredProcedure ||
                cmdText.StartsWith("SELECT ", StringComparison.CurrentCultureIgnoreCase) ||
                cmdText.StartsWith("WITH ", StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
