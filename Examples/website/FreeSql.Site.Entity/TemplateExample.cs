using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeSql.Site.Entity
{
    /// <summary>
    /// 模板示例
    /// </summary>
    public class TemplateExample : BaseEntity
    {

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
        /// 修改时间
        /// </summary>
        public DateTime? UpdateDt { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public string UpdateBy { get; set; }
    }
}
