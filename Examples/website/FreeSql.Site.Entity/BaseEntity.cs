//using FreeSql.DataAnnotations;
using FreeSql.DataAnnotations;
using System;

namespace FreeSql.Site.Entity
{
    public class BaseEntity
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int ID { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }

        public DateTime? CreateDt { get; set; }

        public string CreateBy { get; set; }

    }
}
