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
        /// 初始化分页信息
        /// </summary>
        public BasePagingInfo() { }
        /// <summary>
        /// 初始化分页信息
        /// </summary>
        /// <param name="pageNumber">第几页，从1开始</param>
        /// <param name="pageSize">每页多少</param>
        public BasePagingInfo(int pageNumber, int pageSize = 20)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
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
        /// <summary>
        /// 查询后的页大小
        /// </summary>
        public int PageCount { get => Math.Max(1, (int)Math.Ceiling(Count / (double)PageSize)); }
    }
}
