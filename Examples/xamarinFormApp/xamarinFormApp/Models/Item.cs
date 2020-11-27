using System;
using FreeSql.DataAnnotations;

namespace xamarinFormApp.Models
{
    public class Item
    {
        [Column(IsPrimary = false)]
        public string fId { get; set; }

        public string Id { get; set; }
        public string Text { get; set; }
        public string Description { get; set; }
    }
}