using FreeSql.DataAnnotations;
using System;
using Xunit;

namespace FreeSql.Tests.Firebird
{
    public class FirebirdAdoTest
    {
        [Fact]
        public void Pool()
        {
            var t1 = g.firebird.Ado.MasterPool.StatisticsFullily;

            var connectionString = @"database=localhost:D:\fbdata\EXAMPLES.fdb;user=sysdba;password=123456;max pool size=5";
            using (var t2 = new FreeSqlBuilder()
                .UseConnectionFactory(FreeSql.DataType.Firebird, () => new FirebirdSql.Data.FirebirdClient.FbConnection(connectionString))
                .Build())
            {
                Assert.Equal(connectionString, t2.Ado.ConnectionString);
            }
        }

        [Fact]
        public void SlavePools()
        {
            var t2 = g.firebird.Ado.SlavePools.Count;
        }

        [Fact]
        public void ExecuteTest()
        {
            Assert.True(g.firebird.Ado.ExecuteConnectTest());
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

            var t3 = g.firebird.Ado.Query<xxx>("select * from \"TB_TOPIC22\"");

            var t4 = g.firebird.Ado.Query<(int, string, string)>("select * from \"TB_TOPIC22\"");

            var t5 = g.firebird.Ado.Query<dynamic>("select * from \"TB_TOPIC22\"");

            var t6 = g.firebird.Ado.Query<xxx>("select * from \"TB_TOPIC22\" where \"ID\" in @ids", new { ids = new[] { 1, 2, 3 } });
        }

        [Fact]
        public void QueryMultipline()
        {
            //var t3 = g.firebird.Ado.Query<xxx, (int, string, string), dynamic>("select * from \"TB_TOPIC\"; select * from \"TB_TOPIC\"; select * from \"TB_TOPIC\"");
        }

        class xxx
        {
            public int Id { get; set; }
            public string Path { get; set; }
            public string Title2 { get; set; }
        }
    }
}
