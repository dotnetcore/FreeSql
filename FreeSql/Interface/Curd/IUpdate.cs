using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql
{
    public interface IUpdate<T1>
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
        /// 命令超时设置(秒)
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        IUpdate<T1> CommandTimeout(int timeout);

        /// <summary>
        /// 不使用参数化，可通过 IFreeSql.CodeFirst.IsNotCommandParameter 全局性设置
        /// </summary>
        /// <param name="isNotCommandParameter">是否不使用参数化</param>
        /// <returns></returns>
        IUpdate<T1> NoneParameter(bool isNotCommandParameter = true);

        /// <summary>
        /// 批量执行选项设置，一般不需要使用该方法<para></para>
        /// 各数据库 rows, parameters 限制不一样，默认设置：<para></para>
        /// MySql 500 3000<para></para>
        /// PostgreSQL 500 3000<para></para>
        /// SqlServer 500 2100<para></para>
        /// Oracle 200 999<para></para>
        /// Sqlite 200 999<para></para>
        /// 若没有事务传入，内部(默认)会自动开启新事务，保证拆包执行的完整性。
        /// </summary>
        /// <param name="rowsLimit">指定根据 rows 上限数量拆分执行</param>
        /// <param name="parameterLimit">指定根据 parameters 上限数量拆分执行</param>
        /// <param name="autoTransaction">是否自动开启事务</param>
        /// <returns></returns>
        IUpdate<T1> BatchOptions(int rowsLimit, int parameterLimit, bool autoTransaction = true);

        /// <summary>
        /// 批量执行时，分批次执行的进度状态
        /// </summary>
        /// <param name="callback">批量执行时的回调委托</param>
        /// <returns></returns>
        IUpdate<T1> BatchProgress(Action<BatchProgressStatus<T1>> callback);

        /// <summary>
        /// 更新数据，设置更新的实体<para></para>
        /// 注意：实体必须定义主键，并且最终会自动附加条件 where id = source.Id
        /// </summary>
        /// <param name="source">实体</param>
        /// <returns></returns>
        IUpdate<T1> SetSource(T1 source);
        /// <summary>
        /// 更新数据，设置更新的实体集合<para></para>
        /// 注意：实体必须定义主键，并且最终会自动附加条件 where id in (source.Id)
        /// </summary>
        /// <param name="source">实体集合</param>
        /// <param name="tempPrimarys">根据临时主键更新，a => a.Name | a => new{a.Name,a.Time} | a => new[]{"name","time"}</param>
        /// <returns></returns>
        IUpdate<T1> SetSource(IEnumerable<T1> source, Expression<Func<T1, object>> tempPrimarys = null);
        /// <summary>
        /// 更新数据，设置更新的实体，同时设置忽略的列<para></para>
        /// 忽略 null 属性：fsql.Update&lt;T&gt;().SetSourceAndIgnore(item, colval => colval == null)<para></para>
        /// 注意：参数 ignore 与 IUpdate.IgnoreColumns/UpdateColumns 不能同时使用
        /// </summary>
        /// <param name="source">实体</param>
        /// <param name="ignore">属性值忽略判断, true忽略</param>
        /// <returns></returns>
        IUpdate<T1> SetSourceIgnore(T1 source, Func<object, bool> ignore);

        /// <summary>
        /// 忽略的列，IgnoreColumns(a => a.Name) | IgnoreColumns(a => new{a.Name,a.Time}) | IgnoreColumns(a => new[]{"name","time"})<para></para>
        /// 注意：不能与 UpdateColumns 不能同时使用
        /// </summary>
        /// <param name="columns">lambda选择列</param>
        /// <returns></returns>
        IUpdate<T1> IgnoreColumns(Expression<Func<T1, object>> columns);
        /// <summary>
        /// 忽略的列<para></para>
        /// 注意：不能与 UpdateColumns 不能同时使用
        /// </summary>
        /// <param name="columns">属性名，或者字段名</param>
        /// <returns></returns>
        IUpdate<T1> IgnoreColumns(string[] columns);

        /// <summary>
        /// 指定的列，UpdateColumns(a => a.Name) | UpdateColumns(a => new{a.Name,a.Time}) | UpdateColumns(a => new[]{"name","time"})<para></para>
        /// 注意：不能与 IgnoreColumns 不能同时使用
        /// </summary>
        /// <param name="columns">lambda选择列</param>
        /// <returns></returns>
        IUpdate<T1> UpdateColumns(Expression<Func<T1, object>> columns);
        /// <summary>
        /// 指定的列<para></para>
        /// 注意：不能与 IgnoreColumns 同时使用
        /// </summary>
        /// <param name="columns">属性名，或者字段名</param>
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
        /// 设置列的新值，Set(a => a.Name, "newvalue")
        /// </summary>
        /// <typeparam name="TMember"></typeparam>
        /// <param name="condition">true 时生效</param>
        /// <param name="column">lambda选择列</param>
        /// <param name="value">新值</param>
        /// <returns></returns>
        IUpdate<T1> SetIf<TMember>(bool condition, Expression<Func<T1, TMember>> column, TMember value);
        /// <summary>
        /// 设置列的的新值为基础上增加，格式：Set(a => a.Clicks + 1) 相当于 clicks=clicks+1
        /// <para></para>
        /// 指定更新，格式：Set(a => new T { Clicks = a.Clicks + 1, Time = DateTime.Now }) 相当于 set clicks=clicks+1,time='2019-06-19....'
        /// </summary>
        /// <typeparam name="TMember"></typeparam>
        /// <param name="exp"></param>
        /// <returns></returns>
        IUpdate<T1> Set<TMember>(Expression<Func<T1, TMember>> exp);
        /// <summary>
        /// 设置列的的新值为基础上增加，格式：Set(a => a.Clicks + 1) 相当于 clicks=clicks+1
        /// <para></para>
        /// 指定更新，格式：Set(a => new T { Clicks = a.Clicks + 1, Time = DateTime.Now }) 相当于 set clicks=clicks+1,time='2019-06-19....'
        /// </summary>
        /// <typeparam name="TMember"></typeparam>
        /// <param name="condition">true 时生效</param>
        /// <param name="exp"></param>
        /// <returns></returns>
        IUpdate<T1> SetIf<TMember>(bool condition, Expression<Func<T1, TMember>> exp);
        /// <summary>
        /// 设置值，自定义SQL语法，SetRaw("title = @title", new { title = "newtitle" })<para></para>
        /// 提示：parms 参数还可以传 Dictionary&lt;string, object&gt;
        /// </summary>
        /// <param name="sql">sql语法</param>
        /// <param name="parms">参数</param>
        /// <returns></returns>
        IUpdate<T1> SetRaw(string sql, object parms = null);

        /// <summary>
        /// 设置更新的列<para></para>
        /// SetDto(new { title = "xxx", clicks = 2 })<para></para>
        /// SetDto(new Dictionary&lt;string, object&gt; { ["title"] = "xxx", ["clicks"] = 2 })<para></para>
        /// 注意：标记 [Column(CanUpdate = false)] 的属性不会被更新
        /// </summary>
        /// <param name="dto">dto 或 Dictionary&lt;string, object&gt;</param>
        /// <returns></returns>
        IUpdate<T1> SetDto(object dto);

        /// <summary>
        /// lambda表达式条件，仅支持实体基础成员（不包含导航对象）<para></para>
        /// 若想使用导航对象，请使用 ISelect.ToUpdate() 方法
        /// </summary>
        /// <param name="exp">lambda表达式条件</param>
        /// <returns></returns>
        IUpdate<T1> Where(Expression<Func<T1, bool>> exp);
        /// <summary>
        /// lambda表达式条件，仅支持实体基础成员（不包含导航对象）<para></para>
        /// 若想使用导航对象，请使用 ISelect.ToUpdate() 方法
        /// </summary>
        /// <param name="condition">true 时生效</param>
        /// <param name="exp">lambda表达式条件</param>
        /// <returns></returns>
        IUpdate<T1> WhereIf(bool condition, Expression<Func<T1, bool>> exp);
        /// <summary>
        /// 原生sql语法条件，Where("id = @id", new { id = 1 })<para></para>
        /// 提示：parms 参数还可以传 Dictionary&lt;string, object&gt;
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
        /// 传入动态条件，如：主键值 | new[]{主键值1,主键值2} | TEntity1 | new[]{TEntity1,TEntity2} | new{id=1}
        /// </summary>
        /// <param name="dywhere">主键值、主键值集合、实体、实体集合、匿名对象、匿名对象集合</param>
        /// <param name="not">是否标识为NOT</param>
        /// <returns></returns>
        IUpdate<T1> WhereDynamic(object dywhere, bool not = false);

        /// <summary>
        /// 禁用全局过滤功能，不传参数时将禁用所有
        /// </summary>
        /// <param name="name">零个或多个过滤器名字</param>
        /// <returns></returns>
        IUpdate<T1> DisableGlobalFilter(params string[] name);

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
        /// <summary>
        /// 执行SQL语句，返回更新后的记录<para></para>
        /// 注意：此方法只有 Postgresql/SqlServer 有效果
        /// </summary>
        /// <returns></returns>
        List<T1> ExecuteUpdated();

#if net40
#else
        Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default);
        Task<List<T1>> ExecuteUpdatedAsync(CancellationToken cancellationToken = default);
#endif
    }
}