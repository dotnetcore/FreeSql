using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace FreeSql.Provider.QuestDb.Models
{
    internal class QuestResetApiFeatures
    {
        internal string BaseAddress { get; set; }

        internal string BasicToken { get; set; }

        internal HttpClient HttpClient => ServiceContainer.GetService<IHttpClientFactory>().CreateClient("QuestDb");

        internal async Task<string> ExecAsync(string sql)
        {
            //HTTP GET 执行SQL
            var url = $"exec?query={HttpUtility.UrlEncode(sql)}";
            if (!string.IsNullOrWhiteSpace(BasicToken))
                HttpClient.DefaultRequestHeaders.Add("Authorization", BasicToken);
            var httpResponseMessage = await HttpClient.GetAsync(url);
            var result = await httpResponseMessage.Content.ReadAsStringAsync();
            return result;
        }
    }
}