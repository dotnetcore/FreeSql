using FreeSql;
using FreeSql.Firebird.Curd;
using System;

public static partial class FreeSqlFirebirdGlobalExtensions
{
    /// <summary>
    /// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
    /// </summary>
    /// <param name="that"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string FormatFirebird(this string that, params object[] args) => _firebirdAdo.Addslashes(that, args);
    static FreeSql.Firebird.FirebirdAdo _firebirdAdo = new FreeSql.Firebird.FirebirdAdo();
}
