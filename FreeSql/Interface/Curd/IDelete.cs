using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql
{
    public interface IDelete<T1>
    {

        /// <summary>
        /// 指定事务对象
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        IDelete<T1> WithTransaction(DbTransaction transaction);
        /// <summary>
        /// 指定事务对象
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        IDelete<T1> WithConnection(DbConnection connection);
        /// <summary>
        /// 命令超时设置(秒)
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        IDelete<T1> CommandTimeout(int timeout);

        /// <summary>
        /// lambda表达式条件，仅支持实体基础成员（不包含导航对象）<para></para>
        /// 若想使用导航对象，请使用 ISelect.ToDelete() 方法
        /// </summary>
        /// <param name="exp">lambda表达式条件</param>
        /// <returns></returns>
        IDelete<T1> Where(Expression<Func<T1, bool>> exp);
        /// <summary>
        /// lambda表达式条件，仅支持实体基础成员（不包含导航对象）<para></para>
        /// 若想使用导航对象，请使用 ISelect.ToUpdate() 方法
        /// </summary>
        /// <param name="condition">true 时生效</param>
        /// <param name="exp">lambda表达式条件</param>
        /// <returns></returns>
        IDelete<T1> WhereIf(bool condition, Expression<Func<T1, bool>> exp);
        /// <summary>
        /// 原生sql语法条件，Where("id = @id", new { id = 1 })<para></para>
        /// 提示：parms 参数还可以传 Dictionary&lt;string, object&gt;
        /// </summary>
        /// <param name="sql">sql语法条件</param>
        /// <param name="parms">参数</param>
        /// <returns></returns>
        IDelete<T1> Where(string sql, object parms = null);
        /// <summary>
        /// 原生sql语法条件，Where("id = @id", new { id = 1 })<para></para>
        /// 提示：parms 参数还可以传 Dictionary&lt;string, object&gt;
        /// </summary>
        /// <param name="condition">true 时生效</param>
        /// <param name="sql">sql语法条件</param>
        /// <param name="parms">参数</param>
        /// <returns></returns>
        IDelete<T1> WhereIf(bool condition, string sql, object parms = null);
        /// <summary>
        /// 传入实体，将主键作为条件
        /// </summary>
        /// <param name="item">实体</param>
        /// <returns></returns>
        IDelete<T1> Where(T1 item);
        /// <summary>
        /// 传入实体集合，将主键作为条件
        /// </summary>
        /// <param name="items">实体集合</param>
        /// <returns></returns>
        IDelete<T1> Where(IEnumerable<T1> items);
        /// <summary>
        /// 传入动态条件，如：主键值 | new[]{主键值1,主键值2} | TEntity1 | new[]{TEntity1,TEntity2} | new{id=1}
        /// </summary>
        /// <param name="dywhere">主键值、主键值集合、实体、实体集合、匿名对象、匿名对象集合</param>
        /// <param name="not">是否标识为NOT</param>
        /// <returns></returns>
        IDelete<T1> WhereDynamic(object dywhere, bool not = false);

        /// <summary>
        /// 禁用全局过滤功能，不传参数时将禁用所有
        /// </summary>
        /// <param name="name">零个或多个过滤器名字</param>
        /// <returns></returns>
        IDelete<T1> DisableGlobalFilter(params string[] name);

        /// <summary>
        /// 设置表名规则，可用于分库/分表，参数1：默认表名；返回值：新表名；
        /// </summary>
        /// <param name="tableRule"></param>
        /// <returns></returns>
        IDelete<T1> AsTable(Func<string, string> tableRule);
        /// <summary>
        /// 动态Type，在使用 Delete&lt;object&gt; 后使用本方法，指定实体类型
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        IDelete<T1> AsType(Type entityType);
        /// <summary>
        /// 返回即将执行的SQL语句
        /// </summary>
        /// <returns></returns>
        string ToSql();
        /// <summary>
        /// 执行SQL语句，返回影响的行数
        /// </summary>
        /// <returns></returns>
        int ExecuteAffrows();
        /// <summary>
        /// 执行SQL语句，返回被删除的记录<para></para>
        /// 注意：此方法只有 Postgresql/SqlServer 有效果
        /// </summary>
        /// <returns></returns>
        List<T1> ExecuteDeleted();

#if net40
#else
        Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default);
        Task<List<T1>> ExecuteDeletedAsync(CancellationToken cancellationToken = default);
#endif
    }
}