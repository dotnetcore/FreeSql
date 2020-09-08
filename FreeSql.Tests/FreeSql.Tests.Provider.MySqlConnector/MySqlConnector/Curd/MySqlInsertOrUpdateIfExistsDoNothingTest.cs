using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.MySqlConnector
{
    public class MySqlInsertOrUpdateIfExistsDoNothingTest
    {

        IFreeSql fsql => g.mysql;

        [Fact]
        public void InsertOrUpdate_OnePrimary()
        {
            fsql.Delete<tbioudb02>().Where("1=1").ExecuteAffrows();
            var iou = fsql.InsertOrUpdate<tbioudb02>().IfExistsDoNothing().SetSource(new tbioudb02 { id = 1, name = "01" });
            var sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO `tbioudb02`(`id`, `name`) SELECT 1, '01' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb02` a 
    WHERE (a.`id` = 1) 
    limit 0,1)", sql);
            Assert.Equal(1, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbioudb02>().IfExistsDoNothing().SetSource(new tbioudb02 { id = 1, name = "011" });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO `tbioudb02`(`id`, `name`) SELECT 1, '011' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb02` a 
    WHERE (a.`id` = 1) 
    limit 0,1)", sql);
            Assert.Equal(0, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbioudb02>().IfExistsDoNothing().SetSource(new tbioudb02 { id = 2, name = "02" });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO `tbioudb02`(`id`, `name`) SELECT 2, '02' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb02` a 
    WHERE (a.`id` = 2) 
    limit 0,1)", sql);
            Assert.Equal(1, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbioudb02>().IfExistsDoNothing().SetSource(new[] { new tbioudb02 { id = 1, name = "01" }, new tbioudb02 { id = 2, name = "02" }, new tbioudb02 { id = 3, name = "03" }, new tbioudb02 { id = 4, name = "04" } });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO `tbioudb02`(`id`, `name`) SELECT 1, '01' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb02` a 
    WHERE (a.`id` = 1) 
    limit 0,1) 
UNION ALL
 SELECT 2, '02' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb02` a 
    WHERE (a.`id` = 2) 
    limit 0,1) 
UNION ALL
 SELECT 3, '03' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb02` a 
    WHERE (a.`id` = 3) 
    limit 0,1) 
UNION ALL
 SELECT 4, '04' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb02` a 
    WHERE (a.`id` = 4) 
    limit 0,1)", sql);
            Assert.Equal(2, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbioudb02>().IfExistsDoNothing().SetSource(new[] { new tbioudb02 { id = 1, name = "001" }, new tbioudb02 { id = 2, name = "002" }, new tbioudb02 { id = 3, name = "003" }, new tbioudb02 { id = 4, name = "004" } });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO `tbioudb02`(`id`, `name`) SELECT 1, '001' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb02` a 
    WHERE (a.`id` = 1) 
    limit 0,1) 
UNION ALL
 SELECT 2, '002' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb02` a 
    WHERE (a.`id` = 2) 
    limit 0,1) 
UNION ALL
 SELECT 3, '003' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb02` a 
    WHERE (a.`id` = 3) 
    limit 0,1) 
UNION ALL
 SELECT 4, '004' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb02` a 
    WHERE (a.`id` = 4) 
    limit 0,1)", sql);
            Assert.Equal(0, iou.ExecuteAffrows());
            var lst = fsql.Select<tbioudb02>().Where(a => new[] { 1, 2, 3, 4 }.Contains(a.id)).ToList();
            Assert.Equal(4, lst.Where(a => a.name == "0" + a.id).Count());
        }
        class tbioudb02
        {
            public int id { get; set; }
            public string name { get; set; }
        }
        [Fact]
        public void InsertOrUpdate_OnePrimaryAndIdentity()
        {
            fsql.Delete<tbioudb022>().Where("1=1").ExecuteAffrows();
            var iou = fsql.InsertOrUpdate<tbioudb022>().IfExistsDoNothing().SetSource(new tbioudb022 { id = 1, name = "01" });
            var sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO `tbioudb022`(`id`, `name`) SELECT 1, '01' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb022` a 
    WHERE (a.`id` = 1) 
    limit 0,1)", sql);
            Assert.Equal(1, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbioudb022>().IfExistsDoNothing().SetSource(new tbioudb022 { id = 1, name = "011" });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO `tbioudb022`(`id`, `name`) SELECT 1, '011' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb022` a 
    WHERE (a.`id` = 1) 
    limit 0,1)", sql);
            Assert.Equal(0, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbioudb022>().IfExistsDoNothing().SetSource(new tbioudb022 { id = 2, name = "02" });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO `tbioudb022`(`id`, `name`) SELECT 2, '02' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb022` a 
    WHERE (a.`id` = 2) 
    limit 0,1)", sql);
            Assert.Equal(1, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbioudb022>().IfExistsDoNothing().SetSource(new[] { new tbioudb022 { id = 1, name = "01" }, new tbioudb022 { id = 2, name = "02" }, new tbioudb022 { id = 3, name = "03" }, new tbioudb022 { id = 4, name = "04" } });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO `tbioudb022`(`id`, `name`) SELECT 1, '01' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb022` a 
    WHERE (a.`id` = 1) 
    limit 0,1) 
UNION ALL
 SELECT 2, '02' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb022` a 
    WHERE (a.`id` = 2) 
    limit 0,1) 
