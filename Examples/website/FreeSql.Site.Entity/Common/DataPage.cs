using System;
using System.Collections.Generic;
using System.Text;

namespace FreeSql.Site.Entity.Common
{
    /// <summary>
    /// 列表数据返回对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataPage<T>
        where T : class
    {
        /// <summary>
        /// 返回成功与否
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// 如果返回报错，具体报错内容
        /// </summary>
        public string msg { get; set; }

        /// <summary>
        /// 总计记录行数
        /// </summary>
        public long count { get; set; }

        /// <summary>
        /// 返回具体的数据
        /// </summary>
        public List<T> data { get; set; }
    }
}
