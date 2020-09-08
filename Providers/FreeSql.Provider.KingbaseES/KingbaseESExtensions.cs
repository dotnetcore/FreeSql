public static partial class FreeSqlKingbaseESGlobalExtensions
{

    /// <summary>
    /// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
    /// </summary>
    /// <param name="that"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string FormatKingbaseES(this string that, params object[] args) => _kingbaseesAdo.Addslashes(that, args);
    static FreeSql.KingbaseES.KingbaseESAdo _kingbaseesAdo = new FreeSql.KingbaseES.KingbaseESAdo();
}
