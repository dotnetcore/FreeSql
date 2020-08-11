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
    }
}
