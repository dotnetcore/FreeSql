using FreeSql;
using FreeSql.PostgreSQL.Curd;
using System;
using System.Linq.Expressions;

public static partial class FreeSqlPostgreSQLGlobalExtensions
{

    /// <summary>
    /// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
    /// </summary>
    /// <param name="that"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string FormatPostgreSQL(this string that, params object[] args) => _postgresqlAdo.Addslashes(that, args);
    static FreeSql.PostgreSQL.PostgreSQLAdo _postgresqlAdo = new FreeSql.PostgreSQL.PostgreSQLAdo();

    /// <summary>
    /// PostgreSQL9.5+ 特有的功能，On Conflict Do Update<para></para>
    /// 注意：此功能会开启插入【自增列】
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="that"></param>
    /// <param name="columns">默认是以主键作为重复判断，也可以指定其他列：a => a.Name | a => new{a.Name,a.Time} | a => new[]{"name","time"}</param>
    /// <returns></returns>
    public static OnConflictDoUpdate<T1> OnConflictDoUpdate<T1>(this IInsert<T1> that, Expression<Func<T1, object>> columns = null) where T1 : class => new FreeSql.PostgreSQL.Curd.OnConflictDoUpdate<T1>(that.InsertIdentity(), columns);
}
