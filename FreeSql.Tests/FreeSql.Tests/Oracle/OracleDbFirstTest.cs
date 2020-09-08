using FreeSql.DataAnnotations;
using System;
using Xunit;

namespace FreeSql.Tests.Oracle
{
    public class OracleDbFirstTest
    {
        [Fact]
        public void GetDatabases()
        {
            var t1 = g.oracle.DbFirst.GetDatabases();
        }

        [Fact]
        public void GetTablesByDatabase()
        {
            var t2 = g.oracle.DbFirst.GetTablesByDatabase();
            Assert.True(t2.Count > 0);
            //var tb = g.oracle.Ado.ExecuteArray(System.Data.CommandType.Text, "select * from \"tb_dbfirst\"");
        }

        [Fact]
        public void GetTableByName()
        {
            var fsql = g.oracle;
            var t1 = fsql.DbFirst.GetTableByName("tb_alltype");
            var t2 = fsql.DbFirst.GetTableByName("1user.tb_alltype");
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
            var fsql = g.oracle;
            Assert.False(fsql.DbFirst.ExistsTable("test_existstb01"));
            Assert.False(fsql.DbFirst.ExistsTable("1user.test_existstb01"));
            Assert.False(fsql.DbFirst.ExistsTable("test_existstb01", false));
            Assert.False(fsql.DbFirst.ExistsTable("1user.test_existstb01", false));
            fsql.CodeFirst.SyncStructure(typeof(test_existstb01));
            Assert.True(fsql.DbFirst.ExistsTable("test_existstb01"));
            Assert.True(fsql.DbFirst.ExistsTable("1user.test_existstb01"));
            Assert.False(fsql.DbFirst.ExistsTable("test_existstb01", false));
            Assert.False(fsql.DbFirst.ExistsTable("1user.test_existstb01", false));
            fsql.Ado.ExecuteNonQuery("drop table test_existstb01");

            Assert.False(fsql.DbFirst.ExistsTable("1user.test_existstb01"));
            Assert.False(fsql.DbFirst.ExistsTable("1user.test_existstb01", false));
            fsql.CodeFirst.SyncStructure(typeof(test_existstb01), "1user.test_existstb01");
            Assert.True(fsql.DbFirst.ExistsTable("1user.test_existstb01"));
            Assert.False(fsql.DbFirst.ExistsTable("1user.test_existstb01", false));
            fsql.Ado.ExecuteNonQuery("drop table \"1USER\".test_existstb01");
        }
        class test_existstb01
        {
            public Guid id { get; set; }
        }
    }
}
