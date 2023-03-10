using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql
{
    public interface IUpdateJoin<T1, T2>
    {
        /// <summary>
        /// 指定事务对象
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        IUpdateJoin<T1, T2> WithTransaction(DbTransaction transaction);
        /// <summary>
        /// 指定事务对象
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        IUpdateJoin<T1, T2> WithConnection(DbConnection connection);
        /// <summary>
        /// 命令超时设置(秒)
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        IUpdateJoin<T1, T2> CommandTimeout(int timeout);

        /// <summary>
        /// 设置列的固定新值，Set(a => a.Name, "newvalue")
        /// </summary>
        /// <typeparam name="TMember"></typeparam>
        /// <param name="column">lambda选择列</param>
        /// <param name="value">新值</param>
        /// <returns></returns>
        IUpdateJoin<T1, T2> Set<TMember>(Expression<Func<T1, TMember>> column, TMember value);
        /// <summary>
        /// 设置列的固定新值，Set(a => a.Name, "newvalue")
        /// </summary>
        /// <typeparam name="TMember"></typeparam>
        /// <param name="condition">true 时生效</param>
        /// <param name="column">lambda选择列</param>
        /// <param name="value">新值</param>
        /// <returns></returns>
        IUpdateJoin<T1, T2> SetIf<TMember>(bool condition, Expression<Func<T1, TMember>> column, TMember value);
        /// <summary>
        /// 设置列的联表值，格式：<para></para>
        /// Set((a, b) => a.Clicks == b.xxx)<para></para>
        /// Set((a, b) => a.Clicks == a.Clicks + 1)
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        IUpdateJoin<T1, T2> Set(Expression<Func<T1, T2, bool>> exp);
        /// <summary>
        /// 设置列的联表值，格式：<para></para>
        /// Set((a, b) => a.Clicks == b.xxx)<para></para>
        /// Set((a, b) => a.Clicks == a.Clicks + 1)
        /// <para></para>
        /// </summary>
        /// <param name="condition">true 时生效</param>
        /// <param name="exp"></param>
        /// <returns></returns>
        IUpdateJoin<T1, T2> SetIf(bool condition, Expression<Func<T1, T2, bool>> exp);
        /// <summary>
        /// 设置值，自定义SQL语法，SetRaw("title = @title", new { title = "newtitle" })<para></para>
        /// 提示：parms 参数还可以传 Dictionary&lt;string, object&gt;
        /// </summary>
        /// <param name="sql">sql语法</param>
        /// <param name="parms">参数</param>
        /// <returns></returns>
        IUpdateJoin<T1, T2> SetRaw(string sql, object parms = null);

        /// <summary>
        /// lambda表达式条件，仅支持实体基础成员（不包含导航对象）
        /// </summary>
        /// <param name="exp">lambda表达式条件</param>
        /// <returns></returns>
        IUpdateJoin<T1, T2> Where(Expression<Func<T1, T2, bool>> exp);
        /// <summary>
        /// lambda表达式条件，仅支持实体基础成员（不包含导航对象）
        /// </summary>
        /// <param name="condition">true 时生效</param>
        /// <param name="exp">lambda表达式条件</param>
        /// <returns></returns>
        IUpdateJoin<T1, T2> WhereIf(bool condition, Expression<Func<T1, T2, bool>> exp);
        /// <summary>
        /// 原生sql语法条件，Where("id = @id", new { id = 1 })<para></para>
        /// 提示：parms 参数还可以传 Dictionary&lt;string, object&gt;
        /// </summary>
        /// <param name="sql">sql语法条件</param>
        /// <param name="parms">参数</param>
        /// <returns></returns>
        IUpdateJoin<T1, T2> Where(string sql, object parms = null);

        /// <summary>
        /// 禁用全局过滤功能，不传参数时将禁用所有
        /// </summary>
        /// <param name="name">零个或多个过滤器名字</param>
        /// <returns></returns>
        IUpdateJoin<T1, T2> DisableGlobalFilter(params string[] name);

        /// <summary>
        /// 设置表名
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        IUpdateJoin<T1, T2> AsTable(string tableName);
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

#if net40
#else
        Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default);
#endif
    }
}