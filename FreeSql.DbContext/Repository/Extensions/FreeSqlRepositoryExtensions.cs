using FreeSql;
using System;
using System.Linq;
using System.Linq.Expressions;

partial class FreeSqlDbContextExtensions
{

    /// <summary>
    /// 返回默认仓库类
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="that"></param>
    /// <param name="filter">数据过滤 + 验证</param>
    /// <returns></returns>
    public static IBaseRepository<TEntity, TKey> GetRepository<TEntity, TKey>(this IFreeSql that, Expression<Func<TEntity, bool>> filter = null) where TEntity : class
    {
        return new DefaultRepository<TEntity, TKey>(that, filter);
    }

    /// <summary>
    /// 返回默认仓库类，适用联合主键的仓储类
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="that"></param>
    /// <param name="filter">数据过滤 + 验证</param>
    /// <returns></returns>
    public static IBaseRepository<TEntity> GetRepository<TEntity>(this IFreeSql that, Expression<Func<TEntity, bool>> filter = null) where TEntity : class
    {
        return new DefaultRepository<TEntity, int>(that, filter);
    }

    /// <summary>
    /// 返回仓库类
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="that"></param>
    /// <param name="filter">数据过滤 + 验证</param>
    /// <param name="asTable">分表规则，参数：旧表名；返回：新表名 https://github.com/2881099/FreeSql/wiki/Repository</param>
    /// <returns></returns>
    public static IBaseRepository<TEntity, Guid> GetGuidRepository<TEntity>(this IFreeSql that, Expression<Func<TEntity, bool>> filter = null, Func<string, string> asTable = null) where TEntity : class
    {
        return new GuidRepository<TEntity>(that, filter, asTable);
    }

    /// <summary>
    /// 创建基于仓储功能的工作单元，务必使用 using 包含使用
    /// </summary>
    /// <param name="that"></param>
    /// <returns></returns>
    public static IRepositoryUnitOfWork CreateUnitOfWork(this IFreeSql that)
    {
        return new RepositoryUnitOfWork(that);
    }
}