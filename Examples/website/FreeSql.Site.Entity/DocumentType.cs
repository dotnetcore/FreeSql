//using FreeSql.DataAnnotations;
using FreeSql.DataAnnotations;
using FreeSql.Site.Entity.Common;
using System;

namespace FreeSql.Site.Entity
{
    public class DocumentType : BaseEntity
    {
        /// <summary>
        /// 类型名称
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// 上级类型名称
        /// </summary>
        public int? UpID { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        public string Tag { get; set; }

        public DateTime? UpdateDt { get; set; }

        public string UpdateBy { get; set; }
    }

    /// <summary>
    /// 类型树形结构
    /// </summary>
    public class DocumentTypeTreeNode : TreeNode
    {
        /// <summary>
        /// 标签
        /// </summary>
        public string tag { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? createdt { get; set; }
    }
}
