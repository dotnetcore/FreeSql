// SonnetDB 全局扩展。
using FreeSql.SonnetDB;

public static class FreeSqlSonnetDBGlobalExtensions
{
    /// <summary>
    /// 将格式化参数转义后嵌入 SQL 模板字符串，防止 SQL 注入。
    /// 等价于 <c>SonnetDBAdo.Addslashes(that, args)</c>。
    /// </summary>
    public static string FormatSonnetDB(this string that, params object[] args) => _sonnetDBAdo.Addslashes(that, args);

    static readonly SonnetDBAdo _sonnetDBAdo = new SonnetDBAdo();
}
