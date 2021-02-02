using AME.Helpers;
using FreeSql.DataAnnotations;
using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
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
