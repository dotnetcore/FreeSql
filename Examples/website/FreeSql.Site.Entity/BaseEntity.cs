//using FreeSql.DataAnnotations;
using FreeSql.DataAnnotations;
using System;

namespace FreeSql.Site.Entity
{
    public class BaseEntity
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int ID { get; set; } = 0;

        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; } = 1;

        public DateTime? CreateDt { get; set; } = DateTime.Now;

        public string CreateBy { get; set; } = "admin";

    }
}
