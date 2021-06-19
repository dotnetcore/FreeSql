using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace FreeSql.Tests.Issues
{
    public class _803
    {
        [Table(Name = "crm_sale_order")]
        class crm_sale_order
        {
            [Column(IsPrimary = true, IsIdentity = true)]
            public int id { get; set; }
            public string name { get; set; }

            public int tag_count { get; set; }
            public int gateway_count { get; set; }

            [JsonProperty, Column(DbType = "varchar(30)", CanUpdate = false)]
            public string create_by { get; set; } = string.Empty;

            [JsonProperty, Column(DbType = "datetime", CanUpdate = false)]
            public DateTime create_time { get; set; }
        }

        [Fact]
        public void IgnoreColumnsTest()
        {
            IFreeSql fsql = g.mysql;
            var dto = new crm_sale_order
            {
                name = "name",
                create_by = "create_by",
                create_time = DateTime.Now
            };
            fsql.Insert<crm_sale_order>().AppendData(dto).ExecuteAffrows();

            var crmDto = fsql.Select<crm_sale_order>().OrderByDescending(r => r.id).First();

            crmDto.name = "name" + new Random().Next(100);
            crmDto.tag_count = new Random().Next(100);
            crmDto.gateway_count = new Random().Next(100);
            crmDto.create_by = "create_by" + new Random().Next(100);
            crmDto.create_time = DateTime.Now.AddMinutes(10);

            fsql.Update<crm_sale_order>().SetSource(crmDto).IgnoreColumns(s => new { s.tag_count, s.gateway_count }).ExecuteAffrows();

            var updateDto = fsql.Select<crm_sale_order>().OrderByDescending(r => r.id).First();

            Assert.Equal(updateDto.tag_count, dto.tag_count);
            Assert.Equal(updateDto.gateway_count, dto.gateway_count);
            Assert.Equal(updateDto.create_time.ToString("g"), dto.create_time.ToString("g"));
            Assert.Equal(updateDto.create_by, dto.create_by);

            Assert.Equal(updateDto.name, crmDto.name);

        }
    }
}
