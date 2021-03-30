using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql
{
    public interface ISelectGrouping<TKey, TValue>
    {

#if net40
#else
        Task<long> CountAsync(CancellationToken cancellationToken = default);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TElement>(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, TElement>> elementSelector, CancellationToken cancellationToken = default);
#endif

        /// <summary>
        /// 按聚合条件过滤，Where(a => a.Count() > 10)
        /// </summary>
        /// <param name="exp">lambda表达式</param>
        /// <returns></returns>
        ISelectGrouping<TKey, TValue> Having(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, bool>> exp);

        /// <summary>
        /// 按列排序，OrderBy(a => a.Time)
        /// </summary>
        /// <typeparam name="TMember"></typeparam>
        /// <param name="column"></param>
        /// <returns></returns>
        ISelectGrouping<TKey, TValue> OrderBy<TMember>(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, TMember>> column);
        /// <summary>
        /// 按列倒向排序，OrderByDescending(a => a.Time)
        /// </summary>
        /// <param name="column">列</param>
        /// <returns></returns>
        ISelectGrouping<TKey, TValue> OrderByDescending<TMember>(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, TMember>> column);

        /// <summary>
        /// 执行SQL查询，返回指定字段的记录，记录不存在时返回 Count 为 0 的列表
        /// </summary>
        /// <typeparam name="TReturn">返回类型</typeparam>
        /// <param name="select">选择列</param>
        /// <returns></returns>
        List<TReturn> ToList<TReturn>(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, TReturn>> select);
        Dictionary<TKey, TElement> ToDictionary<TElement>(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, TElement>> elementSelector);

        /// <summary>
        /// 【linq to sql】专用方法，不建议直接使用
        /// </summary>
        List<TReturn> Select<TReturn>(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, TReturn>> select);

        /// <summary>
        /// 返回即将执行的SQL语句
        /// </summary>
        /// <typeparam name="TReturn">返回类型</typeparam>
        /// <param name="select">选择列</param>
        /// <param name="fieldAlias"></param>
        /// <returns></returns>
        string ToSql<TReturn>(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex);
        /// <summary>
        /// 返回即将执行的SQL语句
        /// </summary>
        /// <param name="field">指定字段</param>
        /// <returns></returns>
        string ToSql(string field);


        /// <summary>
        /// 查询向后偏移行数
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        ISelectGrouping<TKey, TValue> Skip(int offset);
        /// <summary>
        /// 查询向后偏移行数
        /// </summary>
        /// <param name="offset">行数</param>
        /// <returns></returns>
        ISelectGrouping<TKey, TValue> Offset(int offset);
        /// <summary>
        /// 查询多少条数据
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        ISelectGrouping<TKey, TValue> Limit(int limit);
        /// <summary>
        /// 查询多少条数据
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        ISelectGrouping<TKey, TValue> Take(int limit);

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="pageNumber">第几页</param>
        /// <param name="pageSize">每页多少</param>
        /// <returns></returns>
        ISelectGrouping<TKey, TValue> Page(int pageNumber, int pageSize);

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="pagingInfo">分页信息</param>
        /// <returns></returns>
        ISelectGrouping<TKey, TValue> Page(BasePagingInfo pagingInfo);

        /// <summary>
        /// 查询的记录数量
        /// </summary>
        /// <returns></returns>
        long Count();
        /// <summary>
        /// 查询的记录数量，以参数out形式返回
        /// </summary>
        /// <param name="count">返回的变量</param>
        /// <returns></returns>
        ISelectGrouping<TKey, TValue> Count(out long count);
    }

    public interface ISelectGroupingAggregate<TKey>
    {
        /// <summary>
        /// 分组的数据
        /// </summary>
        TKey Key { get; set; }
        /// <summary>
        /// 记录总数
        /// </summary>
        /// <returns></returns>
        int Count();
        int Count<T3>(T3 column);
        /// <summary>
        /// 求和
        /// </summary>
        /// <typeparam name="T3"></typeparam>
        /// <param name="column"></param>
        /// <returns></returns>
        decimal Sum<T3>(T3 column);
        /// <summary>
        /// 平均值
        /// </summary>
        /// <typeparam name="T3"></typeparam>
        /// <param name="column"></param>
        /// <returns></returns>
        decimal Avg<T3>(T3 column);
        /// <summary>
        /// 最大值
        /// </summary>
        /// <typeparam name="T3"></typeparam>
        /// <param name="column"></param>
        /// <returns></returns>
        T3 Max<T3>(T3 column);
        /// <summary>
        /// 最小值
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        T3 Min<T3>(T3 column);
    }
    public interface ISelectGroupingAggregate<TKey, TValue> : ISelectGroupingAggregate<TKey>
    {
        /// <summary>
        /// 所有元素
        /// </summary>
        TValue Value { get; set; }
    }
}
