using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.ShenTong
{
    public class ShenTongInsertOrUpdateIfExistsDoNothingTest
    {
        IFreeSql fsql => g.shentong;

        [Fact]
        public void InsertOrUpdate_OnlyPrimary()
        {
            fsql.Delete<tbioudb01>().Where("1=1").ExecuteAffrows();
            var iou = fsql.InsertOrUpdate<tbioudb01>().IfExistsDoNothing().SetSource(new tbioudb01 { id = 1 });
            var sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDB01"" t1 
USING (SELECT 1 as ID ) t2 ON (t1.""ID"" = t2.ID) 
WHEN NOT MATCHED THEN 
  insert (""ID"") 
  values (t2.ID)", sql);
            Assert.Equal(1, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbioudb01>().IfExistsDoNothing().SetSource(new tbioudb01 { id = 1 });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDB01"" t1 
USING (SELECT 1 as ID ) t2 ON (t1.""ID"" = t2.ID) 
WHEN NOT MATCHED THEN 
  insert (""ID"") 
  values (t2.ID)", sql);
            Assert.Equal(0, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbioudb01>().IfExistsDoNothing().SetSource(new tbioudb01 { id = 2 });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDB01"" t1 
USING (SELECT 2 as ID ) t2 ON (t1.""ID"" = t2.ID) 
WHEN NOT MATCHED THEN 
  insert (""ID"") 
  values (t2.ID)", sql);
            Assert.Equal(1, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbioudb01>().IfExistsDoNothing().SetSource(new[] { new tbioudb01 { id = 1 }, new tbioudb01 { id = 2 }, new tbioudb01 { id = 3 }, new tbioudb01 { id = 4 } });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDB01"" t1 
USING (SELECT 1 as ID 
UNION ALL
 SELECT 2 
UNION ALL
 SELECT 3 
UNION ALL
 SELECT 4 ) t2 ON (t1.""ID"" = t2.ID) 
WHEN NOT MATCHED THEN 
  insert (""ID"") 
  values (t2.ID)", sql);
            Assert.Equal(2, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbioudb01>().IfExistsDoNothing().SetSource(new[] { new tbioudb01 { id = 1 }, new tbioudb01 { id = 2 }, new tbioudb01 { id = 3 }, new tbioudb01 { id = 4 } });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDB01"" t1 
USING (SELECT 1 as ID 
UNION ALL
 SELECT 2 
UNION ALL
 SELECT 3 
UNION ALL
 SELECT 4 ) t2 ON (t1.""ID"" = t2.ID) 
WHEN NOT MATCHED THEN 
  insert (""ID"") 
  values (t2.ID)", sql);
            Assert.Equal(0, iou.ExecuteAffrows());
        }
        class tbioudb01
        {
            public int id { get; set; }
        }

        [Fact]
        public void InsertOrUpdate_OnePrimary()
        {
            fsql.Delete<tbioudb02>().Where("1=1").ExecuteAffrows();
            var iou = fsql.InsertOrUpdate<tbioudb02>().IfExistsDoNothing().SetSource(new tbioudb02 { id = 1, name = "01" });
            var sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDB02"" t1 
USING (SELECT 1 as ID, '01' as NAME ) t2 ON (t1.""ID"" = t2.ID) 
WHEN NOT MATCHED THEN 
  insert (""ID"", ""NAME"") 
  values (t2.ID, t2.NAME)", sql);
            Assert.Equal(1, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbioudb02>().IfExistsDoNothing().SetSource(new tbioudb02 { id = 1, name = "011" });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDB02"" t1 
USING (SELECT 1 as ID, '011' as NAME ) t2 ON (t1.""ID"" = t2.ID) 
WHEN NOT MATCHED THEN 
  insert (""ID"", ""NAME"") 
  values (t2.ID, t2.NAME)", sql);
            Assert.Equal(0, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbioudb02>().IfExistsDoNothing().SetSource(new tbioudb02 { id = 2, name = "02" });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDB02"" t1 
USING (SELECT 2 as ID, '02' as NAME ) t2 ON (t1.""ID"" = t2.ID) 
WHEN NOT MATCHED THEN 
  insert (""ID"", ""NAME"") 
  values (t2.ID, t2.NAME)", sql);
            Assert.Equal(1, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbioudb02>().IfExistsDoNothing().SetSource(new[] { new tbioudb02 { id = 1, name = "01" }, new tbioudb02 { id = 2, name = "02" }, new tbioudb02 { id = 3, name = "03" }, new tbioudb02 { id = 4, name = "04" } });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDB02"" t1 
USING (SELECT 1 as ID, '01' as NAME 
UNION ALL
 SELECT 2, '02' 
UNION ALL
 SELECT 3, '03' 
UNION ALL
 SELECT 4, '04' ) t2 ON (t1.""ID"" = t2.ID) 
WHEN NOT MATCHED THEN 
  insert (""ID"", ""NAME"") 
  values (t2.ID, t2.NAME)", sql);
            Assert.Equal(2, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbioudb02>().IfExistsDoNothing().SetSource(new[] { new tbioudb02 { id = 1, name = "001" }, new tbioudb02 { id = 2, name = "002" }, new tbioudb02 { id = 3, name = "003" }, new tbioudb02 { id = 4, name = "004" } });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDB02"" t1 
USING (SELECT 1 as ID, '001' as NAME 
UNION ALL
 SELECT 2, '002' 
UNION ALL
 SELECT 3, '003' 
UNION ALL
 SELECT 4, '004' ) t2 ON (t1.""ID"" = t2.ID) 
WHEN NOT MATCHED THEN 
  insert (""ID"", ""NAME"") 
  values (t2.ID, t2.NAME)", sql);
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
        public void InsertOrUpdate_TwoPrimary()
        {
            fsql.Delete<tbioudb03>().Where("1=1").ExecuteAffrows();
            var iou = fsql.InsertOrUpdate<tbioudb03>().IfExistsDoNothing().SetSource(new tbioudb03 { id1 = 1, id2 = "01", name = "01" });
            var sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDB03"" t1 
USING (SELECT 1 as ID1, '01' as ID2, '01' as NAME ) t2 ON (t1.""ID1"" = t2.ID1 AND t1.""ID2"" = t2.ID2) 
WHEN NOT MATCHED THEN 
  insert (""ID1"", ""ID2"", ""NAME"") 
  values (t2.ID1, t2.ID2, t2.NAME)", sql);
            Assert.Equal(1, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbioudb03>().IfExistsDoNothing().SetSource(new tbioudb03 { id1 = 1, id2 = "01", name = "011" });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDB03"" t1 
USING (SELECT 1 as ID1, '01' as ID2, '011' as NAME ) t2 ON (t1.""ID1"" = t2.ID1 AND t1.""ID2"" = t2.ID2) 
WHEN NOT MATCHED THEN 
  insert (""ID1"", ""ID2"", ""NAME"") 
  values (t2.ID1, t2.ID2, t2.NAME)", sql);
            Assert.Equal(0, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbioudb03>().IfExistsDoNothing().SetSource(new tbioudb03 { id1 = 2, id2 = "02", name = "02" });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDB03"" t1 
USING (SELECT 2 as ID1, '02' as ID2, '02' as NAME ) t2 ON (t1.""ID1"" = t2.ID1 AND t1.""ID2"" = t2.ID2) 
WHEN NOT MATCHED THEN 
  insert (""ID1"", ""ID2"", ""NAME"") 
  values (t2.ID1, t2.ID2, t2.NAME)", sql);
            Assert.Equal(1, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbioudb03>().IfExistsDoNothing().SetSource(new[] { new tbioudb03 { id1 = 1, id2 = "01", name = "01" }, new tbioudb03 { id1 = 2, id2 = "02", name = "02" }, new tbioudb03 { id1 = 3, id2 = "03", name = "03" }, new tbioudb03 { id1 = 4, id2 = "04", name = "04" } });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDB03"" t1 
USING (SELECT 1 as ID1, '01' as ID2, '01' as NAME 
UNION ALL
 SELECT 2, '02', '02' 
UNION ALL
 SELECT 3, '03', '03' 
UNION ALL
 SELECT 4, '04', '04' ) t2 ON (t1.""ID1"" = t2.ID1 AND t1.""ID2"" = t2.ID2) 
WHEN NOT MATCHED THEN 
  insert (""ID1"", ""ID2"", ""NAME"") 
  values (t2.ID1, t2.ID2, t2.NAME)", sql);
            Assert.Equal(2, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbioudb03>().IfExistsDoNothing().SetSource(new[] { new tbioudb03 { id1 = 1, id2 = "01", name = "001" }, new tbioudb03 { id1 = 2, id2 = "02", name = "002" }, new tbioudb03 { id1 = 3, id2 = "03", name = "003" }, new tbioudb03 { id1 = 4, id2 = "04", name = "004" } });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDB03"" t1 
USING (SELECT 1 as ID1, '01' as ID2, '001' as NAME 
UNION ALL
 SELECT 2, '02', '002' 
UNION ALL
 SELECT 3, '03', '003' 
UNION ALL
 SELECT 4, '04', '004' ) t2 ON (t1.""ID1"" = t2.ID1 AND t1.""ID2"" = t2.ID2) 
WHEN NOT MATCHED THEN 
  insert (""ID1"", ""ID2"", ""NAME"") 
  values (t2.ID1, t2.ID2, t2.NAME)", sql);
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
            Assert.Equal(@"MERGE INTO ""TBIOUDB04"" t1 
USING (SELECT 1 as ID, '01' as NAME, 0 as VERSION, current_timestamp as CREATETIME ) t2 ON (t1.""ID"" = t2.ID) 
WHEN NOT MATCHED THEN 
  insert (""ID"", ""NAME"", ""VERSION"", ""CREATETIME"") 
  values (t2.ID, t2.NAME, t2.VERSION, t2.CREATETIME)", sql);
            Assert.Equal(1, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbioudb04>().IfExistsDoNothing().SetSource(new tbioudb04 { id = 1, name = "011" });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDB04"" t1 
USING (SELECT 1 as ID, '011' as NAME, 0 as VERSION, current_timestamp as CREATETIME ) t2 ON (t1.""ID"" = t2.ID) 
WHEN NOT MATCHED THEN 
  insert (""ID"", ""NAME"", ""VERSION"", ""CREATETIME"") 
  values (t2.ID, t2.NAME, t2.VERSION, t2.CREATETIME)", sql);
            Assert.Equal(0, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbioudb04>().IfExistsDoNothing().SetSource(new tbioudb04 { id = 2, name = "02" });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDB04"" t1 
USING (SELECT 2 as ID, '02' as NAME, 0 as VERSION, current_timestamp as CREATETIME ) t2 ON (t1.""ID"" = t2.ID) 
WHEN NOT MATCHED THEN 
  insert (""ID"", ""NAME"", ""VERSION"", ""CREATETIME"") 
  values (t2.ID, t2.NAME, t2.VERSION, t2.CREATETIME)", sql);
            Assert.Equal(1, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbioudb04>().IfExistsDoNothing().SetSource(new[] { new tbioudb04 { id = 1, name = "01" }, new tbioudb04 { id = 2, name = "02" }, new tbioudb04 { id = 3, name = "03" }, new tbioudb04 { id = 4, name = "04" } });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDB04"" t1 
USING (SELECT 1 as ID, '01' as NAME, 0 as VERSION, current_timestamp as CREATETIME 
UNION ALL
 SELECT 2, '02', 0, current_timestamp 
UNION ALL
 SELECT 3, '03', 0, current_timestamp 
UNION ALL
 SELECT 4, '04', 0, current_timestamp ) t2 ON (t1.""ID"" = t2.ID) 
WHEN NOT MATCHED THEN 
  insert (""ID"", ""NAME"", ""VERSION"", ""CREATETIME"") 
  values (t2.ID, t2.NAME, t2.VERSION, t2.CREATETIME)", sql);
            Assert.Equal(2, iou.ExecuteAffrows());

            iou = fsql.InsertOrUpdate<tbioudb04>().IfExistsDoNothing().SetSource(new[] { new tbioudb04 { id = 1, name = "001" }, new tbioudb04 { id = 2, name = "002" }, new tbioudb04 { id = 3, name = "003" }, new tbioudb04 { id = 4, name = "004" } });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDB04"" t1 
USING (SELECT 1 as ID, '001' as NAME, 0 as VERSION, current_timestamp as CREATETIME 
UNION ALL
 SELECT 2, '002', 0, current_timestamp 
UNION ALL
 SELECT 3, '003', 0, current_timestamp 
UNION ALL
 SELECT 4, '004', 0, current_timestamp ) t2 ON (t1.""ID"" = t2.ID) 
WHEN NOT MATCHED THEN 
  insert (""ID"", ""NAME"", ""VERSION"", ""CREATETIME"") 
  values (t2.ID, t2.NAME, t2.VERSION, t2.CREATETIME)", sql);
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
