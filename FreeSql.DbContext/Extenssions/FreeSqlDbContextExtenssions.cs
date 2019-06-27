using FreeSql;
using System;
using System.Collections.Concurrent;

public static class FreeSqlDbContextExtenssions
{

    /// <summary>
    /// 创建普通数据上下文档对象
    /// </summary>
    /// <param name="that"></param>
    /// <returns></returns>
    public static DbContext CreateDbContext(this IFreeSql that)
    {
        return new FreeContext(that);
    }

    /// <summary>
    /// 不跟踪查询的实体数据（在不需要更新其数据时使用），可提长查询性能
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="select"></param>
    /// <returns></returns>
    public static ISelect<T> NoTracking<T>(this ISelect<T> select) where T : class
    {
        return select.TrackToList(null);
    }

    /// <summary>
    /// 设置 DbContext 选项设置
    /// </summary>
    /// <param name="that"></param>
    /// <param name="options"></param>
    public static void SetDbContextOptions(this IFreeSql that, Action<DbContextOptions> options)
    {
        if (options == null) return;
        var cfg = _dicSetDbContextOptions.GetOrAdd(that, t => new DbContextOptions());
        options(cfg);
        _dicSetDbContextOptions.AddOrUpdate(that, cfg, (t, o) => cfg);
    }
    internal static ConcurrentDictionary<IFreeSql, DbContextOptions> _dicSetDbContextOptions = new ConcurrentDictionary<IFreeSql, DbContextOptions>();
}