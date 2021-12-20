using FreeSql;
using FreeSql.GBase.Curd;
using System;

public static partial class FreeSqlGBaseGlobalExtensions
{
    /// <summary>
    /// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
    /// </summary>
    /// <param name="that"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string FormatGBase(this string that, params object[] args) => _gbaseAdo.Addslashes(that, args);
    static FreeSql.GBase.GBaseAdo _gbaseAdo = new FreeSql.GBase.GBaseAdo();
}
