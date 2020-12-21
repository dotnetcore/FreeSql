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
    public class UnitTest4
    {
        [Fact]
        public void WithLambdaParameter01()
        {
            using (var fsql = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.SqlServer, "Data Source=.;Integrated Security=True;Initial Catalog=freesqlTest;Pooling=true;Max Pool Size=6")
                .UseAutoSyncStructure(true)
                .UseGenerateCommandParameterWithLambda(true)
                .UseMonitorCommand(
                    cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText) //监听SQL命令对象，在执行前
                    //, (cmd, traceLog) => Console.WriteLine(traceLog)
                    )
                .UseLazyLoading(true)
                .Build())
            {
                fsql.Delete<ts_wlp01>().Where("1=1").ExecuteAffrows();

                var id1 = Guid.NewGuid();
                var id2 = Guid.NewGuid();
                var affrows = fsql.Insert(new[] {
                    new ts_wlp01
                    {
                        id = id1,
                        status = ts_wlp01_status.completed,
                    },
                    new ts_wlp01
                    {
                        id = id2,
                        status = ts_wlp01_status.pending,
                    }
                }).ExecuteAffrows();
                Assert.Equal(2, affrows);
                var items = fsql.Select<ts_wlp01>().OrderBy(a => a.status).ToList();
                Assert.Equal(2, items.Count);
                Assert.Equal(id1, items[0].id);
                Assert.Equal(ts_wlp01_status.completed, items[0].status);
                Assert.Equal(id2, items[1].id);
                Assert.Equal(ts_wlp01_status.pending, items[1].status);

                var item1Select = fsql.Select<ts_wlp01>().Where(a => a.status == ts_wlp01_status.completed);
                var item1S0p = item1Select as Select0Provider;
                Assert.Single(item1S0p._params);
                var p0 = item1S0p._params[0];
                Assert.Equal(p0.DbType, System.Data.DbType.String);
                Assert.Equal(p0.Value, "completed");
                items = item1Select.ToList();
                Assert.Single(items);
                Assert.Equal(id1, items[0].id);
                Assert.Equal(ts_wlp01_status.completed, items[0].status);

                var item2Select = fsql.Select<ts_wlp01>().Where(a => a.status == ts_wlp01_status.pending);
                var item2S0p = item2Select as Select0Provider;
                Assert.Single(item2S0p._params);
                p0 = item2S0p._params[0];
                Assert.Equal(p0.DbType, System.Data.DbType.String);
                Assert.Equal(p0.Value, "pending");
                items = item2Select.ToList();
                Assert.Single(items);
                Assert.Equal(id2, items[0].id);
                Assert.Equal(ts_wlp01_status.pending, items[0].status);

                // use var
                var item1status = ts_wlp01_status.completed;
                item1Select = fsql.Select<ts_wlp01>().Where(a => a.status == item1status);
                item1S0p = item1Select as Select0Provider;
                Assert.Single(item1S0p._params);
                p0 = item1S0p._params[0];
                Assert.Equal(p0.DbType, System.Data.DbType.String);
                Assert.Equal(p0.Value, "completed");
                items = item1Select.ToList();
                Assert.Single(items);
                Assert.Equal(id1, items[0].id);
                Assert.Equal(item1status, items[0].status);

                var item2status = ts_wlp01_status.pending;
                item2Select = fsql.Select<ts_wlp01>().Where(a => a.status == item2status);
                item2S0p = item2Select as Select0Provider;
                Assert.Single(item2S0p._params);
                p0 = item2S0p._params[0];
                Assert.Equal(p0.DbType, System.Data.DbType.String);
                Assert.Equal(p0.Value, "pending");
                items = item2Select.ToList();
                Assert.Single(items);
                Assert.Equal(id2, items[0].id);
                Assert.Equal(item2status, items[0].status);
            }
        }
        class ts_wlp01
        {
            public Guid id { get; set; }
            [Column(MapType = typeof(string), StringLength = 20)]
            public ts_wlp01_status status { get; set; }
        }
        enum ts_wlp01_status { pending, completed, failed }
        [Fact]
        public void NonoLambdaParameter()
        {
            var fsql = g.sqlserver;
            fsql.Delete<ts_wlp02>().Where("1=1").ExecuteAffrows();

            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var affrows = fsql.Insert(new[] {
                    new ts_wlp02
                    {
                        id = id1,
                        status = ts_wlp02_status.completed,
                    },
                    new ts_wlp02
                    {
                        id = id2,
                        status = ts_wlp02_status.pending,
                    }
                }).ExecuteAffrows();
            Assert.Equal(2, affrows);
            var items = fsql.Select<ts_wlp02>().OrderBy(a => a.status).ToList();
            Assert.Equal(2, items.Count);
            Assert.Equal(id1, items[0].id);
            Assert.Equal(ts_wlp02_status.completed, items[0].status);
            Assert.Equal(id2, items[1].id);
            Assert.Equal(ts_wlp02_status.pending, items[1].status);

            var item1Select = fsql.Select<ts_wlp02>().Where(a => a.status == ts_wlp02_status.completed);
            Assert.Equal(@"SELECT a.[id], a.[status] 
FROM [ts_wlp02] a 
WHERE (a.[status] = N'completed')", item1Select.ToSql());
            items = item1Select.ToList();
            Assert.Single(items);
            Assert.Equal(id1, items[0].id);
            Assert.Equal(ts_wlp02_status.completed, items[0].status);

            var item2Select = fsql.Select<ts_wlp02>().Where(a => a.status == ts_wlp02_status.pending);
            Assert.Equal(@"SELECT a.[id], a.[status] 
FROM [ts_wlp02] a 
WHERE (a.[status] = N'pending')", item2Select.ToSql());
            items = item2Select.ToList();
            Assert.Single(items);
            Assert.Equal(id2, items[0].id);
            Assert.Equal(ts_wlp02_status.pending, items[0].status);

            // use var
            var item1status = ts_wlp02_status.completed;
            item1Select = fsql.Select<ts_wlp02>().Where(a => a.status == item1status);
            Assert.Equal(@"SELECT a.[id], a.[status] 
FROM [ts_wlp02] a 
WHERE (a.[status] = N'completed')", item1Select.ToSql());
            items = item1Select.ToList();
            Assert.Single(items);
            Assert.Equal(id1, items[0].id);
            Assert.Equal(item1status, items[0].status);

            var item2status = ts_wlp02_status.pending;
            item2Select = fsql.Select<ts_wlp02>().Where(a => a.status == item2status);
            Assert.Equal(@"SELECT a.[id], a.[status] 
FROM [ts_wlp02] a 
WHERE (a.[status] = N'pending')", item2Select.ToSql());
            items = item2Select.ToList();
            Assert.Single(items);
            Assert.Equal(id2, items[0].id);
            Assert.Equal(item2status, items[0].status);
        }
        class ts_wlp02
        {
            public Guid id { get; set; }
            [Column(MapType = typeof(string), StringLength = 20)]
            public ts_wlp02_status status { get; set; }
        }
        enum ts_wlp02_status { pending, completed, failed }

        [Fact]
        public void GroupSubSelect()
        {
            var fsql = g.sqlite;
            fsql.Delete<ts_group_sub_select01>().Where("1=1").ExecuteAffrows();

            var affrows = fsql.Insert(new[]{
                new ts_group_sub_select01
                {
                    code = "code_01",
                    seqid = 1,
                    name = "name_01"
                },
                new ts_group_sub_select01
                {
                    code = "code_02",
                    seqid = 2,
                    name = "name_02"
                }
            }).ExecuteAffrows();
            Assert.Equal(2, affrows);

            var sql = fsql.Select<ts_group_sub_select01>()
                .GroupBy(a => new
                {
                    a.code,
                    a.seqid,
                    a.name
                })
                .ToSql(g => new
                {
                    g.Key.code,
                    g.Key.seqid,
                    g.Key.name,
                    number = fsql.Select<ts_group_sub_select01>()
                        .Where(o => o.seqid == 6)
                        .Count(),
                    number2 = 3,
                    number3 = 5
                });
            Assert.Equal(@"SELECT a.""code"" as1, a.""seqid"" as2, a.""name"" as3, (SELECT count(1) 
    FROM ""ts_group_sub_select01"" o 
    WHERE (o.""seqid"" = 6)) as4, 3 as5, 5 as6 
FROM ""ts_group_sub_select01"" a 
GROUP BY a.""code"", a.""seqid"", a.""name""", sql);
            Assert.Equal(2, fsql.Select<ts_group_sub_select01>()
                .GroupBy(a => new
                {
                    a.code,
                    a.seqid,
                    a.name
                })
                .ToList(g => new
                {
                    g.Key.code,
                    g.Key.seqid,
                    g.Key.name,
                    number = fsql.Select<ts_group_sub_select01>()
                        .Where(o => o.seqid == 6)
                        .Count(),
                    number2 = 3,
                    number3 = 5
                }).Count);

            sql = fsql.Select<ts_group_sub_select01>()
                .GroupBy(a => new
                {
                    a.code,
                    a.seqid,
                    a.name
                })
                .ToSql(g => new
                {
                    g.Key.code,
                    g.Key.seqid,
                    g.Key.name,
                    number = fsql.Select<ts_group_sub_select01>()
                        .Where(o => o.seqid == g.Key.seqid)
                        .Count(),
                    number2 = 3,
                    number3 = 5
                });
            Assert.Equal(@"SELECT a.""code"" as1, a.""seqid"" as2, a.""name"" as3, (SELECT count(1) 
    FROM ""ts_group_sub_select01"" o 
    WHERE (o.""seqid"" = a.""seqid"")) as4, 3 as5, 5 as6 
FROM ""ts_group_sub_select01"" a 
GROUP BY a.""code"", a.""seqid"", a.""name""", sql);
            Assert.Equal(2, fsql.Select<ts_group_sub_select01>()
                .GroupBy(a => new
                {
                    a.code,
                    a.seqid,
                    a.name
                })
                .ToList(g => new
                {
                    g.Key.code,
                    g.Key.seqid,
                    g.Key.name,
                    number = fsql.Select<ts_group_sub_select01>()
                        .Where(o => o.seqid == g.Key.seqid)
                        .Count(),
                    number2 = 3,
                    number3 = 5
                }).Count);
        }
        class ts_group_sub_select01
        {
            public Guid id { get; set; }
            public string code { get; set; }
            public int seqid { get; set; }
            public string name { get; set; }
        }

        [Fact]
        public void OneToManyLazyloading()
        {
            var fsql = g.sqlite;
            fsql.Delete<ts_otm_ll_01>().Where("1=1").ExecuteAffrows();
            fsql.Delete<ts_otm_ll_02>().Where("1=1").ExecuteAffrows();

            var repo = fsql.GetRepository<ts_otm_ll_01>();
            repo.DbContextOptions.EnableAddOrUpdateNavigateList = true;
            repo.Insert(new ts_otm_ll_01
            {
                name = "001",
                ll_02s = new List<ts_otm_ll_02>(new[] {
                    new ts_otm_ll_02 { title = "sub_001" },
                    new ts_otm_ll_02 { title = "sub_002" },
                    new ts_otm_ll_02 { title = "sub_003" }
                })
            });

            var item = fsql.Select<ts_otm_ll_01>().First();
            Assert.NotNull(item);
            var childs = item.ll_02s;
            Assert.NotNull(childs);
            Assert.Equal(3, childs.Count);
        }
        public class ts_otm_ll_01
        {
            public Guid id { get; set; }
            public string name { get; set; }
            [Navigate(nameof(ts_otm_ll_02.ll_01id))]
            public virtual List<ts_otm_ll_02> ll_02s { get; set; }
        }
        public class ts_otm_ll_02
        {
            public Guid id { get; set; }
            public Guid ll_01id { get; set; }
            public string title { get; set; }
        }

        [Fact]
        public void SelectLambdaParameter()
        {
            using (var fsql = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.SqlServer, "Data Source=.;Integrated Security=True;Initial Catalog=freesqlTest;Pooling=true;min pool size=1;Max Pool Size=51")
                .UseAutoSyncStructure(true)
                .UseGenerateCommandParameterWithLambda(true)
                .UseMonitorCommand(
                    cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText) //监听SQL命令对象，在执行前
                    //, (cmd, traceLog) => Console.WriteLine(traceLog)
                    )
                .Build())
            {
                fsql.Delete<ts_slp01>().Where("1=1").ExecuteAffrows();

                var testItem = new ts_slp01();
                var sql1 = fsql.Select<ts_slp01>().Where(a => a.name == testItem.GetTestName1()).ToSql();
                var sql2 = fsql.Select<ts_slp01>().Where(a => a.name == testItem.GetTestName2()).ToSql();

                Assert.Equal(@"SELECT a.[id], a.[name] 
FROM [ts_slp01] a 
WHERE (a.[name] = @exp_0)", sql1);
                Assert.Equal(@"SELECT a.[id], a.[name] 
FROM [ts_slp01] a 
WHERE (a.[name]  IS  NULL)", sql2);
            }
        }
        class ts_slp01
        {
            public Guid id { get; set; }
            public string name { get; set; }

            public string GetTestName1() => "xxx";
            public string GetTestName2() => null;
        }

        [Fact]
        public void SelectN_SubSelectN()
        {
            var fsql = g.sqlite;
            var plansql1 = fsql.Select<ts_tplan, ts_tproductmode, ts_tflowversion, ts_tproflow>()
                .LeftJoin((a, b, c, d) => a.pmcode == b.pmcode)
                .LeftJoin((a, b, c, d) => b.pmcode == c.pmcode && c.isdefault == 1)
                .InnerJoin((a, b, c, d) => c.fvcode == d.fvcode)
                .ToSql((a, b, c, d) => new
                {
                    a.billcode,
                    b.pmname,
                    d.techcode
                });

            var plansql2 = fsql.Select<ts_tplan, ts_tproductmode, ts_tflowversion, ts_tproflow>()
                .LeftJoin((a, b, c, d) => a.pmcode == b.pmcode)
                .LeftJoin((a, b, c, d) => b.pmcode == c.pmcode && c.isdefault == 1)
                .InnerJoin((a, b, c, d) => c.fvcode == d.fvcode)
                .ToSql((a, b, c, d) => new
                {
                    a.billcode,
                    b.pmname,
                    d.techcode,
                    planQty = fsql.Select<ts_tproduct_catering, ts_tproduct_catering_detail>()
                        .InnerJoin((e, f) => e.code == f.pccode)
                        .Where((e, f) => a.code == e.plancode)
                        .Count()
                });
            Assert.Equal(@"SELECT a.""billcode"" as1, b.""pmname"" as2, d.""techcode"" as3, (SELECT count(1) 
    FROM ""ts_tproduct_catering"" e 
    INNER JOIN ""ts_tproduct_catering_detail"" f ON e.""code"" = f.""pccode"" 
    WHERE (a.""code"" = e.""plancode"")) as4 
FROM ""ts_tplan"" a 
LEFT JOIN ""ts_tproductmode"" b ON a.""pmcode"" = b.""pmcode"" 
LEFT JOIN ""ts_tflowversion"" c ON b.""pmcode"" = c.""pmcode"" AND c.""isdefault"" = 1 
INNER JOIN ""ts_tproflow"" d ON c.""fvcode"" = d.""fvcode""", plansql2);
        }
        class ts_tplan
        {
            public string code { get; set; }
            public string pmcode { get; set; }
            public string billcode { get; set; }
        }
        class ts_tproductmode
        {
            public string pmcode { get; set; }
            public string pmname { get; set; }
        }
        class ts_tflowversion
        {
            public string fvcode { get; set; }
            public string pmcode { get; set; }
            public int isdefault { get; set; }
        }
        class ts_tproflow
        {
            public string fvcode { get; set; }
            public string techcode { get; set; }
        }
        class ts_tproduct_catering
        {
            public string code { get; set; }
            public string plancode { get; set; }
        }
        class ts_tproduct_catering_detail
        {
            public string pccode { get; set; }
        }
        class ts_tmain_record
        {
            public string code { get; set; }
            public string barcode { get; set; }
        }
        class ts_tprocess_record
        {
            public string mrcode { get; set; }
            public string barcode { get; set; }
            public string techcode { get; set; }
            public int assemres { get; set; }
        }

        class ts_lawsuit
        {
            public Guid id { get; set; }
            public Guid lawsuit_member_id { get; set; }
            public Guid lawsuit_memberObligee_id { get; set; }

            public ts_lawsuit_member ts_lawsuit_member { get; set; }
            public ts_lawsuit_member ts_lawsuit_memberObligee { get; set; }

        }
        class ts_lawsuit_member
        {
            public Guid id { get; set; }
            public string title { get; set; }
        }

        [Fact]
        public void VersionByte()
        {
            var ts_lawsuititem = new ts_lawsuit { id = Guid.NewGuid(), lawsuit_memberObligee_id = Guid.NewGuid(), lawsuit_member_id = Guid.NewGuid() };
            g.mysql.Insert(new[]
            {
                new ts_lawsuit_member{id = ts_lawsuititem.lawsuit_member_id, title = "ts_lawsuit_member_title"},
                new ts_lawsuit_member{id = ts_lawsuititem.lawsuit_memberObligee_id, title = "ts_lawsuit_memberObligee_title"}
            }).ExecuteAffrows();
            g.mysql.Insert(ts_lawsuititem).ExecuteAffrows();

            var xxx = g.mysql.Select<ts_lawsuit>()
                .LeftJoin(a => a.ts_lawsuit_member.id == a.lawsuit_member_id)
                .LeftJoin(a => a.ts_lawsuit_memberObligee.id == a.lawsuit_memberObligee_id)
                .First();

            g.sqlserver.Insert(new AppSettingII
            {
                 
            }).ExecuteAffrows();
            var item33 = g.sqlserver.Select<AppSettingII>().ToList();
            var sql22 = g.sqlserver.Select<AppSettingII>()
                .OrderBy(a => a.ID)
                .Count(out var total)
                .Page(1, 10).ToSql();
            var items22 = g.sqlserver.Select<AppSettingII>().WithSql(sql22).ToList();

            var defv1 = typeof(decimal).CreateInstanceGetDefaultValue();
            var defv2 = typeof(decimal?).CreateInstanceGetDefaultValue();
           
            var fsql = g.mysql;
            fsql.Delete<ts_ver_byte>().Where("1=1").ExecuteAffrows();
            var id = Guid.NewGuid();
            Assert.Equal(1, fsql.Insert(new ts_ver_byte { id = id, title = "001" }).ExecuteAffrows());

            var item = fsql.Select<ts_ver_byte>(id).First();
            item.title = "002";
            Assert.Equal(1, fsql.Update<ts_ver_byte>().SetSource(item).ExecuteAffrows());
            item.title = "003";
            Assert.Equal(1, fsql.Update<ts_ver_byte>().SetSource(item).ExecuteAffrows());

            item.version = Utils.GuidToBytes(Guid.NewGuid());
            item.title = "004";
            Assert.Throws<DbUpdateVersionException>(() => fsql.Update<ts_ver_byte>().SetSource(item).ExecuteAffrows());

            fsql.Delete<ts_ver_byte>().Where("1=1").ExecuteAffrows();
            Assert.Equal(2, fsql.Insert(new[] { new ts_ver_byte { id = Guid.NewGuid(), title = "001" }, new ts_ver_byte { id = Guid.NewGuid(), title = "0011" } }).ExecuteAffrows());
            var items = fsql.Select<ts_ver_byte>().OrderBy(a => a.title).ToList();
            Assert.Equal(2, items.Count);
            items[0].title = "002";
            items[1].title = "0022";
            Assert.Equal(2, fsql.Update<ts_ver_byte>().SetSource(items).ExecuteAffrows());
            items[0].title = "003";
            items[1].title = "0033";
            Assert.Equal(2, fsql.Update<ts_ver_byte>().SetSource(items).ExecuteAffrows());

            items[0].version = Utils.GuidToBytes(Guid.NewGuid());
            items[0].title = "004";
            items[1].title = "0044";
            Assert.Throws<DbUpdateVersionException>(() => fsql.Update<ts_ver_byte>().SetSource(items).ExecuteAffrows());

            items[0].version = Utils.GuidToBytes(Guid.NewGuid());
            items[1].version = Utils.GuidToBytes(Guid.NewGuid());
            items[0].title = "004";
            items[1].title = "0044";
            Assert.Throws<DbUpdateVersionException>(() => fsql.Update<ts_ver_byte>().SetSource(items).ExecuteAffrows());
        }
        class ts_ver_byte
        {
            public Guid id { get; set; }
            public string title { get; set; }
            [Column(IsVersion = true)]
            public byte[] version { get; set; }
        }


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
