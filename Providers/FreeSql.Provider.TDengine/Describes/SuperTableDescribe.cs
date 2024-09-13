using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.TDengine.Describes
{
    internal class SuperTableDescribe
    {
        /// <summary>
        /// 超级表Type
        /// </summary>
        public Type SuperTableType { get; set; } 

        /// <summary>
        /// 超级表名称
        /// </summary>
        public string SuperTableName { get; set; }

    }
}