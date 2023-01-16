using FreeSql.DataAnnotations;
using MySql.Data.MySqlClient;
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

            var connectionString = "Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;Initial Catalog=cccddd;Charset=utf8;SslMode=none;Max pool size=51;Allow User Variables=True";
            using (var t2 = new FreeSqlBuilder()
                .UseConnectionFactory(FreeSql.DataType.MySql, () => new MySqlConnection(connectionString))
                .Build())
            {
                Assert.Equal("server=127.0.0.1;port=3306;user id=root;password=root;database=cccddd;characterset=utf8;sslmode=Disabled;maxpoolsize=51;allowuservariables=True", t2.Ado.ConnectionString);
            }
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

            var t6 = g.mysql.Ado.Query<xxx>("select * from song where id in ?ids", new { ids = new[] { 1, 2, 3 } });

            var t7 = g.mysql.Ado.Query<xxx>("select * from song where title in ?titles", new { titles = new[] { "title1", "title2", "title2" } });
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
