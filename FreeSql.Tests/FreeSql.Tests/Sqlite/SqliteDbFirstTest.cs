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
            Assert.True(t2.Count > 0);
        }

        [Fact]
        public void GetTableByName()
        {
            var fsql = g.sqlite;
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
            var fsql = g.sqlite;
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

            Assert.False(fsql.DbFirst.ExistsTable("xxxtb.test_existstb01"));
            Assert.False(fsql.DbFirst.ExistsTable("xxxtb.test_existstb01", false));
            fsql.CodeFirst.SyncStructure(typeof(test_existstb01), "xxxtb.test_existstb01");
            Assert.True(fsql.DbFirst.ExistsTable("xxxtb.test_existstb01"));
            Assert.False(fsql.DbFirst.ExistsTable("xxxtb.Test_existstb01", false));
            fsql.Ado.ExecuteNonQuery("drop table xxxtb.test_existstb01");
        }
        class test_existstb01
        {
            public Guid id { get; set; }
        }

        [Fact]
        public void TestIdentity()
        {
            var fsql = g.sqlite;
            fsql.CodeFirst.SyncStructure<ts_identity01>();

            var tb = fsql.DbFirst.GetTableByName("ts_identity01");
            Assert.NotNull(tb);
            Assert.True(tb.Primarys.Count == 1);
            Assert.True(tb.Primarys[0].IsIdentity);
        }
        class ts_identity01
        {
            [Column(IsIdentity = true)]
            public int id { get; set; }

            public string name { get; set; }
        }
    }
}
