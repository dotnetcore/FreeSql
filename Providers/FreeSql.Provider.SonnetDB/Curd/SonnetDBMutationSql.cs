using System;
using System.Text;

namespace FreeSql.SonnetDB.Curd
{
    static class SonnetDBMutationSql
    {
        const string SelectStarFromDerived = "select * from (";

        internal static string RewritePrimaryKeySubquery(string sql, string projection)
        {
            if (string.IsNullOrEmpty(sql) || string.IsNullOrEmpty(projection)) return sql;
            var first = sql.IndexOf(SelectStarFromDerived, StringComparison.OrdinalIgnoreCase);
            if (first < 0) return sql;

            var result = new StringBuilder(sql.Length);
            var offset = 0;
            while (first >= 0)
            {
                result.Append(sql, offset, first - offset);
                result.Append("SELECT ").Append(projection).Append(" FROM (");
                offset = first + SelectStarFromDerived.Length;
                first = sql.IndexOf(SelectStarFromDerived, offset, StringComparison.OrdinalIgnoreCase);
            }
            result.Append(sql, offset, sql.Length - offset);
            return result.ToString();
        }
    }
}
