using FreeSql.DataAnnotations;
using System;
using Xunit;

namespace FreeSql.Tests.Odbc.MySql
{
    public class MySqlDbFirstTest
    {
        [Fact]
        public void GetDatabases()
        {

            var t1 = g.mysql.DbFirst.GetDatabases();

        }

        [Fact]
        public void GetTablesByDatabase()
        {

            var t2 = g.mysql.DbFirst.GetTablesByDatabase(g.mysql.DbFirst.GetDatabases()[0]);

        }

        [Fact]
        public void ExistsTable()
        {
            var fsql = g.mysql;
            Assert.False(fsql.DbFirst.ExistsTable("test_existstb01"));
            Assert.False(fsql.DbFirst.ExistsTable("cccddd_odbc.test_existstb01"));
            Assert.False(fsql.DbFirst.ExistsTable("test_existstb01", false));
            Assert.False(fsql.DbFirst.ExistsTable("cccddd_odbc.test_existstb01", false));
            fsql.CodeFirst.SyncStructure(typeof(test_existstb01));
            Assert.True(fsql.DbFirst.ExistsTable("test_existstb01"));
            Assert.True(fsql.DbFirst.ExistsTable("cccddd_odbc.test_existstb01"));
            Assert.True(fsql.DbFirst.ExistsTable("Test_existstb01", false));
            Assert.True(fsql.DbFirst.ExistsTable("cccddd_odbc.Test_existstb01", false));
            fsql.Ado.ExecuteNonQuery("drop table test_existstb01");

            Assert.False(fsql.DbFirst.ExistsTable("cccddd_odbc.test_existstb01"));
            Assert.False(fsql.DbFirst.ExistsTable("cccddd_odbc.test_existstb01", false));
            fsql.CodeFirst.SyncStructure(typeof(test_existstb01), "cccddd_odbc.test_existstb01");
            Assert.True(fsql.DbFirst.ExistsTable("cccddd_odbc.test_existstb01"));
            Assert.True(fsql.DbFirst.ExistsTable("cccddd_odbc.Test_existstb01", false));
            fsql.Ado.ExecuteNonQuery("drop table cccddd_odbc.test_existstb01");
        }
        class test_existstb01
        {
            public Guid id { get; set; }
        }
    }
}
