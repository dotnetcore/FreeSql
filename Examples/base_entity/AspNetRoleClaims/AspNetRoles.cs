using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
#nullable disable

namespace Densen.Models.ids
{

    /// <summary>
    /// 角色定义
    /// </summary>
    [JsonObject(MemberSerialization.OptIn), Table(DisableSyncStructure = true)]
    public partial class AspNetRoles
    {

        [DisplayName("ID")]
        [JsonProperty, Column(StringLength = -2, IsPrimary = true, IsNullable = false)]
        public string Id { get; set; }

        [DisplayName("角色")]
        [JsonProperty, Column(StringLength = -2)]
        public string Name { get; set; }

        [DisplayName("标准化名称")]
        [JsonProperty, Column(StringLength = -2)]
        public string NormalizedName { get; set; }

        [DisplayName("并发票据")]
        [JsonProperty, Column(StringLength = -2)]
        public string ConcurrencyStamp { get; set; }

        //导航属性
        [Navigate(nameof(AspNetUserRoles.RoleId))]
        [DisplayName("角色表")]
        public virtual List<AspNetUserRoles> AspNetUserRoless { get; set; }

    }
}