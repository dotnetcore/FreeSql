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
    public static string FormatOdbcSqlServer(this string that, params object[] args) => _odbcSqlServerAdo.Addslashes(that, args);
    static FreeSql.Odbc.SqlServer.OdbcSqlServerAdo _odbcSqlServerAdo = new FreeSql.Odbc.SqlServer.OdbcSqlServerAdo();

    /// <summary>
    /// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
    /// </summary>
    /// <param name="that"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string FormatOdbcMySql(this string that, params object[] args) => _odbcMySqlAdo.Addslashes(that, args);
    static FreeSql.Odbc.MySql.OdbcMySqlAdo _odbcMySqlAdo = new FreeSql.Odbc.MySql.OdbcMySqlAdo();

    /// <summary>
    /// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
    /// </summary>
    /// <param name="that"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string FormatOdbcPostgreSQL(this string that, params object[] args) => _odbcPostgreSQLAdo.Addslashes(that, args);
    static FreeSql.Odbc.PostgreSQL.OdbcPostgreSQLAdo _odbcPostgreSQLAdo = new FreeSql.Odbc.PostgreSQL.OdbcPostgreSQLAdo();

    /// <summary>
    /// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
    /// </summary>
    /// <param name="that"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string FormatOdbc(this string that, params object[] args) => _odbcAdo.Addslashes(that, args);
    static FreeSql.Odbc.Default.OdbcAdo _odbcAdo = new FreeSql.Odbc.Default.OdbcAdo();

    /// <summary>
    /// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
    /// </summary>
    /// <param name="that"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string FormatOdbcDameng(this string that, params object[] args) => _odbcDamengAdo.Addslashes(that, args);
    static FreeSql.Odbc.Dameng.OdbcDamengAdo _odbcDamengAdo = new FreeSql.Odbc.Dameng.OdbcDamengAdo();
}
