using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using static FreeSql.SqlExtExtensions;

[ExpressionCall]
public static class FreeSqlGlobalExpressionCallExtensions
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
}

namespace FreeSql
{
    /// <summary>
    /// SqlExt 是利用自定表达式函数解析功能，解析默认常用的SQL函数，欢迎 PR
    /// </summary>
    [ExpressionCall]
    public static class SqlExt
    {
        internal static ThreadLocal<ExpressionCallContext> expContext = new ThreadLocal<ExpressionCallContext>();

        #region SqlServer/PostgreSQL over
        /// <summary>
        /// rank() over(order by ...)
        /// </summary>
        /// <returns></returns>
        public static ISqlOver<long> Rank() => Over<long>("rank()");
        /// <summary>
        /// dense_rank() over(order by ...)
        /// </summary>
        /// <returns></returns>
        public static ISqlOver<long> DenseRank() => Over<long>("dense_rank()");
        /// <summary>
        /// count() over(order by ...)
        /// </summary>
        /// <returns></returns>
        public static ISqlOver<long> Count() => Over<long>("count()");
        /// <summary>
        /// sum(..) over(order by ...)
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public static ISqlOver<decimal> Sum(object column) => Over<decimal>($"sum({expContext.Value.ParsedContent["column"]})");
        /// <summary>
        /// avg(..) over(order by ...)
        /// </summary>
        /// <returns></returns>
        public static ISqlOver<decimal> Avg() => Over<decimal>($"avg({expContext.Value.ParsedContent["column"]})");
        /// <summary>
        /// max(..) over(order by ...)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="column"></param>
        /// <returns></returns>
        public static ISqlOver<T> Max<T>(T column) => Over<T>($"max({expContext.Value.ParsedContent["column"]})");
        /// <summary>
        /// min(..) over(order by ...)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="column"></param>
        /// <returns></returns>
        public static ISqlOver<T> Min<T>(T column) => Over<T>($"min({expContext.Value.ParsedContent["column"]})");
        /// <summary>
        /// SqlServer row_number() over(order by ...)
        /// </summary>
        /// <returns></returns>
        public static ISqlOver<long> RowNumber() => Over<long>("row_number()");
        #endregion

        /// <summary>
        /// case when .. then .. end
        /// </summary>
        /// <returns></returns>
        public static ICaseWhenEnd Case() => SqlExtExtensions.Case();
        /// <summary>
        /// MySql group_concat(distinct .. order by .. separator ..)
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public static IGroupConcat GroupConcat(object column) => SqlExtExtensions.GroupConcat(column);
    }

    [ExpressionCall]
    public static class SqlExtExtensions //这个类存在的意义，是不想使用者方法名污染
    {
        static ThreadLocal<ExpressionCallContext> expContextSelf = new ThreadLocal<ExpressionCallContext>();
        static ExpressionCallContext expContext => expContextSelf.Value ?? SqlExt.expContext.Value;
        internal static ThreadLocal<List<ExpSbInfo>> expSb = new ThreadLocal<List<ExpSbInfo>>();
        internal static ExpSbInfo expSbLast => expSb.Value.Last();
        internal class ExpSbInfo
        {
            public StringBuilder Sb { get; } = new StringBuilder();
            public bool IsOrderBy = false;
            public bool IsDistinct = false;
        }

        #region .. over([partition by ..] order by ...)
        internal static ISqlOver<TValue> Over<TValue>(string sqlFunc)
        {
            if (expSb.Value == null) expSb.Value = new List<ExpSbInfo>();
            expSb.Value.Add(new ExpSbInfo());
            expSbLast.Sb.Append(sqlFunc).Append(" ");
            return null;
        }
        public static ISqlOver<TValue> Over<TValue>(this ISqlOver<TValue> that)
        {
            expSbLast.Sb.Append("over(");
            return that;
        }
        public static ISqlOver<TValue> PartitionBy<TValue>(this ISqlOver<TValue> that, object column)
        {
            expSbLast.Sb.Append(" partition by ").Append(expContext.ParsedContent["column"]).Append(",");
            return that;
        }
        public static ISqlOver<TValue> OrderBy<TValue>(this ISqlOver<TValue> that, object column) => OrderByPriv(that, false);
        public static ISqlOver<TValue> OrderByDescending<TValue>(this ISqlOver<TValue> that, object column) => OrderByPriv(that, true);
        static ISqlOver<TValue> OrderByPriv<TValue>(this ISqlOver<TValue> that, bool isDesc)
        {
            var sb = expSbLast.Sb;
            if (expSbLast.IsOrderBy == false)
            {
                sb.Append(" order by ");
                expSbLast.IsOrderBy = true;
            }
            sb.Append(expContext.ParsedContent["column"]);
            if (isDesc) sb.Append(" desc");
            sb.Append(",");
            return that;
        }
        public static TValue ToValue<TValue>(this ISqlOver<TValue> that)
        {
            var sql = expSbLast.Sb.ToString().TrimEnd(',');
            expSbLast.Sb.Clear();
            expSb.Value.RemoveAt(expSb.Value.Count - 1);
            expContext.Result = $"{sql})";
            return default;
        }
        public interface ISqlOver<TValue> { }
        #endregion

