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
    public class _494
    {
        [Fact]
        public void SelectTest()
        {
            var fsql = g.sqlite;
            var sql = fsql.Queryable<WorkSite>().ToSql(w => new LocStatusViewModel
            {
                CardNumber = w.Number.ToString(),
                CardType = w.CommType + 1,
                Name = w.Address,
                No = w.Number.ToString()
            }, FieldAliasOptions.AsProperty);
            Assert.Equal(@"SELECT cast(a.""Number"" as character) ""CardNumber"", ((a.""CommType"" + 1)) ""CardType"", a.""Address"" ""Name"", cast(a.""Number"" as character) ""No"" 
FROM ""WorkSite"" a", sql);
        }

        public class WorkSite
        {
            [Column(IsPrimary = true, IsIdentity = true)]
            public int Id { get; set; }

            public string Name { get; set; }
            public int Number { get; set; }
            public int CommType { get; set; }
            public string Address { get; set; }
        }
        public class LocStatusViewModel
        {
            public string CardNumber { get; set; }
            public int CardType { get; set; }
            public string Name { get; set; }
            public string No { get; set; }
        }
    }
}
