using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FreeSql
{
    public interface IUpdate<T1> where T1 : class
    {

        /// <summary>
        /// 指定事务对象
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        IUpdate<T1> WithTransaction(DbTransaction transaction);
        /// <summary>
        /// 指定事务对象
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        IUpdate<T1> WithConnection(DbConnection connection);

        /// <summary>
        /// 不使用参数化，可通过 IFreeSql.CodeFirst.IsNotCommandParameter 全局性设置
        /// </summary>
        /// <returns></returns>
        IUpdate<T1> NoneParameter();

        /// <summary>
        /// 更新数据，设置更新的实体
        /// </summary>
        /// <param name="source">实体</param>
        /// <returns></returns>
        IUpdate<T1> SetSource(T1 source);
        /// <summary>
        /// 更新数据，设置更新的实体集合
        /// </summary>
        /// <param name="source">实体集合</param>
        /// <returns></returns>
        IUpdate<T1> SetSource(IEnumerable<T1> source);
        /// <summary>
        /// 忽略的列，IgnoreColumns(a => a.Name) | IgnoreColumns(a => new{a.Name,a.Time}) | IgnoreColumns(a => new[]{"name","time"})
        /// </summary>
        /// <param name="columns">lambda选择列</param>
        /// <returns></returns>
        IUpdate<T1> IgnoreColumns(Expression<Func<T1, object>> columns);
        /// <summary>
        /// 忽略的列
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        IUpdate<T1> IgnoreColumns(string[] columns);

        /// <summary>
        /// 指定的列，UpdateColumns(a => a.Name) | UpdateColumns(a => new{a.Name,a.Time}) | UpdateColumns(a => new[]{"name","time"})
        /// </summary>
        /// <param name="columns">lambda选择列</param>
        /// <returns></returns>
        IUpdate<T1> UpdateColumns(Expression<Func<T1, object>> columns);
        /// <summary>
        /// 指定的列
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        IUpdate<T1> UpdateColumns(string[] columns);

        /// <summary>
        /// 设置列的新值，Set(a => a.Name, "newvalue")
        /// </summary>
        /// <typeparam name="TMember"></typeparam>
        /// <param name="column">lambda选择列</param>
        /// <param name="value">新值</param>
        /// <returns></returns>
        IUpdate<T1> Set<TMember>(Expression<Func<T1, TMember>> column, TMember value);
        /// <summary>
        /// 设置列的的新值为基础上增加，格式：Set(a => a.Clicks + 1) 相当于 clicks=clicks+1
        /// <para></para>
        /// 指定更新，格式：Set(a => new { Clicks = a.Clicks + 1, Time = DateTime.Now }) 相当于 set clicks=clicks+1,time='2019-06-19....'
        /// </summary>
        /// <typeparam name="TMember"></typeparam>
        /// <param name="exp"></param>
        /// <returns></returns>
        IUpdate<T1> Set<TMember>(Expression<Func<T1, TMember>> exp);
        /// <summary>
        /// 设置值，自定义SQL语法，SetRaw("title = ?title", new { title = "newtitle" })
        /// </summary>
        /// <param name="sql">sql语法</param>
        /// <param name="parms">参数</param>
        /// <returns></returns>
        IUpdate<T1> SetRaw(string sql, object parms = null);

        /// <summary>
        /// lambda表达式条件，仅支持实体基础成员（不包含导航对象）
        /// </summary>
        /// <param name="exp">lambda表达式条件</param>
        /// <returns></returns>
        IUpdate<T1> Where(Expression<Func<T1, bool>> exp);
        /// <summary>
        /// 原生sql语法条件，Where("id = ?id", new { id = 1 })
        /// </summary>
        /// <param name="sql">sql语法条件</param>
        /// <param name="parms">参数</param>
        /// <returns></returns>
        IUpdate<T1> Where(string sql, object parms = null);
        /// <summary>
        /// 传入实体，将主键作为条件
        /// </summary>
        /// <param name="item">实体</param>
        /// <returns></returns>
        IUpdate<T1> Where(T1 item);
        /// <summary>
        /// 传入实体集合，将主键作为条件
        /// </summary>
        /// <param name="items">实体集合</param>
        /// <returns></returns>
        IUpdate<T1> Where(IEnumerable<T1> items);
        /// <summary>
        /// 子查询是否存在
        /// </summary>
        /// <typeparam name="TEntity2"></typeparam>
        /// <param name="select">子查询</param>
        /// <param name="notExists">不存在</param>
        /// <returns></returns>
        IUpdate<T1> WhereExists<TEntity2>(ISelect<TEntity2> select, bool notExists = false) where TEntity2 : class;
        /// <summary>
        /// 传入动态对象如：主键值 | new[]{主键值1,主键值2} | TEntity1 | new[]{TEntity1,TEntity2} | new{id=1}
        /// </summary>
        /// <param name="dywhere">主键值、主键值集合、实体、实体集合、匿名对象、匿名对象集合</param>
        /// <returns></returns>
        IUpdate<T1> WhereDynamic(object dywhere);

        /// <summary>
        /// 设置表名规则，可用于分库/分表，参数1：默认表名；返回值：新表名；
        /// </summary>
        /// <param name="tableRule"></param>
        /// <returns></returns>
        IUpdate<T1> AsTable(Func<string, string> tableRule);
        /// <summary>
        /// 动态Type，在使用 Update&lt;object&gt; 后使用本方法，指定实体类型
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        IUpdate<T1> AsType(Type entityType);
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
        Task<int> ExecuteAffrowsAsync();
        /// <summary>
        /// 执行SQL语句，返回更新后的记录
        /// </summary>
        /// <returns></returns>
        List<T1> ExecuteUpdated();
        Task<List<T1>> ExecuteUpdatedAsync();
    }
}