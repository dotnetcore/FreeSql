using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System.ComponentModel;
#nullable disable

namespace Densen.Models.ids
{

    /// <summary>
    /// 角色表
    /// <para>存储向哪些用户分配哪些角色</para>
    /// </summary>
    [JsonObject(MemberSerialization.OptIn), Table(DisableSyncStructure = true)]
    public partial class AspNetUserRoles
    {

        [DisplayName("用户ID")]
        [JsonProperty, Column(StringLength = -2, IsPrimary = true, IsNullable = false)]
        public string UserId { get; set; }

        [JsonProperty, Column(IsIgnore = true)]
        [DisplayName("用户")]
        public string UserName { get => roleName ?? (AspNetUserss?.UserName); set => userName = value; }
        string userName;

        [DisplayName("角色ID")]
        [JsonProperty, Column(StringLength = -2, IsNullable = false)]
        public string RoleId { get; set; }

        [JsonProperty, Column(IsIgnore = true)]
        [DisplayName("角色名称")]
        public string RoleName { get => roleName ?? (AspNetRoless?.Name); set => roleName = value; }
        string roleName;

        [DisplayName("角色定义")]
        [Navigate(nameof(RoleId))]
        public virtual AspNetRoles AspNetRoless { get; set; }

        [DisplayName("用户表")]
        [Navigate(nameof(UserId))]
        public virtual AspNetUsers AspNetUserss { get; set; }

    }

}