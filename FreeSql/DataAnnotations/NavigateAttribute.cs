using System;
using System.Linq;

namespace FreeSql.DataAnnotations
{
    public class NavigateAttribute : Attribute
    {

        /// <summary>
        /// 手工绑定 OneToMany、ManyToOne 导航关系
        /// </summary>
        public string Bind { get; set; }
        /// <summary>
        /// 手工绑定 ManyToMany 导航关系
        /// </summary>
        public Type ManyToMany { get; set; }

        public NavigateAttribute(string bind)
        {
            this.Bind = bind;
        }
        public NavigateAttribute()
        {
        }
    }
}
