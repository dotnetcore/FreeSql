using FreeSql.DataAnnotations;
using FreeSql.Tests.DataContext.SqlServer;
using System;
using Xunit;

namespace FreeSql.Tests.SqlServer
{
    [Collection("SqlServerCollection")]
    public class SqlServerDbFirstTest
    {

        SqlServerFixture _sqlserverFixture;

        public SqlServerDbFirstTest(SqlServerFixture sqlserverFixture)
        {
            _sqlserverFixture = sqlserverFixture;
        }

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
