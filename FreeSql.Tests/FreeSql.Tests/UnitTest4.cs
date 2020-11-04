using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Xunit;
using static FreeSql.Tests.UnitTest1;

namespace FreeSql.Tests
{
    public class UnitTest4
    {

        [Fact]
        public void TestHzyTuple()
        {
            var xxxhzytuple = g.sqlite.Select<Templates, TaskBuild>()
                    .LeftJoin(w => w.t1.Id2 == w.t2.TemplatesId)
                    .Where(w => w.t1.Code == "xxx" && w.t2.OptionsEntity03 == true)
                    .OrderBy(w => w.t1.AddTime)
                    .ToSql();

            var xxxhzytupleGroupBy = g.sqlite.Select<Templates, TaskBuild>()
                    .LeftJoin(w => w.t1.Id2 == w.t2.TemplatesId)
                    .Where(w => w.t1.Code == "xxx" && w.t2.OptionsEntity03 == true)
                    .GroupBy(w => new { w.t1 })
                    .OrderBy(w => w.Key.t1.AddTime)
                    .ToSql(w => w.Key );

        }
    }
}
