using FreeSql.DataAnnotations;
using System;

namespace FreeSql.Site.Entity
{
    public class DocumentType
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int ID { get; set; }

        public string TypeName { get; set; }

        public int? UpID { get; set; }

        public DateTime? CreateDt { get; set; }

        public string CreateBy { get; set; }

        public DateTime? UpdateDt { get; set; }

        public string UpdateBy { get; set; }
    }
}
