using FreeSql.DataAnnotations;
using System;
using Xunit;

namespace FreeSql.Tests.Odbc.Dameng
{
    public class DamengDbFirstTest
    {
        [Fact]
        public void GetDatabases()
        {

            var t1 = g.dameng.DbFirst.GetDatabases();

        }

        [Fact]
        public void GetTablesByDatabase()
        {

            var t2 = g.dameng.DbFirst.GetTablesByDatabase();
            //var tb = g.dameng.Ado.ExecuteArray(System.Data.CommandType.Text, "select * from \"tb_dbfirst\"");
        }
    }
}