UNION ALL
 SELECT 3, '03' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb022` a 
    WHERE (a.`id` = 3) 
    limit 0,1) 
UNION ALL
 SELECT 4, '04' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb022` a 
    WHERE (a.`id` = 4) 
    limit 0,1)", sql);
            Assert.Equal(2, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbioudb022>().IfExistsDoNothing().SetSource(new[] { new tbioudb022 { id = 1, name = "001" }, new tbioudb022 { id = 2, name = "002" }, new tbioudb022 { id = 3, name = "003" }, new tbioudb022 { id = 4, name = "004" } });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO `tbioudb022`(`id`, `name`) SELECT 1, '001' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb022` a 
    WHERE (a.`id` = 1) 
    limit 0,1) 
UNION ALL
 SELECT 2, '002' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb022` a 
    WHERE (a.`id` = 2) 
    limit 0,1) 
UNION ALL
 SELECT 3, '003' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb022` a 
    WHERE (a.`id` = 3) 
    limit 0,1) 
UNION ALL
 SELECT 4, '004' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb022` a 
    WHERE (a.`id` = 4) 
    limit 0,1)", sql);
            Assert.Equal(0, iou.ExecuteAffrows());
            var lst = fsql.Select<tbioudb022>().Where(a => new[] { 1, 2, 3, 4 }.Contains(a.id)).ToList();
            Assert.Equal(4, lst.Where(a => a.name == "0" + a.id).Count());

            //--no primary
            iou = fsql.InsertOrUpdate<tbioudb022>().IfExistsDoNothing().SetSource(new tbioudb022 { name = "01" });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO `tbioudb022`(`name`) VALUES('01')", sql);
            Assert.Equal(1, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbioudb022>().IfExistsDoNothing().SetSource(new tbioudb022 { name = "011" });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO `tbioudb022`(`name`) VALUES('011')", sql);
            Assert.Equal(1, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbioudb022>().IfExistsDoNothing().SetSource(new tbioudb022 { name = "02" });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO `tbioudb022`(`name`) VALUES('02')", sql);
            Assert.Equal(1, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbioudb022>().IfExistsDoNothing().SetSource(new[] { new tbioudb022 { name = "01" }, new tbioudb022 { name = "02" }, new tbioudb022 { name = "03" }, new tbioudb022 { name = "04" } });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO `tbioudb022`(`name`) VALUES('01'), ('02'), ('03'), ('04')", sql);
            Assert.Equal(4, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbioudb022>().IfExistsDoNothing().SetSource(new[] { new tbioudb022 { name = "001" }, new tbioudb022 { name = "002" }, new tbioudb022 { name = "003" }, new tbioudb022 { name = "004" } });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO `tbioudb022`(`name`) VALUES('001'), ('002'), ('003'), ('004')", sql);
            Assert.Equal(4, iou.ExecuteAffrows());

            //--no primary and yes
            iou = fsql.InsertOrUpdate<tbioudb022>().IfExistsDoNothing().SetSource(new[] { new tbioudb022 { id = 1, name = "100001" }, new tbioudb022 { name = "00001" }, new tbioudb022 { id = 2, name = "100002" }, new tbioudb022 { name = "00002" }, new tbioudb022 { id = 3, name = "100003" }, new tbioudb022 { name = "00003" }, new tbioudb022 { id = 4, name = "100004" }, new tbioudb022 { name = "00004" } });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO `tbioudb022`(`id`, `name`) SELECT 1, '100001' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb022` a 
    WHERE (a.`id` = 1) 
    limit 0,1) 
UNION ALL
 SELECT 2, '100002' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb022` a 
    WHERE (a.`id` = 2) 
    limit 0,1) 
UNION ALL
 SELECT 3, '100003' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb022` a 
    WHERE (a.`id` = 3) 
    limit 0,1) 
UNION ALL
 SELECT 4, '100004' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb022` a 
    WHERE (a.`id` = 4) 
    limit 0,1)

;

INSERT INTO `tbioudb022`(`name`) VALUES('00001'), ('00002'), ('00003'), ('00004')", sql);
            Assert.Equal(4, iou.ExecuteAffrows());
            lst = fsql.Select<tbioudb022>().Where(a => new[] { 1, 2, 3, 4 }.Contains(a.id)).ToList();
            Assert.Equal(4, lst.Where(a => a.name == "0" + a.id).Count());
        }
        class tbioudb022
        {
            [Column(IsIdentity = true)]
            public int id { get; set; }
            public string name { get; set; }
        }

        [Fact]
        public void InsertOrUpdate_TwoPrimary()
        {
            fsql.Delete<tbioudb03>().Where("1=1").ExecuteAffrows();
            var iou = fsql.InsertOrUpdate<tbioudb03>().IfExistsDoNothing().SetSource(new tbioudb03 { id1 = 1, id2 = "01", name = "01" });
            var sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO `tbioudb03`(`id1`, `id2`, `name`) SELECT 1, '01', '01' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb03` a 
    WHERE (a.`id1` = 1 AND a.`id2` = '01') 
    limit 0,1)", sql);
            Assert.Equal(1, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbioudb03>().IfExistsDoNothing().SetSource(new tbioudb03 { id1 = 1, id2 = "01", name = "011" });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO `tbioudb03`(`id1`, `id2`, `name`) SELECT 1, '01', '011' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb03` a 
    WHERE (a.`id1` = 1 AND a.`id2` = '01') 
    limit 0,1)", sql);
            Assert.Equal(0, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbioudb03>().IfExistsDoNothing().SetSource(new tbioudb03 { id1 = 2, id2 = "02", name = "02" });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO `tbioudb03`(`id1`, `id2`, `name`) SELECT 2, '02', '02' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb03` a 
    WHERE (a.`id1` = 2 AND a.`id2` = '02') 
    limit 0,1)", sql);
            Assert.Equal(1, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbioudb03>().IfExistsDoNothing().SetSource(new[] { new tbioudb03 { id1 = 1, id2 = "01", name = "01" }, new tbioudb03 { id1 = 2, id2 = "02", name = "02" }, new tbioudb03 { id1 = 3, id2 = "03", name = "03" }, new tbioudb03 { id1 = 4, id2 = "04", name = "04" } });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO `tbioudb03`(`id1`, `id2`, `name`) SELECT 1, '01', '01' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb03` a 
    WHERE (a.`id1` = 1 AND a.`id2` = '01') 
    limit 0,1) 
UNION ALL
 SELECT 2, '02', '02' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb03` a 
    WHERE (a.`id1` = 2 AND a.`id2` = '02') 
    limit 0,1) 
UNION ALL
 SELECT 3, '03', '03' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb03` a 
    WHERE (a.`id1` = 3 AND a.`id2` = '03') 
    limit 0,1) 
