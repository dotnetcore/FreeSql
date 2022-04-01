using FreeSql;
using FreeSql.ClickHouse.Curd;
using System;

public static partial class FreeSqlClickHouseGlobalExtensions
{

    /// <summary>
    /// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
    /// </summary>
    /// <param name="that"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string FormatClickHouse(this string that, params object[] args) => _clickHouseAdo.Addslashes(that, args);
    static FreeSql.ClickHouse.ClickHouseAdo _clickHouseAdo = new FreeSql.ClickHouse.ClickHouseAdo();


}
