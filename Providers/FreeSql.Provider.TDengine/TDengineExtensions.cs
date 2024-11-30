using System.Text;
using FreeSql.TDengine;

public static class FreeSqlTDengineGlobalExtensions
{
    /// <summary>
    /// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
    /// </summary>
    /// <param name="that"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string FormatTDengine(this string that, params object[] args) => TDengineAdo.Addslashes(that, args);

    static readonly TDengineAdo TDengineAdo = new TDengineAdo();
}