using System;
using System.Collections.Generic;
using FreeSql.DataAnnotations;
using Zeus.Utility.Entity;

namespace Zeus
{
    /// <summary>
    /// 用户表
    /// </summary>
    [Table(Name = "system_user")]
    [Index("UK_DisplayName", "DisplayName", true)]
    public partial class SystemUser : EntityBase<long>
    {
        /// <summary>
        /// 显示名称
        /// </summary>
        [Column(DbType = "varchar(20)", IsNullable = false)]
        public string DisplayName { get; set; }
        /// <summary>
        /// 真实名称
        /// </summary>
        [Column(DbType = "varchar(20)")]
        public string RealName { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        [Column(DbType = "varchar(2)")]
        public string Gender { get; set; }
        /// <summary>
        /// 生日
        /// </summary>
        [Column(DbType = "datetime")]
        public DateTime? Birthday { get; set; }
        /// <summary>
        /// 头像URL地址
        /// </summary>
        [Column(DbType = "varchar(200)")]
        public string AvaterURL { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [Column(DbType = "varchar(500)")]
        public string Remark { get; set; }
        /// <summary>
        /// 启用标志
        /// </summary>
        [Column(DbType = "bit(1)", IsNullable = false)]
        public bool IsEnabled { get; set; }
        /// <summary>
        /// 删除标志
        /// </summary>
        [Column(DbType = "bit(1)", IsNullable = false)]
        public bool IsDeleted { get; set; }
        /// <summary>
        /// 所属用户认证记录
        /// </summary>
        [Navigate("SystemUserID")]
        public ICollection<SystemUserAuthentication> SystemUserAuthentication_List { get; set; }
    }
}
