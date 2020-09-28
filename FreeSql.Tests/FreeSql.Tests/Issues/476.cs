using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Xunit;

namespace FreeSql.Tests.Issues
{
    public class _476
    {
        [Fact]
        public void SelectTest()
        {
            var fsql = g.sqlite;
            var repo = fsql.GetRepository<AreaEntity>();
            var list = repo.Select.Where(m => m.Name == "辽宁省").AsTreeCte().ToList();
        }

        [Table(Name = "Area476")]
        public class AreaEntity
        {
            [Column(IsIdentity = true)]
            public long Id { get; set; }
            public string Name { get; set; }
            public long ParentId { get; set; }

            [Navigate(nameof(ParentId))]
            public AreaEntity Parent { get; set; }
            [Navigate(nameof(ParentId))]
            public List<AreaEntity> Childs { get; set; }
        }
    }
}
