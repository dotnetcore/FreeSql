using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace FreeSql.Tests.ClickHouse
{
    /// <summary>
    /// 实时数据
    /// </summary>
    [Index("idx_{tablename}_01", nameof(Guid), true)]
    [Index("idx_{tablename}_02", nameof(TenantId), true)]
    //复合索引
    [Index("idx_{tablename}_03", $"{nameof(CreatedUserId)},{nameof(Version)}", false)]
    [Table(OldName = "CollectDataEntityUpdate")]
    public partial class CollectDataEntityUpdate01
    {
        /// <summary>
        /// Guid
        /// </summary>
        [Column(StringLength = 50)]
        public string Guid
        {
            get; set;
        }

        /// <summary>
        /// 租户Id
        /// </summary>
        [Description("租户Id")]
        [Column(CanUpdate = false)]
        public virtual long? TenantId
        {
            get; set;
        }

        /// <summary>
        /// 版本
        /// </summary>
        [Description("版本")]
        [Column(IsVersion = false)]
        public long Version
        {
            get; set;
        }

        /// <summary>
        /// 是否删除
        /// </summary>
        [Description("是否删除")]
        [Column()]
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// 创建者Id
        /// </summary>
        [Description("创建者Id")]
        [Column(CanUpdate = false)]
        public long? CreatedUserId
        {
            get; set;
        }

        /// <summary>
        /// 创建者
        /// </summary>
        [Description("创建者")]
        [Column(CanUpdate = false, StringLength = 50, OldName = "CreatedUserNameUpdate")]
        public string CreatedUserNameUpdate01
        {
            get; set;
        }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Description("创建时间")]
        [Column(CanUpdate = false, ServerTime = DateTimeKind.Local)]
        public DateTime? CreatedTime
        {
            get; set;
        }

        /// <summary>
        /// 修改者Id
        /// </summary>
        [Description("修改者Id")]
        [Column(CanInsert = false)]
        public long? ModifiedUserId
        {
            get; set;
        }

        /// <summary>
        /// 修改者
        /// </summary>
        [Description("修改者")]
        [Column(CanInsert = false, StringLength = 50)]
        public string ModifiedUserName
        {
            get; set;
        }

        /// <summary>
        /// 修改时间
        /// </summary>
        [Description("修改时间")]
        [Column(CanInsert = false, ServerTime = DateTimeKind.Local)]
        public DateTime? ModifiedTime
        {
            get; set;
        }
        /// <summary>
        /// 数据标识
        /// </summary>
        [Description("数据标识")]
        [Column(CanInsert = false, StringLength = 2)]
        public string DataFlag
        {
            get; set;
        }
        /// <summary>
        /// 主键Id
        /// </summary>
        [Description("主键Id")]
        [Column(Position = 1, IsPrimary = true)]
        public long Id
        {
            get; set;
        }
        /// <summary>
        /// 设备编号
        /// </summary>
        [Column(StringLength = 50)]
        public string EquipmentCode
        {
            get; set;
        }

        /// <summary>
        /// 数据编号，如为空使用默认数据
        /// </summary>
        [Column(StringLength = 50)]
        public string PropertyCode
        {
            get; set;
        }
        /// <summary>
        ///  数据名称，如为空使用默认数据
        /// </summary>
        [Column(StringLength = 50)]
        public string PropertyName
        {
            get; set;
        }

        /// <summary>
        /// 数值或状态是否变更
        /// </summary>
        public bool IsValueOrStateChanged
        {
            get; set;
        }

        /// <summary>
        /// 采集数值
        /// </summary>
        [Column(StringLength = 18)]
        public decimal? NumericValue
        {
            get; set;
        }

        /// <summary>
        /// 备注
        /// </summary>
        [Column(StringLength = 200)]
        public string Remark
        {
            get; set;
        }

        /// <summary>
        /// 服务标记
        /// </summary>
        [Column(StringLength = 20)]
        public string ServiceFlag
        {
            get; set;
        }

        /// <summary>
        /// 状态
        /// </summary>
        [Column(StringLength = 50)]
        public string StrState
        {
            get; set;
        }

        /// <summary>
        /// 文本数值
        /// </summary>
        [Column(StringLength = 50)]
        public string StrValue
        {
            get; set;
        }

        /// <summary>
        /// 单位
        /// </summary>
        [Column(StringLength = 10)]
        public string UnitStr
        {
            get; set;
        }

        /// <summary>
        /// 采集时间
        /// </summary>
        public DateTime CollectTime
        {
            get; set;
        }


        public string FieldKey
        {
            get
            {
                return EquipmentCode + "_" + PropertyCode;
            }
        }
    }

}