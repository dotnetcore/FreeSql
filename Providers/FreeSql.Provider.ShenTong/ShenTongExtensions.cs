using FreeSql;
using FreeSql.ShenTong.Curd;
using System;
using System.Linq.Expressions;

public static partial class FreeSqlShenTongGlobalExtensions
{

    /// <summary>
    /// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
    /// </summary>
    /// <param name="that"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string FormatShenTong(this string that, params object[] args) => _shentongAdo.Addslashes(that, args);
    static FreeSql.ShenTong.ShenTongAdo _shentongAdo = new FreeSql.ShenTong.ShenTongAdo();
}
