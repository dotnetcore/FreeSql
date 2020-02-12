using System.Collections.Generic;

namespace FreeSql.DatabaseModel
{
    public class DbTypeInfo
    {

        /// <summary>
        /// 类型标识
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 枚举项
        /// </summary>
        public List<(string label, string value)> Labels { get; set; }
    }
}
