using FreeSql.DataAnnotations;
using System;
using Xunit;

namespace FreeSql.Tests.MySqlConnector
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
        public void ExecuteTest()
        {
            Assert.True(g.mysql.Ado.ExecuteConnectTest());
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
            var item = new TestExecute01 { title = "title01" };
            g.mysql.Insert(item).ExecuteAffrows();
            var affrows = g.mysql.Ado.ExecuteNonQuery("update TestExecute01 set title = '完成' where id=@id", new { id = item.id });
            Assert.Equal(1, affrows);
            var item2 = g.mysql.Select<TestExecute01>(item).First();
            Assert.NotNull(item2);
            Assert.Equal("完成", item2.title);
        }
        class TestExecute01
        {
            public Guid id { get; set; }
            public string title { get; set; }
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

            var t6 = g.mysql.Ado.Query<xxx>("select * from song where id in @ids", new { ids = new[] { 1, 2, 3 } });
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
