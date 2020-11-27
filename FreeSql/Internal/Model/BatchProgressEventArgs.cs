using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace FreeSql.Internal.Model
{
    public class BatchProgressStatus<T1>
    {
        /// <summary>
        /// 当前操作的数据
        /// </summary>
        public IEnumerable<T1> Data { get; }

        /// <summary>
        /// 当前批次
        /// </summary>
        public int Current { get; }

        /// <summary>
        /// 总批次数量
        /// </summary>
        public int Total { get; }

        public BatchProgressStatus(List<T1> data, int current, int total)
        {
            this.Data = data;
            this.Current = current;
            this.Total = total;
        }
    }
}
