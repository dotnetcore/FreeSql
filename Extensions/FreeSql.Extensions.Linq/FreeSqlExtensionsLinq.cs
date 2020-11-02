using FreeSql;
using FreeSql.Extensions.Linq;
using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

public static class FreeSqlExtensionsLinqSql
{

    /// <summary>
    /// 将 ISelect&lt;T1&gt; 转换为 IQueryable&lt;T1&gt;<para></para>
    /// 用于扩展如：abp IRepository GetAll() 接口方法需要返回 IQueryable 对象<para></para>
    /// 提示：IQueryable 方法污染严重，查询功能的实现也不理想，应尽量避免此转换<para></para>
    /// IQueryable&lt;T1&gt; 扩展方法 RestoreToSelect() 可以还原为 ISelect&lt;T1&gt;
    /// </summary>
    /// <returns></returns>
    public static IQueryable<T1> AsQueryable<T1>(this ISelect<T1> that) where T1 : class
    {
        return new QueryableProvider<T1, T1>(that as Select1Provider<T1>);
    }
    /// <summary>
    /// 将 IQueryable&lt;T1&gt; 转换为 ISelect&lt;T1&gt;<para></para>
    /// 前提：IQueryable 必须由 FreeSql.Extensions.Linq.QueryableProvider 实现
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="that"></param>
    /// <returns></returns>
    public static ISelect<T1> RestoreToSelect<T1>(this IQueryable<T1> that) where T1 : class
    {
        var queryable = that as QueryableProvider<T1, T1> ?? throw new Exception($"无法将 IQueryable<{typeof(T1).Name}> 转换为 ISelect<{typeof(T1).Name}>，因为他的实现不是 FreeSql.Extensions.Linq.QueryableProvider");
        return queryable._select;
    }

    /// <summary>
    /// 【linq to sql】专用扩展方法，不建议直接使用
    /// </summary>
    public static ISelect<TReturn> Select<T1, TReturn>(this ISelect<T1> that, Expression<Func<T1, TReturn>> select)
    {
        var s1p = that as Select1Provider<T1>;
        if (typeof(TReturn) == typeof(T1)) return that as ISelect<TReturn>;
        s1p._tables[0].Parameter = select.Parameters[0];
        s1p._selectExpression = select.Body;
        if (s1p._orm.CodeFirst.IsAutoSyncStructure)
            (s1p._orm.CodeFirst as CodeFirstProvider)._dicSycedTryAdd(typeof(TReturn)); //._dicSyced.TryAdd(typeof(TReturn), true);
        var ret = (s1p._orm as BaseDbProvider).CreateSelectProvider<TReturn>(null) as Select1Provider<TReturn>;
        Select0Provider.CopyData(s1p, ret, null);
        return ret;
    }
    /// <summary>
    /// 【linq to sql】专用扩展方法，不建议直接使用
    /// </summary>
    public static ISelect<TResult> Join<T1, TInner, TKey, TResult>(this ISelect<T1> that, ISelect<TInner> inner, Expression<Func<T1, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<T1, TInner, TResult>> resultSelector) where T1 : class where TInner : class where TResult : class
    {
        var s1p = that as Select1Provider<T1>;
        InternalJoin2(s1p, outerKeySelector, innerKeySelector, resultSelector);
        if (typeof(TResult) == typeof(T1)) return that as ISelect<TResult>;
        s1p._selectExpression = resultSelector.Body;
        if (s1p._orm.CodeFirst.IsAutoSyncStructure)
            (s1p._orm.CodeFirst as CodeFirstProvider)._dicSycedTryAdd(typeof(TResult)); //._dicSyced.TryAdd(typeof(TResult), true);
        var ret = s1p._orm.Select<TResult>() as Select1Provider<TResult>;
        Select0Provider.CopyData(s1p, ret, null);
        return ret;
    }
    internal static void InternalJoin2<T1>(Select1Provider<T1> s1p, LambdaExpression outerKeySelector, LambdaExpression innerKeySelector, LambdaExpression resultSelector) where T1 : class
    {
        s1p._tables[0].Parameter = resultSelector.Parameters[0];
        s1p._commonExpression.ExpressionLambdaToSql(outerKeySelector, new CommonExpression.ExpTSC { _tables = s1p._tables });
        s1p.InternalJoin(Expression.Lambda(typeof(Func<,,>).MakeGenericType(typeof(T1), innerKeySelector.Parameters[0].Type, typeof(bool)),
            Expression.Equal(outerKeySelector.Body, innerKeySelector.Body),
            new[] { outerKeySelector.Parameters[0], innerKeySelector.Parameters[0] }
        ), SelectTableInfoType.InnerJoin);
    }

