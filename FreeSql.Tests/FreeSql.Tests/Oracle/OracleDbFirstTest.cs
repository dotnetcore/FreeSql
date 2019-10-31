using FreeSql.DataAnnotations;
using System;
using Xunit;

namespace FreeSql.Tests.Oracle
{
    public class OracleDbFirstTest
    {
        [Fact]
        public void GetDatabases()
        {

            var t1 = g.oracle.DbFirst.GetDatabases();

        }

        [Fact]
        public void GetTablesByDatabase()
        {

            var t2 = g.oracle.DbFirst.GetTablesByDatabase();
            //var tb = g.oracle.Ado.ExecuteArray(System.Data.CommandType.Text, "select * from \"tb_dbfirst\"");
        }
    }
}
