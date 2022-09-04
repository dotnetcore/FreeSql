using System;
using System.Linq;

namespace FreeSql.DataAnnotations
{

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class AggregateRootBoundaryAttribute : Attribute
    {
        public string Name { get; set; }
        /// <summary>
        /// 边界是否终止
        /// </summary>
        public bool Break { get; set; }
        /// <summary>
        /// 边界是否终止向下探测
        /// </summary>
        public bool BreakThen { get; set; }

        public AggregateRootBoundaryAttribute(string name)
        {
            this.Name = name;
        }
        public AggregateRootBoundaryAttribute()
        {
        }
    }
}
