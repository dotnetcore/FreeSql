using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
#nullable disable

namespace Densen.Models.ids
{

    /// <summary>
    /// 用户表
    /// </summary>
    [JsonObject(MemberSerialization.OptIn), Table(DisableSyncStructure = true)]
    public partial class AspNetUsers
    {

        [DisplayName("用户ID")]
        [JsonProperty, Column(StringLength = -2, IsPrimary = true, IsNullable = false)]
        public string Id { get; set; }

        [JsonProperty, Column(StringLength = -2)]
        [DisplayName("用户名")]
        public string UserName { get; set; }

        [JsonProperty, Column(IsIgnore = true)]
        [DisplayName("角色")]
        public string RoleName { get => roleName ?? (AspNetUserRoless != null ? string.Join(",", AspNetUserRoless?.Select(a => a.RoleName ?? a.RoleId).ToList()) : ""); set => roleName = value; }
        string roleName;

        [JsonProperty, Column(StringLength = -2)]
        public string Email { get; set; }

        [DisplayName("电话")]
        [JsonProperty, Column(StringLength = -2)]
        public string PhoneNumber { get; set; }

        [DisplayName("自定义名称")]
        [JsonProperty, Column(StringLength = -2)]
        public string Name { get; set; }

        [DisplayName("自定义角色")]
        [JsonProperty, Column(StringLength = -2)]
        public string UserRole { get; set; }

        [DisplayName("密码哈希")]
        [JsonProperty, Column(StringLength = -2)]
        public string PasswordHash { get; set; }

        [DisplayName("电子邮件已确认")]
        [JsonProperty]
        public int EmailConfirmed { get; set; }

        [DisplayName("电话号码已确认")]
        [JsonProperty]
        public int PhoneNumberConfirmed { get; set; }

        [DisplayName("锁定结束")]
        [JsonProperty, Column(StringLength = -2)]
        public string LockoutEnd { get; set; }

        [DisplayName("启用双因素登录")]
        [JsonProperty]
        public int TwoFactorEnabled { get; set; }

        [DisplayName("并发票据")]
        [JsonProperty, Column(StringLength = -2)]
        public string ConcurrencyStamp { get; set; }

        [DisplayName("防伪印章")]
        [JsonProperty, Column(StringLength = -2)]
        public string SecurityStamp { get; set; }

        [DisplayName("标准化电子邮件")]
        [JsonProperty, Column(StringLength = -2)]
        public string NormalizedEmail { get; set; }

        [DisplayName("标准化用户名")]
        [JsonProperty, Column(StringLength = -2)]
        public string NormalizedUserName { get; set; }

        [DisplayName("启用锁定")]
        [JsonProperty]
        public int LockoutEnabled { get; set; }

        [DisplayName("国家")]
        [JsonProperty, Column(StringLength = -2)]
        public string Country { get; set; }

        [DisplayName("省")]
        [JsonProperty, Column(StringLength = -2)]
        public string Province { get; set; }

        [DisplayName("城市")]
        [JsonProperty, Column(StringLength = -2)]
        public string City { get; set; }

        [DisplayName("县")]
        [JsonProperty, Column(StringLength = -2)]
        public string County { get; set; }

        [DisplayName("邮编")]
        [JsonProperty, Column(StringLength = -2)]
        public string Zip { get; set; }

        [DisplayName("街道")]
        [JsonProperty, Column(StringLength = -2)]
        public string Street { get; set; }

        [DisplayName("税号")]
        [JsonProperty, Column(StringLength = -2)]
        public string TaxNumber { get; set; }

        [DisplayName("提供者")]
        [JsonProperty, Column(StringLength = -2)]
        public string provider { get; set; }

        [DisplayName("UUID")]
        [JsonProperty, Column(StringLength = -2)]
        public string UUID { get; set; }

        [DisplayName("生日")]
        [JsonProperty, Column(StringLength = -2)]
        public string DOB { get; set; }

        [DisplayName("访问失败次数")]
        [JsonProperty]
        public int AccessFailedCount { get; set; }

        //导航属性
        [Navigate(nameof(AspNetUserRoles.UserId))]
        [DisplayName("角色表")]
        public virtual List<AspNetUserRoles> AspNetUserRoless { get; set; }

        [Navigate(nameof(AspNetUserClaims.UserId))]
        [DisplayName("用户声明")]
        public virtual List<AspNetUserClaims> AspNetUserClaimss { get; set; }

        [Navigate(nameof(AspNetUserLogins.UserId))]
        [DisplayName("用户登录")]
        public virtual List<AspNetUserLogins> AspNetUserLoginss { get; set; }

        [JsonProperty, Column(IsIgnore = true)]
        [DisplayName("1st角色")]
        public string RoleName1st { get => roleName1st ?? ((AspNetUserRoless != null && AspNetUserRoless.Any()) ? AspNetUserRoless?.Select(a => a.RoleName ?? a.RoleId ?? "").First() : ""); set => roleName1st = value; }
        string roleName1st;

    }
}