using FreeSql.DataAnnotations;
using System;
using Xunit;

namespace FreeSql.Tests.Duckdb
{
    public class DuckdbAdoTest
    {
        IFreeSql fsql => g.duckdb;

        [Fact]
        public void SlavePools()
        {
            var t2 = fsql.Ado.SlavePools.Count;
        }

        [Fact]
        public void ExecuteTest()
        {
            Assert.True(fsql.Ado.ExecuteConnectTest());
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

            fsql.CodeFirst.SyncStructure<xxx>();
            var t3 = fsql.Ado.Query<xxx>("select * from xxx");

            var t4 = fsql.Ado.Query<(int, string, string)>("select * from xxx");

            var t5 = fsql.Ado.Query<dynamic>("select * from xxx");

            //var t6 = fsql.Ado.Query<xxx>("select * from xxx where id in $ids", new { ids = new[] { "1", "2", "3" } });
        }

        [Fact]
        public void QueryMultipline()
        {
            fsql.CodeFirst.SyncStructure<xxx>();
            var t3 = fsql.Ado.Query<xxx, (int, string, string), dynamic>("select * from xxx; select * from xxx; select * from xxx");
        }

        class xxx
        {
            public string Id { get; set; }
            public string Path { get; set; }
            public string Title2 { get; set; }
        }
    }
}
