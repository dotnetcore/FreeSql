public static partial class FreeSqlCustomGlobalExtensions
{

    /// <summary>
    /// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
    /// </summary>
    /// <param name="that"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    internal static string FormatCustomOracle(this string that, params object[] args) => _customOracleAdo.Addslashes(that, args);
    static FreeSql.Custom.Oracle.CustomOracleAdo _customOracleAdo = new FreeSql.Custom.Oracle.CustomOracleAdo();

    /// <summary>
    /// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
    /// </summary>
    /// <param name="that"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    internal static string FormatCustomSqlServer(this string that, params object[] args) => _customSqlServerAdo.Addslashes(that, args);
    static FreeSql.Custom.SqlServer.CustomSqlServerAdo _customSqlServerAdo = new FreeSql.Custom.SqlServer.CustomSqlServerAdo();

    /// <summary>
    /// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
    /// </summary>
    /// <param name="that"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    internal static string FormatCustomMySql(this string that, params object[] args) => _customMySqlAdo.Addslashes(that, args);
    static FreeSql.Custom.MySql.CustomMySqlAdo _customMySqlAdo = new FreeSql.Custom.MySql.CustomMySqlAdo();

    /// <summary>
    /// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
    /// </summary>
    /// <param name="that"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    internal static string FormatCustomPostgreSQL(this string that, params object[] args) => _customPostgreSQLAdo.Addslashes(that, args);
    static FreeSql.Custom.PostgreSQL.CustomPostgreSQLAdo _customPostgreSQLAdo = new FreeSql.Custom.PostgreSQL.CustomPostgreSQLAdo();
}
