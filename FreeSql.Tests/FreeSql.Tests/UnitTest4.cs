using FreeSql.DataAnnotations;
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

        public record ts_iif(Guid id, string title);
        [Fact]
        public void IIF()
        {
            var fsql = g.sqlserver;
            fsql.Delete<ts_iif>().Where("1=1").ExecuteAffrows();
            var id = Guid.NewGuid();
            fsql.Insert(new ts_iif(id, "001")).ExecuteAffrows();

            var item = fsql.Select<ts_iif>().Where(a => a.id == (id != Guid.NewGuid() ? id : a.id)).First();
            Assert.Equal(id, item.id);

            var item2 = fsql.Select<ts_iif>().First(a => new
            {
                xxx = id != Guid.NewGuid() ? a.id : Guid.Empty
            });
            Assert.Equal(id, item2.xxx);

            fsql.Delete<ts_iif_topic>().Where("1=1").ExecuteAffrows();
            fsql.Delete<ts_iif_type>().Where("1=1").ExecuteAffrows();
            var typeid = Guid.NewGuid();
            fsql.Insert(new ts_iif_type { id = typeid, name = "type001" }).ExecuteAffrows();
            fsql.Insert(new ts_iif_topic { id = id, typeid = typeid, title = "title001" }).ExecuteAffrows();

            var more1 = true;
            var more2 = (bool?)true;
            var more3 = (bool?)false;
            var more4 = (bool?)null;
            var moreitem = fsql.Select<ts_iif_topic>().First(a => new
            {
                a.id,
                a.title,
                a.type
            });
            Assert.Equal(id, moreitem.id);
            Assert.Equal("title001", moreitem.title);
            Assert.Equal(typeid, moreitem.type.id);
            Assert.Equal("type001", moreitem.type.name);
            var moreitem1 = fsql.Select<ts_iif_topic>().First(a => new
            {
                a.id,
                a.title,
                type1 = more1 == true ? a.type : null,
            });
            Assert.Equal(id, moreitem1.id);
            Assert.Equal("title001", moreitem1.title);
            Assert.Equal(typeid, moreitem1.type1.id);
            Assert.Equal("type001", moreitem1.type1.name);
            var moreitem2 = fsql.Select<ts_iif_topic>().First(a => new
            {
                a.id,
                a.title,
                type2 = more2 == true ? a.type : null,
            });
            Assert.Equal(id, moreitem2.id);
            Assert.Equal("title001", moreitem2.title);
            Assert.Equal(typeid, moreitem2.type2.id);
            Assert.Equal("type001", moreitem2.type2.name);
            var moreitem3 = fsql.Select<ts_iif_topic>().First(a => new
            {
                a.id,
                a.title,
                type3 = more3 == true ? a.type : null,
            });
            Assert.Equal(id, moreitem3.id);
            Assert.Equal("title001", moreitem3.title);
            Assert.Null(moreitem3.type3);
            var moreitem4 = fsql.Select<ts_iif_topic>().First(a => new
            {
                a.id,
                a.title,
                type4 = more4 == true ? a.type : null,
            });
            Assert.Equal(id, moreitem4.id);
            Assert.Equal("title001", moreitem4.title);
            Assert.Null(moreitem4.type4);
        }
        class ts_iif_topic
        {
            public Guid id { get; set; }
            public Guid typeid { get; set; }
            [Navigate(nameof(typeid))]
            public ts_iif_type type { get; set; }
            public string title { get; set; }
        }
        class ts_iif_type
        {
            public Guid id { get; set; }
            public string name { get; set; }
        }
        

        public record ts_record(DateTime Date, int TemperatureC, int TemperatureF, string Summary)
        {
            public ts_record parent { get; set; }
        }
        public record ts_record_dto(DateTime Date, int TemperatureC, string Summary);

        [Fact]
        public void LeftJoinNull01()
        {
            var fsql = g.sqlite;

            fsql.Delete<ts_record>().Where("1=1").ExecuteAffrows();
            fsql.Insert(new ts_record(DateTime.Now, 1, 2, "123")).ExecuteAffrows();
            var fores = fsql.Select<ts_record>().ToList();
            var fores_dtos1 = fsql.Select<ts_record>().ToList<ts_record_dto>();
            var fores_dtos2 = fsql.Select<ts_record>().ToList(a => new ts_record_dto(a.Date, a.TemperatureC, a.Summary));



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
