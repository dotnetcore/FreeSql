public static partial class FreeSqlDamengGlobalExtensions
{

    /// <summary>
    /// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
    /// </summary>
    /// <param name="that"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string FormatDameng(this string that, params object[] args) => _damengAdo.Addslashes(that, args);
    static FreeSql.Dameng.DamengAdo _damengAdo = new FreeSql.Dameng.DamengAdo();
}
