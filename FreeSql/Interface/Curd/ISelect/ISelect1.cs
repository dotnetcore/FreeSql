using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql
{
    public interface ISelect<T1> : ISelect0<ISelect<T1>, T1>
    {

#if net40
#else
        Task<bool> AnyAsync(Expression<Func<T1, bool>> exp, CancellationToken cancellationToken = default);

        Task<int> InsertIntoAsync<TTargetEntity>(string tableName, Expression<Func<T1, TTargetEntity>> select, CancellationToken cancellationToken = default) where TTargetEntity : class;
        Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<T1, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, TReturn>> select, CancellationToken cancellationToken = default);
        Task<List<TDto>> ToListAsync<TDto>(CancellationToken cancellationToken = default);
        
        Task<TReturn> ToOneAsync<TReturn>(Expression<Func<T1, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TDto> ToOneAsync<TDto>(CancellationToken cancellationToken = default);
        Task<TReturn> FirstAsync<TReturn>(Expression<Func<T1, TReturn>> select, CancellationToken cancellationToken = default);
        Task<TDto> FirstAsync<TDto>(CancellationToken cancellationToken = default);
        
        Task<TReturn> ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, TReturn>> select, CancellationToken cancellationToken = default);
        Task<decimal> SumAsync<TMember>(Expression<Func<T1, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MinAsync<TMember>(Expression<Func<T1, TMember>> column, CancellationToken cancellationToken = default);
        Task<TMember> MaxAsync<TMember>(Expression<Func<T1, TMember>> column, CancellationToken cancellationToken = default);
        Task<double> AvgAsync<TMember>(Expression<Func<T1, TMember>> column, CancellationToken cancellationToken = default);
#endif

        /// <summary>
        /// 执行SQL查询，是否有记录
        /// </summary>
        /// <param name="exp">lambda表达式</param>
        /// <returns></returns>
        bool Any(Expression<Func<T1, bool>> exp);

        /// <summary>
        /// 将查询转换为 INSERT INTO tableName SELECT ... FROM t 执行插入
        /// </summary>
        /// <typeparam name="TTargetEntity"></typeparam>
        /// <param name="tableName">指定插入的表名，若为 null 则使用 TTargetEntity 实体表名</param>
        /// <param name="select">选择列</param>
        /// <returns>返回影响的行数</returns>
        int InsertInto<TTargetEntity>(string tableName, Expression<Func<T1, TTargetEntity>> select) where TTargetEntity : class;

        /// <summary>
        /// 执行SQL查询，返回 DataTable
        /// </summary>
        /// <returns></returns>
        DataTable ToDataTable<TReturn>(Expression<Func<T1, TReturn>> select);

        /// <summary>
        /// 执行SQL查询，返回指定字段的记录，记录不存在时返回 Count 为 0 的列表
        /// </summary>
        /// <typeparam name="TReturn">返回类型</typeparam>
        /// <param name="select">选择列</param>
        /// <returns></returns>
        List<TReturn> ToList<TReturn>(Expression<Func<T1, TReturn>> select);
        /// <summary>
        /// 执行SQL查询，返回 TDto 映射的字段，记录不存在时返回 Count 为 0 的列表
        /// </summary>
        /// <typeparam name="TDto"></typeparam>
        /// <returns></returns>
        List<TDto> ToList<TDto>();
        /// <summary>
        /// 执行SQL查询，分块返回数据，可减少内存开销。比如读取10万条数据，每次返回100条处理。
        /// </summary>
        /// <typeparam name="TReturn">返回类型</typeparam>
        /// <param name="select">选择列</param>
        /// <param name="size">数据块的大小</param>
        /// <param name="done">处理数据块</param>
        void ToChunk<TReturn>(Expression<Func<T1, TReturn>> select, int size, Action<FetchCallbackArgs<List<TReturn>>> done);

        /// <summary>
        /// 执行SQL查询，返回指定字段的记录的第一条记录，记录不存在时返回 TReturn 默认值
        /// </summary>
        /// <typeparam name="TReturn">返回类型</typeparam>
        /// <param name="select">选择列</param>
        /// <returns></returns>
        TReturn ToOne<TReturn>(Expression<Func<T1, TReturn>> select);
        /// <summary>
        /// 执行SQL查询，返回 TDto 映射的字段，记录不存在时返回 Dto 默认值
        /// </summary>
        /// <typeparam name="TDto"></typeparam>
        /// <returns></returns>
        TDto ToOne<TDto>();

        /// <summary>
        /// 执行SQL查询，返回指定字段的记录的第一条记录，记录不存在时返回 TReturn 默认值
        /// </summary>
        /// <typeparam name="TReturn">返回类型</typeparam>
        /// <param name="select">选择列</param>
        /// <returns></returns>
        TReturn First<TReturn>(Expression<Func<T1, TReturn>> select);
        /// <summary>
        /// 执行SQL查询，返回 TDto 映射的字段，记录不存在时返回 Dto 默认值
        /// </summary>
        /// <typeparam name="TDto"></typeparam>
        /// <returns></returns>
        TDto First<TDto>();

        /// <summary>
        /// 返回即将执行的SQL语句
        /// </summary>
        /// <typeparam name="TReturn">返回类型</typeparam>
        /// <param name="select">选择列</param>
        /// <param name="fieldAlias">字段别名</param>
        /// <returns></returns>
        string ToSql<TReturn>(Expression<Func<T1, TReturn>> select, FieldAliasOptions fieldAlias = FieldAliasOptions.AsIndex);

        /// <summary>
        /// 执行SQL查询，返回指定字段的聚合结果
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="select"></param>
        /// <returns></returns>
        TReturn ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, TReturn>> select);
        /// <summary>
        /// 执行SQL查询，返回指定字段的聚合结果给 output 参数<para></para>
        /// fsql.Select&lt;T&gt;()<para></para>
        /// .Aggregate(a =&gt; new { count = a.Count, sum = a.Sum(a.Key.Price) }, out var agg)<para></para>
        /// .Page(1, 10).ToList();
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="select"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        ISelect<T1> Aggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, TReturn>> select, out TReturn result);

        /// <summary>
        /// 求和
        /// </summary>
        /// <typeparam name="TMember">返回类型</typeparam>
        /// <param name="column">列</param>
        /// <returns></returns>
        decimal Sum<TMember>(Expression<Func<T1, TMember>> column);
        /// <summary>
        /// 最小值
        /// </summary>
        /// <typeparam name="TMember">返回类型</typeparam>
        /// <param name="column">列</param>
        /// <returns></returns>
        TMember Min<TMember>(Expression<Func<T1, TMember>> column);
        /// <summary>
        /// 最大值
        /// </summary>
        /// <typeparam name="TMember">返回类型</typeparam>
        /// <param name="column">列</param>
        /// <returns></returns>
        TMember Max<TMember>(Expression<Func<T1, TMember>> column);
        /// <summary>
        /// 平均值
        /// </summary>
        /// <typeparam name="TMember">返回类型</typeparam>
        /// <param name="column">列</param>
        /// <returns></returns>
        double Avg<TMember>(Expression<Func<T1, TMember>> column);

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
        ISelect<T1, T2, T3> From<T2, T3>(Expression<Func<ISelectFromExpression<T1>, T2, T3, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class;
        ISelect<T1, T2, T3, T4> From<T2, T3, T4>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class where T4 : class;
        ISelect<T1, T2, T3, T4, T5> From<T2, T3, T4, T5>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class where T4 : class where T5 : class;
        ISelect<T1, T2, T3, T4, T5, T6> From<T2, T3, T4, T5, T6>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class;
        ISelect<T1, T2, T3, T4, T5, T6, T7> From<T2, T3, T4, T5, T6, T7>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class;
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8> From<T2, T3, T4, T5, T6, T7, T8>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class;
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T2, T3, T4, T5, T6, T7, T8, T9>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class;
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T2, T3, T4, T5, T6, T7, T8, T9, T10>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class;
        
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class;
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class;
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class where T13 : class;
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class where T13 : class where T14 : class;
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class where T13 : class where T14 : class where T15 : class;
        ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> From<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class where T11 : class where T12 : class where T13 : class where T14 : class where T15 : class where T16 : class;

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
        /// <typeparam name="T5"></typeparam>
        /// <param name="exp">lambda表达式</param>
        /// <returns></returns>
        ISelect<T1> Where<T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, bool>> exp) where T2 : class where T3 : class where T4 : class where T5 : class;
        /// <summary>
        /// 传入动态条件，如：主键值 | new[]{主键值1,主键值2} | TEntity1 | new[]{TEntity1,TEntity2} | new{id=1}
        /// </summary>
        /// <param name="dywhere">主键值、主键值集合、实体、实体集合、匿名对象、匿名对象集合</param>
        /// <param name="not">是否标识为NOT</param>
        /// <returns></returns>
        ISelect<T1> WhereDynamic(object dywhere, bool not = false);

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
        /// 按列排序，OrderByIf(true, a => a.Time)
        /// </summary>
        /// <typeparam name="TMember"></typeparam>
        /// <param name="condition">true 时生效</param>
        /// <param name="column"></param>
        /// <param name="descending">true: DESC, false: ASC</param>
        /// <returns></returns>
        ISelect<T1> OrderByIf<TMember>(bool condition, Expression<Func<T1, TMember>> column, bool descending = false);
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
        /// 贪婪加载导航属性，如果查询中已经使用了 a.Parent.Parent 类似表达式，则可以无需此操作
        /// </summary>
        /// <typeparam name="TNavigate"></typeparam>
        /// <param name="condition">true 时生效</param>
        /// <param name="navigateSelector">选择一个导航属性</param>
        /// <returns></returns>
        ISelect<T1> IncludeIf<TNavigate>(bool condition, Expression<Func<T1, TNavigate>> navigateSelector) where TNavigate : class;
        /// <summary>
        /// 贪婪加载集合的导航属性，其实是分两次查询，ToList 后进行了数据重装<para></para>
        /// 文档：https://github.com/2881099/FreeSql/wiki/%e8%b4%aa%e5%a9%aa%e5%8a%a0%e8%bd%bd#%E5%AF%BC%E8%88%AA%E5%B1%9E%E6%80%A7-onetomanymanytomany
        /// </summary>
        /// <typeparam name="TNavigate"></typeparam>
        /// <param name="navigateSelector">选择一个集合的导航属性，如： .IncludeMany(a => a.Tags)<para></para>
        /// 可以 .Where 设置临时的关系映射，如： .IncludeMany(a => a.Tags.Where(tag => tag.TypeId == a.Id))<para></para>
        /// 可以 .Take(5) 每个子集合只取5条，如： .IncludeMany(a => a.Tags.Take(5))<para></para>
        /// 可以 .Select 设置只查询部分字段，如： (a => new TNavigate { Title = a.Title }) 
        /// </param>
        /// <param name="then">即能 ThenInclude，还可以二次过滤（这个 EFCore 做不到？）</param>
        /// <returns></returns>
        ISelect<T1> IncludeMany<TNavigate>(Expression<Func<T1, IEnumerable<TNavigate>>> navigateSelector, Action<ISelect<TNavigate>> then = null) where TNavigate : class;

        /// <summary>
        /// 按属性名字符串进行 Include/IncludeMany 操作
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        ISelect<T1> IncludeByPropertyName(string property);
        /// <summary>
        /// 按属性名字符串进行 Include/IncludeMany 操作
        /// </summary>
        /// <param name="condition">true 时生效</param>
        /// <param name="property"></param>
        /// <returns></returns>
        ISelect<T1> IncludeByPropertyNameIf(bool condition, string property);

        /// <summary>
        /// 实现 select .. from ( select ... from t ) a 这样的功能<para></para>
        /// 使用 AsTable 方法也可以达到效果<para></para>
        /// 示例：WithSql("select * from id=@id", new { id = 1 })<para></para>
        /// 提示：parms 参数还可以传 Dictionary&lt;string, object&gt;
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parms">参数</param>
        /// <returns></returns>
        ISelect<T1> WithSql(string sql, object parms = null);
    }
}