using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.DataAnnotations
{
    /// <summary>
    /// TDengine 超级表-子表
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TDengineSubTableAttribute : TableAttribute
    {
        /// <summary>
        /// 超表名称
        /// </summary>
        public string SuperTableName { get; set; }
    }
}