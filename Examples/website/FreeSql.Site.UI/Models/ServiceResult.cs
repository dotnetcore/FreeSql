using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FreeSql.Site.UI
{

    /// <summary>
    /// ServiceResult操作结果
    /// </summary>
    public enum EnumServiceResult
    {
        /// <summary>
        /// 操作成功
        /// </summary>
        Success = 1,
        /// <summary>
        /// 操作失败
        /// </summary>
        Failure = 0
    }

    /// <summary>
    /// 方法错误结果
    /// </summary>
    [Serializable]
    public class ServiceResult
    {
        /// <summary>
        ///  返回结果 =1 表示成功 否则失败
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 返回结果信息
        /// </summary>
        public string Msg { get; set; }
    }

    [Serializable]
    public class ServiceResult<T> : ServiceResult
    {
        /// <summary>
        /// 数据返回
        /// </summary>
        public T Data { get; set; }
    }
}
