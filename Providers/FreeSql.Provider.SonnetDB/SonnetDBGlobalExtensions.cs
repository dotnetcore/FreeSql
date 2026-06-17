// SonnetDBGlobalExtensions.cs
// SonnetDB 全局字符串格式化扩展。
//
// 提供 FormatSonnetDB 扩展方法，用于将 SQL 模板中的占位符参数安全地转义并格式化，
// 防止 SQL 注入风险。底层调用 SonnetDBAdo.Addslashes 进行转义处理。
//
// 用法示例（手写 SQL 时）：
//   string sql = "SELECT * FROM \"sensors\" WHERE device = {0}".FormatSonnetDB(deviceId);
//
// 注意：FreeSql LINQ 查询（Where / Select 等）会自动完成转义，无需手动调用此方法。
// SonnetDB 专有函数（PID、时序、向量、地理空间等）请通过 SonnetDBFunctions 类使用，
// 并在 FreeSqlBuilder 注册时配合 ExpressionCall 机制自动翻译为 SQL。

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
