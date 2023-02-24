using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System.ComponentModel;
#nullable disable

namespace Densen.Models.ids
{

    /// <summary>
    /// 用户登录
    /// </summary>
    [JsonObject(MemberSerialization.OptIn), Table(DisableSyncStructure = true)]
    public partial class AspNetUserLogins
    {

        [DisplayName("外联登录")]
        [JsonProperty, Column(StringLength = -2, IsPrimary = true, IsNullable = false)]
        public string LoginProvider { get; set; }

        [DisplayName("用户ID")]
        [JsonProperty, Column(StringLength = -2, IsNullable = false)]
        public string UserId { get; set; }

        [DisplayName("外联Key")]
        [JsonProperty, Column(StringLength = -2, IsNullable = false)]
        public string ProviderKey { get; set; }

        [DisplayName("外联名称")]
        [JsonProperty, Column(StringLength = -2)]
        public string ProviderDisplayName { get; set; }

        /// <summary>
        /// 用户
        /// </summary>
        [Navigate(nameof(UserId))]
        public virtual AspNetUsers AspNetUsers { get; set; }

    }
}