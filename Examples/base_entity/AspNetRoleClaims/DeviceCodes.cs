using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
#nullable disable

namespace Densen.Models.ids
{
    /// <summary>
    /// 设备代码
    /// </summary>
    [JsonObject(MemberSerialization.OptIn), Table(DisableSyncStructure = true)]
    public partial class DeviceCodes
    {

        [Display(Name = "用户代码")]
        [JsonProperty, Column(StringLength = -2, IsPrimary = true, IsNullable = false)]
        public string UserCode { get; set; }

        [Display(Name = "设备代码")]
        [JsonProperty, Column(StringLength = -2, IsNullable = false)]
        public string DeviceCode { get; set; }

        [Display(Name = "主题编号")]
        [JsonProperty, Column(StringLength = -2)]
        public string SubjectId { get; set; }

        [Display(Name = "会话编号")]
        [JsonProperty, Column(StringLength = -2)]
        public string SessionId { get; set; }

        [Display(Name = "客户编号")]
        [JsonProperty, Column(StringLength = -2, IsNullable = false)]
        public string ClientId { get; set; }

        [Display(Name = "描述")]
        [JsonProperty, Column(StringLength = -2)]
        public string Description { get; set; }

        [Display(Name = "创建时间")]
        [JsonProperty, Column(StringLength = -2, IsNullable = false)]
        public string CreationTime { get; set; }

        [Display(Name = "到期")]
        [JsonProperty, Column(StringLength = -2, IsNullable = false)]
        public string Expiration { get; set; }

        [DisplayName("数据")]
        [JsonProperty, Column(StringLength = -2, IsNullable = false)]
        public string Data { get; set; }

    }
}