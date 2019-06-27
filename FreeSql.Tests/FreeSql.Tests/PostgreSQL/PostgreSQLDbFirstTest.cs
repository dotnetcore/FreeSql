using FreeSql.DataAnnotations;
using System;
using Xunit;

namespace FreeSql.Tests.PostgreSQL
{
    public class PostgreSQLDbFirstTest
    {
        [Fact]
        public void GetDatabases()
        {

            var t1 = g.pgsql.DbFirst.GetDatabases();

        }

        [Fact]
        public void GetTablesByDatabase()
        {

            var t2 = g.pgsql.DbFirst.GetTablesByDatabase(g.pgsql.DbFirst.GetDatabases()[1]);

        }
    }
}
