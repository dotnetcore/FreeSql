using FreeSql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

public static class FreeSqlAdoNetExtensions
{
    static Dictionary<Type, IFreeSql> _dicCurd = new Dictionary<Type, IFreeSql>();
    static object _dicCurdLock = new object();
    static IFreeSql GetCrud(IDbConnection dbconn)
    {
        if (dbconn == null) throw new ArgumentNullException($"{nameof(dbconn)} 不能为 null"); ;
        Type dbconType = dbconn.GetType();
        var connType = dbconType.UnderlyingSystemType;
        if (_dicCurd.TryGetValue(connType, out var fsql)) return fsql;

        Type providerType = null;
        switch (connType.Name)
        {
            case "MySqlConnection":
                providerType = Type.GetType("FreeSql.MySql.MySqlProvider`1,FreeSql.Provider.MySql")?.MakeGenericType(connType);
                if (providerType == null) providerType = Type.GetType("FreeSql.MySql.MySqlProvider`1,FreeSql.Provider.MySqlConnector")?.MakeGenericType(connType);
                if (providerType == null) throw new Exception("缺少 FreeSql 数据库实现包：FreeSql.Provider.MySql.dll，可前往 nuget 下载");
                break;
            case "SqlConnection":
                providerType = Type.GetType("FreeSql.SqlServer.SqlServerProvider`1,FreeSql.Provider.SqlServer")?.MakeGenericType(connType);
                if (providerType == null) throw new Exception("缺少 FreeSql 数据库实现包：FreeSql.Provider.SqlServer.dll，可前往 nuget 下载");
                break;
            case "NpgsqlConnection":
                providerType = Type.GetType("FreeSql.PostgreSQL.PostgreSQLProvider`1,FreeSql.Provider.PostgreSQL")?.MakeGenericType(connType);
                if (providerType == null) throw new Exception("缺少 FreeSql 数据库实现包：FreeSql.Provider.PostgreSQL.dll，可前往 nuget 下载");
                break;
            case "OracleConnection":
                providerType = Type.GetType("FreeSql.Oracle.OracleProvider`1,FreeSql.Provider.Oracle")?.MakeGenericType(connType);
                if (providerType == null) throw new Exception("缺少 FreeSql 数据库实现包：FreeSql.Provider.Oracle.dll，可前往 nuget 下载");
                break;
            case "SQLiteConnection":
                providerType = Type.GetType("FreeSql.Sqlite.SqliteProvider`1,FreeSql.Provider.Sqlite")?.MakeGenericType(connType);
                if (providerType == null) throw new Exception("缺少 FreeSql 数据库实现包：FreeSql.Provider.Sqlite.dll，可前往 nuget 下载");
                break;
            case "DmConnection":
                providerType = Type.GetType("FreeSql.Dameng.DamengProvider`1,FreeSql.Provider.Dameng")?.MakeGenericType(connType);
                if (providerType == null) throw new Exception("缺少 FreeSql 数据库实现包：FreeSql.Provider.Dameng.dll，可前往 nuget 下载");
                break;
            case "OscarConnection":
                providerType = Type.GetType("FreeSql.ShenTong.ShenTongProvider`1,FreeSql.Provider.ShenTong")?.MakeGenericType(connType);
                if (providerType == null) throw new Exception("缺少 FreeSql 数据库实现包：FreeSql.Provider.ShenTong.dll，可前往 nuget 下载");
                break;
            default:
                throw new Exception("未实现");
        }
        lock (_dicCurdLock)
        {
            if (_dicCurd.TryGetValue(connType, out fsql)) return fsql;
            lock (_dicCurdLock)
                _dicCurd.Add(connType, fsql = Activator.CreateInstance(providerType, new object[] { null, null, null }) as IFreeSql);
        }
        return fsql;
    }
    static IFreeSql GetCrud(IDbTransaction dbtran)
    {
        if (dbtran == null) throw new ArgumentNullException($"{nameof(dbtran)} 不能为 null");
        return GetCrud(dbtran.Connection);
    }

