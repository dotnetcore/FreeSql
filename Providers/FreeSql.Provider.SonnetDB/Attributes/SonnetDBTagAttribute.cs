// SonnetDB TAG 特性。
using System;
using FreeSql.DataAnnotations;

namespace FreeSql.Provider.SonnetDB.Attributes
{
    /// <summary>
    /// 强制将属性映射为 SonnetDB <c>TAG</c>（维度列，建索引，参与序列分区）。
    /// 优先级高于 <see cref="SonnetDBFieldAttribute"/>；
    /// 对于默认会映射为 FIELD 的数值类型，可通过此标记覆盖为 TAG。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SonnetDBTagAttribute : ColumnAttribute
    {
    }
}
