using FreeSql.DataAnnotations;
using System;
using Xunit;

namespace FreeSql.Tests.ShenTong
{
    public class ShenTongDbFirstTest
    {
        [Fact]
        public void GetDatabases()
        {

            var t1 = g.shentong.DbFirst.GetDatabases();

        }

        [Fact]
        public void GetTablesByDatabase()
        {
            var t1 = g.shentong.DbFirst.GetTablesByDatabase();
            var t2 = g.shentong.DbFirst.GetTablesByDatabase(g.shentong.DbFirst.GetDatabases()[0]);

        }

        [Fact]
        public void ExistsTable()
        {
            var fsql = g.shentong;
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
            fsql.Ado.ExecuteNonQuery("drop table \"TBEXTS\".test_existstb01");
        }
        class test_existstb01
        {
            public Guid id { get; set; }
        }
    }
}
