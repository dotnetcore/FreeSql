using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FreeSql
{
    public interface ISelect<T1> : ISelect0<ISelect<T1>, T1>, ILinqToSql<T1> where T1 : class
    {

        /// <summary>
        /// 执行SQL查询，是否有记录
        /// </summary>
        /// <param name="exp">lambda表达式</param>
        /// <returns></returns>
        bool Any(Expression<Func<T1, bool>> exp);
        Task<bool> AnyAsync(Expression<Func<T1, bool>> exp);

        /// <summary>
        /// 执行SQL查询，返回 DataTable
        /// </summary>
        /// <returns></returns>
        DataTable ToDataTable<TReturn>(Expression<Func<T1, TReturn>> select);
        Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<T1, TReturn>> select);

        /// <summary>
        /// 执行SQL查询，返回指定字段的记录，记录不存在时返回 Count 为 0 的列表
        /// </summary>
        /// <typeparam name="TReturn">返回类型</typeparam>
        /// <param name="select">选择列</param>
        /// <returns></returns>
        List<TReturn> ToList<TReturn>(Expression<Func<T1, TReturn>> select);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, TReturn>> select);
        /// <summary>
        /// 执行SQL查询，返回 TDto 映射的字段，记录不存在时返回 Count 为 0 的列表
        /// </summary>
        /// <typeparam name="TDto"></typeparam>
        /// <returns></returns>
        List<TDto> ToList<TDto>();
        Task<List<TDto>> ToListAsync<TDto>();

        /// <summary>
        /// 执行SQL查询，返回指定字段的记录的第一条记录，记录不存在时返回 TReturn 默认值
        /// </summary>
        /// <typeparam name="TReturn">返回类型</typeparam>
        /// <param name="select">选择列</param>
        /// <returns></returns>
        TReturn ToOne<TReturn>(Expression<Func<T1, TReturn>> select);
        Task<TReturn> ToOneAsync<TReturn>(Expression<Func<T1, TReturn>> select);

        /// <summary>
        /// 执行SQL查询，返回指定字段的记录的第一条记录，记录不存在时返回 TReturn 默认值
        /// </summary>
        /// <typeparam name="TReturn">返回类型</typeparam>
        /// <param name="select">选择列</param>
        /// <returns></returns>
        TReturn First<TReturn>(Expression<Func<T1, TReturn>> select);
        Task<TReturn> FirstAsync<TReturn>(Expression<Func<T1, TReturn>> select);

        /// <summary>
        /// 返回即将执行的SQL语句
        /// </summary>
        /// <typeparam name="TReturn">返回类型</typeparam>
        /// <param name="select">选择列</param>
        /// <returns></returns>
        string ToSql<TReturn>(Expression<Func<T1, TReturn>> select);

        /// <summary>
        /// 执行SQL查询，返回指定字段的聚合结果
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="select"></param>
        /// <returns></returns>
        TReturn ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, TReturn>> select);
        Task<TReturn> ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, TReturn>> select);

        /// <summary>
        /// 求和
        /// </summary>
        /// <typeparam name="TMember">返回类型</typeparam>
        /// <param name="column">列</param>
        /// <returns></returns>
        TMember Sum<TMember>(Expression<Func<T1, TMember>> column);
        Task<TMember> SumAsync<TMember>(Expression<Func<T1, TMember>> column);
        /// <summary>
        /// 最小值
        /// </summary>
        /// <typeparam name="TMember">返回类型</typeparam>
        /// <param name="column">列</param>
        /// <returns></returns>
        TMember Min<TMember>(Expression<Func<T1, TMember>> column);
        Task<TMember> MinAsync<TMember>(Expression<Func<T1, TMember>> column);
        /// <summary>
        /// 最大值
        /// </summary>
        /// <typeparam name="TMember">返回类型</typeparam>
        /// <param name="column">列</param>
        /// <returns></returns>
        TMember Max<TMember>(Expression<Func<T1, TMember>> column);
        Task<TMember> MaxAsync<TMember>(Expression<Func<T1, TMember>> column);
        /// <summary>
        /// 平均值
        /// </summary>
        /// <typeparam name="TMember">返回类型</typeparam>
        /// <param name="column">列</param>
        /// <returns></returns>
        TMember Avg<TMember>(Expression<Func<T1, TMember>> column);
        Task<TMember> AvgAsync<TMember>(Expression<Func<T1, TMember>> column);

        /// <summary>
        /// 指定别名
        /// </summary>
        /// <param name="alias">别名</param>
        /// <returns></returns>
        ISelect<T1> As(string alias = "a");

        /// <summary>
        /// 多表查询
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <param name="exp"></param>
        /// <returns></returns>
        ISelect<T1, T2> From<T2>(Expression<Func<ISelectFromExpression<T1>, T2, ISelectFromExpression<T1>>> exp) where T2 : class;
        /// <summary>
        /// 多表查询
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="exp"></param>
        /// <returns></returns>
        ISelect<T1, T2, T3> From<T2, T3>(Expression<Func<ISelectFromExpression<T1>, T2, T3, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class;
        /// <summary>
        /// 多表查询
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="exp"></param>
        /// <returns></returns>
        ISelect<T1, T2, T3, T4> From<T2, T3, T4>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class where T4 : class;
        /// <summary>
        /// 多表查询
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <param name="exp"></param>
        /// <returns></returns>
        ISelect<T1, T2, T3, T4, T5> From<T2, T3, T4, T5>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class where T4 : class where T5 : class;
        /// <summary>
        /// 多表查询
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <param name="exp"></param>
        /// <returns></returns>
        ISelect<T1, T2, T3, T4, T5, T6> From<T2, T3, T4, T5, T6>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class;
        /// <summary>
        /// 多表查询
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <param name="exp"></param>
        /// <returns></returns>
        ISelect<T1, T2, T3, T4, T5, T6, T7> From<T2, T3, T4, T5, T6, T7>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class;
        /// <summary>
        /// 多表查询
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <param name="exp"></param>
        /// <returns></returns>
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> From<T2, T3, T4, T5, T6, T7, T8>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class;
        /// <summary>
        /// 多表查询
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <param name="exp"></param>
        /// <returns></returns>
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T2, T3, T4, T5, T6, T7, T8, T9>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class;
        /// <summary>
        /// 多表查询
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <param name="exp"></param>
        /// <returns></returns>
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T2, T3, T4, T5, T6, T7, T8, T9, T10>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class;

        /// <summary>
        /// 查询条件，Where(a => a.Id > 10)，支持导航对象查询，Where(a => a.Author.Email == "2881099@qq.com")
        /// </summary>
        /// <param name="exp">lambda表达式</param>
        /// <returns></returns>
        ISelect<T1> Where(Expression<Func<T1, bool>> exp);
        /// <summary>
        /// 查询条件，Where(true, a => a.Id > 10)，支导航对象查询，Where(true, a => a.Author.Email == "2881099@qq.com")
        /// </summary>
        /// <param name="condition">true 时生效</param>
        /// <param name="exp">lambda表达式</param>
        /// <returns></returns>
        ISelect<T1> WhereIf(bool condition, Expression<Func<T1, bool>> exp);
        /// <summary>
        /// 多表条件查询
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <param name="exp">lambda表达式</param>
        /// <returns></returns>
        ISelect<T1> Where<T2>(Expression<Func<T1, T2, bool>> exp) where T2 : class;
        /// <summary>
        /// 多表条件查询
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <param name="exp">lambda表达式</param>
        /// <returns></returns>
        ISelect<T1> Where<T2>(Expression<Func<T2, bool>> exp) where T2 : class;
        /// <summary>
        /// 多表条件查询
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="exp">lambda表达式</param>
        /// <returns></returns>
        ISelect<T1> Where<T2, T3>(Expression<Func<T1, T2, T3, bool>> exp) where T2 : class where T3 : class;
        /// <summary>
        /// 多表条件查询
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="exp">lambda表达式</param>
        /// <returns></returns>
        ISelect<T1> Where<T2, T3, T4>(Expression<Func<T1, T2, T3, T4, bool>> exp) where T2 : class where T3 : class where T4 : class;
        /// <summary>
        /// 多表条件查询
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="exp">lambda表达式</param>
        /// <returns></returns>
        ISelect<T1> Where<T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, bool>> exp) where T2 : class where T3 : class where T4 : class where T5 : class;
        /// <summary>
        /// 传入动态对象如：主键值 | new[]{主键值1,主键值2} | TEntity1 | new[]{TEntity1,TEntity2} | new{id=1}
        /// </summary>
        /// <param name="dywhere">主键值、主键值集合、实体、实体集合、匿名对象、匿名对象集合</param>
        /// <returns></returns>
        ISelect<T1> WhereDynamic(object dywhere);

        /// <summary>
        /// 多表查询时，该方法标记后，表达式条件将对所有表进行附加
        /// <para></para>
        /// 例如：软删除、租户，每个表都给条件，挺麻烦的
        /// <para></para>
        /// fsql.Select&lt;T1&gt;().LeftJoin&lt;T2&gt;(...).Where&lt;T2&gt;((t1, t2 => t1.IsDeleted == false &amp;&amp; t2.IsDeleted == false)
        /// <para></para>
        /// 修改：fsql.Select&lt;T1&gt;().LeftJoin&lt;T2&gt;(...).WhereCascade(t1 => t1.IsDeleted == false)
        /// <para></para>
        /// 当其中的实体可附加表达式才会进行，表越多时收益越大
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        ISelect<T1> WhereCascade(Expression<Func<T1, bool>> exp);

        /// <summary>
        /// 按选择的列分组，GroupBy(a => a.Name) | GroupBy(a => new{a.Name,a.Time})
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        ISelectGrouping<TKey, T1> GroupBy<TKey>(Expression<Func<T1, TKey>> exp);

        /// <summary>
        /// 按列排序，OrderBy(a => a.Time)
        /// </summary>
        /// <typeparam name="TMember"></typeparam>
        /// <param name="column"></param>
        /// <returns></returns>
        ISelect<T1> OrderBy<TMember>(Expression<Func<T1, TMember>> column);
        /// <summary>
        /// 按列排序，OrderBy(true, a => a.Time)
        /// </summary>
        /// <typeparam name="TMember"></typeparam>
        /// <param name="condition">true 时生效</param>
        /// <param name="column"></param>
        /// <returns></returns>
        ISelect<T1> OrderBy<TMember>(bool condition, Expression<Func<T1, TMember>> column);
        /// <summary>
        /// 按列倒向排序，OrderByDescending(a => a.Time)
        /// </summary>
        /// <param name="column">列</param>
        /// <returns></returns>
        ISelect<T1> OrderByDescending<TMember>(Expression<Func<T1, TMember>> column);
        /// <summary>
        /// 按列倒向排序，OrderByDescending(true, a => a.Time)
        /// </summary>
        /// <param name="condition">true 时生效</param>
        /// <param name="column">列</param>
        /// <returns></returns>
        ISelect<T1> OrderByDescending<TMember>(bool condition, Expression<Func<T1, TMember>> column);

        /// <summary>
        /// 贪婪加载导航属性，如果查询中已经使用了 a.Parent.Parent 类似表达式，则可以无需此操作
        /// </summary>
        /// <typeparam name="TNavigate"></typeparam>
        /// <param name="navigateSelector">选择一个导航属性</param>
        /// <returns></returns>
        ISelect<T1> Include<TNavigate>(Expression<Func<T1, TNavigate>> navigateSelector) where TNavigate : class;
        /// <summary>
        /// 贪婪加载集合的导航属性，其实是分两次查询，ToList 后进行了数据重装
        /// </summary>
        /// <typeparam name="TNavigate"></typeparam>
        /// <param name="navigateSelector">选择一个集合的导航属性，也可通过 .Where 设置临时的关系映射，还可以 .Take(5) 每个子集合只取5条</param>
        /// <param name="then">即能 ThenInclude，还可以二次过滤（这个 EFCore 做不到？）</param>
        /// <returns></returns>
        ISelect<T1> IncludeMany<TNavigate>(Expression<Func<T1, IEnumerable<TNavigate>>> navigateSelector, Action<ISelect<TNavigate>> then = null) where TNavigate : class;
    }
}