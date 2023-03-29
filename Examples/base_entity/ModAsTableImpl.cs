using FreeSql;
using FreeSql.DataAnnotations;
using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

class ModAsTableImpl : IAsTable
{
    public ModAsTableImpl(IFreeSql fsql)
    {
        AllTables = Enumerable.Range(0, 9).Select(a => $"order_{a}").ToArray();
        fsql.Aop.CommandBefore += (_, e) =>
        {
            e.Command.CommandText = Regex.Replace(e.Command.CommandText, @"/\*astable\([^\)]+\)*\/", "1=1");
        };
    }

    public string[] AllTables { get; }

    public string GetTableNameByColumnValue(object columnValue, bool autoExpand = false)
    {
        var modid = (int)columnValue;
        return $"order_{(modid % 10)}";
    }

    public string[] GetTableNamesByColumnValueRange(object columnValue1, object columnValue2)
    {
        throw new NotImplementedException();
    }

    public string[] GetTableNamesBySqlWhere(string sqlWhere, List<DbParameter> dbParams, SelectTableInfo tb, CommonUtils commonUtils)
    {
        var match = Regex.Match(sqlWhere, @"/\*astable\([^\)]+\)*\/");
        if (match.Success == false) return AllTables;
        var tables = match.Groups[1].Value.Split(',').Where(a => AllTables.Contains(a)).ToArray();
        if (tables.Any() == false) return AllTables;
        return tables;
    }
}
