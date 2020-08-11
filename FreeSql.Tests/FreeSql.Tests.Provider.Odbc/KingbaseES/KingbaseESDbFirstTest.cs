using FreeSql.DataAnnotations;
using System;
using Xunit;

namespace FreeSql.Tests.Odbc.KingbaseES
{
    public class KingbaseESDbFirstTest
    {
        [Fact]
        public void GetDatabases()
        {
            var t1 = g.kingbaseES.DbFirst.GetDatabases();
        }

        [Fact]
        public void GetTablesByDatabase()
        {
            var t2 = g.kingbaseES.DbFirst.GetTablesByDatabase();
            Assert.True(t2.Count > 0);
        }

        [Fact]
        public void GetTableByName()
        {
            var fsql = g.kingbaseES;
            var t1 = fsql.DbFirst.GetTableByName("tb_alltype");
            var t2 = fsql.DbFirst.GetTableByName("public.tb_alltype");
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
            var fsql = g.kingbaseES;
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
