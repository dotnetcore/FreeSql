using FreeSql.DataAnnotations;
using System;
using Xunit;

namespace FreeSql.Tests.Sqlite
{
    public class SqliteAdoTest
    {
        [Fact]
        public void Pool()
        {
            var t1 = g.sqlite.Ado.MasterPool.StatisticsFullily;
        }

        [Fact]
        public void SlavePools()
        {
            var t2 = g.sqlite.Ado.SlavePools.Count;
        }

        [Fact]
        public void ExecuteTest()
        {
            Assert.True(g.sqlite.Ado.ExecuteConnectTest());
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

            var t0 = g.sqlite.Ado.Query<testallDto>("select * from \"song\"");

            var t1 = g.sqlite.Ado.Query<testallDto>("select id, url, create_time from \"song\"");

            var t2 = g.sqlite.Ado.Query<testallDto>("select id, url, create_time test_time from \"song\"");

            var t3 = g.sqlite.Ado.Query<xxx>("select * from \"song\"");

            var t4 = g.sqlite.Ado.Query<(int, string, string)>("select * from \"song\"");

            var t5 = g.sqlite.Ado.Query<dynamic>("select * from \"song\"");

            var t6 = g.sqlite.Ado.Query<xxx>("select * from song where id in @ids", new { ids = new[] { 1, 2, 3 } });
        }

        [Fact]
        public void QueryMultipline()
        {
            var t3 = g.sqlite.Ado.Query<xxx, (int, string, string), dynamic>("select * from song; select * from song; select * from song");
        }

        class xxx
        {
            public int Id { get; set; }
            public string Path { get; set; }
            public string Title2 { get; set; }
        }

        class testallDto
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Url { get; set; }
            public DateTime Test_time { get; set; }
            public DateTime Create_time { get; set; }
            public bool Is_deleted { get; set; }
        }
    }
}
