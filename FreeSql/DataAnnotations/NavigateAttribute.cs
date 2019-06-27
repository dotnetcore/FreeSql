using System;
using System.Linq;

namespace FreeSql.DataAnnotations
{
    public class NavigateAttribute : Attribute
    {

        /// <summary>
        /// 导航属性，手工绑定
        /// </summary>
        public string Bind { get; set; }

        public NavigateAttribute(string bind)
        {
            this.Bind = bind;
        }
    }
}
