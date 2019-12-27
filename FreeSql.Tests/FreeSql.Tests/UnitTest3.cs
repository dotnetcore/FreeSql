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
