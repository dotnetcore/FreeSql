using System;
using System.Collections.Generic;
using System.Text;

namespace FreeSql.Internal.Model
{
    /// <summary>
    /// 分页信息
    /// </summary>
    public class BasePagingInfo
    {
        /// <summary>
        /// 第几页，从1开始
        /// </summary>
        public int PageNumber { get; set; }
        /// <summary>
        /// 每页多少
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// 查询的记录数量
        /// </summary>
        public long Count { get; set; }
    }
}