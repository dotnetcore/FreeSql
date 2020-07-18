using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.Odbc.Dameng
{
    public class DamengInsertOrUpdateIfExistsNothingTest
    {
        IFreeSql fsql => g.dameng;

        [Fact]
        public void InsertOrUpdate_OnlyPrimary()
        {
            fsql.Delete<tbioudn01>().Where("1=1").ExecuteAffrows();
            var iou = fsql.InsertOrUpdate<tbioudn01>().IfExistsDoNothing().SetSource(new tbioudn01 { id = 1 });
            var sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDN01"" t1 
USING (SELECT 1 as ID FROM dual ) t2 ON (t1.""ID"" = t2.ID) 
WHEN NOT MATCHED THEN 
  insert (""ID"") 
  values (t2.ID)", sql);
            iou.ExecuteAffrows();

            iou = fsql.InsertOrUpdate<tbioudn01>().IfExistsDoNothing().SetSource(new tbioudn01 { id = 1 });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDN01"" t1 
USING (SELECT 1 as ID FROM dual ) t2 ON (t1.""ID"" = t2.ID) 
WHEN NOT MATCHED THEN 
  insert (""ID"") 
  values (t2.ID)", sql);
            iou.ExecuteAffrows();

            iou = fsql.InsertOrUpdate<tbioudn01>().IfExistsDoNothing().SetSource(new tbioudn01 { id = 2 });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDN01"" t1 
USING (SELECT 2 as ID FROM dual ) t2 ON (t1.""ID"" = t2.ID) 
WHEN NOT MATCHED THEN 
  insert (""ID"") 
  values (t2.ID)", sql);
            iou.ExecuteAffrows();

            iou = fsql.InsertOrUpdate<tbioudn01>().IfExistsDoNothing().SetSource(new[] { new tbioudn01 { id = 1 }, new tbioudn01 { id = 2 }, new tbioudn01 { id = 3 }, new tbioudn01 { id = 4 } });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDN01"" t1 
USING (SELECT 1 as ID FROM dual 
UNION ALL
 SELECT 2 FROM dual 
UNION ALL
 SELECT 3 FROM dual 
UNION ALL
 SELECT 4 FROM dual ) t2 ON (t1.""ID"" = t2.ID) 
WHEN NOT MATCHED THEN 
  insert (""ID"") 
  values (t2.ID)", sql);
            iou.ExecuteAffrows();

            iou = fsql.InsertOrUpdate<tbioudn01>().IfExistsDoNothing().SetSource(new[] { new tbioudn01 { id = 1 }, new tbioudn01 { id = 2 }, new tbioudn01 { id = 3 }, new tbioudn01 { id = 4 } });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDN01"" t1 
USING (SELECT 1 as ID FROM dual 
UNION ALL
 SELECT 2 FROM dual 
UNION ALL
 SELECT 3 FROM dual 
UNION ALL
 SELECT 4 FROM dual ) t2 ON (t1.""ID"" = t2.ID) 
WHEN NOT MATCHED THEN 
  insert (""ID"") 
  values (t2.ID)", sql);
            iou.ExecuteAffrows();
        }
        class tbioudn01
        {
            public int id { get; set; }
        }

        [Fact]
        public void InsertOrUpdate_OnePrimary()
        {
            fsql.Delete<tbioudn02>().Where("1=1").ExecuteAffrows();
            var iou = fsql.InsertOrUpdate<tbioudn02>().IfExistsDoNothing().SetSource(new tbioudn02 { id = 1, name = "01" });
            var sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDN02"" t1 
USING (SELECT 1 as ID, '01' as NAME FROM dual ) t2 ON (t1.""ID"" = t2.ID) 
WHEN NOT MATCHED THEN 
  insert (""ID"", ""NAME"") 
  values (t2.ID, t2.NAME)", sql);
            iou.ExecuteAffrows();

            iou = fsql.InsertOrUpdate<tbioudn02>().IfExistsDoNothing().SetSource(new tbioudn02 { id = 1, name = "011" });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDN02"" t1 
USING (SELECT 1 as ID, '011' as NAME FROM dual ) t2 ON (t1.""ID"" = t2.ID) 
WHEN NOT MATCHED THEN 
  insert (""ID"", ""NAME"") 
  values (t2.ID, t2.NAME)", sql);
            iou.ExecuteAffrows();

            iou = fsql.InsertOrUpdate<tbioudn02>().IfExistsDoNothing().SetSource(new tbioudn02 { id = 2, name = "02" });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDN02"" t1 
USING (SELECT 2 as ID, '02' as NAME FROM dual ) t2 ON (t1.""ID"" = t2.ID) 
WHEN NOT MATCHED THEN 
  insert (""ID"", ""NAME"") 
  values (t2.ID, t2.NAME)", sql);
            iou.ExecuteAffrows();

            iou = fsql.InsertOrUpdate<tbioudn02>().IfExistsDoNothing().SetSource(new[] { new tbioudn02 { id = 1, name = "01" }, new tbioudn02 { id = 2, name = "02" }, new tbioudn02 { id = 3, name = "03" }, new tbioudn02 { id = 4, name = "04" } });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDN02"" t1 
USING (SELECT 1 as ID, '01' as NAME FROM dual 
UNION ALL
 SELECT 2, '02' FROM dual 
UNION ALL
 SELECT 3, '03' FROM dual 
UNION ALL
 SELECT 4, '04' FROM dual ) t2 ON (t1.""ID"" = t2.ID) 
WHEN NOT MATCHED THEN 
  insert (""ID"", ""NAME"") 
  values (t2.ID, t2.NAME)", sql);
            iou.ExecuteAffrows();

            iou = fsql.InsertOrUpdate<tbioudn02>().IfExistsDoNothing().SetSource(new[] { new tbioudn02 { id = 1, name = "001" }, new tbioudn02 { id = 2, name = "002" }, new tbioudn02 { id = 3, name = "003" }, new tbioudn02 { id = 4, name = "004" } });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDN02"" t1 
USING (SELECT 1 as ID, '001' as NAME FROM dual 
UNION ALL
 SELECT 2, '002' FROM dual 
UNION ALL
 SELECT 3, '003' FROM dual 
UNION ALL
 SELECT 4, '004' FROM dual ) t2 ON (t1.""ID"" = t2.ID) 
WHEN NOT MATCHED THEN 
  insert (""ID"", ""NAME"") 
  values (t2.ID, t2.NAME)", sql);
            iou.ExecuteAffrows();
            var lst = fsql.Select<tbioudn02>().Where(a => new[] { 1, 2, 3, 4 }.Contains(a.id)).ToList();
            Assert.Equal(4, lst.Where(a => a.name == "0" + a.id).Count());
        }
        class tbioudn02
        {
            public int id { get; set; }
            public string name { get; set; }
        }
        [Fact]
        public void InsertOrUpdate_OnePrimaryAndIdentity()
        {
            fsql.Delete<tbioudn022>().Where("1=1").ExecuteAffrows();
            var iou = fsql.InsertOrUpdate<tbioudn022>().IfExistsDoNothing().SetSource(new tbioudn022 { id = 1, name = "01" });
            var sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDN022"" t1 
USING (SELECT 1 as ID, '01' as NAME FROM dual ) t2 ON (t1.""ID"" = t2.ID) 
WHEN NOT MATCHED THEN 
  insert (""ID"", ""NAME"") 
  values (t2.ID, t2.NAME)", sql);
            iou.ExecuteAffrows();

            iou = fsql.InsertOrUpdate<tbioudn022>().IfExistsDoNothing().SetSource(new tbioudn022 { id = 1, name = "011" });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDN022"" t1 
USING (SELECT 1 as ID, '011' as NAME FROM dual ) t2 ON (t1.""ID"" = t2.ID) 
WHEN NOT MATCHED THEN 
  insert (""ID"", ""NAME"") 
  values (t2.ID, t2.NAME)", sql);
            iou.ExecuteAffrows();

            iou = fsql.InsertOrUpdate<tbioudn022>().IfExistsDoNothing().SetSource(new tbioudn022 { id = 2, name = "02" });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDN022"" t1 
USING (SELECT 2 as ID, '02' as NAME FROM dual ) t2 ON (t1.""ID"" = t2.ID) 
WHEN NOT MATCHED THEN 
  insert (""ID"", ""NAME"") 
  values (t2.ID, t2.NAME)", sql);
            iou.ExecuteAffrows();

            iou = fsql.InsertOrUpdate<tbioudn022>().IfExistsDoNothing().SetSource(new[] { new tbioudn022 { id = 1, name = "01" }, new tbioudn022 { id = 2, name = "02" }, new tbioudn022 { id = 3, name = "03" }, new tbioudn022 { id = 4, name = "04" } });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDN022"" t1 
USING (SELECT 1 as ID, '01' as NAME FROM dual 
UNION ALL
 SELECT 2, '02' FROM dual 
UNION ALL
 SELECT 3, '03' FROM dual 
UNION ALL
 SELECT 4, '04' FROM dual ) t2 ON (t1.""ID"" = t2.ID) 
WHEN NOT MATCHED THEN 
  insert (""ID"", ""NAME"") 
  values (t2.ID, t2.NAME)", sql);
            iou.ExecuteAffrows();

            iou = fsql.InsertOrUpdate<tbioudn022>().IfExistsDoNothing().SetSource(new[] { new tbioudn022 { id = 1, name = "001" }, new tbioudn022 { id = 2, name = "002" }, new tbioudn022 { id = 3, name = "003" }, new tbioudn022 { id = 4, name = "004" } });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDN022"" t1 
USING (SELECT 1 as ID, '001' as NAME FROM dual 
UNION ALL
 SELECT 2, '002' FROM dual 
UNION ALL
 SELECT 3, '003' FROM dual 
UNION ALL
 SELECT 4, '004' FROM dual ) t2 ON (t1.""ID"" = t2.ID) 
WHEN NOT MATCHED THEN 
  insert (""ID"", ""NAME"") 
  values (t2.ID, t2.NAME)", sql);
            iou.ExecuteAffrows();
            var lst = fsql.Select<tbioudn022>().Where(a => new[] { 1, 2, 3, 4 }.Contains(a.id)).ToList();
            //Assert.Equal(4, lst.Where(a => a.name == "00" + a.id).Count());

            //--no primary
            iou = fsql.InsertOrUpdate<tbioudn022>().IfExistsDoNothing().SetSource(new tbioudn022 { name = "01" });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO ""TBIOUDN022""(""NAME"") VALUES('01')", sql);
            iou.ExecuteAffrows();

            iou = fsql.InsertOrUpdate<tbioudn022>().IfExistsDoNothing().SetSource(new tbioudn022 { name = "011" });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO ""TBIOUDN022""(""NAME"") VALUES('011')", sql);
            iou.ExecuteAffrows();

            iou = fsql.InsertOrUpdate<tbioudn022>().IfExistsDoNothing().SetSource(new tbioudn022 { name = "02" });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT INTO ""TBIOUDN022""(""NAME"") VALUES('02')", sql);
            iou.ExecuteAffrows();

            iou = fsql.InsertOrUpdate<tbioudn022>().IfExistsDoNothing().SetSource(new[] { new tbioudn022 { name = "01" }, new tbioudn022 { name = "02" }, new tbioudn022 { name = "03" }, new tbioudn022 { name = "04" } });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT ALL
INTO ""TBIOUDN022""(""NAME"") VALUES('01')
INTO ""TBIOUDN022""(""NAME"") VALUES('02')
INTO ""TBIOUDN022""(""NAME"") VALUES('03')
INTO ""TBIOUDN022""(""NAME"") VALUES('04')
 SELECT 1 FROM DUAL", sql);
            iou.ExecuteAffrows();

            iou = fsql.InsertOrUpdate<tbioudn022>().IfExistsDoNothing().SetSource(new[] { new tbioudn022 { name = "001" }, new tbioudn022 { name = "002" }, new tbioudn022 { name = "003" }, new tbioudn022 { name = "004" } });
            sql = iou.ToSql();
            Assert.Equal(@"INSERT ALL
INTO ""TBIOUDN022""(""NAME"") VALUES('001')
INTO ""TBIOUDN022""(""NAME"") VALUES('002')
INTO ""TBIOUDN022""(""NAME"") VALUES('003')
INTO ""TBIOUDN022""(""NAME"") VALUES('004')
 SELECT 1 FROM DUAL", sql);
            iou.ExecuteAffrows();

            //--no primary and yes
            iou = fsql.InsertOrUpdate<tbioudn022>().IfExistsDoNothing().SetSource(new[] { new tbioudn022 { id = 1, name = "100001" }, new tbioudn022 { name = "00001" }, new tbioudn022 { id = 2, name = "100002" }, new tbioudn022 { name = "00002" }, new tbioudn022 { id = 3, name = "100003" }, new tbioudn022 { name = "00003" }, new tbioudn022 { id = 4, name = "100004" }, new tbioudn022 { name = "00004" } });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDN022"" t1 
USING (SELECT 1 as ID, '100001' as NAME FROM dual 
UNION ALL
 SELECT 2, '100002' FROM dual 
UNION ALL
 SELECT 3, '100003' FROM dual 
UNION ALL
 SELECT 4, '100004' FROM dual ) t2 ON (t1.""ID"" = t2.ID) 
WHEN NOT MATCHED THEN 
  insert (""ID"", ""NAME"") 
  values (t2.ID, t2.NAME)

;

INSERT ALL
INTO ""TBIOUDN022""(""NAME"") VALUES('00001')
INTO ""TBIOUDN022""(""NAME"") VALUES('00002')
INTO ""TBIOUDN022""(""NAME"") VALUES('00003')
INTO ""TBIOUDN022""(""NAME"") VALUES('00004')
 SELECT 1 FROM DUAL", sql);
            iou.ExecuteAffrows();
            lst = fsql.Select<tbioudn022>().Where(a => new[] { 1, 2, 3, 4 }.Contains(a.id)).ToList();
            //Assert.Equal(4, lst.Where(a => a.name == "10000" + a.id).Count());
        }
        class tbioudn022
        {
            [Column(IsIdentity = true)]
            public int id { get; set; }
            public string name { get; set; }
        }

        [Fact]
        public void InsertOrUpdate_TwoPrimary()
        {
            fsql.Delete<tbioudn03>().Where("1=1").ExecuteAffrows();
            var iou = fsql.InsertOrUpdate<tbioudn03>().IfExistsDoNothing().SetSource(new tbioudn03 { id1 = 1, id2 = "01", name = "01" });
            var sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDN03"" t1 
USING (SELECT 1 as ID1, '01' as ID2, '01' as NAME FROM dual ) t2 ON (t1.""ID1"" = t2.ID1 AND t1.""ID2"" = t2.ID2) 
WHEN NOT MATCHED THEN 
  insert (""ID1"", ""ID2"", ""NAME"") 
  values (t2.ID1, t2.ID2, t2.NAME)", sql);
            iou.ExecuteAffrows();

            iou = fsql.InsertOrUpdate<tbioudn03>().IfExistsDoNothing().SetSource(new tbioudn03 { id1 = 1, id2 = "02", name = "011" });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDN03"" t1 
USING (SELECT 1 as ID1, '02' as ID2, '011' as NAME FROM dual ) t2 ON (t1.""ID1"" = t2.ID1 AND t1.""ID2"" = t2.ID2) 
WHEN NOT MATCHED THEN 
  insert (""ID1"", ""ID2"", ""NAME"") 
  values (t2.ID1, t2.ID2, t2.NAME)", sql);
            iou.ExecuteAffrows();

            iou = fsql.InsertOrUpdate<tbioudn03>().IfExistsDoNothing().SetSource(new tbioudn03 { id1 = 2, id2 = "02", name = "02" });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDN03"" t1 
USING (SELECT 2 as ID1, '02' as ID2, '02' as NAME FROM dual ) t2 ON (t1.""ID1"" = t2.ID1 AND t1.""ID2"" = t2.ID2) 
WHEN NOT MATCHED THEN 
  insert (""ID1"", ""ID2"", ""NAME"") 
  values (t2.ID1, t2.ID2, t2.NAME)", sql);
            iou.ExecuteAffrows();

            iou = fsql.InsertOrUpdate<tbioudn03>().IfExistsDoNothing().SetSource(new[] { new tbioudn03 { id1 = 1, id2 = "01", name = "01" }, new tbioudn03 { id1 = 2, id2 = "02", name = "02" }, new tbioudn03 { id1 = 3, id2 = "03", name = "03" }, new tbioudn03 { id1 = 4, id2 = "04", name = "04" } });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDN03"" t1 
USING (SELECT 1 as ID1, '01' as ID2, '01' as NAME FROM dual 
UNION ALL
 SELECT 2, '02', '02' FROM dual 
UNION ALL
 SELECT 3, '03', '03' FROM dual 
UNION ALL
 SELECT 4, '04', '04' FROM dual ) t2 ON (t1.""ID1"" = t2.ID1 AND t1.""ID2"" = t2.ID2) 
WHEN NOT MATCHED THEN 
  insert (""ID1"", ""ID2"", ""NAME"") 
  values (t2.ID1, t2.ID2, t2.NAME)", sql);
            iou.ExecuteAffrows();

            iou = fsql.InsertOrUpdate<tbioudn03>().IfExistsDoNothing().SetSource(new[] { new tbioudn03 { id1 = 1, id2 = "01", name = "001" }, new tbioudn03 { id1 = 2, id2 = "02", name = "002" }, new tbioudn03 { id1 = 3, id2 = "03", name = "003" }, new tbioudn03 { id1 = 4, id2 = "04", name = "004" } });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDN03"" t1 
USING (SELECT 1 as ID1, '01' as ID2, '001' as NAME FROM dual 
UNION ALL
 SELECT 2, '02', '002' FROM dual 
UNION ALL
 SELECT 3, '03', '003' FROM dual 
UNION ALL
 SELECT 4, '04', '004' FROM dual ) t2 ON (t1.""ID1"" = t2.ID1 AND t1.""ID2"" = t2.ID2) 
WHEN NOT MATCHED THEN 
  insert (""ID1"", ""ID2"", ""NAME"") 
  values (t2.ID1, t2.ID2, t2.NAME)", sql);
            iou.ExecuteAffrows();
            var lst = fsql.Select<tbioudn03>().Where(a => a.id1 == 1 && a.id2 == "01" || a.id1 == 2 && a.id2 == "02" || a.id1 == 3 && a.id2 == "03" || a.id1 == 4 && a.id2 == "04").ToList();
            Assert.Equal(4, lst.Where(a => a.name == "0" + a.id1).Count());
        }
        class tbioudn03
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
            fsql.Delete<tbioudn04>().Where("1=1").ExecuteAffrows();
            var iou = fsql.InsertOrUpdate<tbioudn04>().IfExistsDoNothing().SetSource(new tbioudn04 { id = 1, name = "01" });
            var sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDN04"" t1 
USING (SELECT 1 as ID, '01' as NAME, 0 as VERSION, systimestamp as CREATETIME FROM dual ) t2 ON (t1.""ID"" = t2.ID) 
WHEN NOT MATCHED THEN 
  insert (""ID"", ""NAME"", ""VERSION"", ""CREATETIME"") 
  values (t2.ID, t2.NAME, t2.VERSION, t2.CREATETIME)", sql);
            iou.ExecuteAffrows();

            iou = fsql.InsertOrUpdate<tbioudn04>().IfExistsDoNothing().SetSource(new tbioudn04 { id = 1, name = "011" });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDN04"" t1 
USING (SELECT 1 as ID, '011' as NAME, 0 as VERSION, systimestamp as CREATETIME FROM dual ) t2 ON (t1.""ID"" = t2.ID) 
WHEN NOT MATCHED THEN 
  insert (""ID"", ""NAME"", ""VERSION"", ""CREATETIME"") 
  values (t2.ID, t2.NAME, t2.VERSION, t2.CREATETIME)", sql);
            iou.ExecuteAffrows();

            iou = fsql.InsertOrUpdate<tbioudn04>().IfExistsDoNothing().SetSource(new tbioudn04 { id = 2, name = "02" });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDN04"" t1 
USING (SELECT 2 as ID, '02' as NAME, 0 as VERSION, systimestamp as CREATETIME FROM dual ) t2 ON (t1.""ID"" = t2.ID) 
WHEN NOT MATCHED THEN 
  insert (""ID"", ""NAME"", ""VERSION"", ""CREATETIME"") 
  values (t2.ID, t2.NAME, t2.VERSION, t2.CREATETIME)", sql);
            iou.ExecuteAffrows();

            iou = fsql.InsertOrUpdate<tbioudn04>().IfExistsDoNothing().SetSource(new[] { new tbioudn04 { id = 1, name = "01" }, new tbioudn04 { id = 2, name = "02" }, new tbioudn04 { id = 3, name = "03" }, new tbioudn04 { id = 4, name = "04" } });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDN04"" t1 
USING (SELECT 1 as ID, '01' as NAME, 0 as VERSION, systimestamp as CREATETIME FROM dual 
UNION ALL
 SELECT 2, '02', 0, systimestamp FROM dual 
UNION ALL
 SELECT 3, '03', 0, systimestamp FROM dual 
UNION ALL
 SELECT 4, '04', 0, systimestamp FROM dual ) t2 ON (t1.""ID"" = t2.ID) 
WHEN NOT MATCHED THEN 
  insert (""ID"", ""NAME"", ""VERSION"", ""CREATETIME"") 
  values (t2.ID, t2.NAME, t2.VERSION, t2.CREATETIME)", sql);
            iou.ExecuteAffrows();

            iou = fsql.InsertOrUpdate<tbioudn04>().IfExistsDoNothing().SetSource(new[] { new tbioudn04 { id = 1, name = "001" }, new tbioudn04 { id = 2, name = "002" }, new tbioudn04 { id = 3, name = "003" }, new tbioudn04 { id = 4, name = "004" } });
            sql = iou.ToSql();
            Assert.Equal(@"MERGE INTO ""TBIOUDN04"" t1 
USING (SELECT 1 as ID, '001' as NAME, 0 as VERSION, systimestamp as CREATETIME FROM dual 
UNION ALL
 SELECT 2, '002', 0, systimestamp FROM dual 
UNION ALL
 SELECT 3, '003', 0, systimestamp FROM dual 
UNION ALL
 SELECT 4, '004', 0, systimestamp FROM dual ) t2 ON (t1.""ID"" = t2.ID) 
WHEN NOT MATCHED THEN 
  insert (""ID"", ""NAME"", ""VERSION"", ""CREATETIME"") 
  values (t2.ID, t2.NAME, t2.VERSION, t2.CREATETIME)", sql);
            iou.ExecuteAffrows();
            var lst = fsql.Select<tbioudn04>().Where(a => new[] { 1, 2, 3, 4 }.Contains(a.id)).ToList();
            Assert.Equal(4, lst.Where(a => a.name == "0" + a.id).Count());
        }
        class tbioudn04
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
