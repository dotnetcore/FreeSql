using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.DataAnnotations
{
    /// <summary>
    /// TDengine 超级表
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SuperTableAttribute : TableAttribute
    {
        /// <summary>
        /// 超表名称
        /// </summary>
        public string STableName { get; set; }
    }
}