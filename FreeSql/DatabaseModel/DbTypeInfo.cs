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
        public List<LabelInfo> Labels { get; set; }

        public class LabelInfo
        {
            public string label { get; }
            public string value { get; }

            public LabelInfo(string label, string value)
            {
                this.label = label;
                this.value = value;
            }
        }
    }
}
