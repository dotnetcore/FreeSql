using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.PostgreSQL
{
    public class PostgreSQLInsertOrUpdateTest
    {
        IFreeSql fsql => g.pgsql;

        [Fact]
        public void InsertOrUpdate_OnlyPrimary()
        {
            fsql.Delete<tbiou01>().Where("1=1").ExecuteAffrows();
            var iou = fsql.InsertOrUpdate<tbiou01>().SetSource(new tbiou01 { id = 1 });
            var sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO ""tbiou01""(""id"") VALUES(1)
ON CONFLICT(""id"") DO NOTHING", sql);
            Assert.Equal(1, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbiou01>().SetSource(new tbiou01 { id = 1 });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO ""tbiou01""(""id"") VALUES(1)
ON CONFLICT(""id"") DO NOTHING", sql);
            Assert.Equal(0, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbiou01>().SetSource(new tbiou01 { id = 2 });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO ""tbiou01""(""id"") VALUES(2)
ON CONFLICT(""id"") DO NOTHING", sql);
            Assert.Equal(1, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbiou01>().SetSource(new[] { new tbiou01 { id = 1 }, new tbiou01 { id = 2 }, new tbiou01 { id = 3 }, new tbiou01 { id = 4 } });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO ""tbiou01""(""id"") VALUES(1), (2), (3), (4)
ON CONFLICT(""id"") DO NOTHING", sql);
            Assert.Equal(2, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbiou01>().SetSource(new[] { new tbiou01 { id = 1 }, new tbiou01 { id = 2 }, new tbiou01 { id = 3 }, new tbiou01 { id = 4 } });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO ""tbiou01""(""id"") VALUES(1), (2), (3), (4)
ON CONFLICT(""id"") DO NOTHING", sql);
            Assert.Equal(0, iou.ExecuteAffrows());
        }
        class tbiou01
        {
            public int id { get; set; }
        }

        [Fact]
        public void InsertOrUpdate_OnePrimary()
        {
            fsql.Delete<tbiou02>().Where("1=1").ExecuteAffrows();
            var iou = fsql.InsertOrUpdate<tbiou02>().SetSource(new tbiou02 { id = 1, name = "01" });
            var sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO ""tbiou02""(""id"", ""name"") VALUES(1, '01')
ON CONFLICT(""id"") DO UPDATE SET
""name"" = EXCLUDED.""name""", sql);
            Assert.Equal(1, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbiou02>().SetSource(new tbiou02 { id = 1, name = "011" });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO ""tbiou02""(""id"", ""name"") VALUES(1, '011')
ON CONFLICT(""id"") DO UPDATE SET
""name"" = EXCLUDED.""name""", sql);
            Assert.Equal(1, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbiou02>().SetSource(new tbiou02 { id = 2, name = "02" });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO ""tbiou02""(""id"", ""name"") VALUES(2, '02')
ON CONFLICT(""id"") DO UPDATE SET
""name"" = EXCLUDED.""name""", sql);
            Assert.Equal(1, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbiou02>().SetSource(new[] { new tbiou02 { id = 1, name = "01" }, new tbiou02 { id = 2, name = "02" }, new tbiou02 { id = 3, name = "03" }, new tbiou02 { id = 4, name = "04" } });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO ""tbiou02""(""id"", ""name"") VALUES(1, '01'), (2, '02'), (3, '03'), (4, '04')
ON CONFLICT(""id"") DO UPDATE SET
""name"" = EXCLUDED.""name""", sql);
            Assert.Equal(4, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbiou02>().SetSource(new[] { new tbiou02 { id = 1, name = "001" }, new tbiou02 { id = 2, name = "002" }, new tbiou02 { id = 3, name = "003" }, new tbiou02 { id = 4, name = "004" } });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO ""tbiou02""(""id"", ""name"") VALUES(1, '001'), (2, '002'), (3, '003'), (4, '004')
ON CONFLICT(""id"") DO UPDATE SET
""name"" = EXCLUDED.""name""", sql);
            Assert.Equal(4, iou.ExecuteAffrows());
            var lst = fsql.Select<tbiou02>().Where(a => new[] { 1, 2, 3, 4 }.Contains(a.id)).ToList();
            Assert.Equal(4, lst.Where(a => a.name == "00" + a.id).Count());
        }
        class tbiou02
        {
            public int id { get; set; }
            public string name { get; set; }
        }
        [Fact]
        public void InsertOrUpdate_OnePrimaryAndIdentity()
        {
            fsql.Delete<tbiou022>().Where("1=1").ExecuteAffrows();
            var iou = fsql.InsertOrUpdate<tbiou022>().SetSource(new tbiou022 { id = 1, name = "01" });
            var sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO ""tbiou022""(""id"", ""name"") VALUES(1, '01')
ON CONFLICT(""id"") DO UPDATE SET
""name"" = EXCLUDED.""name""", sql);
            Assert.Equal(1, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbiou022>().SetSource(new tbiou022 { id = 1, name = "011" });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO ""tbiou022""(""id"", ""name"") VALUES(1, '011')
ON CONFLICT(""id"") DO UPDATE SET
""name"" = EXCLUDED.""name""", sql);
            Assert.Equal(1, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbiou022>().SetSource(new tbiou022 { id = 2, name = "02" });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO ""tbiou022""(""id"", ""name"") VALUES(2, '02')
ON CONFLICT(""id"") DO UPDATE SET
""name"" = EXCLUDED.""name""", sql);
            Assert.Equal(1, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbiou022>().SetSource(new[] { new tbiou022 { id = 1, name = "01" }, new tbiou022 { id = 2, name = "02" }, new tbiou022 { id = 3, name = "03" }, new tbiou022 { id = 4, name = "04" } });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO ""tbiou022""(""id"", ""name"") VALUES(1, '01'), (2, '02'), (3, '03'), (4, '04')
ON CONFLICT(""id"") DO UPDATE SET
""name"" = EXCLUDED.""name""", sql);
            Assert.Equal(4, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbiou022>().SetSource(new[] { new tbiou022 { id = 1, name = "001" }, new tbiou022 { id = 2, name = "002" }, new tbiou022 { id = 3, name = "003" }, new tbiou022 { id = 4, name = "004" } });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO ""tbiou022""(""id"", ""name"") VALUES(1, '001'), (2, '002'), (3, '003'), (4, '004')
ON CONFLICT(""id"") DO UPDATE SET
""name"" = EXCLUDED.""name""", sql);
            Assert.Equal(4, iou.ExecuteAffrows());
            var lst = fsql.Select<tbiou022>().Where(a => new[] { 1, 2, 3, 4 }.Contains(a.id)).ToList();
            Assert.Equal(4, lst.Where(a => a.name == "00" + a.id).Count());

            //--no primary
            iou = fsql.InsertOrUpdate<tbiou022>().SetSource(new tbiou022 { name = "01" });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO ""tbiou022""(""name"") VALUES('01')", sql);
            Assert.Equal(1, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbiou022>().SetSource(new tbiou022 { name = "011" });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO ""tbiou022""(""name"") VALUES('011')", sql);
            Assert.Equal(1, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbiou022>().SetSource(new tbiou022 { name = "02" });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO ""tbiou022""(""name"") VALUES('02')", sql);
            Assert.Equal(1, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbiou022>().SetSource(new[] { new tbiou022 { name = "01" }, new tbiou022 { name = "02" }, new tbiou022 { name = "03" }, new tbiou022 { name = "04" } });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO ""tbiou022""(""name"") VALUES('01'), ('02'), ('03'), ('04')", sql);
            Assert.Equal(4, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbiou022>().SetSource(new[] { new tbiou022 { name = "001" }, new tbiou022 { name = "002" }, new tbiou022 { name = "003" }, new tbiou022 { name = "004" } });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO ""tbiou022""(""name"") VALUES('001'), ('002'), ('003'), ('004')", sql);
            Assert.Equal(4, iou.ExecuteAffrows());

            //--no primary and yes
            iou = fsql.InsertOrUpdate<tbiou022>().SetSource(new[] { new tbiou022 { id = 1, name = "100001" }, new tbiou022 { name = "00001" }, new tbiou022 { id = 2, name = "100002" }, new tbiou022 { name = "00002" }, new tbiou022 { id = 3, name = "100003" }, new tbiou022 { name = "00003" }, new tbiou022 { id = 4, name = "100004" }, new tbiou022 { name = "00004" } });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO ""tbiou022""(""id"", ""name"") VALUES(1, '100001'), (2, '100002'), (3, '100003'), (4, '100004')
ON CONFLICT(""id"") DO UPDATE SET
""name"" = EXCLUDED.""name""

;

INSERT INTO ""tbiou022""(""name"") VALUES('00001'), ('00002'), ('00003'), ('00004')", sql);
            Assert.Equal(8, iou.ExecuteAffrows());
            lst = fsql.Select<tbiou022>().Where(a => new[] { 1, 2, 3, 4 }.Contains(a.id)).ToList();
            Assert.Equal(4, lst.Where(a => a.name == "10000" + a.id).Count());
        }
        class tbiou022
        {
            [Column(IsIdentity = true)]
            public int id { get; set; }
            public string name { get; set; }
        }

        [Fact]
        public void InsertOrUpdate_TwoPrimary()
        {
            fsql.Delete<tbiou03>().Where("1=1").ExecuteAffrows();
            var iou = fsql.InsertOrUpdate<tbiou03>().SetSource(new tbiou03 { id1 = 1, id2 = "01", name = "01" });
            var sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO ""tbiou03""(""id1"", ""id2"", ""name"") VALUES(1, '01', '01')
ON CONFLICT(""id1"", ""id2"") DO UPDATE SET
""name"" = EXCLUDED.""name""", sql);
            Assert.Equal(1, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbiou03>().SetSource(new tbiou03 { id1 = 1, id2 = "01", name = "011" });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO ""tbiou03""(""id1"", ""id2"", ""name"") VALUES(1, '01', '011')
ON CONFLICT(""id1"", ""id2"") DO UPDATE SET
""name"" = EXCLUDED.""name""", sql);
            Assert.Equal(1, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbiou03>().SetSource(new tbiou03 { id1 = 2, id2 = "02", name = "02" });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO ""tbiou03""(""id1"", ""id2"", ""name"") VALUES(2, '02', '02')
ON CONFLICT(""id1"", ""id2"") DO UPDATE SET
""name"" = EXCLUDED.""name""", sql);
            Assert.Equal(1, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbiou03>().SetSource(new[] { new tbiou03 { id1 = 1, id2 = "01", name = "01" }, new tbiou03 { id1 = 2, id2 = "02", name = "02" }, new tbiou03 { id1 = 3, id2 = "03", name = "03" }, new tbiou03 { id1 = 4, id2 = "04", name = "04" } });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO ""tbiou03""(""id1"", ""id2"", ""name"") VALUES(1, '01', '01'), (2, '02', '02'), (3, '03', '03'), (4, '04', '04')
ON CONFLICT(""id1"", ""id2"") DO UPDATE SET
""name"" = EXCLUDED.""name""", sql);
            Assert.Equal(4, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbiou03>().SetSource(new[] { new tbiou03 { id1 = 1, id2 = "01", name = "001" }, new tbiou03 { id1 = 2, id2 = "02", name = "002" }, new tbiou03 { id1 = 3, id2 = "03", name = "003" }, new tbiou03 { id1 = 4, id2 = "04", name = "004" } });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO ""tbiou03""(""id1"", ""id2"", ""name"") VALUES(1, '01', '001'), (2, '02', '002'), (3, '03', '003'), (4, '04', '004')
ON CONFLICT(""id1"", ""id2"") DO UPDATE SET
""name"" = EXCLUDED.""name""", sql);
            Assert.Equal(4, iou.ExecuteAffrows());
            var lst = fsql.Select<tbiou03>().Where(a => a.id1 == 1 && a.id2 == "01" || a.id1 == 2 && a.id2 == "02" || a.id1 == 3 && a.id2 == "03" || a.id1 == 4 && a.id2 == "04").ToList();
            Assert.Equal(4, lst.Where(a => a.name == "00" + a.id1).Count());
        }
        class tbiou03
        {
            [Column(IsPrimary = true)]
            public int id1 { get; set; }
            [Column(IsPrimary = true)]
            public string id2 { get; set; }
            public string name { get; set; }
        }

        [Fact]
        public void InsertOrUpdate_OnePrimaryAndVersionAndCanUpdate()
        {
            fsql.Delete<tbiou04>().Where("1=1").ExecuteAffrows();
            var iou = fsql.InsertOrUpdate<tbiou04>().SetSource(new tbiou04 { id = 1, name = "01" });
            var sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO ""tbiou04""(""id"", ""name"", ""version"", ""createtime"") VALUES(1, '01', 0, current_timestamp)
ON CONFLICT(""id"") DO UPDATE SET
""name"" = EXCLUDED.""name"", 
""version"" = ""tbiou04"".""version"" + 1", sql);
            Assert.Equal(1, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbiou04>().SetSource(new tbiou04 { id = 1, name = "011" });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO ""tbiou04""(""id"", ""name"", ""version"", ""createtime"") VALUES(1, '011', 0, current_timestamp)
ON CONFLICT(""id"") DO UPDATE SET
""name"" = EXCLUDED.""name"", 
""version"" = ""tbiou04"".""version"" + 1", sql);
            Assert.Equal(1, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbiou04>().SetSource(new tbiou04 { id = 2, name = "02" });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO ""tbiou04""(""id"", ""name"", ""version"", ""createtime"") VALUES(2, '02', 0, current_timestamp)
ON CONFLICT(""id"") DO UPDATE SET
""name"" = EXCLUDED.""name"", 
""version"" = ""tbiou04"".""version"" + 1", sql);
            Assert.Equal(1, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbiou04>().SetSource(new[] { new tbiou04 { id = 1, name = "01" }, new tbiou04 { id = 2, name = "02" }, new tbiou04 { id = 3, name = "03" }, new tbiou04 { id = 4, name = "04" } });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO ""tbiou04""(""id"", ""name"", ""version"", ""createtime"") VALUES(1, '01', 0, current_timestamp), (2, '02', 0, current_timestamp), (3, '03', 0, current_timestamp), (4, '04', 0, current_timestamp)
ON CONFLICT(""id"") DO UPDATE SET
""name"" = EXCLUDED.""name"", 
""version"" = ""tbiou04"".""version"" + 1", sql);
            Assert.Equal(4, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbiou04>().SetSource(new[] { new tbiou04 { id = 1, name = "001" }, new tbiou04 { id = 2, name = "002" }, new tbiou04 { id = 3, name = "003" }, new tbiou04 { id = 4, name = "004" } });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO ""tbiou04""(""id"", ""name"", ""version"", ""createtime"") VALUES(1, '001', 0, current_timestamp), (2, '002', 0, current_timestamp), (3, '003', 0, current_timestamp), (4, '004', 0, current_timestamp)
ON CONFLICT(""id"") DO UPDATE SET
""name"" = EXCLUDED.""name"", 
""version"" = ""tbiou04"".""version"" + 1", sql);
            Assert.Equal(4, iou.ExecuteAffrows());
            var lst = fsql.Select<tbiou04>().Where(a => new[] { 1, 2, 3, 4 }.Contains(a.id)).ToList();
            Assert.Equal(4, lst.Where(a => a.name == "00" + a.id).Count());
        }
        class tbiou04
        {
            public int id { get; set; }
            public string name { get; set; }
            [Column(IsVersion = true)]
            public int version { get; set; }
            [Column(CanUpdate = false, ServerTime = DateTimeKind.Local)]
            public DateTime CreateTime { get; set; }
        }
    }
}
