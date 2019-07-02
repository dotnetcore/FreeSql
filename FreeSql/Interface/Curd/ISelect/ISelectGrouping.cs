using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql
{
    public interface ISelectGrouping<TKey, TValue>
    {
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
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, TReturn>> select);

        /// <summary>
        /// 【linq to sql】专用方法，不建议直接使用
        /// </summary>
        List<TReturn> Select<TReturn>(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, TReturn>> select);

        /// <summary>
        /// 返回即将执行的SQL语句
        /// </summary>
        /// <typeparam name="TReturn">返回类型</typeparam>
        /// <param name="select">选择列</param>
        /// <returns></returns>
        string ToSql<TReturn>(Expression<Func<ISelectGroupingAggregate<TKey, TValue>, TReturn>> select);
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
        /// <summary>
        /// 求和
        /// </summary>
        /// <typeparam name="T3"></typeparam>
        /// <param name="column"></param>
        /// <returns></returns>
        T3 Sum<T3>(T3 column);
        /// <summary>
        /// 平均值
        /// </summary>
        /// <typeparam name="T3"></typeparam>
        /// <param name="column"></param>
        /// <returns></returns>
        T3 Avg<T3>(T3 column);
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
