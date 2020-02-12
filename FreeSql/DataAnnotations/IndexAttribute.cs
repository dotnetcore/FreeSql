using System;
using System.Collections.Generic;
using System.Text;

namespace FreeSql.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class IndexAttribute : Attribute
    {
        public IndexAttribute(string name, string fields)
        {
            this.Name = name;
            this.Fields = fields;
        }
        public IndexAttribute(string name, string fields, bool isUnique)
        {
            this.Name = name;
            this.Fields = fields;
            this.IsUnique = isUnique;
        }
        /// <summary>
        /// 索引名
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
