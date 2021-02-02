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
        public void TestDistinctCount()
        {
            var fsql = g.sqlite;

            var sql = fsql.Select<ts_up_dywhere01>().ToSql(a => SqlExt.DistinctCount(a.status));
            fsql.Select<ts_up_dywhere01>().Aggregate(a => SqlExt.DistinctCount(a.Key.status), out var count);

            Assert.Equal(@"SELECT count(distinct a.""status"") as1 
FROM ""ts_up_dywhere01"" a", sql);

            sql = fsql.Select<ts_up_dywhere01>().Select(a => new { a.status }).Distinct().ToSql();
            fsql.Select<ts_up_dywhere01>().Select(a => new { a.status }).Distinct().Count(out count);

            Assert.Equal(@"SELECT DISTINCT a.""status"" as1 
FROM ""ts_up_dywhere01"" a", sql);
        }

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
