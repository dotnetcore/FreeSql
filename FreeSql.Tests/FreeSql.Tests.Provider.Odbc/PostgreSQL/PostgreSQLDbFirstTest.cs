using FreeSql.DataAnnotations;
using System;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.Odbc.PostgreSQL
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

            var t2 = g.pgsql.DbFirst.GetTablesByDatabase(g.pgsql.DbFirst.GetDatabases()[2]);

            var tb_alltype = t2.Where(a => a.Name == "tb_alltype").FirstOrDefault();

            var tb_identity = t2.Where(a => a.Name == "test_new").FirstOrDefault();
        }
    }
}
