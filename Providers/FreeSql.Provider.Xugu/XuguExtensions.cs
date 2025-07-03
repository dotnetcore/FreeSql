using FreeSql;
using FreeSql.Xugu.Curd;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using XuguClient;

public static partial class FreeSqlXuguGlobalExtensions
{

    /// <summary>
    /// 特殊处理类似 string.Format 的使用方法，防止注入，以及 IS NULL 转换
    /// </summary>
    /// <param name="that"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string FormatXuguSQL(this string that, params object[] args) => _xgsqlAdo.Addslashes(that, args);
    static FreeSql.Xugu.XuguAdo _xgsqlAdo = new FreeSql.Xugu.XuguAdo();
     
}
