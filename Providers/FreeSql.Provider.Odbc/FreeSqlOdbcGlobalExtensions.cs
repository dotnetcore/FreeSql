public static partial class FreeSqlOdbcGlobalExtensions
{

    /// <summary>
    /// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
    /// </summary>
    /// <param name="that"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string FormatOdbcOracle(this string that, params object[] args) => _odbcOracleAdo.Addslashes(that, args);
    static FreeSql.Odbc.Oracle.OdbcOracleAdo _odbcOracleAdo = new FreeSql.Odbc.Oracle.OdbcOracleAdo();

    /// <summary>
    /// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
    /// </summary>
    /// <param name="that"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string FormatOdbcSqlServer(this string that, params object[] args) => _sqlserverAdo.Addslashes(that, args);
    static FreeSql.Odbc.SqlServer.OdbcSqlServerAdo _sqlserverAdo = new FreeSql.Odbc.SqlServer.OdbcSqlServerAdo();

    /// <summary>
    /// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
    /// </summary>
    /// <param name="that"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string FormatOdbcMySql(this string that, params object[] args) => _mysqlAdo.Addslashes(that, args);
    static FreeSql.Odbc.MySql.OdbcMySqlAdo _mysqlAdo = new FreeSql.Odbc.MySql.OdbcMySqlAdo();
}
