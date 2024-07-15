﻿using FreeSql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public static partial class FreeSqlDbContextExtensions
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
    /// 不跟踪查询的实体数据（在不需要更新其数据时使用），可提升查询性能
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
    public static IFreeSql SetDbContextOptions(this IFreeSql that, Action<DbContextOptions> options)
    {
        if (options == null) return that;
        var cfg = _dicSetDbContextOptions.GetOrAdd(that.Ado.Identifier, t => new DbContextOptions());
        options(cfg);
        _dicSetDbContextOptions.AddOrUpdate(that.Ado.Identifier, cfg, (t, o) => cfg);
        return that;
    }
    internal static ConcurrentDictionary<Guid, DbContextOptions> _dicSetDbContextOptions = FreeSql.Internal.Utils.GlobalCacheFactory.CreateCacheItem<ConcurrentDictionary<Guid, DbContextOptions>>();
}