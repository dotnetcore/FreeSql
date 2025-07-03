 
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
            fsql.CodeFirst.SyncStructure(typeof(test_existstb01));
            var t1 = fsql.DbFirst.GetTableByName("test_existstb01"); 
            Assert.NotNull(t1); 
            Assert.True(t1.Columns.Count > 0);  
            var t3 = fsql.DbFirst.GetTableByName("notexists_tb");
            Assert.Null(t3);
            fsql.Ado.ExecuteNonQuery("drop table test_existstb01");
        }

        [Fact]
        public void ExistsTable()
        {
            var fsql = g.xugu;
            Assert.False(fsql.DbFirst.ExistsTable("test_existstb01"));
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
