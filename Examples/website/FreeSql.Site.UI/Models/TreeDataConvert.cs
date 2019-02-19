using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FreeSql.Site.Entity;

namespace FreeSql.Site.UI.Models
{
    public class TreeData
    {
        public TreeData() { }

        public TreeData(DocumentType type)
        {
            this.id = type.ID;
            this.text = type.TypeName;
        }

        public TreeData(DocumentType type, List<DocumentType> list)
        {
            this.id = type.ID;
            this.text = type.TypeName;
            this.children = (from l in list where l.UpID == type.ID select new TreeData(l, list)).ToList();
        }

        /// <summary>
        /// 唯一编号
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string text { get; set; }

        /// <summary>
        /// 类型 =0 表示类型 =1 表示内容
        /// </summary>
        public int datatype { get; set; } = 0;

        public List<TreeData> children { get; set; }

        public TreeData AddChildrens(List<TreeData> list, Func<int, List<TreeData>> bind = null)
        {
            if (this.children != null && bind != null)
            {
                this.children.ForEach(f =>
                {
                    f.children = bind(f.id);
                });
            }
            this.children?.AddRange(list);
            return this;
        }
    }
}
