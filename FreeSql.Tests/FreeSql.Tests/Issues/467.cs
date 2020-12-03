using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Xunit;

namespace FreeSql.Tests.Issues
{
    public class _467
    {
        [Fact]
        public void SelectTest()
        {
            using (IFreeSql fsql = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.PostgreSQL, "Host=192.168.164.10;Port=5432;Username=postgres;Password=123456;Database=tedb;Pooling=true;Maximum Pool Size=1")
                //.UseNameConvert(FreeSql.Internal.NameConvertType.ToUpper)
                .UseGenerateCommandParameterWithLambda(true)
                .UseAutoSyncStructure(true)
                .UseMonitorCommand(cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText))
                .Build())
            {
                var orderSql1 = fsql
                    .Select<PayOrder>()
                    .As(nameof(PayOrder).ToLower())
                    .Where(p => p.Status == 1)
                    .ToSql(p => new
                    {
                        p.PayOrderId,
                        p.Money,
                        p.OrderTime
                    }, FreeSql.FieldAliasOptions.AsProperty);

                Assert.Equal(@"SELECT payorder.""PayOrderId"", payorder.""Money"", payorder.""OrderTime"" 
FROM ""pay_order"" payorder 
WHERE (payorder.""Status"" = 1)", orderSql1);
                
                var orderSql2 = fsql
                    .Select<PayOrder>()
                    .As(nameof(PayOrder).ToLower())
                    .Where(p => p.Status == 1)
                    .ToSql(p => new
                    {
                        p.PayOrderId,
                        p.Money,
                        NewOrderTime = p.OrderTime
                    }, FreeSql.FieldAliasOptions.AsProperty);

                Assert.Equal(@"SELECT payorder.""PayOrderId"", payorder.""Money"", payorder.""OrderTime"" ""NewOrderTime"" 
FROM ""pay_order"" payorder 
WHERE (payorder.""Status"" = 1)", orderSql2);
            }
        }

        [JsonObject(MemberSerialization.OptIn), Table(Name = "pay_order", DisableSyncStructure = true)]
        public partial class PayOrder
        {
            /// <summary>
            /// 收款金额
            /// </summary>
            [JsonProperty, Column(DbType = "money")]
            public decimal Money { get; set; }

            /// <summary>
            /// 订单时间
            /// </summary>
            [JsonProperty, Column(DbType = "timestamptz")]
            public DateTime? OrderTime { get; set; }

            /// <summary>
            /// 支付Id
            /// </summary>
            [JsonProperty, Column(StringLength = 50)]
            public string PayOrderId { get; set; }

            public int Status { get; set; }
        }
    }
}
