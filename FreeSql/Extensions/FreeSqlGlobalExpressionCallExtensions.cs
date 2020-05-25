using FreeSql.DataAnnotations;
using System;
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