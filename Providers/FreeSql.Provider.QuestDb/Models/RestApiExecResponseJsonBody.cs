using System;
using Newtonsoft.Json;

namespace FreeSql.Provider.QuestDb.Models
{
    internal class RestApiExecResponseJsonBody
    {
        [JsonProperty("ddl")]
        public string Ddl { get; set; }

        [JsonProperty("dml")]
        public string Dml { get; set; }

        [JsonProperty("updated")]
        public int Updated { get; set; }

        [JsonIgnore]
        public bool IsSuccessful => string.Equals(Ddl, "ok", StringComparison.CurrentCultureIgnoreCase) ||
                                    string.Equals(Dml, "ok", StringComparison.CurrentCultureIgnoreCase);
    }
}