using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System.ComponentModel;
#nullable disable

namespace Densen.Models.ids
{

    /// <summary>
    /// 角色声明
    /// </summary>
    [JsonObject(MemberSerialization.OptIn), Table(DisableSyncStructure = true)]
    public partial class AspNetRoleClaims
    {

        [DisplayName("ID")]
        [JsonProperty, Column(IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        [DisplayName("角色ID")]
        [JsonProperty, Column(StringLength = -2, IsNullable = false)]
        public string RoleId { get; set; }

        [DisplayName("角色声明")]
        [JsonProperty, Column(StringLength = -2)]
        public string ClaimType { get; set; }

        [DisplayName("值")]
        [JsonProperty, Column(StringLength = -2)]
        public string ClaimValue { get; set; }

        [Navigate(nameof(RoleId))]
        public virtual AspNetRoles AspNetRoles { get; set; }

    }
}