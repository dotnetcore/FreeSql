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
    }
}
