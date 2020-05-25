using FreeSql.DataAnnotations;
using System;
using Xunit;

namespace FreeSql.Tests.Sqlite
{
    public class SqliteDbFirstTest
    {
        [Fact]
        public void GetDatabases()
        {

            var t1 = g.sqlite.DbFirst.GetDatabases();

        }

        [Fact]
        public void GetTablesByDatabase()
        {

            var t2 = g.sqlite.DbFirst.GetTablesByDatabase();

        }
    }
}
