using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeSql.Site.Entity
{
    /// <summary>
    /// 模板示例
    /// </summary>
    public class TemplateExample
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int ID { get; set; }

        /// <summary>
        /// 模板图片
        /// </summary>
        public string TemplateImg { get; set; }

        /// <summary>
        /// 模板名称
        /// </summary>
        public string TempateName { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Describe { get; set; }

        /// <summary>
        /// 模板路径
        /// </summary>
        public string TemplatePath { get; set; }

        /// <summary>
        /// 查看次数
        /// </summary>
        public int WatchCount { get; set; }

        /// <summary>
        /// 下载统计
        /// </summary>
        public int DownloadCount { get; set; }

        /// <summary>
        /// Star统计
        /// </summary>
        public int StarCount { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }

        public DateTime? CreateDt { get; set; }

        public string CreateBy { get; set; }

        public DateTime? UpdateDt { get; set; }

        public string UpdateBy { get; set; }
    }
}
