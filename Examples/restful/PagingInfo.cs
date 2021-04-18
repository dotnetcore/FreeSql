using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace restful
{
    public class PagingInfo : BasePagingInfo
    {
        /// <summary>
        /// 无参构造函数
        /// </summary>
        public PagingInfo()
        {
        }
        /// <summary>
        /// 当前为第1页，每页大小的构造函数
        /// </summary>
        /// <param name="pageSize"></param>
        public PagingInfo(int pageSize)
        {
            PageNumber = 1;
            PageSize = pageSize;
        }
        /// <summary>
        /// 带当前页和每页大小的构造函数
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        public PagingInfo(int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
        /// <summary>
        /// 当前有多少页【只读】
        /// </summary>
        public long PageCount => PageSize == 0 ? 0 : (Count + PageSize - 1) / PageSize;
        /// <summary>
        /// 是否有上一页【只读】
        /// </summary>
        public bool HasPrevious => PageNumber > 1 && PageNumber <= PageCount;
        /// <summary>
        /// 是否有下一页【只读】
        /// </summary>
        public bool HasNext => PageNumber < PageCount;
        /// <summary>
        /// 是否在第一页【只读】
        /// </summary>
        public bool IsFrist => PageNumber == 1;
        /// <summary>
        /// 是否在最后一页【只读】
        /// </summary>
        public bool IsLast => PageNumber == PageCount;
    }
}