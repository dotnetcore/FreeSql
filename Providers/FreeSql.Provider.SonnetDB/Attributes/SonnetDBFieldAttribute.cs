// SonnetDB FIELD 特性。
using System;
using FreeSql.DataAnnotations;

namespace FreeSql.Provider.SonnetDB.Attributes
{
    /// <summary>
    /// 强制将属性映射为 SonnetDB <c>FIELD</c>（观测值列，不建索引）。
    /// 与 <see cref="SonnetDBTagAttribute"/> 互斥；同时标记时 <c>[SonnetDBTag]</c> 优先。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SonnetDBFieldAttribute : ColumnAttribute
    {
    }
}
