//using FreeSql.DataAnnotations;
using FreeSql.DataAnnotations;
using System;

namespace FreeSql.Site.Entity
{
    public class DocumentContent
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int ID { get; set; }

        /// <summary>
        /// 类型编号
        /// </summary>
        public int TypeID { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string DocTitle { get; set; }

        /// <summary>
        /// 摘要
        /// </summary>
        public string DocAbstract { get; set; }

        /// <summary>
        /// 文档内容
        /// </summary>
        public string DocContent { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 查看次数
        /// </summary>
        public int WatchCount { get; set; }

        /// <summary>
        /// Star统计
        /// </summary>
        public int StarCount { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateDt { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateBy { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? UpdateDt { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public string UpdateBy { get; set; }
    }
}
