using FreeSql.DataAnnotations;
using System;
using Xunit;

namespace FreeSql.Tests.Odbc.SqlServer
{
    [Collection("SqlServerCollection")]
    public class SqlServerDbFirstTest
    {
        [Fact]
        public void GetDatabases()
        {

            var t1 = g.sqlserver.DbFirst.GetDatabases();

        }

        [Fact]
        public void GetTablesByDatabase()
        {

            var t2 = g.sqlserver.DbFirst.GetTablesByDatabase();

        }
    }
}
