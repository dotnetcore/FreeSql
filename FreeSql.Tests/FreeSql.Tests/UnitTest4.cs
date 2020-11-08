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
        public void LeftJoinNull01()
        {
            var fsql = g.sqlite;
            fsql.Delete<leftjoin_null01>().Where("1=1").ExecuteAffrows();
            fsql.Delete<leftjoin_null02>().Where("1=1").ExecuteAffrows();

            var item = new leftjoin_null01 { name = "xx01" };
            fsql.Insert(item).ExecuteAffrows();

            var sel1 = fsql.Select<leftjoin_null01, leftjoin_null02>()
                .LeftJoin((a, b) => a.id == b.null01_id)
                .First((a, b) => new
                {
                    a.id,
                    a.name,
                    id2 = (Guid?)b.id,
                    time2 = (DateTime?)b.time
                });
            Assert.Null(sel1.id2);
            Assert.Null(sel1.time2);
        }

        class leftjoin_null01
        {
            public Guid id { get; set; }
            public string name { get; set; }
        }
        class leftjoin_null02
        {
            public Guid id { get; set; }
            public Guid null01_id { get; set; }
            public DateTime time { get; set; }
        }


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
