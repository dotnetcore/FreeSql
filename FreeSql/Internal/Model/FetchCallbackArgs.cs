using System;
using System.Collections.Generic;
using System.Text;

namespace FreeSql.Internal.Model
{
    public class FetchCallbackArgs<T>
    {
        public T Object { get; set; }

        /// <summary>
        /// 是否放弃继续读取
        /// </summary>
        public bool IsBreak { get; set; }
    }
}
