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

        [Fact]
        public void ExistsTable()
        {
            var fsql = g.pgsql;
            Assert.False(fsql.DbFirst.ExistsTable("test_existstb01"));
            Assert.False(fsql.DbFirst.ExistsTable("public.test_existstb01"));
            Assert.False(fsql.DbFirst.ExistsTable("test_existstb01", false));
            Assert.False(fsql.DbFirst.ExistsTable("public.test_existstb01", false));
            fsql.CodeFirst.SyncStructure(typeof(test_existstb01));
            Assert.True(fsql.DbFirst.ExistsTable("test_existstb01"));
            Assert.True(fsql.DbFirst.ExistsTable("public.test_existstb01"));
            Assert.False(fsql.DbFirst.ExistsTable("Test_existstb01", false));
            Assert.False(fsql.DbFirst.ExistsTable("public.Test_existstb01", false));
            fsql.Ado.ExecuteNonQuery("drop table test_existstb01");

            Assert.False(fsql.DbFirst.ExistsTable("tbexts.test_existstb01"));
            Assert.False(fsql.DbFirst.ExistsTable("tbexts.test_existstb01", false));
            fsql.CodeFirst.SyncStructure(typeof(test_existstb01), "tbexts.test_existstb01");
            Assert.True(fsql.DbFirst.ExistsTable("tbexts.test_existstb01"));
            Assert.False(fsql.DbFirst.ExistsTable("tbexts.Test_existstb01", false));
            fsql.Ado.ExecuteNonQuery("drop table \"tbexts\".test_existstb01");
        }
        class test_existstb01
        {
            public Guid id { get; set; }
        }
    }
}
