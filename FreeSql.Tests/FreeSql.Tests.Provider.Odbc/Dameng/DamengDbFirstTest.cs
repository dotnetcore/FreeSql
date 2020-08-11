using FreeSql.DataAnnotations;
using System;
using Xunit;

namespace FreeSql.Tests.Odbc.Dameng
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
            //var tb = g.dameng.Ado.ExecuteArray(System.Data.CommandType.Text, "select * from \"tb_dbfirst\"");
        }

        [Fact]
        public void ExistsTable()
        {
            var fsql = g.dameng;
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
