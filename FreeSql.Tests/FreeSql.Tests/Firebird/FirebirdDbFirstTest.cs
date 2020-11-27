using FreeSql.DataAnnotations;
using System;
using Xunit;

namespace FreeSql.Tests.Firebird
{
    public class FirebirdDbFirstTest
    {
        [Fact]
        public void GetDatabases()
        {
            var t1 = g.firebird.DbFirst.GetDatabases();
        }

        [Fact]
        public void GetTablesByDatabase()
        {
            var t1 = g.firebird.DbFirst.GetTablesByDatabase();
            var t2 = g.firebird.DbFirst.GetTablesByDatabase(g.firebird.DbFirst.GetDatabases()[0]);
            Assert.True(t1.Count > 0);
            Assert.True(t2.Count > 0);
        }

        [Fact]
        public void GetTableByName()
        {
            var fsql = g.firebird;
            var t1 = fsql.DbFirst.GetTableByName("tb_alltype");
            Assert.NotNull(t1);
            Assert.True(t1.Columns.Count > 0);
            var t3 = fsql.DbFirst.GetTableByName("notexists_tb");
            Assert.Null(t3);
        }

        [Fact]
        public void ExistsTable()
        {
            var fsql = g.firebird;
            try
            {
                fsql.Ado.ExecuteNonQuery("drop table test_existstb011");
            }
            catch { }
            Assert.False(fsql.DbFirst.ExistsTable("test_existstb011"));
            Assert.False(fsql.DbFirst.ExistsTable("test_existstb011", false));
            fsql.CodeFirst.SyncStructure(typeof(test_existstb011));
            Assert.True(fsql.DbFirst.ExistsTable("test_existstb011"));
            Assert.False(fsql.DbFirst.ExistsTable("Test_existstb011", false));
            fsql.Ado.ExecuteNonQuery("drop table test_existstb011");
        }
        class test_existstb011
        {
            public Guid id { get; set; }
        }
    }
}
