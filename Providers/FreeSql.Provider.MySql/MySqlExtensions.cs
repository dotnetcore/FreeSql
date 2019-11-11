using FreeSql;
using FreeSql.MySql.Curd;

public static partial class FreeSqlMySqlGlobalExtensions
{

    /// <summary>
    /// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
    /// </summary>
    /// <param name="that"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string FormatMySql(this string that, params object[] args) => _mysqlAdo.Addslashes(that, args);
    static FreeSql.MySql.MySqlAdo _mysqlAdo = new FreeSql.MySql.MySqlAdo();

    /// <summary>
    /// MySql 特有的功能，On Duplicate Key Update<para></para>
    /// 注意：此功能会开启插入【自增列】
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="that"></param>
    /// <returns></returns>
    public static OnDuplicateKeyUpdate<T1> OnDuplicateKeyUpdate<T1>(this IInsert<T1> that) where T1 : class => new FreeSql.MySql.Curd.OnDuplicateKeyUpdate<T1>(that.InsertIdentity());

}
