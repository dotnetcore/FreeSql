// SonnetDBFieldAttribute.cs
// 标记属性映射为 SonnetDB FIELD 列（数值/布尔观测列）。
//
// SonnetDB 数据模型中，FIELD 存储实际的测量观测值（如温度、压力、电流等数值），
// 不建索引，写入性能高，但按 FIELD 过滤需要全扫描。
//
// 使用场景：
//   - 当属性类型为 string/char/Guid 时，FreeSql 默认映射为 TAG；
//     若需强制映射为 FIELD，请标记此 Attribute。
//   - 对于数值类型（int/float/bool），不标记也会默认映射为 FIELD。
//
// 示例：
//   [SonnetDBField]
//   public string RawPayload { get; set; }  // 强制为 FIELD STRING，不建索引

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
