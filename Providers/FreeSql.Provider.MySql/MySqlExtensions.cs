public static partial class FreeSqlGlobalExtensions
{

    /// <summary>
    /// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
    /// </summary>
    /// <param name="that"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string FormatMySql(this string that, params object[] args) => _mysqlAdo.Addslashes(that, args);
    static FreeSql.MySql.MySqlAdo _mysqlAdo = new FreeSql.MySql.MySqlAdo();
}
