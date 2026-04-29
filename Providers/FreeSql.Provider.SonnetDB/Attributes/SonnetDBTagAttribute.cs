using System;
using FreeSql.DataAnnotations;

namespace FreeSql.Provider.SonnetDB.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SonnetDBTagAttribute : ColumnAttribute
    {
    }
}
