using FreeSql.DataAnnotations;
using System;
using Xunit;

namespace FreeSql.Tests.Dameng
{
    public class DamengDbFirstTest
    {
        [Fact]
        public void GetDatabases()
        {
            var t1 = g.dameng.DbFirst.GetDatabases();
        }

        [Fact]
        public void GetTablesByDatabase()
        {
            var t2 = g.dameng.DbFirst.GetTablesByDatabase();
            Assert.True(t2.Count > 0);
            //var tb = g.dameng.Ado.ExecuteArray(System.Data.CommandType.Text, "select * from \"tb_dbfirst\"");
        }

        [Fact]
        public void GetTableByName()
        {
            var fsql = g.dameng;
            var t1 = fsql.DbFirst.GetTableByName("tb_alltype");
            var t2 = fsql.DbFirst.GetTableByName("2user.tb_alltype");
            Assert.NotNull(t1);
            Assert.NotNull(t2);
            Assert.True(t1.Columns.Count > 0);
            Assert.True(t2.Columns.Count > 0);
            Assert.Equal(t1.Columns.Count, t2.Columns.Count);
            var t3 = fsql.DbFirst.GetTableByName("notexists_tb");
            Assert.Null(t3);

            var t4 = fsql.DbFirst.GetTableByName("v_2user_v1");
            Assert.NotNull(t4);
        }

        [Fact]
        public void ExistsTable()
        {
            var fsql = g.dameng;
            Assert.False(fsql.DbFirst.ExistsTable("test_existstb01"));
            Assert.False(fsql.DbFirst.ExistsTable("2user.test_existstb01"));
            Assert.False(fsql.DbFirst.ExistsTable("test_existstb01", false));
            Assert.False(fsql.DbFirst.ExistsTable("2user.test_existstb01", false));
            fsql.CodeFirst.SyncStructure(typeof(test_existstb01));
            Assert.True(fsql.DbFirst.ExistsTable("test_existstb01"));
            Assert.True(fsql.DbFirst.ExistsTable("2user.test_existstb01"));
            Assert.False(fsql.DbFirst.ExistsTable("test_existstb01", false));
            Assert.False(fsql.DbFirst.ExistsTable("2user.test_existstb01", false));
            fsql.Ado.ExecuteNonQuery("drop table test_existstb01");

            Assert.False(fsql.DbFirst.ExistsTable("2user.test_existstb01"));
            Assert.False(fsql.DbFirst.ExistsTable("2user.test_existstb01", false));
            fsql.CodeFirst.SyncStructure(typeof(test_existstb01), "2user.test_existstb01");
            Assert.True(fsql.DbFirst.ExistsTable("2user.test_existstb01"));
            Assert.False(fsql.DbFirst.ExistsTable("2user.test_existstb01", false));
            fsql.Ado.ExecuteNonQuery("drop table \"2USER\".test_existstb01");
        }
        class test_existstb01
        {
            public Guid id { get; set; }
        }
    }
}
