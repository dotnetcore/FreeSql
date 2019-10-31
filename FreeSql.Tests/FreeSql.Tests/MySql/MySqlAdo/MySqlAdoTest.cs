using FreeSql.DataAnnotations;
using System;
using Xunit;

namespace FreeSql.Tests.MySql
{
    public class MySqlAdoTest
    {
        [Fact]
        public void Pool()
        {
            var t1 = g.mysql.Ado.MasterPool.StatisticsFullily;
        }

        [Fact]
        public void SlavePools()
        {
            var t2 = g.mysql.Ado.SlavePools.Count;
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
            var t3 = g.mysql.Ado.Query<xxx>("select * from song");

            var t4 = g.mysql.Ado.Query<(int, string, string)>("select * from song");

            var t5 = g.mysql.Ado.Query<dynamic>("select * from song");
        }

        [Fact]
        public void QueryMultipline()
        {
            var t3 = g.mysql.Ado.Query<xxx, (int, string, string), dynamic>("select * from song; select * from song; select * from song");
        }

        class xxx
        {
            public int Id { get; set; }
            public string Path { get; set; }
            public string Title2 { get; set; }
        }
    }
}
