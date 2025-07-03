using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System.ComponentModel;
#nullable disable

namespace Densen.Models.ids
{

    /// <summary>
    /// 用户令牌
    /// </summary>
    [JsonObject(MemberSerialization.OptIn), Table(DisableSyncStructure = true)]
    public partial class AspNetUserTokens
    {

        [DisplayName("用户ID")]
        [JsonProperty, Column(StringLength = -2, IsPrimary = true, IsNullable = false)]
        public string UserId { get; set; }

        [DisplayName("名称")]
        [JsonProperty, Column(StringLength = -2, IsNullable = false)]
        public string Name { get; set; }

        [DisplayName("外部登录提供程序")]
        [JsonProperty, Column(StringLength = -2, IsNullable = false)]
        public string LoginProvider { get; set; }

        [DisplayName("值")]
        [JsonProperty, Column(StringLength = -2)]
        public string Value { get; set; }

        [Navigate(nameof(UserId))]
        public virtual AspNetUsers AspNetUsers { get; set; }

    }
}