    /// <summary>
    /// 获取 IDbConnection 对应的 IFreeSql 实例
    /// </summary>
    /// <param name="that"></param>
    /// <returns></returns>
    public static IFreeSql GetIFreeSql(this IDbConnection that) => GetCrud(that);

    #region IDbConnection
    /// <summary>
    /// 插入数据
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <returns></returns>
    public static IInsert<T1> Insert<T1>(this IDbConnection that) where T1 : class => GetCrud(that).Insert<T1>().WithConnection(that as DbConnection);
    /// <summary>
    /// 插入数据，传入实体
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static IInsert<T1> Insert<T1>(this IDbConnection that, T1 source) where T1 : class => GetCrud(that).Insert<T1>(source).WithConnection(that as DbConnection);
    /// <summary>
    /// 插入数据，传入实体数组
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static IInsert<T1> Insert<T1>(this IDbConnection that, T1[] source) where T1 : class => GetCrud(that).Insert<T1>(source).WithConnection(that as DbConnection);
    /// <summary>
    /// 插入数据，传入实体集合
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static IInsert<T1> Insert<T1>(this IDbConnection that, List<T1> source) where T1 : class => GetCrud(that).Insert<T1>(source).WithConnection(that as DbConnection);
    /// <summary>
    /// 插入数据，传入实体集合
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static IInsert<T1> Insert<T1>(this IDbConnection that, IEnumerable<T1> source) where T1 : class => GetCrud(that).Insert<T1>(source).WithConnection(that as DbConnection);

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
    public static IInsertOrUpdate<T1> InsertOrUpdate<T1>(this IDbConnection that) where T1 : class => GetCrud(that).InsertOrUpdate<T1>().WithConnection(that as DbConnection);

    /// <summary>
    /// 修改数据
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <returns></returns>
    public static IUpdate<T1> Update<T1>(this IDbConnection that) where T1 : class => GetCrud(that).Update<T1>().WithConnection(that as DbConnection);
    /// <summary>
    /// 修改数据，传入动态条件，如：主键值 | new[]{主键值1,主键值2} | TEntity1 | new[]{TEntity1,TEntity2} | new{id=1}
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="dywhere">主键值、主键值集合、实体、实体集合、匿名对象、匿名对象集合</param>
    /// <returns></returns>
    public static IUpdate<T1> Update<T1>(this IDbConnection that, object dywhere) where T1 : class => GetCrud(that).Update<T1>(dywhere).WithConnection(that as DbConnection);

    /// <summary>
    /// 查询数据
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <returns></returns>
    public static ISelect<T1> Select<T1>(this IDbConnection that) where T1 : class => GetCrud(that).Select<T1>().WithConnection(that as DbConnection);
    /// <summary>
    /// 查询数据，传入动态条件，如：主键值 | new[]{主键值1,主键值2} | TEntity1 | new[]{TEntity1,TEntity2} | new{id=1}
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="dywhere">主键值、主键值集合、实体、实体集合、匿名对象、匿名对象集合</param>
    /// <returns></returns>
    public static ISelect<T1> Select<T1>(this IDbConnection that, object dywhere) where T1 : class => GetCrud(that).Select<T1>(dywhere).WithConnection(that as DbConnection);

    /// <summary>
    /// 删除数据
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <returns></returns>
    public static IDelete<T1> Delete<T1>(this IDbConnection that) where T1 : class => GetCrud(that).Delete<T1>().WithConnection(that as DbConnection);
    /// <summary>
    /// 删除数据，传入动态条件，如：主键值 | new[]{主键值1,主键值2} | TEntity1 | new[]{TEntity1,TEntity2} | new{id=1}
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="dywhere">主键值、主键值集合、实体、实体集合、匿名对象、匿名对象集合</param>
    /// <returns></returns>
    public static IDelete<T1> Delete<T1>(this IDbConnection that, object dywhere) where T1 : class => GetCrud(that).Delete<T1>(dywhere).WithConnection(that as DbConnection);
    #endregion

