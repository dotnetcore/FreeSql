using FreeSql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public static partial class FreeSqlSqlServerGlobalExtensions
{

    /// <summary>
    /// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
    /// </summary>
    /// <param name="that"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string FormatSqlServer(this string that, params object[] args) => _sqlserverAdo.Addslashes(that, args);
    static FreeSql.SqlServer.SqlServerAdo _sqlserverAdo = new FreeSql.SqlServer.SqlServerAdo();

    /// <summary>
    /// SqlServer with(nolock) 查询
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="that"></param>
    /// <param name="lockType"></param>
    /// <param name="rule">多表查询时的锁规则</param>
    /// <returns></returns>
    public static ISelect<T> WithLock<T>(this ISelect<T> that, SqlServerLock lockType = SqlServerLock.NoLock, Dictionary<Type, bool> rule = null) where T : class 
        => rule == null ? 
        that.AsAlias((type, old) => $"{old} with({lockType.ToString()})") :
        that.AsAlias((type, old) => rule.TryGetValue(type, out var trybool) && trybool ? $"{old} with({lockType.ToString()})" : old);

    /// <summary>
    /// 设置全局 SqlServer with(nolock) 查询
    /// </summary>
    /// <param name="that"></param>
    /// <param name="options"></param>
    public static IFreeSql SetGlobalSelectWithLock(this IFreeSql that, SqlServerLock lockType, Dictionary<Type, bool> rule)
    {
        var value = (lockType, rule);
        _dicSetGlobalSelectWithLock.AddOrUpdate(that, value, (_, __) => value);
        return that;
    }
    internal static ConcurrentDictionary<IFreeSql, (SqlServerLock, Dictionary<Type, bool>)> _dicSetGlobalSelectWithLock = new ConcurrentDictionary<IFreeSql, (SqlServerLock, Dictionary<Type, bool>)>();
}

public enum SqlServerLock
{
    HoldLock, NoLock, PagLock, ReadCommitted, ReadPast, ReadUnCommitted, RepeaTableRead, RowLock, Serializable, TabLock, TabLockX, UpdLock, XLock
}