    /// <summary>
    /// 【linq to sql】专用扩展方法，不建议直接使用
    /// </summary>
    public static ISelect<TResult> GroupJoin<T1, TInner, TKey, TResult>(this ISelect<T1> that, ISelect<TInner> inner, Expression<Func<T1, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<T1, ISelect<TInner>, TResult>> resultSelector) where T1 : class where TInner : class where TResult : class
    {
        var s1p = that as Select1Provider<T1>;
        InternalJoin2(s1p, outerKeySelector, innerKeySelector, resultSelector);
        if (typeof(TResult) == typeof(T1)) return that as ISelect<TResult>;
        s1p._selectExpression = resultSelector.Body;
        if (s1p._orm.CodeFirst.IsAutoSyncStructure)
            (s1p._orm.CodeFirst as CodeFirstProvider)._dicSycedTryAdd(typeof(TResult)); //._dicSyced.TryAdd(typeof(TResult), true);
        var ret = s1p._orm.Select<TResult>() as Select1Provider<TResult>;
        Select0Provider.CopyData(s1p, ret, null);
        return ret;
    }
    /// <summary>
    /// 【linq to sql】专用扩展方法，不建议直接使用
    /// </summary>
    public static ISelect<TResult> SelectMany<T1, TCollection, TResult>(this ISelect<T1> that, Expression<Func<T1, ISelect<TCollection>>> collectionSelector, Expression<Func<T1, TCollection, TResult>> resultSelector) where T1 : class where TCollection : class where TResult : class
    {
        var s1p = that as Select1Provider<T1>;
        InternalSelectMany2(s1p, collectionSelector, resultSelector);
        if (typeof(TResult) == typeof(T1)) return that as ISelect<TResult>;
        s1p._selectExpression = resultSelector.Body;
        if (s1p._orm.CodeFirst.IsAutoSyncStructure)
            (s1p._orm.CodeFirst as CodeFirstProvider)._dicSycedTryAdd(typeof(TResult)); //._dicSyced.TryAdd(typeof(TResult), true);
        var ret = s1p._orm.Select<TResult>() as Select1Provider<TResult>;
        Select0Provider.CopyData(s1p, ret, null);
        return ret;
    }
    internal static void InternalSelectMany2<T1>(Select1Provider<T1> s1p, LambdaExpression collectionSelector, LambdaExpression resultSelector) where T1 : class
    {
        SelectTableInfo find = null;
        if (collectionSelector.Body.NodeType == ExpressionType.Call)
        {
            var callExp = collectionSelector.Body as MethodCallExpression;
            if (callExp.Method.Name == "DefaultIfEmpty" && callExp.Method.GetGenericArguments().Any())
            {
                find = s1p._tables.Where((a, idx) => idx > 0 && a.Type == SelectTableInfoType.InnerJoin && a.Table.Type == callExp.Method.GetGenericArguments()[0]).LastOrDefault();
                if (find != null)
                {
                    if (!string.IsNullOrEmpty(find.On)) find.On = Regex.Replace(find.On, $@"\b{find.Alias}\.", $"{resultSelector.Parameters[1].Name}.");
                    if (!string.IsNullOrEmpty(find.NavigateCondition)) find.NavigateCondition = Regex.Replace(find.NavigateCondition, $@"\b{find.Alias}\.", $"{resultSelector.Parameters[1].Name}.");
                    find.Type = SelectTableInfoType.LeftJoin;
                    find.Alias = resultSelector.Parameters[1].Name;
                    find.Parameter = resultSelector.Parameters[1];
                }
            }
        }
        if (find == null)
        {
            var tb = s1p._commonUtils.GetTableByEntity(resultSelector.Parameters[1].Type);
            if (tb == null) throw new Exception($"SelectMany 错误的类型：{resultSelector.Parameters[1].Type.FullName}");
            s1p._tables.Add(new SelectTableInfo { Alias = resultSelector.Parameters[1].Name, AliasInit = resultSelector.Parameters[1].Name, Parameter = resultSelector.Parameters[1], Table = tb, Type = SelectTableInfoType.From });
        }
    }

    /// <summary>
    /// 【linq to sql】专用扩展方法，不建议直接使用
    /// </summary>
    public static ISelect<T1> DefaultIfEmpty<T1>(this ISelect<T1> that) where T1 : class
    {
        return that;
    }

    /// <summary>
    /// 【linq to sql】专用扩展方法，不建议直接使用
    /// </summary>
    public static ISelect<T1> ThenBy<T1, TMember>(this ISelect<T1> that, Expression<Func<T1, TMember>> column) where T1 : class => that.OrderBy(column);
    /// <summary>
    /// 【linq to sql】专用扩展方法，不建议直接使用
    /// </summary>
    public static ISelect<T1> ThenByDescending<T1, TMember>(this ISelect<T1> that, Expression<Func<T1, TMember>> column) where T1 : class => that.OrderByDescending(column);
}
