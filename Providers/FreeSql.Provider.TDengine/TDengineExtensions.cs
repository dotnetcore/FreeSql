public static partial class FreeSqlTDengineGlobalExtensions
{

    /// <summary>
    /// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
    /// </summary>
    /// <param name="that"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string FormatTDengine(this string that, params object[] args) => _TDengineAdo.Addslashes(that, args);
    static FreeSql.TDengine.TDengineAdo _TDengineAdo = new FreeSql.TDengine.TDengineAdo();
}
