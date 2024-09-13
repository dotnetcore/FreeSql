using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.DataAnnotations;

namespace FreeSql.Provider.TDengine.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class TDengineTagAttribute : ColumnAttribute
    {
    }
}