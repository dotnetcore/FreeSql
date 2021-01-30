using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        /// isnull、ifnull、coalesce、nvl
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static TValue IsNull<TValue>(TValue value, TValue defaultValue)
        {
            expContext.Value.Result = expContext.Value._commonExp._common.IsNull(expContext.Value.ParsedContent["value"], expContext.Value.ParsedContent["defaultValue"]);
            return default(TValue);
        }

        /// <summary>
        /// count(distinct name)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="column"></param>
        /// <returns></returns>
        public static long DistinctCount<T>(T column)
        {
            expContext.Value.Result = $"count(distinct {expContext.Value.ParsedContent["column"]})";
            return 0;
        }

        #region 大小判断
        /// <summary>
        /// 大于 &gt;
        /// </summary>
        /// <returns></returns>
        public static bool GreaterThan<TValue>(TValue value1, TValue value2)
        {
            expContext.Value.Result = $"{expContext.Value.ParsedContent["value1"]} > {expContext.Value.ParsedContent["value2"]}";
            return false;
        }
        /// <summary>
        /// 大于或等于 &gt;=
        /// </summary>
        /// <returns></returns>
        public static bool GreaterThanOrEqual<TValue>(TValue value1, TValue value2)
        {
            expContext.Value.Result = $"{expContext.Value.ParsedContent["value1"]} >= {expContext.Value.ParsedContent["value2"]}";
            return false;
        }
        /// <summary>
        /// 小于 &lt;
        /// </summary>
        /// <returns></returns>
        public static bool LessThan<TValue>(TValue value1, TValue value2)
        {
            expContext.Value.Result = $"{expContext.Value.ParsedContent["value1"]} < {expContext.Value.ParsedContent["value2"]}";
            return false;
        }
        /// <summary>
        /// 小于或等于 &lt;=
        /// </summary>
        /// <returns></returns>
        public static bool LessThanOrEqual<TValue>(TValue value1, TValue value2)
        {
            expContext.Value.Result = $"{expContext.Value.ParsedContent["value1"]} <= {expContext.Value.ParsedContent["value2"]}";
            return false;
        }
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
        /// <summary>
        /// MySql find_in_set(str, strlist)
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="str"></param>
        /// <param name="strlist"></param>
        /// <returns></returns>
        public static int FindInSet<TValue>(TValue str, string strlist)
        {
            expContext.Value.Result = $"find_in_set({expContext.Value.ParsedContent["str"]}, {expContext.Value.ParsedContent["strlist"]})";
            return 0;
        }

        /// <summary>
        /// PostgreSQL string_agg(.., ..)
        /// </summary>
        /// <param name="column"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static string StringAgg(object column, object delimiter)
        {
            expContext.Value.Result = $"string_agg({expContext.Value.ParsedContent["column"]}, {expContext.Value.ParsedContent["delimiter"]})";
            return "";
        }
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
            public bool IsOver = false;
            public bool IsOrderBy = false;
            public bool IsDistinct = false;
        }

        #region .. over([partition by ..] order by ...)
        internal static ISqlOver<TValue> Over<TValue>(string sqlFunc)
        {
            if (expSb.Value == null) expSb.Value = new List<ExpSbInfo>();
            expSb.Value.Add(new ExpSbInfo());
            expSbLast.Sb.Append(sqlFunc);
            return null;
        }
        public static ISqlOver<TValue> Over<TValue>(this ISqlOver<TValue> that)
        {
            expSbLast.Sb.Append(" over(");
            expSbLast.IsOver = true;
            return that;
        }
        public static ISqlOver<TValue> PartitionBy<TValue>(this ISqlOver<TValue> that, object column)
        {
            var sb = expSbLast.Sb;
            sb.Append(" partition by ");
            var exp = expContext.RawExpression["column"];
            if (exp.NodeType == ExpressionType.New)
            {
                var expNew = exp as NewExpression;
                for (var a = 0; a < expNew.Arguments.Count; a++)
                {
                    if (a > 0) sb.Append(",");
                    sb.Append(expContext.Utility.ParseExpression(expNew.Arguments[a]));
                }
            } else
                sb.Append(expContext.ParsedContent["column"]);
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
            var exp = expContext.RawExpression["column"];
            if (exp.NodeType == ExpressionType.New)
            {
                var expNew = exp as NewExpression;
                for (var a = 0; a < expNew.Arguments.Count; a++)
                {
                    sb.Append(expContext.Utility.ParseExpression(expNew.Arguments[a]));
                    if (isDesc) sb.Append(" desc");
                    sb.Append(",");
                }
            }
            else
            {
                sb.Append(expContext.ParsedContent["column"]);
                if (isDesc) sb.Append(" desc");
                sb.Append(",");
            }
            return that;
        }
        public static TValue ToValue<TValue>(this ISqlOver<TValue> that)
        {
            var sql = expSbLast.Sb.ToString().TrimEnd(',');
            if (expSbLast.IsOver) sql = $"{sql})";
            expSbLast.Sb.Clear();
            expSb.Value.RemoveAt(expSb.Value.Count - 1);
            expContext.Result = sql;
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
            var exp = expContext.RawExpression["column"];
            if (exp.NodeType == ExpressionType.New)
            {
                var expNew = exp as NewExpression;
                for (var a = 0; a < expNew.Arguments.Count; a++)
                {
                    sb.Append(expContext.Utility.ParseExpression(expNew.Arguments[a]));
                    if (isDesc) sb.Append(" desc");
                    sb.Append(",");
                }
            }
            else
            {
                sb.Append(expContext.ParsedContent["column"]);
                if (isDesc) sb.Append(" desc");
                sb.Append(",");
            }
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

        #region string.Join 反射处理，此块代码用于反射，所以别修改定义
        public static string StringJoinSqliteGroupConcat(object column, object delimiter)
        {
            expContext.Result = $"group_concat({expContext.ParsedContent["column"]},{expContext.ParsedContent["delimiter"]})";
            return null;
        }
        public static string StringJoinPgsqlGroupConcat(object column, object delimiter)
        {
            expContext.Result = $"string_agg(({expContext.ParsedContent["column"]})::text,{expContext.ParsedContent["delimiter"]})";
            return null;
        }
        public static string StringJoinMySqlGroupConcat(object column, object delimiter)
        {
            expContext.Result = $"group_concat({expContext.ParsedContent["column"]} separator {expContext.ParsedContent["delimiter"]})";
            return null;
        }
        public static string StringJoinOracleGroupConcat(object column, object delimiter)
        {
            string orderby = null;
            var subSelect = expContext._tsc?.subSelect001;
            if (subSelect != null)
            {
                orderby = subSelect?._orderby?.Trim('\r', '\n');
                if (string.IsNullOrEmpty(orderby))
                {
                    var subSelectTb1 = subSelect._tables.FirstOrDefault();
                    if (subSelectTb1 != null && subSelectTb1.Table.Primarys.Any() == true)
                        orderby = $"order by {string.Join(",", subSelectTb1.Table.Primarys.Select(a => $"{subSelectTb1.Alias}.{subSelect._commonUtils.QuoteSqlName(a.Attribute.Name)}"))}";
                }
            }
            if (string.IsNullOrEmpty(orderby)) orderby = "order by 1";
            expContext.Result = $"listagg(to_char({expContext.ParsedContent["column"]}),{expContext.ParsedContent["delimiter"]}) within group({orderby})";
            return null;
        }
        public static string StringJoinFirebirdList(object column, object delimiter)
        {
            expContext.Result = $"list({expContext.ParsedContent["column"]},{expContext.ParsedContent["delimiter"]})";
            return null;
        }
        #endregion
    }
}