using FreeSql.DataAnnotations;
using System;
using Xunit;

namespace FreeSql.Tests.MySql
{
    public class MySqlDbFirstTest
    {
        [Fact]
        public void GetDatabases()
        {

            var t1 = g.mysql.DbFirst.GetDatabases();

        }

        [Fact]
        public void GetTablesByDatabase()
        {

            var t2 = g.mysql.DbFirst.GetTablesByDatabase(g.mysql.DbFirst.GetDatabases()[0]);

        }
    }
}
