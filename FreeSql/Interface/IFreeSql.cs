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
    /// 修改数据
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <returns></returns>
    IUpdate<T1> Update<T1>() where T1 : class;
    /// <summary>
    /// 修改数据，传入动态对象如：主键值 | new[]{主键值1,主键值2} | TEntity1 | new[]{TEntity1,TEntity2} | new{id=1}
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
    /// 查询数据，传入动态对象如：主键值 | new[]{主键值1,主键值2} | TEntity1 | new[]{TEntity1,TEntity2} | new{id=1}
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
    /// 删除数据，传入动态对象如：主键值 | new[]{主键值1,主键值2} | TEntity1 | new[]{TEntity1,TEntity2} | new{id=1}
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="dywhere">主键值、主键值集合、实体、实体集合、匿名对象、匿名对象集合</param>
    /// <returns></returns>
    IDelete<T1> Delete<T1>(object dywhere) where T1 : class;

    /// <summary>
    /// 开启事务（不支持异步），60秒未执行完成（可能）被其他线程事务自动提交
    /// </summary>
    /// <param name="handler">事务体 () => {}</param>
    void Transaction(Action handler);
    /// <summary>
    /// 开启事务（不支持异步）
    /// </summary>
    /// <param name="timeout">超时，未执行完成（可能）被其他线程事务自动提交</param>
    /// <param name="handler">事务体 () => {}</param>
    void Transaction(TimeSpan timeout, Action handler);
    /// <summary>
    /// 开启事务（不支持异步）
    /// </summary>
    /// <param name="isolationLevel"></param>
    /// <param name="handler">事务体 () => {}</param>
    /// <param name="timeout">超时，未执行完成（可能）被其他线程事务自动提交</param>
    void Transaction(IsolationLevel isolationLevel, TimeSpan timeout, Action handler);

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