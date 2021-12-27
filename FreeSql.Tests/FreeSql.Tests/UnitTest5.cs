using AME.Helpers;
using FreeSql.DataAnnotations;
using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Xunit;
using static FreeSql.Tests.UnitTest1;

namespace FreeSql.Tests
{
    public class UnitTest5
    {
        [Fact]
        public void TestLambdaParameterWhereIn()
        {
            using (var fsql = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=|DataDirectory|\TestLambdaParameterWhereIn.db")
                .UseAutoSyncStructure(true)
                .UseGenerateCommandParameterWithLambda(true)
                .UseLazyLoading(true)
                .UseMonitorCommand(
                    cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText) //监听SQL命令对象，在执行前
                    //, (cmd, traceLog) => Console.WriteLine(traceLog)
                    )
                .Build())
            {

                string dwId = "123456";
                string yhId = "654321";

                var sql = fsql.Select<wygl_wygs_gzry_wyglqyModelTest1>()
                      .Where(a => a.dw_id == dwId &&
                         fsql.Select<wygl_wygs_gzry_wyglqyModel>()
                               .Where(b => b.yh_id == yhId).ToList(b => b.wyqy_id).Contains(a.wyqy_id)
                      );

                var sql1 = sql.ToSql();
                Assert.Equal(@"SELECT a.""dw_id"", a.""wyqy_id"" 
FROM ""wygl_wygs_gzry_wyglqyModelTest1"" a 
WHERE (a.""dw_id"" = @exp_0 AND ((a.""wyqy_id"") in (SELECT b.""wyqy_id"" 
    FROM ""wygl_wygs_gzry_wyglqyModel"" b 
    WHERE (b.""yh_id"" = @exp_1))))", sql1);
                Assert.Equal(2, (sql as Select0Provider)._params.Count);
                Assert.Equal("123456", (sql as Select0Provider)._params[0].Value);
                Assert.Equal("654321", (sql as Select0Provider)._params[1].Value);
            }
        }
        class wygl_wygs_gzry_wyglqyModelTest1
        {
            public string dw_id { get; set; }
            public string wyqy_id { get; set; }
        }
        class wygl_wygs_gzry_wyglqyModel
        {
            public string yh_id { get; set; }
            public string wyqy_id { get; set; }
        }



        [Fact]
        public void TestJsonb01()
        {
            var fsql = g.pgsql;
            fsql.Delete<TestJsonb01Cls1>().Where("1=1").ExecuteAffrows();

            var item = new TestJsonb01Cls1
            {
                jsonb01 = new List<int> { 1, 5, 10, 20 },
                jsonb02 = new List<long> { 11, 51, 101, 201 },
                jsonb03 = new List<string> { "12", "52", "102", "202" },
            };
            fsql.Insert(item).ExecuteAffrows();

            var items = fsql.Select<TestJsonb01Cls1>().ToList();
        }

        [Fact]
        public void TestClickHouse()
        {
            var fsql = g.mysql;
            fsql.Delete<TestJsonb01Cls1>().Where("1=1").ExecuteAffrows();

            var item = new TestJsonb01Cls1
            {
                jsonb01 = new List<int> { 1, 5, 10, 20 },
                jsonb02 = new List<long> { 11, 51, 101, 201 },
                jsonb03 = new List<string> { "12", "52", "102", "202" },
            };
            fsql.Insert(item).ExecuteAffrows();

        }
        [FreeSql.DataAnnotations.Table(Name = "ClickHouseTest")]
        public class ClickHouse
        {
            public long Id { get; set; }

            public string Name { get; set; }
        }

        public class TestJsonb01Cls1
        {
            public Guid id { get; set; }
            [Column(MapType = typeof(JArray))]
            public List<int> jsonb01 { get; set; }
            [Column(MapType = typeof(JToken))]
            public List<long> jsonb02 { get; set; }
            [Column(MapType = typeof(JToken))]
            public List<string> jsonb03 { get; set; }
        }

