
namespace FreeSql
{
    public class DbContextOptions
    {

        /// <summary>
        /// 是否开启一对多，联级保存功能
        /// </summary>
        public bool EnableAddOrUpdateNavigateList { get; set; } = true;
    }
}
