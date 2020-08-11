using FreeSql.DataAnnotations;
using FreeSql.Tests.DataContext.SqlServer;
using System;
using Xunit;

namespace FreeSql.Tests.SqlServer
{
    [Collection("SqlServerCollection")]
    public class SqlServerDbFirstTest
    {

        SqlServerFixture _sqlserverFixture;

        public SqlServerDbFirstTest(SqlServerFixture sqlserverFixture)
        {
            _sqlserverFixture = sqlserverFixture;
        }

        [Fact]
        public void GetDatabases()
        {

            var t1 = g.sqlserver.DbFirst.GetDatabases();

        }

        [Fact]
        public void GetTablesByDatabase()
        {

            var t2 = g.sqlserver.DbFirst.GetTablesByDatabase();

        }

        [Fact]
        public void ExistsTable()
        {
            var fsql = g.sqlserver;
            Assert.False(fsql.DbFirst.ExistsTable("test_existstb01"));
            Assert.False(fsql.DbFirst.ExistsTable("dbo.test_existstb01"));
            Assert.False(fsql.DbFirst.ExistsTable("test_existstb01", false));
            Assert.False(fsql.DbFirst.ExistsTable("dbo.test_existstb01", false));
            fsql.CodeFirst.SyncStructure(typeof(test_existstb01));
            Assert.True(fsql.DbFirst.ExistsTable("test_existstb01"));
            Assert.True(fsql.DbFirst.ExistsTable("dbo.test_existstb01"));
            Assert.True(fsql.DbFirst.ExistsTable("Test_existstb01", false));
            Assert.True(fsql.DbFirst.ExistsTable("dbo.Test_existstb01", false));
            fsql.Ado.ExecuteNonQuery("drop table test_existstb01");

            Assert.False(fsql.DbFirst.ExistsTable("xxxtb.test_existstb01"));
            Assert.False(fsql.DbFirst.ExistsTable("xxxtb.test_existstb01", false));
            fsql.CodeFirst.SyncStructure(typeof(test_existstb01), "xxxtb.test_existstb01");
            Assert.True(fsql.DbFirst.ExistsTable("xxxtb.test_existstb01"));
            Assert.True(fsql.DbFirst.ExistsTable("xxxtb.Test_existstb01", false));
            fsql.Ado.ExecuteNonQuery("drop table xxxtb.test_existstb01");
        }
        class test_existstb01
        {
            public Guid id { get; set; }
        }
    }
}