        [Fact]
        public void DebugUpdateSet01()
        {
            var fsql = g.mysql;

            var report = new
            {
                NotTaxCostPrice = 47.844297M,
                ProductId = Guid.Empty,
                MerchantId = Guid.Empty,
            };
            var sql = fsql.Update<ProductStockBak>()
                .NoneParameter()
                .Set(a => a.NotTaxTotalCostPrice == report.NotTaxCostPrice * a.CurrentQty)
                .Set(a => a.NotTaxCostPrice, report.NotTaxCostPrice)
                .Where(x => x.ProductId == report.ProductId && x.MerchantId == report.MerchantId)
                .ToSql();
            Assert.Equal(@"UPDATE `ProductStockBak` SET `NotTaxTotalCostPrice` = 47.844297 * `CurrentQty`, `NotTaxCostPrice` = 47.844297 
WHERE (`ProductId` = '00000000-0000-0000-0000-000000000000' AND `MerchantId` = '00000000-0000-0000-0000-000000000000')", sql);


            //fsql.Aop.CommandBefore += (_, e) =>
            //{
            //    foreach (MySqlParameter cp in e.Command.Parameters)
            //        if (cp.MySqlDbType == MySqlDbType.Enum) cp.MySqlDbType = MySqlDbType.Int32;
            //};

            var aaa = fsql.Ado.QuerySingle<string>("select ?et", new Dictionary<string, object>
            {
                ["et"] = SystemUserType.StroeAdmin
            });

            using (var conn = fsql.Ado.MasterPool.Get())
            {
                var cmd = conn.Value.CreateCommand();
                cmd.CommandText = "select ?et";
                cmd.Parameters.Add(new MySqlParameter("et", SystemUserType.StroeAdmin));
                var aaa2 = cmd.ExecuteScalar();
            }
        }

        public enum SystemUserType
        {
            /// <summary>
            /// 未知的权限
            /// </summary>
            Unknow = 0,
            /// <summary>
            /// 超级管理员
            /// </summary>
            SuperAdmin = 1,
            /// <summary>
            /// 机构管理员
            /// </summary>
            TenantAdmin = 2,
            /// <summary>
            /// 门店管理员
            /// </summary>
            StroeAdmin = 3
        }
        public partial class ProductStockBak
        {
            [Column(IsPrimary = true)]
            public Guid ProductStockBakId { get; set; }

            public DateTime BakTime { get; set; }
            public Guid GoodsId { get; set; }
            public Guid ProductId { get; set; }
            public Guid MerchantId { get; set; }
            public string ProductCode { get; set; }
            public string Barcode { get; set; }
            public long CurrentQty { get; set; }
            public long UsableQty { get; set; }
            public long OrderQty { get; set; }
            public long LockQty { get; set; }
            public decimal CostPrice { get; set; }
            public decimal TotalCostPrice { get; set; }
            public decimal NotTaxCostPrice { get; set; }
            public decimal NotTaxTotalCostPrice { get; set; }
            public DateTime? CreationTime { get; set; }
            public DateTime? LastModificationTime { get; set; }
        }

        [Fact]
        public void TestDistinctCount()
        {
            var fsql = g.sqlite;

            var sql = fsql.Select<ts_up_dywhere01>().ToSql(a => SqlExt.DistinctCount(a.status));
            fsql.Select<ts_up_dywhere01>().Aggregate(a => SqlExt.DistinctCount(a.Key.status), out var count);

            Assert.Equal(@"SELECT count(distinct a.""status"") as1 
FROM ""ts_up_dywhere01"" a", sql);

            sql = fsql.Select<ts_up_dywhere01>().Select(a => new { a.status }).Distinct().ToSql();
            fsql.Select<ts_up_dywhere01>().Select(a => new { a.status }).Distinct().Count(out count);

            Assert.Equal(@"SELECT DISTINCT a.""status"" as1 
FROM ""ts_up_dywhere01"" a", sql);
        }

        [Fact]
        public void TestUpdateDyWhere()
        {
            var fsql = g.sqlite;

            var sql = fsql.Update<ts_up_dywhere01>(new { status = "xxx" })
                .Set(a => a.status, "yyy")
                .ToSql();

            Assert.Equal(@"UPDATE ""ts_up_dywhere01"" SET ""status"" = @p_0 
WHERE (""status"" = 'xxx')", sql);
        }
        class ts_up_dywhere01
        {
            public Guid id { get; set; }
            public string status { get; set; }
        }
    }
}
