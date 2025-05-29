using FreeSql.DataAnnotations;
using System;
using System.Diagnostics;
using System.Threading;
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
        public void ExecuteDataTable()
        {
            var dataTable = fsql.Ado.ExecuteDataTable("select * from tbiou04");

            using (var duck = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.DuckDB, "DataSource=D:\\WeChat Files\\WeChat Files\\q2881099\\FileStorage\\File\\2025-05")
                .UseAutoSyncStructure(true)
                .UseMonitorCommand(
                    cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText) //监听SQL命令对象，在执行前
                    , (cmd, traceLog) => Console.WriteLine(traceLog)
                    )
                .Build())
            {
                var dataTable2 = fsql.Ado.ExecuteDataTable("select * from db_db");
            }
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
