using FreeSql;
using System;
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
}