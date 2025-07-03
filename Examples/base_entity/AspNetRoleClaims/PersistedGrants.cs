using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System.ComponentModel;
#nullable disable

namespace Densen.Models.ids
{

    /// <summary>
    /// 持久化保存
    /// </summary>
    [JsonObject(MemberSerialization.OptIn), Table(DisableSyncStructure = true)]
    public partial class PersistedGrants
    {

        [DisplayName("键值")]
        [JsonProperty, Column(StringLength = -2, IsPrimary = true, IsNullable = false)]
        public string Key { get; set; }

        [DisplayName("类型")]
        [JsonProperty, Column(StringLength = -2, IsNullable = false)]
        public string Type { get; set; }

        [DisplayName("主题编号")]
        [JsonProperty, Column(StringLength = -2)]
        public string SubjectId { get; set; }

        [DisplayName("会话编号")]
        [JsonProperty, Column(StringLength = -2)]
        public string SessionId { get; set; }

        [DisplayName("客户编号")]
        [JsonProperty, Column(StringLength = -2, IsNullable = false)]
        public string ClientId { get; set; }

        [DisplayName("描述")]
        [JsonProperty, Column(StringLength = -2)]
        public string Description { get; set; }

        [DisplayName("创建时间")]
        [JsonProperty, Column(StringLength = -2, IsNullable = false)]
        public string CreationTime { get; set; }

        [DisplayName("到期")]
        [JsonProperty, Column(StringLength = -2)]
        public string Expiration { get; set; }

        [DisplayName("消耗时间")]
        [JsonProperty, Column(StringLength = -2)]
        public string ConsumedTime { get; set; }

        [DisplayName("数据")]
        [JsonProperty, Column(StringLength = -2, IsNullable = false)]
        public string Data { get; set; }

    }
}