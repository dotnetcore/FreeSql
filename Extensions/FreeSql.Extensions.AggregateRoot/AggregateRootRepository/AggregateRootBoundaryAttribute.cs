using System;
using System.Linq;

namespace FreeSql.DataAnnotations
{

    /// <summary>
    /// 设置 AggregateRootRepository 边界范围<para></para>
    /// 在边界范围之内的规则 ：<para></para>
    /// 1、OneToOne/OneToMany/ManyToMany(中间表) 可以查询、可以增删改<para></para>
    /// 2、ManyToOne/ManyToMany外部表/PgArrayToMany 只可以查询，不支持增删改（会被忽略）<para></para>
    /// </summary>
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
