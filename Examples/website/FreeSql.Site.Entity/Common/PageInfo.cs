using System;
using System.Collections.Generic;
using System.Text;

namespace FreeSql.Site.Entity.Common
{
    /// <summary>
    /// 列表数据返回对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PageInfo
    {
        /// <summary>
        /// 排序字段
        /// </summary>
        public string Order { get; set; }

        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 页记录数
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 排序方式
        /// </summary>
        public string Sort { get; set; }

        /// <summary>
        /// 总计数量
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// 是否获取总数
        /// </summary>
        public bool IsPaging { get; set; }
    }
}
