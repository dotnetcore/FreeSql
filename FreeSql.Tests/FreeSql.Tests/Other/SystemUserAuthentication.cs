using System;
using System.Collections.Generic;
using FreeSql.DataAnnotations;
using Zeus.Domain.Enum;
using Zeus.Utility.Entity;

namespace Zeus
{
    /// <summary>
    /// 用户认证表
    /// </summary>
    [Table(Name = "system_user_authentication")]
    [Index("UK_Identifier", "Identifier", true)]
    public partial class SystemUserAuthentication : EntityBase<long>
    {
        /// <summary>
        /// 用户表ID
        /// </summary>
        [Column(DbType = "bigint(20)", IsNullable = false)]
        public long SystemUserID { get; set; }
        /// <summary>
        /// 用户
        /// </summary>
        [Navigate("SystemUserID")]
        public SystemUser SystemUser { get; set; }
        /// <summary>
        /// 登录类型
        /// </summary>
        [Column(DbType = "varchar(10)", MapType = typeof(string), IsNullable = false)]
        public IdentityType IdentityType { get; set; }
        /// <summary>
        /// 登录标识
        /// </summary>
        [Column(DbType = "varchar(50)", IsNullable = false)]
        public string Identifier { get; set; }
        /// <summary>
        /// 登录凭证
        /// </summary>
        [Column(DbType = "varchar(50)", IsNullable = false)]
        public string Credential { get; set; }
        /// <summary>
        /// 验证标志
        /// </summary>
        [Column(DbType = "bit(1)", IsNullable = false)]
        public bool IsVerified { get; set; }
    }
}
