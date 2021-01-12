using AME.Helpers;
using FreeSql.DataAnnotations;
using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Xunit;
using static FreeSql.Tests.UnitTest1;

namespace FreeSql.Tests
{
    public class UnitTest5
    {
        [Fact]
        public void TestUpdateDyWhere()
        {
            var fsql = g.sqlite;

            var sql = fsql.Update<ts_up_dywhere01>(new { status = "xxx" })
                .Set(a => a.status, "yyy")
                .ToSql();

            Assert.Equal(@"UPDATE ""ts_up_dywhere01"" SET ""status"" = @p_0 
WHERE (""status"" = 'xxx')", sql);
        }
        class ts_up_dywhere01
        {
            public Guid id { get; set; }
            public string status { get; set; }
        }
    }
}