    #region IDbTransaction
    /// <summary>
    /// 插入数据
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <returns></returns>
    public static IInsert<T1> Insert<T1>(this IDbTransaction that) where T1 : class => GetCrud(that).Insert<T1>().WithTransaction(that as DbTransaction);
    /// <summary>
    /// 插入数据，传入实体
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static IInsert<T1> Insert<T1>(this IDbTransaction that, T1 source) where T1 : class => GetCrud(that).Insert<T1>(source).WithTransaction(that as DbTransaction);
    /// <summary>
    /// 插入数据，传入实体数组
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static IInsert<T1> Insert<T1>(this IDbTransaction that, T1[] source) where T1 : class => GetCrud(that).Insert<T1>(source).WithTransaction(that as DbTransaction);
    /// <summary>
    /// 插入数据，传入实体集合
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static IInsert<T1> Insert<T1>(this IDbTransaction that, List<T1> source) where T1 : class => GetCrud(that).Insert<T1>(source).WithTransaction(that as DbTransaction);
    /// <summary>
    /// 插入数据，传入实体集合
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static IInsert<T1> Insert<T1>(this IDbTransaction that, IEnumerable<T1> source) where T1 : class => GetCrud(that).Insert<T1>(source).WithTransaction(that as DbTransaction);

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
    public static IInsertOrUpdate<T1> InsertOrUpdate<T1>(this IDbTransaction that) where T1 : class => GetCrud(that).InsertOrUpdate<T1>().WithTransaction(that as DbTransaction);

    /// <summary>
    /// 修改数据
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <returns></returns>
    public static IUpdate<T1> Update<T1>(this IDbTransaction that) where T1 : class => GetCrud(that).Update<T1>().WithTransaction(that as DbTransaction);
    /// <summary>
    /// 修改数据，传入动态条件，如：主键值 | new[]{主键值1,主键值2} | TEntity1 | new[]{TEntity1,TEntity2} | new{id=1}
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="dywhere">主键值、主键值集合、实体、实体集合、匿名对象、匿名对象集合</param>
    /// <returns></returns>
    public static IUpdate<T1> Update<T1>(this IDbTransaction that, object dywhere) where T1 : class => GetCrud(that).Update<T1>(dywhere).WithTransaction(that as DbTransaction);

    /// <summary>
    /// 查询数据
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <returns></returns>
    public static ISelect<T1> Select<T1>(this IDbTransaction that) where T1 : class => GetCrud(that).Select<T1>().WithTransaction(that as DbTransaction);
    /// <summary>
    /// 查询数据，传入动态条件，如：主键值 | new[]{主键值1,主键值2} | TEntity1 | new[]{TEntity1,TEntity2} | new{id=1}
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="dywhere">主键值、主键值集合、实体、实体集合、匿名对象、匿名对象集合</param>
    /// <returns></returns>
    public static ISelect<T1> Select<T1>(this IDbTransaction that, object dywhere) where T1 : class => GetCrud(that).Select<T1>(dywhere).WithTransaction(that as DbTransaction);

    /// <summary>
    /// 删除数据
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <returns></returns>
    public static IDelete<T1> Delete<T1>(this IDbTransaction that) where T1 : class => GetCrud(that).Delete<T1>().WithTransaction(that as DbTransaction);
    /// <summary>
    /// 删除数据，传入动态条件，如：主键值 | new[]{主键值1,主键值2} | TEntity1 | new[]{TEntity1,TEntity2} | new{id=1}
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="dywhere">主键值、主键值集合、实体、实体集合、匿名对象、匿名对象集合</param>
    /// <returns></returns>
    public static IDelete<T1> Delete<T1>(this IDbTransaction that, object dywhere) where T1 : class => GetCrud(that).Delete<T1>(dywhere).WithTransaction(that as DbTransaction);
    #endregion
}