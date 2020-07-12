using System;
using System.Collections.Generic;
using System.Text;

namespace FreeSql.DataAnnotations
{
    /// <summary>
    /// 索引设置，如：[Index("{tablename}_idx_01", "name")]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class IndexAttribute : Attribute
    {
        /// <summary>
        /// 索引设置，如：[Index("{tablename}_idx_01", "name")]
        /// </summary>
        /// <param name="name">索引名<para></para>v1.7.0 增加占位符 {TableName} 表名区分索引名 （解决 AsTable 分表 CodeFirst 导致索引名重复的问题）</param>
        /// <param name="fields">索引字段，为属性名以逗号分隔，如：Create_time ASC, Title ASC</param>
        public IndexAttribute(string name, string fields)
        {
            this.Name = name;
            this.Fields = fields;
        }
        /// <summary>
        /// 索引设置，如：[Index("{tablename}_idx_01", "name", true)]
        /// </summary>
        /// <param name="name">索引名<para></para>v1.7.0 增加占位符 {TableName} 表名区分索引名 （解决 AsTable 分表 CodeFirst 导致索引名重复的问题）</param>
        /// <param name="fields">索引字段，为属性名以逗号分隔，如：Create_time ASC, Title ASC</param>
        /// <param name="isUnique">是否唯一</param>
        public IndexAttribute(string name, string fields, bool isUnique)
        {
            this.Name = name;
            this.Fields = fields;
            this.IsUnique = isUnique;
        }
        /// <summary>
        /// 索引名<para></para>
        /// v1.7.0 增加占位符 {TableName} 表名区分索引名 （解决 AsTable 分表 CodeFirst 导致索引名重复的问题）
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 索引字段，为属性名以逗号分隔，如：Create_time ASC, Title ASC
        /// </summary>
        public string Fields { get; set; }

        internal bool? _IsUnique;
        /// <summary>
        /// 是否唯一
        /// </summary>
        public bool IsUnique { get => _IsUnique ?? false; set => _IsUnique = value; }
    }
}
