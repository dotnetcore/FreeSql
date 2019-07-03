using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql
{
    public interface ISelect0<TSelect, T1>
    {

        /// <summary>
        /// 指定事务对象
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        TSelect WithTransaction(DbTransaction transaction);
        /// <summary>
        /// 指定连接对象
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        TSelect WithConnection(DbConnection connection);

        /// <summary>
        /// 审核或跟踪 ToList 即将返回的数据
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        TSelect TrackToList(Action<object> action);

        /// <summary>
        /// 执行SQL查询，返回 DataTable
        /// </summary>
        /// <returns></returns>
        DataTable ToDataTable(string field = null);
        Task<DataTable> ToDataTableAsync(string field = null);

        /// <summary>
        /// 执行SQL查询，返回 T1 实体所有字段的记录，记录不存在时返回 Count 为 0 的列表
        /// </summary>
        /// <param name="includeNestedMembers">false: 返回 2级 LeftJoin/InnerJoin/RightJoin 对象；true: 返回所有 LeftJoin/InnerJoin/RightJoin 的导航数据</param>
        /// <returns></returns>
        List<T1> ToList(bool includeNestedMembers = false);
        Task<List<T1>> ToListAsync(bool includeNestedMembers = false);
        /// <summary>
        /// 执行SQL查询，返回 field 指定字段的记录，并以元组或基础类型(int,string,long)接收，记录不存在时返回 Count 为 0 的列表
        /// </summary>
        /// <typeparam name="TTuple"></typeparam>
        /// <param name="field"></param>
        /// <returns></returns>
        List<TTuple> ToList<TTuple>(string field);
        Task<List<TTuple>> ToListAsync<TTuple>(string field);
        /// <summary>
        /// 执行SQL查询，返回 T1 实体所有字段的第一条记录，记录不存在时返回 null
        /// </summary>
        /// <returns></returns>
        T1 ToOne();
        Task<T1> ToOneAsync();

        /// <summary>
        /// 执行SQL查询，返回 T1 实体所有字段的第一条记录，记录不存在时返回 null
        /// </summary>
        /// <returns></returns>
        T1 First();
        Task<T1> FirstAsync();

        /// <summary>
        /// 设置表名规则，可用于分库/分表，参数1：实体类型；参数2：默认表名；返回值：新表名；
        /// </summary>
        /// <param name="tableRule"></param>
        /// <returns></returns>
        TSelect AsTable(Func<Type, string, string> tableRule);
        /// <summary>
        /// 动态Type，在使用 Select&lt;object&gt; 后使用本方法，指定实体类型
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        TSelect AsType(Type entityType);
        /// <summary>
        /// 返回即将执行的SQL语句
        /// </summary>
        /// <param name="field">指定字段</param>
        /// <returns></returns>
        string ToSql(string field = null);
        /// <summary>
        /// 执行SQL查询，是否有记录
        /// </summary>
        /// <returns></returns>
        bool Any();
        Task<bool> AnyAsync();

        /// <summary>
        /// 查询的记录数量
        /// </summary>
        /// <returns></returns>
        long Count();
        Task<long> CountAsync();
        /// <summary>
        /// 查询的记录数量，以参数out形式返回
        /// </summary>
        /// <param name="count">返回的变量</param>
        /// <returns></returns>
        TSelect Count(out long count);

        /// <summary>
        /// 指定从主库查询（默认查询从库）
        /// </summary>
        /// <returns></returns>
        TSelect Master();

        /// <summary>
        /// 左联查询，使用导航属性自动生成SQL
        /// </summary>
        /// <param name="exp">表达式</param>
        /// <returns></returns>
        TSelect LeftJoin(Expression<Func<T1, bool>> exp);
        /// <summary>
        /// 联接查询，使用导航属性自动生成SQL
        /// </summary>
        /// <param name="exp">表达式</param>
        /// <returns></returns>
        TSelect InnerJoin(Expression<Func<T1, bool>> exp);
        /// <summary>
        /// 右联查询，使用导航属性自动生成SQL
        /// </summary>
        /// <param name="exp">表达式</param>
        /// <returns></returns>
        TSelect RightJoin(Expression<Func<T1, bool>> exp);
        /// <summary>
        /// 左联查询，指定关联的实体类型
        /// </summary>
        /// <typeparam name="T2">关联的实体类型</typeparam>
        /// <param name="exp">表达式</param>
        /// <returns></returns>
        TSelect LeftJoin<T2>(Expression<Func<T1, T2, bool>> exp);
        /// <summary>
        /// 联接查询，指定关联的实体类型
        /// </summary>
        /// <typeparam name="T2">关联的实体类型</typeparam>
        /// <param name="exp">表达式</param>
        /// <returns></returns>
        TSelect InnerJoin<T2>(Expression<Func<T1, T2, bool>> exp);
        /// <summary>
        /// 右联查询，指定关联的实体类型
        /// </summary>
        /// <typeparam name="T2">关联的实体类型</typeparam>
        /// <param name="exp">表达式</param>
        /// <returns></returns>
        TSelect RightJoin<T2>(Expression<Func<T1, T2, bool>> exp);

        /// <summary>
        /// 左联查询，使用原生sql语法，LeftJoin("type b on b.id = a.id and b.clicks > ?clicks", new { clicks = 1 })
        /// </summary>
        /// <param name="sql">sql语法条件</param>
        /// <param name="parms">参数</param>
        /// <returns></returns>
        TSelect LeftJoin(string sql, object parms = null);
        /// <summary>
        /// 联接查询，使用原生sql语法，InnerJoin("type b on b.id = a.id and b.clicks > ?clicks", new { clicks = 1 })
        /// </summary>
        /// <param name="sql">sql语法条件</param>
        /// <param name="parms">参数</param>
        /// <returns></returns>
        TSelect InnerJoin(string sql, object parms = null);
        /// <summary>
        /// 右联查询，使用原生sql语法，RightJoin("type b on b.id = a.id and b.clicks > ?clicks", new { clicks = 1 })
        /// </summary>
        /// <param name="sql">sql语法条件</param>
        /// <param name="parms">参数</param>
        /// <returns></returns>
        TSelect RightJoin(string sql, object parms = null);

        /// <summary>
        /// 原生sql语法条件，Where("id = ?id", new { id = 1 })
        /// </summary>
        /// <param name="sql">sql语法条件</param>
        /// <param name="parms">参数</param>
        /// <returns></returns>
        TSelect Where(string sql, object parms = null);
        /// <summary>
        /// 原生sql语法条件，WhereIf(true, "id = ?id", new { id = 1 })
        /// </summary>
        /// <param name="condition">true 时生效</param>
        /// <param name="sql">sql语法条件</param>
        /// <param name="parms">参数</param>
        /// <returns></returns>
        TSelect WhereIf(bool condition, string sql, object parms = null);

        /// <summary>
        /// 按原生sql语法分组，GroupBy("concat(name, ?cc)", new { cc = 1 })
        /// </summary>
        /// <param name="sql">sql语法</param>
        /// <param name="parms">参数</param>
        /// <returns></returns>
        TSelect GroupBy(string sql, object parms = null);
        /// <summary>
        /// 按原生sql语法聚合条件过滤，Having("count(name) = ?cc", new { cc = 1 })
        /// </summary>
        /// <param name="sql">sql语法条件</param>
        /// <param name="parms">参数</param>
        /// <returns></returns>
        TSelect Having(string sql, object parms = null);

        /// <summary>
        /// 按原生sql语法排序，OrderBy("count(name) + ?cc desc", new { cc = 1 })
        /// </summary>
        /// <param name="sql">sql语法</param>
        /// <param name="parms">参数</param>
        /// <returns></returns>
        TSelect OrderBy(string sql, object parms = null);
        /// <summary>
        /// 按原生sql语法排序，OrderBy(true, "count(name) + ?cc desc", new { cc = 1 })
        /// </summary>
        /// <param name="condition">true 时生效</param>
        /// <param name="sql">sql语法</param>
        /// <param name="parms">参数</param>
        /// <returns></returns>
        TSelect OrderBy(bool condition, string sql, object parms = null);

        /// <summary>
        /// 查询向后偏移行数
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        TSelect Skip(int offset);
        /// <summary>
        /// 查询向后偏移行数
        /// </summary>
        /// <param name="offset">行数</param>
        /// <returns></returns>
        TSelect Offset(int offset);
        /// <summary>
        /// 查询多少条数据
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        TSelect Limit(int limit);
        /// <summary>
        /// 查询多少条数据
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        TSelect Take(int limit);

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="pageNumber">第几页</param>
        /// <param name="pageSize">每页多少</param>
        /// <returns></returns>
        TSelect Page(int pageNumber, int pageSize);

        /// <summary>
        /// 查询数据前，去重
        /// <para>
        /// .Distinct().ToList(x => x.GroupName) 对指定字段去重
        /// </para>
        /// <para>
        /// .Distinct().ToList() 对整个查询去重
        /// </para>
        /// </summary>
        /// <returns></returns>
        TSelect Distinct();
    }
}
