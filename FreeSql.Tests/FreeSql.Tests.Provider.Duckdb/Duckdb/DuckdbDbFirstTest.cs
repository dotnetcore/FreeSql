using FreeSql.DataAnnotations;
using System;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.Duckdb
{
    public class DuckdbDbFirstTest
    {
        IFreeSql fsql => g.duckdb;

        [Fact]
        public void GetDatabases()
        {
            var t1 = fsql.DbFirst.GetDatabases();
        }

        [Fact]
        public void GetTablesByDatabase()
        {
            var t2 = fsql.DbFirst.GetTablesByDatabase(fsql.DbFirst.GetDatabases()[0]);
            Assert.True(t2.Count > 0);
        }

        [Fact]
        public void GetTableByName()
        {
            fsql.Ado.ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS table_test
(
    id integer NOT NULL,
    coin_list decimal[],
    PRIMARY KEY (id)
)");
            var t111 = fsql.DbFirst.GetTableByName("table_test");
            Assert.True(t111.Columns.Find(a => a.Name == "id").IsPrimary);
            Assert.False(t111.Columns.Find(a => a.Name == "coin_list").IsPrimary);

            var t1 = fsql.DbFirst.GetTableByName("tb_alltype");
            var t2 = fsql.DbFirst.GetTableByName("main.tb_alltype");
            Assert.NotNull(t1);
            Assert.NotNull(t2);
            Assert.True(t1.Columns.Count > 0);
            Assert.True(t2.Columns.Count > 0);
            Assert.Equal(t1.Columns.Count, t2.Columns.Count);
            var t3 = fsql.DbFirst.GetTableByName("notexists_tb");
            Assert.Null(t3);
        }

        [Fact]
        public void ExistsTable()
        {
            Assert.False(fsql.DbFirst.ExistsTable("test_existstb01"));
            Assert.False(fsql.DbFirst.ExistsTable("main.test_existstb01"));
            Assert.False(fsql.DbFirst.ExistsTable("test_existstb01", false));
            Assert.False(fsql.DbFirst.ExistsTable("main.test_existstb01", false));
            fsql.CodeFirst.SyncStructure(typeof(test_existstb01));
            Assert.True(fsql.DbFirst.ExistsTable("test_existstb01"));
            Assert.True(fsql.DbFirst.ExistsTable("main.test_existstb01"));
            Assert.False(fsql.DbFirst.ExistsTable("Test_existstb01", false));
            Assert.False(fsql.DbFirst.ExistsTable("main.Test_existstb01", false));
            fsql.Ado.ExecuteNonQuery("drop table test_existstb01");
        }
        class test_existstb01
        {
            public Guid id { get; set; }
        }
    }
}
