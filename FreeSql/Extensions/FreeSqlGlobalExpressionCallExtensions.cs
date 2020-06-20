using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

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
    [ExpressionCall]
    public static class SqlExt
    {
        public static ThreadLocal<ExpressionCallContext> expContext = new ThreadLocal<ExpressionCallContext>();

        public static ISqlOver<long> Rank() => Over<long>("RANK()");
        public static ISqlOver<long> DenseRank() => Over<long>("DENSE_RANK()");
        public static ISqlOver<long> Count() => Over<long>("COUNT()");
        public static ISqlOver<decimal> Sum(object column) => Over<decimal>($"Sum({expContext.Value.ParsedContent["column"]})");
        public static ISqlOver<decimal> Avg() => Over<decimal>($"AVG({expContext.Value.ParsedContent["column"]})");
        public static ISqlOver<T> Max<T>(T column) => Over<T>($"MAX({expContext.Value.ParsedContent["column"]})");
        public static ISqlOver<T> Min<T>(T column) => Over<T>($"MIN({expContext.Value.ParsedContent["column"]})");
        public static ISqlOver<long> RowNumber() => Over<long>("ROW_NUMBER()");

        #region .. over([partition by ..] order by ...)
        static ThreadLocal<StringBuilder> expOverSb = new ThreadLocal<StringBuilder>();
        static ThreadLocal<bool> expOverSbIsOrderBy = new ThreadLocal<bool>();
        static ISqlOver<TValue> Over<TValue>(string sqlFunc)
        {
            expOverSb.Value = new StringBuilder();
            expOverSbIsOrderBy.Value = false;
            expOverSb.Value.Append($"{sqlFunc} ");
            return null;
        }
        public static ISqlOver<TValue> Over<TValue>(this ISqlOver<TValue> that)
        {
            expOverSb.Value.Append("OVER(");
            return that;
        }
        public static ISqlOver<TValue> PartitionBy<TValue>(this ISqlOver<TValue> that, object column)
        {
            expOverSb.Value.Append("PARTITION BY ").Append(expContext.Value.ParsedContent["column"]).Append(",");
            return that;
        }
        public static ISqlOver<TValue> OrderBy<TValue>(this ISqlOver<TValue> that, object column) => OrderBy(that, false);
        public static ISqlOver<TValue> OrderByDescending<TValue>(this ISqlOver<TValue> that, object column) => OrderBy(that, true);
        static ISqlOver<TValue> OrderBy<TValue>(this ISqlOver<TValue> that, bool isDesc)
        {
            var sb = expOverSb.Value;
            if (expOverSbIsOrderBy.Value == false)
            {
                sb.Append("ORDER BY ");
                expOverSbIsOrderBy.Value = true;
            }
            sb.Append(expContext.Value.ParsedContent["column"]);
            if (isDesc) sb.Append(" desc");
            sb.Append(",");
            return that;
        }
        public static TValue ToValue<TValue>(this ISqlOver<TValue> that)
        {
            var sb = expOverSb.Value.ToString().TrimEnd(',');
            expOverSb.Value.Clear();
            expContext.Value.Result = $"{sb})";
            return default;
        }
        public interface ISqlOver<TValue> { }
        #endregion

        #region case when .. then .. when .. then .. end
        static ThreadLocal<List<StringBuilder>> expCaseWhenEndSb = new ThreadLocal<List<StringBuilder>>();
        public static ICaseWhenEnd Case()
        {
            if (expCaseWhenEndSb.Value == null) expCaseWhenEndSb.Value = new List<StringBuilder>();
            expCaseWhenEndSb.Value.Add(new StringBuilder().Append("CASE "));
            return null;
        }
        public static ICaseWhenEnd<TValue> When<TValue>(this ICaseWhenEnd that, bool test, TValue then)
        {
            expCaseWhenEndSb.Value.Last().Append("\r\n  WHEN ").Append(expContext.Value.ParsedContent["test"]).Append(" THEN ").Append(expContext.Value.ParsedContent["then"]);
            return null;
        }
        public static ICaseWhenEnd<TValue> When<TValue>(this ICaseWhenEnd<TValue> that, bool test, TValue then)
        {
            expCaseWhenEndSb.Value.Last().Append("\r\n  WHEN ").Append(expContext.Value.ParsedContent["test"]).Append(" THEN ").Append(expContext.Value.ParsedContent["then"]);
            return null;
        }
        public static ICaseWhenEnd<TValue> Else<TValue>(this ICaseWhenEnd<TValue> that, TValue then)
        {
            expCaseWhenEndSb.Value.Last().Append("\r\n  ELSE ").Append(expContext.Value.ParsedContent["then"]);
            return null;
        }
        public static TValue End<TValue>(this ICaseWhenEnd<TValue> that)
        {
            var sb = expCaseWhenEndSb.Value;
            var sql = sb.Last().Append("\r\nEND").ToString();
            sb.Last().Clear();
            sb.RemoveAt(sb.Count - 1);
            expContext.Value.Result = sql;
            return default;
        }
        public interface ICaseWhenEnd { }
        public interface ICaseWhenEnd<TValue> { }
        #endregion
    }
}