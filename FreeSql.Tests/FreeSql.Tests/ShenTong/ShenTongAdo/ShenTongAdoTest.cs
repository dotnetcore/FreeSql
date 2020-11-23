using FreeSql.DataAnnotations;
using System;
using Xunit;

namespace FreeSql.Tests.ShenTong
{
    public class ShenTongAdoTest
    {
        [Fact]
        public void Pool()
        {
            var t1 = g.shentong.Ado.MasterPool.StatisticsFullily;
        }

        [Fact]
        public void SlavePools()
        {
            var t2 = g.shentong.Ado.SlavePools.Count;
        }

        [Fact]
        public void ExecuteTest()
        {
            Assert.True(g.shentong.Ado.ExecuteConnectTest());
        }
        [Fact]
        public void ExecuteReader()
        {

        }
        [Fact]
        public void ExecuteArray()
        {

        }
        [Fact]
        public void ExecuteNonQuery()
        {

        }
        [Fact]
        public void ExecuteScalar()
        {

        }

        [Fact]
        public void Query()
        {

            g.shentong.CodeFirst.SyncStructure<xxx>();
            var t3 = g.shentong.Ado.Query<xxx>("select * from xxx");

            var t4 = g.shentong.Ado.Query<(int, string, string)>("select * from xxx");

            var t5 = g.shentong.Ado.Query<dynamic>("select * from xxx");

            var t6 = g.shentong.Ado.Query<xxx>("select * from xxx where id in @ids", new { ids = new[] { "1", "2", "3" } });
        }

        [Fact]
        public void QueryMultipline()
        {
            g.shentong.CodeFirst.SyncStructure<xxx>();
            var t3 = g.shentong.Ado.Query<xxx, (int, string, string), dynamic>("select * from xxx; select * from xxx; select * from xxx");
        }

        class xxx
        {
            public string Id { get; set; }
            public string Path { get; set; }
            public string Title2 { get; set; }
        }
    }
}
