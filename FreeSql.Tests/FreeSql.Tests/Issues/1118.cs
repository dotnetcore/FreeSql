using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using Xunit;

namespace FreeSql.Tests.Issues
{
    public class _1118
    {
        [Fact]
        public void PropertyType()
        {
            var fsql = g.mysql;

            var activityPeopleCountList = fsql.Select<SalesmanActivityConfig>()
                 .Where(a => a.SalesmanId == "xxx")
                 .ToList(a => new
                 {
                     Type = a.Type,
                     Count = fsql.Select<SalesmanActivityOrder>()
                         .Where(o => o.SalesmanId == a.SalesmanId)
                         .Where(o => a.Type == o.ActivityType)//报错ArgumentException: Requested value 'a.`Type`' was not found.
                                                              //.Where("o.ActivityType = a.Type", null)//正常
                         .Where(o => o.PayStatus == SalesmanActivityOrderPayStatusEnum.YiWanCheng)
                         .Count(),
                 });
        }

        [JsonObject(MemberSerialization.OptIn), Table(Name = "salesman_activity_config")]
        public partial class SalesmanActivityConfig
        {
            /// <summary>
            ///<para>    </para>
            /// </summary>
            [JsonProperty, Column(Name = "Id", DbType = "char(32)", IsNullable = false, IsPrimary = true)]
            public string Id { get; set; }

            /// <summary>
            ///<para>推销员Id    </para>
            /// </summary>
            [JsonProperty, Column(Name = "SalesmanId", DbType = "char(32)", IsNullable = false)]
            public string SalesmanId { get; set; }

            /// <summary>
            ///<para>洗车卡业务类型:0=自助卡 1=自动卡 2=通用卡[BaseCardBussinessType]    </para>
            /// </summary>
            [JsonProperty, Column(Name = "Type", DbType = "int", IsNullable = false)]
            public BaseCardBussinessType Type { get; set; }

            /// <summary>
            ///<para>收益    </para>
            /// </summary>
            [JsonProperty, Column(Name = "DivideAmount", DbType = "decimal(10,2)", IsNullable = false)]
            public decimal DivideAmount { get; set; }

            /// <summary>
            ///<para>    </para>
            /// </summary>
            [JsonProperty, Column(Name = "AddTime", DbType = "datetime", IsNullable = false)]
            public DateTime AddTime { get; set; }

            /// <summary>
            ///<para>    </para>
            /// </summary>
            [JsonProperty, Column(Name = "UpdateTime", DbType = "datetime", IsNullable = true)]
            public DateTime? UpdateTime { get; set; }

        }
        [JsonObject(MemberSerialization.OptIn), Table(Name = "salesman_activity_order")]
        public partial class SalesmanActivityOrder
        {
            /// <summary>
            ///<para>    </para>
            /// </summary>
            [JsonProperty, Column(Name = "Id", DbType = "char(32)", IsNullable = false, IsPrimary = true)]
            public string Id { get; set; }

            /// <summary>
            ///<para>推销员活动配置Id    </para>
            /// </summary>
            [JsonProperty, Column(Name = "SalesmanActivityConfigId", DbType = "char(32)", IsNullable = false)]
            public string SalesmanActivityConfigId { get; set; }

            /// <summary>
            ///<para>洗车卡业务类型:0=自助卡 1=自动卡 2=通用卡[BaseCardBussinessType]    </para>
            /// </summary>
            [JsonProperty, Column(Name = "ActivityType", DbType = "int", IsNullable = false)]
            public BaseCardBussinessType ActivityType { get; set; }

            /// <summary>
            ///<para>推销员Id    </para>
            /// </summary>
            [JsonProperty, Column(Name = "SalesmanId", DbType = "char(32)", IsNullable = false)]
            public string SalesmanId { get; set; }

            /// <summary>
            ///<para>用户Id    </para>
            /// </summary>
            [JsonProperty, Column(Name = "AccountId", DbType = "char(32)", IsNullable = false)]
            public string AccountId { get; set; }

            /// <summary>
            ///<para>订单编号    </para>
            /// </summary>
            [JsonProperty, Column(Name = "OrderNo", DbType = "varchar(100)", IsNullable = false)]
            public string OrderNo { get; set; }

            /// <summary>
            ///<para>支付金额    </para>
            /// </summary>
            [JsonProperty, Column(Name = "PayAmount", DbType = "decimal(10,2)", IsNullable = false)]
            public decimal PayAmount { get; set; }

            /// <summary>
            ///<para>支付方式  0、余额支付 1、微信支付 2、支付宝 99、无    </para>
            /// </summary>
            [JsonProperty, Column(Name = "PayMethod", DbType = "int", IsNullable = false)]
            public SalesmanActivityOrderPayMethodEnum PayMethod { get; set; }

            /// <summary>
            ///<para>支付状态：1、待付款，5、已完成，10、关闭，15、退款中，20、退款完成    </para>
            /// </summary>
            [JsonProperty, Column(Name = "PayStatus", DbType = "int", IsNullable = false)]
            public SalesmanActivityOrderPayStatusEnum PayStatus { get; set; }

            /// <summary>
            ///<para>支付时间    </para>
            /// </summary>
            [JsonProperty, Column(Name = "PayTime", DbType = "datetime", IsNullable = true)]
            public DateTime? PayTime { get; set; }

            /// <summary>
            ///<para>是否已激活    </para>
            /// </summary>
            [JsonProperty, Column(Name = "IsActivated", DbType = "bit(1)", IsNullable = false)]
            public bool IsActivated { get; set; }

            /// <summary>
            ///<para>激活时间    </para>
            /// </summary>
            [JsonProperty, Column(Name = "ActivatedTime", DbType = "datetime", IsNullable = true)]
            public DateTime? ActivatedTime { get; set; }

            /// <summary>
            ///<para>购买的365年卡Id    </para>
            /// </summary>
            [JsonProperty, Column(Name = "CardOrderId", DbType = "char(32)", IsNullable = true)]
            public string CardOrderId { get; set; }

            /// <summary>
            ///<para>    </para>
            /// </summary>
            [JsonProperty, Column(Name = "AddTime", DbType = "datetime", IsNullable = false)]
            public DateTime AddTime { get; set; }

            /// <summary>
            ///<para>    </para>
            /// </summary>
            [JsonProperty, Column(Name = "UpdateTime", DbType = "datetime", IsNullable = true)]
            public DateTime? UpdateTime { get; set; }
        }

        public enum SalesmanActivityOrderPayMethodEnum { Wepay, Alipay, Bank }
        public enum SalesmanActivityOrderPayStatusEnum { Pending, Compelte, YiWanCheng }
        public enum BaseCardBussinessType
        {
            /// <summary>
            /// 自助卡
            /// </summary>
            [Description("自助卡")]
            ZiZhuKa = 0,
            /// <summary>
            /// 自动卡
            /// </summary>
            [Description("自动卡")]
            ZiDongKa = 1,
            /// <summary>
            /// 通用卡
            /// </summary>
            [Description("通用卡")]
            TongYongKa = 2,
        }
    }

}
