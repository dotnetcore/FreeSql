using FreeSql;
using FreeSql.ClickHouse.Curd;
using FreeSql.Internal.CommonProvider;
using System;
using System.Linq.Expressions;

public static partial class FreeSqlClickHouseGlobalExtensions
{

    /// <summary>
    /// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
    /// </summary>
    /// <param name="that"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string FormatClickHouse(this string that, params object[] args) => _clickHouseAdo.Addslashes(that, args);
    static FreeSql.ClickHouse.ClickHouseAdo _clickHouseAdo = new FreeSql.ClickHouse.ClickHouseAdo();

    /// <summary>
    /// Clickhouse limit by
    /// </summary>
    public static ISelect<T> LimitBy<T>(this ISelect<T> that, Expression<Func<T, object>> selector, int limit, int offset = 0)
    {
        if (limit <= 0 && offset <= 0) return that;
        var s0p = that as ClickHouseSelect<T>;
        var oldOrderBy = s0p._orderby;
        s0p._orderby = "";
        try
        {
            s0p.InternalOrderBy(selector);
            s0p._limitBy = $"LIMIT {(offset > 0 ? $"{offset}, " : "")}{limit} BY {s0p._orderby}";
        }
        finally
        {
            s0p._orderby = oldOrderBy;
        }
        return that;
    }

    /// <summary>
    /// Clickhouse 采样子句
    /// </summary>
    public static ISelect<T> Sample<T>(this ISelect<T> that, decimal k, int n, decimal m)
    {
        var s0p = that as ClickHouseSelect<T>;
        if (k > 0)
            if (m > 0)
                s0p._sample = $"SAMPLE {k} OFFSET {m}";
            else
                s0p._sample = $"SAMPLE {k}";
        else if (n > 0)
            s0p._sample = $"SAMPLE {k}";
        return that;
    }
}
