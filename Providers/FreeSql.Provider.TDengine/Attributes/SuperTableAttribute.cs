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
    public class TDengineSuperTableAttribute : TableAttribute
    {
       
    }
}