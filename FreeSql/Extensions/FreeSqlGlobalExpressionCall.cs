using FreeSql;
using FreeSql.DataAnnotations;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;

[ExpressionCall]
public static class FreeSqlGlobalExpressionCall
{
    public static ThreadLocal<ExpressionCallContext> expContext = new ThreadLocal<ExpressionCallContext>();

    /// <summary>
    /// C#： that >= between &amp;&amp; that &lt;= and<para></para>
    /// SQL： that BETWEEN between AND and
    /// </summary>
    /// <param name="that"></param>
    /// <param name="between"></param>
    /// <param name="and"></param>
    /// <returns></returns>
    public static bool Between(this DateTime that, DateTime between, DateTime and)
    {
        if (expContext.IsValueCreated == false || expContext.Value == null || expContext.Value.ParsedContent == null)
            return that >= between && that <= and;
        expContext.Value.Result = $"{expContext.Value.ParsedContent["that"]} between {expContext.Value.ParsedContent["between"]} and {expContext.Value.ParsedContent["and"]}";
        return false;
    }

    /// <summary>
    /// 注意：这个方法和 Between 有细微区别<para></para>
    /// C#： that >= start &amp;&amp; that &lt; end<para></para>
    /// SQL： that >= start and that &lt; end
    /// </summary>
    /// <param name="that"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static bool BetweenEnd(this DateTime that, DateTime start, DateTime end)
    {
        if (expContext.IsValueCreated == false || expContext.Value == null || expContext.Value.ParsedContent == null)
            return that >= start && that < end;
        expContext.Value.Result = $"{expContext.Value.ParsedContent["that"]} >= {expContext.Value.ParsedContent["start"]} and {expContext.Value.ParsedContent["that"]} < {expContext.Value.ParsedContent["end"]}";
        return false;
    }

    /// <summary>
    /// C#：从元组集合中查找 exp1, exp2 是否存在<para></para>
    /// SQL： <para></para>
    /// exp1 = that[0].Item1 and exp2 = that[0].Item2 OR <para></para>
    /// exp1 = that[1].Item1 and exp2 = that[1].Item2 OR <para></para>
    /// ... <para></para>
    /// 注意：当 that 为 null 或 empty 时，返回 1=0
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="that"></param>
    /// <param name="exp1"></param>
    /// <param name="exp2"></param>
    /// <returns></returns>
    public static bool Contains<T1, T2>([RawValue] this IEnumerable<(T1, T2)> that, T1 exp1, T2 exp2)
    {
        if (expContext.IsValueCreated == false || expContext.Value == null || expContext.Value.ParsedContent == null)
            return that?.Any(a => a.Item1.Equals(exp1) && a.Item2.Equals(exp2)) == true;
        if (that?.Any() != true)
        {
            expContext.Value.Result = "1=0";
            return false;
        }
        var sb = new StringBuilder();
        var idx = 0;
        foreach (var item in that)
        {
            if (idx++ > 0) sb.Append(" OR \r\n");
            sb
                .Append(expContext.Value.ParsedContent["exp1"]).Append(" = ").Append(expContext.Value.FormatSql(FreeSql.Internal.Utils.GetDataReaderValue(typeof(T1), item.Item1)))
                .Append(" AND ")
                .Append(expContext.Value.ParsedContent["exp2"]).Append(" = ").Append(expContext.Value.FormatSql(FreeSql.Internal.Utils.GetDataReaderValue(typeof(T2), item.Item2)));
        }
        expContext.Value.Result = sb.ToString();
        return true;
    }
    /// <summary>
    /// C#：从元组集合中查找 exp1, exp2, exp2 是否存在<para></para>
    /// SQL： <para></para>
    /// exp1 = that[0].Item1 and exp2 = that[0].Item2 and exp3 = that[0].Item3 OR <para></para>
    /// exp1 = that[1].Item1 and exp2 = that[1].Item2 and exp3 = that[1].Item3 OR <para></para>
    /// ... <para></para>
    /// 注意：当 that 为 null 或 empty 时，返回 1=0
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <param name="that"></param>
    /// <param name="exp1"></param>
    /// <param name="exp2"></param>
    /// <param name="exp3"></param>
    /// <returns></returns>
    public static bool Contains<T1, T2, T3>([RawValue] this IEnumerable<(T1, T2, T3)> that, T1 exp1, T2 exp2, T3 exp3)
    {
        if (expContext.IsValueCreated == false || expContext.Value == null || expContext.Value.ParsedContent == null)
            return that.Any(a => a.Item1.Equals(exp1) && a.Item2.Equals(exp2) && a.Item3.Equals(exp3));
        if (that.Any() == false)
        {
            expContext.Value.Result = "1=0";
            return false;
        }
        var sb = new StringBuilder();
        var idx = 0;
        foreach (var item in that)
        {
            if (idx++ > 0) sb.Append(" OR \r\n");
            sb
                .Append(expContext.Value.ParsedContent["exp1"]).Append(" = ").Append(expContext.Value.FormatSql(FreeSql.Internal.Utils.GetDataReaderValue(typeof(T1), item.Item1)))
                .Append(" AND ")
                .Append(expContext.Value.ParsedContent["exp2"]).Append(" = ").Append(expContext.Value.FormatSql(FreeSql.Internal.Utils.GetDataReaderValue(typeof(T2), item.Item2)))
                .Append(" AND ")
                .Append(expContext.Value.ParsedContent["exp3"]).Append(" = ").Append(expContext.Value.FormatSql(FreeSql.Internal.Utils.GetDataReaderValue(typeof(T3), item.Item3)));
        }
        expContext.Value.Result = sb.ToString();
        return true;
    }
}