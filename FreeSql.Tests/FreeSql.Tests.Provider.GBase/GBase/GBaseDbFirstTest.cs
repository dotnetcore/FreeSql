using FreeSql.DataAnnotations;
using System;
using Xunit;

namespace FreeSql.Tests.GBase
{
    public class GBaseDbFirstTest
    {
        [Fact]
        public void GetDatabases()
        {
        }

        [Fact]
        public void GetTablesByDatabase()
        {
        }

        [Fact]
        public void GetTableByName()
        {
            var fsql = g.gbase;
            var t1 = fsql.DbFirst.GetTableByName("tb_alltype");
            Assert.NotNull(t1);
            var t3 = fsql.DbFirst.GetTableByName("notexists_tb");
            Assert.Null(t3);
        }

        [Fact]
        public void ExistsTable()
        {
            var fsql = g.gbase;
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
