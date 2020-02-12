using FreeSql.DataAnnotations;
using System;

namespace SaleIDO.Entity.Storeage
{
    public class BaseEntity
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }
    }

    /// <summary>
    /// 调价单
    /// </summary>
    [Table(Name = "jxc_AdjustPriceOrder")]
    public class AdjustPriceOrder : BaseEntity
    {
        public AdjustPriceOrder()
        {
            OrderStatus = 0;
        }

        public string OrderSn { get; set; }

        public DateTime AdjustTime { get; set; }

        public string Handler { get; set; }

        public string StoreCode { get; set; }

        public int GoodsNum { get; set; }

        public string Remark { get; set; }

        public DateTime? CheckTime { get; set; }

        public string CheckMan { get; set; }

        public string CheckRemark { get; set; }

        public int OrderStatus { get; set; }

    }

    /// <summary>
    /// 调价单产品明细
    /// </summary>
    [Table(Name = "jxc_AdjustPriceDetail")]
    public class AdjustPriceDetail : BaseEntity
    {
        public string OrderSn { get; set; }

        public string Barcode { get; set; }

        public string GoodsName { get; set; }

        public decimal GoodsWeight { get; set; }

        public decimal CostPrice { get; set; }

        public decimal MarketCostPrice { get; set; }

        /// <summary>
        /// 原标签价
        /// </summary>
        public decimal OldLabelPrice { get; set; }

        /// <summary>
        /// 新的市场成本价
        /// </summary>
        public decimal NewMarketCostPrice { get; set; }

        /// <summary>
        /// 新的标签价倍率
        /// </summary>
        public decimal NewLabelPriceRate { get; set; }

        /// <summary>
        /// 新标签价
        /// </summary>
        public decimal NewLabelPrice { get; set; }

        public string Remark { get; set; }

    }
}
