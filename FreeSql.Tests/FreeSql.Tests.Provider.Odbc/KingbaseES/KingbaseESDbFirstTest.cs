using FreeSql.DataAnnotations;
using System;
using Xunit;

namespace FreeSql.Tests.Odbc.KingbaseES
{
    public class KingbaseESDbFirstTest
    {
        [Fact]
        public void GetDatabases()
        {
            var t1 = g.kingbaseES.DbFirst.GetDatabases();
        }

        [Fact]
        public void GetTablesByDatabase()
        {
            var t2 = g.kingbaseES.DbFirst.GetTablesByDatabase();
            //var tb = g.kingbaseES.Ado.ExecuteArray(System.Data.CommandType.Text, "select * from \"tb_dbfirst\"");
        }
    }
}
