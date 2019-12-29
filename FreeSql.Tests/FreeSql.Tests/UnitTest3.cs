using FreeSql.DataAnnotations;
using FreeSql;
using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using Newtonsoft.Json.Linq;
using NpgsqlTypes;
using Npgsql.LegacyPostgis;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Data.SqlClient;
using kwlib;
using System.Diagnostics;
using System.IO;

namespace FreeSql.Tests
{
    public class UnitTest3
    {

        [Fact]
        public void Test03()
        {
            //using (var conn = new SqlConnection("Data Source=.;Integrated Security=True;Initial Catalog=webchat-abc;Pooling=true;Max Pool Size=13"))
            //{
            //    conn.Open();
            //    conn.Close();
            //}

            //using (var fsql = new FreeSql.FreeSqlBuilder()
            //    .UseConnectionString(FreeSql.DataType.SqlServer, "Data Source=.;Integrated Security=True;Initial Catalog=webchat-abc;Pooling=true;Max Pool Size=13")
            //    .UseAutoSyncStructure(true)
            //    //.UseGenerateCommandParameterWithLambda(true)
            //    .UseMonitorCommand(
            //        cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText) //监听SQL命令对象，在执行前
            //        //, (cmd, traceLog) => Console.WriteLine(traceLog)
            //        )
            //    .UseLazyLoading(true)
            //    .Build())
            //{
            //    fsql.Select<ut3_t1>().ToList();
            //}

            //var testByte = new TestByte { pic = File.ReadAllBytes(@"C:\Users\28810\Desktop\71500003-0ad69400-289e-11ea-85cb-36a54f52ebc0.png") };
            //var sql = g.sqlserver.Insert(testByte).NoneParameter().ToSql();
            //g.sqlserver.Insert(testByte).NoneParameter().ExecuteAffrows();

            //var getTestByte = g.sqlserver.Select<TestByte>(testByte).First();

            //File.WriteAllBytes(@"C:\Users\28810\Desktop\71500003-0ad69400-289e-11ea-85cb-36a54f52ebc0_write.png", getTestByte.pic);

            var ib = new IdleBus<IFreeSql>(TimeSpan.FromMinutes(10), 2);
            ib.Notice += (_, e2) => Trace.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] 线程{Thread.CurrentThread.ManagedThreadId}：{e2.Log}");

            ib.Register("db1", () => new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.MySql, "Data Source=127.0.0.1;Port=3306;User ID=root;Password=root;Initial Catalog=cccddd;Charset=utf8;SslMode=none;Max pool size=3")
                .UseAutoSyncStructure(true)
                .UseMonitorCommand(cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText))
                .UseLazyLoading(true)
                .Build());
            ib.Register("db2", () => new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.Oracle, "user id=user1;password=123456;data source=//127.0.0.1:1521/XE;Pooling=true;Max Pool Size=3")
                .UseAutoSyncStructure(true)
                .UseLazyLoading(true)
                .UseSyncStructureToUpper(true)
                .UseMonitorCommand(cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText))
                .Build());
            ib.Register("db3", () => new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=|DataDirectory|\document.db;Attachs=xxxtb.db;Pooling=true;Max Pool Size=3")
                .UseAutoSyncStructure(true)
                .UseLazyLoading(true)
                .UseMonitorCommand(cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText))
                .Build());
            //...注入很多个

            var fsql = ib.Get("db1"); //使用的时候用 Get 方法，不要存其引用关系
            fsql.Select<ut3_t1>().Limit(10).ToList();

            fsql = ib.Get("db2");
            fsql.Select<ut3_t1>().Limit(10).ToList();

            fsql = ib.Get("db3");
            fsql.Select<ut3_t1>().Limit(10).ToList();
        }

        class TestByte
        {
            public Guid id { get; set; }

            [Column(DbType = "varbinary(max)")]
            public byte[] pic { get; set; }
        }

        class ut3_t1
        {
            [Column(IsIdentity = true)]
            public int id { get; set; }
            public string name { get; set; }
        }
        class ut3_t2
        {
            [Column(IsIdentity = true)]
            public int id { get; set; }
            public string name { get; set; }
        }
    }

}
