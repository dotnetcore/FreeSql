using FreeSql;
using System;
using System.Linq.Expressions;
using System.Linq;

public static class FreeSqlRepositoryExtenssions
{

    /// <summary>
    /// 返回默认仓库类
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="that"></param>
    /// <param name="filter">数据过滤 + 验证</param>
    /// <returns></returns>
    public static DefaultRepository<TEntity, TKey> GetRepository<TEntity, TKey>(this IFreeSql that, Expression<Func<TEntity, bool>> filter = null) where TEntity : class
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
    public static BaseRepository<TEntity> GetRepository<TEntity>(this IFreeSql that, Expression<Func<TEntity, bool>> filter = null) where TEntity : class
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
    public static GuidRepository<TEntity> GetGuidRepository<TEntity>(this IFreeSql that, Expression<Func<TEntity, bool>> filter = null, Func<string, string> asTable = null) where TEntity : class
    {
        return new GuidRepository<TEntity>(that, filter, asTable);
    }

    /// <summary>
    /// 合并两个仓储的设置（过滤+分表），以便查询
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="that"></param>
    /// <param name="repos"></param>
    /// <returns></returns>
    public static ISelect<TEntity> FromRepository<TEntity, T2>(this ISelect<TEntity> that, BaseRepository<T2> repos) where TEntity : class where T2 : class
    {
        var filters = (repos.DataFilter as DataFilter<T2>)._filters.Where(a => a.Value.IsEnabled == true);
        foreach (var filter in filters) that.Where<T2>(filter.Value.Expression);
        return that.AsTable(repos.AsTableSelectInternal);
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