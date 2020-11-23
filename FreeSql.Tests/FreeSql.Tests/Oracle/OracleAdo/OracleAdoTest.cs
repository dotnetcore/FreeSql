using FreeSql.DataAnnotations;
using System;
using Xunit;

namespace FreeSql.Tests.Oracle
{
    public class OracleAdoTest
    {
        [Fact]
        public void Pool()
        {
            var t1 = g.oracle.Ado.MasterPool.StatisticsFullily;
        }

        [Fact]
        public void SlavePools()
        {
            var t2 = g.oracle.Ado.SlavePools.Count;
        }

        [Fact]
        public void ExecuteTest()
        {
            Assert.True(g.oracle.Ado.ExecuteConnectTest());
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

            var t3 = g.oracle.Ado.Query<xxx>("select * from \"TB_TOPIC\"");

            var t4 = g.oracle.Ado.Query<(int, string, string)>("select * from \"TB_TOPIC\"");

            var t5 = g.oracle.Ado.Query<dynamic>("select * from \"TB_TOPIC\"");

            var t6 = g.oracle.Ado.Query<xxx>("select * from TB_TOPIC where id in :ids", new { ids = new[] { 1, 2, 3 } });
        }

        [Fact]
        public void QueryMultipline()
        {
            //var t3 = g.oracle.Ado.Query<xxx, (int, string, string), dynamic>("select * from \"TB_TOPIC\"; select * from \"TB_TOPIC\"; select * from \"TB_TOPIC\"");
        }

        class xxx
        {
            public int Id { get; set; }
            public string Path { get; set; }
            public string Title2 { get; set; }
        }
    }
}
