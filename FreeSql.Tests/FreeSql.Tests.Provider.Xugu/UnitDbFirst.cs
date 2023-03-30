 
namespace FreeSql.Tests.Provider.Xugu
{
    public class UnitDbFirst
    {
        [Fact]
        public void GetDatabases()
        {
            var t1 = g.xugu.DbFirst.GetDatabases();
            
            Assert.True(t1.Count > 0);
        }

        [Fact]
        public void GetTablesByDatabase()
        {
            var t2 = g.xugu.DbFirst.GetTablesByDatabase();
            Assert.True(t2.Count > 0);
        }

        [Fact]
        public void GetTableByName()
        {
            var fsql = g.xugu;
            var t1 = fsql.DbFirst.GetTableByName("GENERAL.system_log");
            Assert.NotNull(t1);
            Assert.True(t1.Columns.Count > 0);
        }

        [Fact]
        public void ExistsTable()
        {
            var fsql = g.xugu;
            Assert.False(fsql.DbFirst.ExistsTable("GENERAL.system_log"));
            fsql.CodeFirst.SyncStructure(typeof(test_existstb01));
            Assert.True(fsql.DbFirst.ExistsTable("test_existstb01"));
            fsql.Ado.ExecuteNonQuery("drop table test_existstb01");
        }
        class test_existstb01
        {
            public long id { get; set; }
        }
    }
}
