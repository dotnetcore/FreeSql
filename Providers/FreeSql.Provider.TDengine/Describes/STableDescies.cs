using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.TDengine.Describes
{
    internal struct STableDescribe
    {
        /// <summary>
        /// 是否是超表
        /// </summary>
        public bool IsSTable { get; set; } 

        /// <summary>
        /// 超表名称
        /// </summary>
        public string STableName { get; set; }

    }
}