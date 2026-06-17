// SonnetDBTagAttribute.cs
// 标记属性映射为 SonnetDB TAG 列（字符串维度列，建索引）。
//
// SonnetDB 数据模型中，TAG 存储字符串维度信息（如设备 ID、传感器名称、地理位置标签等），
// 会建立倒排索引，支持高效的等值过滤和分组聚合。
// TAG 列值在写入后不可更新（不可变），是 series 键的组成部分。
//
// 使用场景：
//   - 对于非 string 类型（int/float/bool 等），FreeSql 默认映射为 FIELD；
//     若需强制映射为 TAG（如枚举编码、状态标签），请标记此 Attribute。
//   - 对于 string/char/Guid 类型，FreeSql 已默认映射为 TAG，无需额外标记。
//
// 示例：
//   [SonnetDBTag]
//   public int RegionCode { get; set; }  // 强制为 TAG，参与 series 分区
//
// 注意：[SonnetDBTag] 优先级高于 [SonnetDBField]；两者同时标记时以 TAG 为准。

using System;
using FreeSql.DataAnnotations;

namespace FreeSql.Provider.SonnetDB.Attributes
{
    /// <summary>
    /// 强制将属性映射为 SonnetDB <c>TAG</c>（维度列，建索引，参与 series 分区）。
    /// 优先级高于 <see cref="SonnetDBFieldAttribute"/>；
    /// 对于默认会映射为 FIELD 的数值类型，可通过此标记覆盖为 TAG。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SonnetDBTagAttribute : ColumnAttribute
    {
    }
}
