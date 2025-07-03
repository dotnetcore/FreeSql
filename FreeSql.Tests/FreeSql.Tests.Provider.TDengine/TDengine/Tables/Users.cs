using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.DataAnnotations;

namespace FreeSql.Tests.Provider.TDengine.TDengine.Tables
{
    [Table(Name = "users")]
    public class Users
    {
        [Column(Name = "ts")]
        public DateTime Ts { get; set; }

        [Column(Name = "id")]
        public float Id { get; set; }

        [Column(Name = "address")]
        public int Address { get; set; }

        [Column(Name = "name", StringLength = 20)]
        public string? Name { get; set; }
    }
}