        #region case when .. then .. when .. then .. end
        public static ICaseWhenEnd Case()
        {
            if (expSb.Value == null) expSb.Value = new List<ExpSbInfo>();
            expSb.Value.Add(new ExpSbInfo());
            expSbLast.Sb.Append("case ");
            return null;
        }
        public static ICaseWhenEnd<TValue> When<TValue>(this ICaseWhenEnd that, bool test, TValue then)
        {
            expSbLast.Sb.Append($"\r\n{"".PadRight(expSb.Value.Count * 2)}when ").Append(expContext.ParsedContent["test"]).Append(" then ").Append(expContext.ParsedContent["then"]);
            return null;
        }
        public static ICaseWhenEnd<TValue> When<TValue>(this ICaseWhenEnd<TValue> that, bool test, TValue then)
        {
            expSbLast.Sb.Append($"\r\n{"".PadRight(expSb.Value.Count * 2)}when ").Append(expContext.ParsedContent["test"]).Append(" then ").Append(expContext.ParsedContent["then"]);
            return null;
        }
        public static ICaseWhenEnd<TValue> Else<TValue>(this ICaseWhenEnd<TValue> that, TValue then)
        {
            expSbLast.Sb.Append($"\r\n{"".PadRight(expSb.Value.Count * 2)}else ").Append(expContext.ParsedContent["then"]);
            return null;
        }
        public static TValue End<TValue>(this ICaseWhenEnd<TValue> that)
        {
            var sql = expSbLast.Sb.Append($"\r\n{"".PadRight(expSb.Value.Count * 2 - 2)}end").ToString();
            expSbLast.Sb.Clear();
            expSb.Value.RemoveAt(expSb.Value.Count - 1);
            expContext.Result = sql;
            return default;
        }
        public interface ICaseWhenEnd { }
        public interface ICaseWhenEnd<TValue> { }
        #endregion

        #region group_concat
        public static IGroupConcat GroupConcat(object column)
        {
            if (expSb.Value == null) expSb.Value = new List<ExpSbInfo>();
            expSb.Value.Add(new ExpSbInfo());
            expSbLast.Sb.Append("group_concat(").Append(expContext.ParsedContent["column"]);
            return null;
        }
        public static IGroupConcat Distinct(this IGroupConcat that)
        {
            if (expSbLast.IsDistinct == false)
            {
                expSbLast.Sb.Insert(expSbLast.Sb.ToString().LastIndexOf("group_concat(") + 13, "distinct ");
                expSbLast.IsDistinct = true;
            }
            return that;
        }
        public static IGroupConcat Separator(this IGroupConcat that, object separator)
        {
            if (expSbLast.IsOrderBy) expSbLast.Sb.Remove(expSbLast.Sb.Length - 1, 1);
            expSbLast.Sb.Append(" separator ").Append(expContext.ParsedContent["separator"]);
            return that;
        }
        public static IGroupConcat OrderBy(this IGroupConcat that, object column) => OrderByPriv(that, false);
        public static IGroupConcat OrderByDescending(this IGroupConcat that, object column) => OrderByPriv(that, true);
        static IGroupConcat OrderByPriv(this IGroupConcat that, bool isDesc)
        {
            var sb = expSbLast.Sb;
            if (expSbLast.IsOrderBy == false)
            {
                sb.Append(" order by ");
                expSbLast.IsOrderBy = true;
            }
            sb.Append(expContext.ParsedContent["column"]);
            if (isDesc) sb.Append(" desc");
            sb.Append(",");
            return that;
        }
        public static string ToValue(this IGroupConcat that)
        {
            var sql = expSbLast.Sb.ToString().TrimEnd(',');
            expSbLast.Sb.Clear();
            expSb.Value.RemoveAt(expSb.Value.Count - 1);
            expContext.Result = $"{sql})";
            return default;
        }
        public interface IGroupConcat { }
        #endregion
    }
}