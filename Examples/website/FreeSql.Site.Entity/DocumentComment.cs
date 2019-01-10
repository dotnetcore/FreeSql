//using FreeSql.DataAnnotations;
using FreeSql.DataAnnotations;
using System;

namespace FreeSql.Site.Entity
{
    public class DocumentComment
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int ID { get; set; }

        /// <summary>
        /// 功能类型（文章、模板、示例等）
        /// </summary>
        public int FunctionType { get; set; }

        /// <summary>
        /// 功能ID  文章、模板、示例等
        /// </summary>
        public int FunctionID { get; set; }

        /// <summary>
        /// 是否匿名访问
        /// </summary>
        public int IsAnonymous { get; set; }

        /// <summary>
        /// 评论人
        /// </summary>
        public string Commentator { get; set; }

        /// <summary>
        /// 评论者IP
        /// </summary>
        public string CommentatorIp { get; set; }

        /// <summary>
        /// 回复评论编号
        /// </summary>
        public int ReplyID { get; set; }

        /// <summary>
        /// 评论内容
        /// </summary>
        public string CommentContent { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateDt { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateBy { get; set; }

    }
}
