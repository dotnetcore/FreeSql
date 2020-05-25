using System;

namespace FreeSql.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Class)]
    public class OraclePrimaryKeyNameAttribute : Attribute
    {
        public OraclePrimaryKeyNameAttribute(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// 主键名
        /// </summary>
        public string Name { get; set; }
    }
}
