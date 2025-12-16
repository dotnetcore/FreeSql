using FreeSql;
using System;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

partial class FreeSqlDbContextExtensions
{
    /// <summary>
    /// 返回默认仓库类，适用联合主键的仓储类
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="that"></param>
    /// <returns></returns>
    public static IBaseRepository<TEntity> GetRepository<TEntity>(this IFreeSql that) where TEntity : class => GetRepository<TEntity, int>(that);
    public static IBaseRepository<TEntity, TKey> GetRepository<TEntity, TKey>(this IFreeSql that) where TEntity : class
    {
        return new DefaultRepository<TEntity, TKey>(that);
    }

    /// <summary>
    /// 创建基于工作单元，务必使用 using 包含使用
    /// </summary>
    /// <param name="that"></param>
    /// <returns></returns>
    public static IRepositoryUnitOfWork CreateUnitOfWork(this IFreeSql that)
    {
        return new RepositoryUnitOfWork(that);
    }

    /// <summary>
    /// 创建基于外部事务的工作单元
    /// <para>注意：该工作单元 Commit/Dispose 时不会关闭传入的 transaction，请自行管理 transaction 生命周期</para>
    /// </summary>
    /// <param name="fsql"></param>
    /// <param name="transaction">外部传入的 DbTransaction</param>
    /// <returns></returns>
    public static IUnitOfWork CreateUnitOfWork(this IFreeSql fsql, DbTransaction transaction)
    {
        return new ExternalUnitOfWork(fsql, transaction);
    }
}