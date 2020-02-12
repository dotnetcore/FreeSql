public static partial class FreeSqlMsAccessGlobalExtensions
{

    /// <summary>
    /// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
    /// </summary>
    /// <param name="that"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string FormatAccess(this string that, params object[] args) => _accessAdo.Addslashes(that, args);
    static FreeSql.MsAccess.MsAccessAdo _accessAdo = new FreeSql.MsAccess.MsAccessAdo();
}
