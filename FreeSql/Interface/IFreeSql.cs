using FreeSql;
using FreeSql.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

public interface IFreeSql<TMark> : IFreeSql { }

public interface IFreeSql : IDisposable
{
    /// <summary>
    /// 插入数据
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <returns></returns>
    IInsert<T1> Insert<T1>() where T1 : class;
    /// <summary>
    /// 插入数据，传入实体
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    IInsert<T1> Insert<T1>(T1 source) where T1 : class;
    /// <summary>
    /// 插入数据，传入实体数组
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    IInsert<T1> Insert<T1>(T1[] source) where T1 : class;
    /// <summary>
    /// 插入数据，传入实体集合
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    IInsert<T1> Insert<T1>(List<T1> source) where T1 : class;
    /// <summary>
    /// 插入数据，传入实体集合
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    IInsert<T1> Insert<T1>(IEnumerable<T1> source) where T1 : class;

    /// <summary>
    /// 插入或更新数据，此功能依赖数据库特性（低版本可能不支持），参考如下：<para></para>
    /// MySql 5.6+: on duplicate key update<para></para>
    /// PostgreSQL 9.4+: on conflict do update<para></para>
    /// SqlServer 2008+: merge into<para></para>
    /// Oracle 11+: merge into<para></para>
    /// Sqlite: replace into<para></para>
    /// 达梦: merge into<para></para>
    /// 人大金仓：on conflict do update<para></para>
    /// 神通：merge into<para></para>
    /// MsAccess：不支持<para></para>
    /// 注意区别：FreeSql.Repository 仓储也有 InsertOrUpdate 方法（不依赖数据库特性）
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <returns></returns>
    IInsertOrUpdate<T1> InsertOrUpdate<T1>() where T1 : class;

    /// <summary>
    /// 修改数据
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <returns></returns>
    IUpdate<T1> Update<T1>() where T1 : class;
    /// <summary>
    /// 修改数据，传入动态条件，如：主键值 | new[]{主键值1,主键值2} | TEntity1 | new[]{TEntity1,TEntity2} | new{id=1}
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="dywhere">主键值、主键值集合、实体、实体集合、匿名对象、匿名对象集合</param>
    /// <returns></returns>
    IUpdate<T1> Update<T1>(object dywhere) where T1 : class;

    /// <summary>
    /// 查询数据
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <returns></returns>
    ISelect<T1> Select<T1>() where T1 : class;
    /// <summary>
    /// 查询数据，传入动态条件，如：主键值 | new[]{主键值1,主键值2} | TEntity1 | new[]{TEntity1,TEntity2} | new{id=1}
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="dywhere">主键值、主键值集合、实体、实体集合、匿名对象、匿名对象集合</param>
    /// <returns></returns>
    ISelect<T1> Select<T1>(object dywhere) where T1 : class;

    /// <summary>
    /// 删除数据
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <returns></returns>
    IDelete<T1> Delete<T1>() where T1 : class;
    /// <summary>
    /// 删除数据，传入动态条件，如：主键值 | new[]{主键值1,主键值2} | TEntity1 | new[]{TEntity1,TEntity2} | new{id=1}
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="dywhere">主键值、主键值集合、实体、实体集合、匿名对象、匿名对象集合</param>
    /// <returns></returns>
    IDelete<T1> Delete<T1>(object dywhere) where T1 : class;

    /// <summary>
    /// 开启事务（不支持异步）<para></para>
    /// v1.5.0 关闭了线程事务超时自动提交的机制
    /// </summary>
    /// <param name="handler">事务体 () => {}</param>
    void Transaction(Action handler);
    /// <summary>
    /// 开启事务（不支持异步）<para></para>
    /// v1.5.0 关闭了线程事务超时自动提交的机制
    /// </summary>
    /// <param name="isolationLevel"></param>
    /// <param name="handler">事务体 () => {}</param>
    void Transaction(IsolationLevel isolationLevel, Action handler);

    /// <summary>
    /// 数据库访问对象
    /// </summary>
    IAdo Ado { get; }
    /// <summary>
    /// 所有拦截方法都在这里
    /// </summary>
    IAop Aop { get; }

    /// <summary>
    /// CodeFirst 模式开发相关方法
    /// </summary>
    ICodeFirst CodeFirst { get; }
    /// <summary>
    /// DbFirst 模式开发相关方法
    /// </summary>
    IDbFirst DbFirst { get; }

    /// <summary>
    /// 全局过滤设置，可默认附加为 Select/Update/Delete 条件
    /// </summary>
    GlobalFilter GlobalFilter { get; }
}