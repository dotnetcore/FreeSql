using System;
using System.Linq;

namespace FreeSql.DataAnnotations
{

    /// <summary>
    /// OneToOne：[Navigate(nameof(Primary))] &lt;-&gt; (缺省)外表.Primary<para></para>
    /// ManyToOne：Topic.cs 文件 [Navigate(nameof(Topic.CategoryId))] &lt;-&gt; (缺省)Category.Id<para></para>
    /// _________________public Category Category { get; set; }<para></para>
    /// OneToMany：Category.cs 文件 (缺省)Category.Id &lt;-&gt; [Navigate(nameof(Topic.CategoryId))]<para></para>
    /// _________________public List&lt;Topic&gt; Topics { get; set; }<para></para>
    /// </summary>
    public class NavigateAttribute : Attribute
    {

        /// <summary>
        /// OneToOne：[Navigate(nameof(Primary))] &lt;-&gt; (缺省)外表.Primary<para></para>
        /// ManyToOne：Topic.cs 文件 [Navigate(nameof(Topic.CategoryId))] &lt;-&gt; (缺省)Category.Id<para></para>
        /// _________________public Category Category { get; set; }<para></para>
        /// OneToMany：Category.cs 文件 (缺省)Category.Id &lt;-&gt; [Navigate(nameof(Topic.CategoryId))]<para></para>
        /// _________________public List&lt;Topic&gt; Topics { get; set; }<para></para>
        /// </summary>
        public string Bind { get; set; }

        /// <summary>
        /// 与非主键进行关联，仅支持 OneToMany、ManyToOne<para></para>
        /// 使用方法参考 Bind 属性
        /// </summary>
        public string TempPrimary { get; set; }

        /// <summary>
        /// 手工绑定 ManyToMany 导航关系
        /// </summary>
        public Type ManyToMany { get; set; }

        /// <summary>
        /// OneToOne：[Navigate(nameof(Primary))] &lt;-&gt; (缺省)外表.Primary<para></para>
        /// ManyToOne：Topic.cs 文件 [Navigate(nameof(Topic.CategoryId))] &lt;-&gt; (缺省)Category.Id<para></para>
        /// _________________public Category Category { get; set; }<para></para>
        /// OneToMany：Category.cs 文件 (缺省)Category.Id &lt;-&gt; [Navigate(nameof(Topic.CategoryId))]<para></para>
        /// _________________public List&lt;Topic&gt; Topics { get; set; }<para></para>
        /// </summary>
        public NavigateAttribute(string bind)
        {
            this.Bind = bind;
        }
        public NavigateAttribute()
        {
        }
    }
}
