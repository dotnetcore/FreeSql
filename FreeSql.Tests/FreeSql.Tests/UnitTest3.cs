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
using System.Text;

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
                .UseGenerateCommandParameterWithLambda(true)
                .UseMonitorCommand(cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText))
                .UseLazyLoading(true)
                .Build());
            ib.Register("db2", () => new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.Oracle, "user id=user1;password=123456;data source=//127.0.0.1:1521/XE;Pooling=true;Max Pool Size=3")
                .UseAutoSyncStructure(true)
                .UseGenerateCommandParameterWithLambda(true)
                .UseLazyLoading(true)
                .UseSyncStructureToUpper(true)
                .UseMonitorCommand(cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText))
                .Build());
            ib.Register("db3", () => new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=|DataDirectory|\document.db;Attachs=xxxtb.db;Pooling=true;Max Pool Size=3")
                .UseAutoSyncStructure(true)
                .UseGenerateCommandParameterWithLambda(true)
                .UseLazyLoading(true)
                .UseMonitorCommand(cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText))
                .Build());
            //...注入很多个

            var fsql = ib.Get("db1"); //使用的时候用 Get 方法，不要存其引用关系
            var sqlparamId = 100;
            fsql.Select<ut3_t1>().Limit(10).Where(a => a.id == sqlparamId).ToList();

            fsql = ib.Get("db2");
            fsql.Select<ut3_t1>().Limit(10).Where(a => a.id == sqlparamId).ToList();

            fsql = ib.Get("db3");
            fsql.Select<ut3_t1>().Limit(10).Where(a => a.id == sqlparamId).ToList();

            fsql = g.sqlserver;
            fsql.Insert<OrderMain>(new OrderMain { OrderNo = "1001", OrderTime = new DateTime(2019, 12, 01) }).ExecuteAffrows();
            fsql.Insert<OrderDetail>(new OrderDetail { OrderNo = "1001", ItemNo = "I001", Qty = 1 }).ExecuteAffrows();
            fsql.Insert<OrderDetail>(new OrderDetail { OrderNo = "1001", ItemNo = "I002", Qty = 1 }).ExecuteAffrows();
            fsql.Insert<OrderDetail>(new OrderDetail { OrderNo = "1001", ItemNo = "I003", Qty = 1 }).ExecuteAffrows();
            fsql.Insert<OrderMain>(new OrderMain { OrderNo = "1002", OrderTime = new DateTime(2019, 12, 02) }).ExecuteAffrows();
            fsql.Insert<OrderDetail>(new OrderDetail { OrderNo = "1002", ItemNo = "I011", Qty = 1 }).ExecuteAffrows();
            fsql.Insert<OrderDetail>(new OrderDetail { OrderNo = "1002", ItemNo = "I012", Qty = 1 }).ExecuteAffrows();
            fsql.Insert<OrderDetail>(new OrderDetail { OrderNo = "1002", ItemNo = "I013", Qty = 1 }).ExecuteAffrows();
            fsql.Ado.Query<object>("select * from orderdetail left join ordermain on orderdetail.orderno=ordermain.orderno where ordermain.orderno='1001'");


            g.oracle.Delete<SendInfo>().Where("1=1").ExecuteAffrows();
            g.oracle.Insert(new[]
                {
                    new SendInfo{ Code = "001", Binary = Encoding.UTF8.GetBytes("我是中国人") },
                    new SendInfo{ Code = "002", Binary = Encoding.UTF8.GetBytes("我是地球人") },
                    new SendInfo{ Code = "003", Binary = Encoding.UTF8.GetBytes("我是.net")},
                    new SendInfo{ Code = "004", Binary = Encoding.UTF8.GetBytes("我是freesql") },
                })
                .NoneParameter().ExecuteAffrows();

            var slslsl = g.oracle.Select<SendInfo>().ToList();

            var slsls1Ids = slslsl.Select(a => a.ID).ToArray();
            var slslss2 = g.oracle.Select<SendInfo>().Where(a => slsls1Ids.Contains(a.ID)).ToList();

            var mt_codeId = Guid.Parse("2f48c5ca-7257-43c8-9ee2-0e16fa990253");
            Assert.Equal(1, g.oracle.Insert(new SendInfo { ID = mt_codeId, Code = "mt_code", Binary = Encoding.UTF8.GetBytes("我是mt_code") })
                .ExecuteAffrows());
            var mt_code = g.oracle.Select<SendInfo>().Where(a => a.ID == mt_codeId).First();
            Assert.NotNull(mt_code);
            Assert.Equal(mt_codeId, mt_code.ID);
            Assert.Equal("mt_code", mt_code.Code);

            mt_code = g.oracle.Select<SendInfo>().Where(a => a.ID == Guid.Parse("2f48c5ca725743c89ee20e16fa990253".ToUpper())).First();
            Assert.NotNull(mt_code);
            Assert.Equal(mt_codeId, mt_code.ID);
            Assert.Equal("mt_code", mt_code.Code);

            mt_codeId = Guid.Parse("2f48c5ca-7257-43c8-9ee2-0e16fa990251");
            Assert.Equal(1, g.oracle.Insert(new SendInfo { ID = mt_codeId, Code = "mt_code2", Binary = Encoding.UTF8.GetBytes("我是mt_code2") })
                .NoneParameter()
                .ExecuteAffrows());
            mt_code = g.oracle.Select<SendInfo>().Where(a => a.ID == mt_codeId).First();
            Assert.NotNull(mt_code);
            Assert.Equal(mt_codeId, mt_code.ID);
            Assert.Equal("mt_code2", mt_code.Code);

            mt_code = g.oracle.Select<SendInfo>().Where(a => a.ID == Guid.Parse("2f48c5ca725743c89ee20e16fa990251".ToUpper())).First();
            Assert.NotNull(mt_code);
            Assert.Equal(mt_codeId, mt_code.ID);
            Assert.Equal("mt_code2", mt_code.Code);
        }

        [Table(Name = "t_text")]
        public class SendInfo
        {
            [Column(IsPrimary = true, DbType = "raw(16)", MapType = typeof(byte[]))]
            public Guid ID { get; set; }

            [Column(Name = "YPID5")]
            public string Code { get; set; }
        
            public byte[] Binary { get; set; }

            [Column(ServerTime = DateTimeKind.Utc, CanUpdate = false)]
            public DateTime 创建时间 { get; set; }

            [Column(ServerTime = DateTimeKind.Utc)]
            public DateTime 更新时间 { get; set; }

            [Column(InsertValueSql = "'123'")]
            public string InsertValue2 { get; set; }
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

        public class OrderMain
        {
            public string OrderNo { get; set; }
            public DateTime OrderTime { get; set; }
            public decimal Amount { get; set; }
        }
        public class OrderDetail
        {
            public string OrderNo { get; set; }
            public string ItemNo { get; set; }
            public decimal Qty { get; set; }
        }
    }

}
