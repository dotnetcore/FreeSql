using FreeSql.DataAnnotations;
using System;
using Xunit;

namespace FreeSql.Tests.KingbaseES
{
    public class KingbaseESAdoTest
    {
        [Fact]
        public void Pool()
        {
            var t1 = g.kingbaseES.Ado.MasterPool.StatisticsFullily;
        }

        [Fact]
        public void SlavePools()
        {
            var t2 = g.kingbaseES.Ado.SlavePools.Count;
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

            var t3 = g.kingbaseES.Ado.Query<xxx>("select * from \"TB_TOPIC\"");

            var t4 = g.kingbaseES.Ado.Query<(int, string, string)>("select * from \"TB_TOPIC\"");

            var t5 = g.kingbaseES.Ado.Query<dynamic>("select * from \"TB_TOPIC\"");

            var t6 = g.kingbaseES.Ado.Query<xxx>("select * from TB_TOPIC where id in @ids", new { ids = new[] { 1, 2, 3 } });
        }

        [Fact]
        public void QueryMultipline()
        {
            //var t3 = g.kingbaseES.Ado.Query<xxx, (int, string, string), dynamic>("select * from \"TB_TOPIC\"; select * from \"TB_TOPIC\"; select * from \"TB_TOPIC\"");
        }

        class xxx
        {
            public int Id { get; set; }
            public string Path { get; set; }
            public string Title2 { get; set; }
        }
    }
}