UNION ALL
 SELECT 4, '04', '04' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb03` a 
    WHERE (a.`id1` = 4 AND a.`id2` = '04') 
    limit 0,1)", sql);
            Assert.Equal(2, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbioudb03>().IfExistsDoNothing().SetSource(new[] { new tbioudb03 { id1 = 1, id2 = "01", name = "001" }, new tbioudb03 { id1 = 2, id2 = "02", name = "002" }, new tbioudb03 { id1 = 3, id2 = "03", name = "003" }, new tbioudb03 { id1 = 4, id2 = "04", name = "004" } });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO `tbioudb03`(`id1`, `id2`, `name`) SELECT 1, '01', '001' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb03` a 
    WHERE (a.`id1` = 1 AND a.`id2` = '01') 
    limit 0,1) 
UNION ALL
 SELECT 2, '02', '002' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb03` a 
    WHERE (a.`id1` = 2 AND a.`id2` = '02') 
    limit 0,1) 
UNION ALL
 SELECT 3, '03', '003' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb03` a 
    WHERE (a.`id1` = 3 AND a.`id2` = '03') 
    limit 0,1) 
UNION ALL
 SELECT 4, '04', '004' 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb03` a 
    WHERE (a.`id1` = 4 AND a.`id2` = '04') 
    limit 0,1)", sql);
            Assert.Equal(0, iou.ExecuteAffrows());
            var lst = fsql.Select<tbioudb03>().Where(a => a.id1 == 1 && a.id2 == "01" || a.id1 == 2 && a.id2 == "02" || a.id1 == 3 && a.id2 == "03" || a.id1 == 4 && a.id2 == "04").ToList();
            Assert.Equal(4, lst.Where(a => a.name == "0" + a.id1).Count());
        }
        class tbioudb03
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
            fsql.Delete<tbioudb04>().Where("1=1").ExecuteAffrows();
            var iou = fsql.InsertOrUpdate<tbioudb04>().IfExistsDoNothing().SetSource(new tbioudb04 { id = 1, name = "01" });
            var sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO `tbioudb04`(`id`, `name`, `version`, `CreateTime`) SELECT 1, '01', 0, now(3) 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb04` a 
    WHERE (a.`id` = 1) 
    limit 0,1)", sql);
            Assert.Equal(1, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbioudb04>().IfExistsDoNothing().SetSource(new tbioudb04 { id = 1, name = "011" });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO `tbioudb04`(`id`, `name`, `version`, `CreateTime`) SELECT 1, '011', 0, now(3) 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb04` a 
    WHERE (a.`id` = 1) 
    limit 0,1)", sql);
            Assert.Equal(0, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbioudb04>().IfExistsDoNothing().SetSource(new tbioudb04 { id = 2, name = "02" });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO `tbioudb04`(`id`, `name`, `version`, `CreateTime`) SELECT 2, '02', 0, now(3) 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb04` a 
    WHERE (a.`id` = 2) 
    limit 0,1)", sql);
            Assert.Equal(1, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbioudb04>().IfExistsDoNothing().SetSource(new[] { new tbioudb04 { id = 1, name = "01" }, new tbioudb04 { id = 2, name = "02" }, new tbioudb04 { id = 3, name = "03" }, new tbioudb04 { id = 4, name = "04" } });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO `tbioudb04`(`id`, `name`, `version`, `CreateTime`) SELECT 1, '01', 0, now(3) 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb04` a 
    WHERE (a.`id` = 1) 
    limit 0,1) 
UNION ALL
 SELECT 2, '02', 0, now(3) 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb04` a 
    WHERE (a.`id` = 2) 
    limit 0,1) 
UNION ALL
 SELECT 3, '03', 0, now(3) 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb04` a 
    WHERE (a.`id` = 3) 
    limit 0,1) 
UNION ALL
 SELECT 4, '04', 0, now(3) 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb04` a 
    WHERE (a.`id` = 4) 
    limit 0,1)", sql);
            Assert.Equal(2, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbioudb04>().IfExistsDoNothing().SetSource(new[] { new tbioudb04 { id = 1, name = "001" }, new tbioudb04 { id = 2, name = "002" }, new tbioudb04 { id = 3, name = "003" }, new tbioudb04 { id = 4, name = "004" } });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO `tbioudb04`(`id`, `name`, `version`, `CreateTime`) SELECT 1, '001', 0, now(3) 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb04` a 
    WHERE (a.`id` = 1) 
    limit 0,1) 
UNION ALL
 SELECT 2, '002', 0, now(3) 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb04` a 
    WHERE (a.`id` = 2) 
    limit 0,1) 
UNION ALL
 SELECT 3, '003', 0, now(3) 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb04` a 
    WHERE (a.`id` = 3) 
    limit 0,1) 
UNION ALL
 SELECT 4, '004', 0, now(3) 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `tbioudb04` a 
    WHERE (a.`id` = 4) 
    limit 0,1)", sql);
            Assert.Equal(0, iou.ExecuteAffrows());
            var lst = fsql.Select<tbioudb04>().Where(a => new[] { 1, 2, 3, 4 }.Contains(a.id)).ToList();
            Assert.Equal(4, lst.Where(a => a.name == "0" + a.id).Count());
        }
        class tbioudb04
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
