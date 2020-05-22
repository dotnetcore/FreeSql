using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.Odbc.MySql
{
    public class MySqlUpdateTest
    {
        IUpdate<Topic> update => g.mysql.Update<Topic>();

        [Table(Name = "tb_topic")]
        class Topic
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }
            public int? Clicks { get; set; }
            public string Title { get; set; }
            public DateTime CreateTime { get; set; }
        }
        class TestEnumUpdateTb
        {
            [Column(IsIdentity = true)]
            public int id { get; set; }
            public TestEnumUpdateTbType type { get; set; }
            public DateTime time { get; set; } = new DateTime();
        }
        enum TestEnumUpdateTbType { str1, biggit, sum211 }

        [Fact]
        public void Dywhere()
        {
            Assert.Null(g.mysql.Update<Topic>().ToSql());
            Assert.Equal("UPDATE `tb_topic` SET title='test' \r\nWHERE (`Id` = 1 OR `Id` = 2)", g.mysql.Update<Topic>(new[] { 1, 2 }).SetRaw("title='test'").ToSql());
            Assert.Equal("UPDATE `tb_topic` SET title='test1' \r\nWHERE (`Id` = 1)", g.mysql.Update<Topic>(new Topic { Id = 1, Title = "test" }).SetRaw("title='test1'").ToSql());
            Assert.Equal("UPDATE `tb_topic` SET title='test1' \r\nWHERE (`Id` = 1 OR `Id` = 2)", g.mysql.Update<Topic>(new[] { new Topic { Id = 1, Title = "test" }, new Topic { Id = 2, Title = "test" } }).SetRaw("title='test1'").ToSql());
            Assert.Equal("UPDATE `tb_topic` SET title='test1' \r\nWHERE (`Id` = 1)", g.mysql.Update<Topic>(new { id = 1 }).SetRaw("title='test1'").ToSql());
        }

        [Fact]
        public void SetSource()
        {
            var sql = update.SetSource(new Topic { Id = 1, Title = "newtitle" }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET `Clicks` = NULL, `Title` = 'newtitle', `CreateTime` = '0001-01-01 00:00:00.000' WHERE (`Id` = 1)", sql);

            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });
            items[0].Clicks = null;

            sql = update.SetSource(items).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET `Clicks` = CASE `Id` WHEN 1 THEN NULL WHEN 2 THEN 100 WHEN 3 THEN 200 WHEN 4 THEN 300 WHEN 5 THEN 400 WHEN 6 THEN 500 WHEN 7 THEN 600 WHEN 8 THEN 700 WHEN 9 THEN 800 WHEN 10 THEN 900 END, `Title` = CASE `Id` WHEN 1 THEN 'newtitle0' WHEN 2 THEN 'newtitle1' WHEN 3 THEN 'newtitle2' WHEN 4 THEN 'newtitle3' WHEN 5 THEN 'newtitle4' WHEN 6 THEN 'newtitle5' WHEN 7 THEN 'newtitle6' WHEN 8 THEN 'newtitle7' WHEN 9 THEN 'newtitle8' WHEN 10 THEN 'newtitle9' END, `CreateTime` = CASE `Id` WHEN 1 THEN '0001-01-01 00:00:00.000' WHEN 2 THEN '0001-01-01 00:00:00.000' WHEN 3 THEN '0001-01-01 00:00:00.000' WHEN 4 THEN '0001-01-01 00:00:00.000' WHEN 5 THEN '0001-01-01 00:00:00.000' WHEN 6 THEN '0001-01-01 00:00:00.000' WHEN 7 THEN '0001-01-01 00:00:00.000' WHEN 8 THEN '0001-01-01 00:00:00.000' WHEN 9 THEN '0001-01-01 00:00:00.000' WHEN 10 THEN '0001-01-01 00:00:00.000' END WHERE (`Id` IN (1,2,3,4,5,6,7,8,9,10))", sql);

            sql = update.SetSource(items).IgnoreColumns(a => new { a.Clicks, a.CreateTime }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET `Title` = CASE `Id` WHEN 1 THEN 'newtitle0' WHEN 2 THEN 'newtitle1' WHEN 3 THEN 'newtitle2' WHEN 4 THEN 'newtitle3' WHEN 5 THEN 'newtitle4' WHEN 6 THEN 'newtitle5' WHEN 7 THEN 'newtitle6' WHEN 8 THEN 'newtitle7' WHEN 9 THEN 'newtitle8' WHEN 10 THEN 'newtitle9' END WHERE (`Id` IN (1,2,3,4,5,6,7,8,9,10))", sql);

            sql = update.SetSource(items).Set(a => a.CreateTime, new DateTime(2020, 1, 1)).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET `CreateTime` = '2020-01-01 00:00:00.000' WHERE (`Id` IN (1,2,3,4,5,6,7,8,9,10))", sql);

            sql = g.mysql.Insert<TestEnumUpdateTb>().AppendData(new TestEnumUpdateTb { type = TestEnumUpdateTbType.sum211 }).ToSql().Replace("\r\n", "");
            Assert.Equal("INSERT INTO `TestEnumUpdateTb`(`type`, `time`) VALUES('sum211', '0001-01-01 00:00:00.000')", sql);
            var id = g.mysql.Insert<TestEnumUpdateTb>().AppendData(new TestEnumUpdateTb { type = TestEnumUpdateTbType.sum211 }).ExecuteIdentity();
            Assert.True(id > 0);
            Assert.Equal(TestEnumUpdateTbType.sum211, g.mysql.Select<TestEnumUpdateTb>().Where(a => a.id == id).First()?.type);

            sql = g.mysql.Update<TestEnumUpdateTb>().SetSource(new TestEnumUpdateTb { type = TestEnumUpdateTbType.sum211 }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `TestEnumUpdateTb` SET `type` = 'sum211', `time` = '0001-01-01 00:00:00.000' WHERE (`id` = 0)", sql);
            g.mysql.Update<TestEnumUpdateTb>().SetSource(new TestEnumUpdateTb { id = (int)id, type = TestEnumUpdateTbType.biggit }).ExecuteAffrows();
            Assert.Equal(TestEnumUpdateTbType.biggit, g.mysql.Select<TestEnumUpdateTb>().Where(a => a.id == id).First()?.type);

            sql = g.mysql.Insert<TestEnumUpdateTb>().NoneParameter().AppendData(new TestEnumUpdateTb { type = TestEnumUpdateTbType.sum211 }).ToSql().Replace("\r\n", "");
            Assert.Equal("INSERT INTO `TestEnumUpdateTb`(`type`, `time`) VALUES('sum211', '0001-01-01 00:00:00.000')", sql);
            id = g.mysql.Insert<TestEnumUpdateTb>().NoneParameter().AppendData(new TestEnumUpdateTb { type = TestEnumUpdateTbType.sum211 }).ExecuteIdentity();
            Assert.True(id > 0);
            Assert.Equal(TestEnumUpdateTbType.sum211, g.mysql.Select<TestEnumUpdateTb>().Where(a => a.id == id).First()?.type);

            sql = g.mysql.Update<TestEnumUpdateTb>().NoneParameter().SetSource(new TestEnumUpdateTb { type = TestEnumUpdateTbType.sum211 }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `TestEnumUpdateTb` SET `type` = 'sum211', `time` = '0001-01-01 00:00:00.000' WHERE (`id` = 0)", sql);
            g.mysql.Update<TestEnumUpdateTb>().NoneParameter().SetSource(new TestEnumUpdateTb { id = (int)id, type = TestEnumUpdateTbType.biggit }).ExecuteAffrows();
            Assert.Equal(TestEnumUpdateTbType.biggit, g.mysql.Select<TestEnumUpdateTb>().Where(a => a.id == id).First()?.type);

            sql = g.mysql.Update<ts_source_mpk>().SetSource(new[] {
                new ts_source_mpk { id1 = 1, id2 = 7, xx = "a1" },
                new ts_source_mpk { id1 = 1, id2 = 8, xx = "b122" }
            }).NoneParameter().ToSql().Replace("\r\n", "");
        }
        public class ts_source_mpk
        {
            [Column(IsPrimary = true)]
            public int id1 { get; set; }
            [Column(IsPrimary = true)]
            public int id2 { get; set; }
            public string xx { get; set; }
        }
        [Fact]
        public void SetSourceIgnore()
        {
            Assert.Equal("UPDATE `tssi01` SET `tint` = 10 WHERE (`id` = '00000000-0000-0000-0000-000000000000')",
                g.mysql.Update<tssi01>().NoneParameter()
                    .SetSourceIgnore(new tssi01 { id = Guid.Empty, tint = 10 }, col => col == null).ToSql().Replace("\r\n", ""));
        }
        public class tssi01
        {
            [Column(CanUpdate = false)]
            public Guid id { get; set; }
            public int tint { get; set; }
            public string title { get; set; }
        }
        [Fact]
        public void IgnoreColumns()
        {
            var sql = update.SetSource(new Topic { Id = 1, Title = "newtitle" }).IgnoreColumns(a => new { a.Clicks, a.CreateTime }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET `Title` = 'newtitle' WHERE (`Id` = 1)", sql);

            sql = g.mysql.Update<TestEnumUpdateTb>().SetSource(new TestEnumUpdateTb { type = TestEnumUpdateTbType.sum211 }).IgnoreColumns(a => a.time).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `TestEnumUpdateTb` SET `type` = 'sum211' WHERE (`id` = 0)", sql);

            sql = g.mysql.Update<TestEnumUpdateTb>().NoneParameter().SetSource(new TestEnumUpdateTb { type = TestEnumUpdateTbType.sum211 }).IgnoreColumns(a => a.time).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `TestEnumUpdateTb` SET `type` = 'sum211' WHERE (`id` = 0)", sql);
        }
        [Fact]
        public void UpdateColumns()
        {
            var sql = update.SetSource(new Topic { Id = 1, Title = "newtitle" }).UpdateColumns(a => a.Title).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET `Title` = 'newtitle' WHERE (`Id` = 1)", sql);

            sql = g.mysql.Update<TestEnumUpdateTb>().SetSource(new TestEnumUpdateTb { type = TestEnumUpdateTbType.sum211 }).UpdateColumns(a => a.type).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `TestEnumUpdateTb` SET `type` = 'sum211' WHERE (`id` = 0)", sql);

            sql = g.mysql.Update<TestEnumUpdateTb>().NoneParameter().SetSource(new TestEnumUpdateTb { type = TestEnumUpdateTbType.sum211 }).UpdateColumns(a => a.type).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `TestEnumUpdateTb` SET `type` = 'sum211' WHERE (`id` = 0)", sql);
        }
        [Fact]
        public void Set()
        {
            var sql = update.Where(a => a.Id == 1).Set(a => a.Title, "newtitle").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET `Title` = 'newtitle' WHERE (`Id` = 1)", sql);

            sql = update.Where(a => a.Id == 1).Set(a => a.Title, "newtitle").Set(a => a.CreateTime, new DateTime(2020, 1, 1)).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET `Title` = 'newtitle', `CreateTime` = '2020-01-01 00:00:00.000' WHERE (`Id` = 1)", sql);

            sql = update.Set(a => a.Clicks * 10 / 1).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET `Clicks` = ifnull(`Clicks`, 0) * 10 div 1 WHERE (`Id` = 1)", sql);

            sql = update.Set(a => a.Id - 10).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET `Id` = (`Id` - 10) WHERE (`Id` = 1)", sql);

            int incrv = 10;
            sql = update.Set(a => a.Clicks * incrv / 1).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET `Clicks` = ifnull(`Clicks`, 0) * 10 div 1 WHERE (`Id` = 1)", sql);

            sql = update.Set(a => a.Id - incrv).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET `Id` = (`Id` - 10) WHERE (`Id` = 1)", sql);

            sql = update.Set(a => a.Clicks == a.Clicks * 10 / 1).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET `Clicks` = `Clicks` * 10 div 1 WHERE (`Id` = 1)", sql);

            var dt2000 = DateTime.Parse("2000-01-01");
            sql = update.Set(a => a.Clicks == (a.CreateTime > dt2000 ? 1 : 2)).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET `Clicks` = case when `CreateTime` > '2000-01-01 00:00:00.000' then 1 else 2 end WHERE (`Id` = 1)", sql);

            sql = update.Set(a => a.Id == 10).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET `Id` = 10 WHERE (`Id` = 1)", sql);

            sql = update.Set(a => a.Clicks == null).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET `Clicks` = NULL WHERE (`Id` = 1)", sql);

            var id = g.mysql.Insert<TestEnumUpdateTb>().AppendData(new TestEnumUpdateTb { type = TestEnumUpdateTbType.sum211 }).ExecuteIdentity();
            Assert.True(id > 0);
            sql = g.mysql.Update<TestEnumUpdateTb>().Where(a => a.id == id).Set(a => a.type, TestEnumUpdateTbType.biggit).ToSql().Replace("\r\n", "");
            Assert.Equal($"UPDATE `TestEnumUpdateTb` SET `type` = 'biggit' WHERE (`id` = {id})", sql);
            g.mysql.Update<TestEnumUpdateTb>().Where(a => a.id == id).Set(a => a.type, TestEnumUpdateTbType.biggit).ExecuteAffrows();
            Assert.Equal(TestEnumUpdateTbType.biggit, g.mysql.Select<TestEnumUpdateTb>().Where(a => a.id == id).First()?.type);

            sql = g.mysql.Update<TestEnumUpdateTb>().NoneParameter().Where(a => a.id == id).Set(a => a.type, TestEnumUpdateTbType.str1).ToSql().Replace("\r\n", "");
            Assert.Equal($"UPDATE `TestEnumUpdateTb` SET `type` = 'str1' WHERE (`id` = {id})", sql);
            g.mysql.Update<TestEnumUpdateTb>().NoneParameter().Where(a => a.id == id).Set(a => a.type, TestEnumUpdateTbType.str1).ExecuteAffrows();
            Assert.Equal(TestEnumUpdateTbType.str1, g.mysql.Select<TestEnumUpdateTb>().Where(a => a.id == id).First()?.type);
        }
        public class tenumcls
        {
            public Guid id { get; set; }
            public tenum status { get; set; }
        }
        public enum tenum
        {
            WaitPay = 1,
            Pay = 3,
            Finsh = 8,
            Cacel = 16,
            Refunding = 32
        }
        [Fact]
        public void SetEnum()
        {
            var fsql = g.mysql;
            //#184
            fsql.Delete<tenumcls>(Guid.Parse("5e83a910-672f-847c-00c1-316b71d153fb")).ExecuteAffrows();
            var item = new tenumcls { id = Guid.Parse("5e83a910-672f-847c-00c1-316b71d153fb"), status = tenum.Finsh };
            Assert.Equal("INSERT INTO `tenumcls`(`id`, `status`) VALUES('5e83a910-672f-847c-00c1-316b71d153fb', 'Finsh')",
                fsql.Insert<tenumcls>().NoneParameter().AppendData(item).ToSql());
            Assert.Equal(1, fsql.Insert<tenumcls>().NoneParameter().AppendData(item).ExecuteAffrows());
            var item2 = fsql.Select<tenumcls>().Where(a => a.id == item.id).First();
            Assert.Equal(item.id, item2.id);
            Assert.Equal(tenum.Finsh, item2.status);

            Assert.Equal(@"UPDATE `tenumcls` SET `status` = case when `id` = '5e83a910-672f-847c-00c1-316b71d153fb' then 'Pay' else 'Refunding' end 
WHERE (`id` = '5e83a910-672f-847c-00c1-316b71d153fb')",
                fsql.Update<tenumcls>(item).NoneParameter().Set(a => a.status == (a.id == item.id ? tenum.Pay : tenum.Refunding)).ToSql());
            Assert.Equal(1, fsql.Update<tenumcls>(item).NoneParameter().Set(a => a.status == (a.id == item.id ? tenum.Pay : tenum.Refunding)).ExecuteAffrows());
            item2 = fsql.Select<tenumcls>().Where(a => a.id == item.id).First();
            Assert.Equal(item.id, item2.id);
            Assert.Equal(tenum.Pay, item2.status);

            Assert.Equal(@"UPDATE `tenumcls` SET `status` = 'Finsh' 
WHERE (`id` = '5e83a910-672f-847c-00c1-316b71d153fb')",
                fsql.Update<tenumcls>(item).NoneParameter().Set(a => a.status == tenum.Finsh).ToSql());
            Assert.Equal(1, fsql.Update<tenumcls>(item).NoneParameter().Set(a => a.status == tenum.Finsh).ExecuteAffrows());
            item2 = fsql.Select<tenumcls>().Where(a => a.id == item.id).First();
            Assert.Equal(item.id, item2.id);
            Assert.Equal(tenum.Finsh, item2.status);

            Assert.Equal(@"UPDATE `tenumcls` SET `status` = 'Pay' 
WHERE (`id` = '5e83a910-672f-847c-00c1-316b71d153fb')",
    fsql.Update<tenumcls>(item).NoneParameter().Set(a => a.status == tenum.Pay).ToSql());
            Assert.Equal(1, fsql.Update<tenumcls>(item).NoneParameter().Set(a => a.status == tenum.Pay).ExecuteAffrows());
            item2 = fsql.Select<tenumcls>().Where(a => a.id == item.id).First();
            Assert.Equal(item.id, item2.id);
            Assert.Equal(tenum.Pay, item2.status);

            Assert.Equal(@"UPDATE `tenumcls` SET `status` = 'Finsh' 
WHERE (`id` = '5e83a910-672f-847c-00c1-316b71d153fb')",
                fsql.Update<tenumcls>(item).NoneParameter().Set(a => a.status, tenum.Finsh).ToSql());
            Assert.Equal(1, fsql.Update<tenumcls>(item).NoneParameter().Set(a => a.status, tenum.Finsh).ExecuteAffrows());
            item2 = fsql.Select<tenumcls>().Where(a => a.id == item.id).First();
            Assert.Equal(item.id, item2.id);
            Assert.Equal(tenum.Finsh, item2.status);

            Assert.Equal(@"UPDATE `tenumcls` SET `status` = 'Pay' 
WHERE (`id` = '5e83a910-672f-847c-00c1-316b71d153fb')",
    fsql.Update<tenumcls>(item).NoneParameter().Set(a => a.status, tenum.Pay).ToSql());
            Assert.Equal(1, fsql.Update<tenumcls>(item).NoneParameter().Set(a => a.status, tenum.Pay).ExecuteAffrows());

            Assert.Equal(@"SELECT a.`id`, a.`status` 
FROM `tenumcls` a 
WHERE (a.`id` = '5e83a910-672f-847c-00c1-316b71d153fb' AND a.`status` = case when a.`id` = '5e83a910-672f-847c-00c1-316b71d153fb' then 'Pay' else 'Refunding' end) 
limit 0,1", fsql.Select<tenumcls>().Where(a => a.id == item.id && a.status == (a.id == item.id ? tenum.Pay : tenum.Refunding)).Limit(1).ToSql());
            item2 = fsql.Select<tenumcls>().Where(a => a.id == item.id && a.status == (a.id == item.id ? tenum.Pay : tenum.Refunding)).First();
            Assert.Equal(item.id, item2.id);
            Assert.Equal(tenum.Pay, item2.status);
        }
        [Fact]
        public void SetRaw()
        {
            var sql = update.Where(a => a.Id == 1).SetRaw("clicks = clicks + ?", new { incrClick = 1 }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET clicks = clicks + ? WHERE (`Id` = 1)", sql);

            sql = g.mysql.Update<TestEnumUpdateTb>().NoneParameter().Where(a => a.id == 0).SetRaw("`type` = {0}".FormatOdbcMySql(TestEnumUpdateTbType.sum211)).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `TestEnumUpdateTb` SET `type` = 'sum211' WHERE (`id` = 0)", sql);
        }
        [Fact]
        public void SetDto()
        {
            var sql = update.SetDto(new { clicks = 1, title = "xxx" }).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET `Clicks` = 1, `Title` = 'xxx' WHERE (`Id` = 1)", sql);

            sql = update.SetDto(new Dictionary<string, object> { ["clicks"] = 1, ["title"] = "xxx" }).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET `Clicks` = 1, `Title` = 'xxx' WHERE (`Id` = 1)", sql);
        }
        [Fact]
        public void Where()
        {
            var sql = update.Where(a => a.Id == 1).SetRaw("title='newtitle'").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET title='newtitle' WHERE (`Id` = 1)", sql);

            sql = update.Where("id = ?", new { id = 1 }).SetRaw("title='newtitle'").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET title='newtitle' WHERE (id = ?)", sql);

            var item = new Topic { Id = 1, Title = "newtitle" };
            sql = update.Where(item).SetRaw("title='newtitle'").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET title='newtitle' WHERE (`Id` = 1)", sql);

            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });
            sql = update.Where(items).SetRaw("title='newtitle'").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `tb_topic` SET title='newtitle' WHERE (`Id` IN (1,2,3,4,5,6,7,8,9,10))", sql);

            sql = g.mysql.Update<TestEnumUpdateTb>().NoneParameter().Where(a => a.id == 0 && a.type == TestEnumUpdateTbType.str1)
                .Set(a => a.type, TestEnumUpdateTbType.sum211).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE `TestEnumUpdateTb` SET `type` = 'sum211' WHERE (`id` = 0 AND `type` = 'str1')", sql);
        }
        [Fact]
        public void ExecuteAffrows()
        {
            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

            var time = DateTime.Now;
            var items222 = g.mysql.Select<Topic>().Where(a => a.CreateTime > time).Limit(10).ToList();

            update.SetSource(items.First()).NoneParameter().ExecuteAffrows();
            update.SetSource(items).NoneParameter().ExecuteAffrows();
        }
        [Fact]
        public void ExecuteUpdated()
        {

        }

        [Fact]
        public void AsTable()
        {
            Assert.Null(g.mysql.Update<Topic>().ToSql());
            Assert.Equal("UPDATE `tb_topicAsTable` SET title='test' \r\nWHERE (`Id` = 1 OR `Id` = 2)", g.mysql.Update<Topic>(new[] { 1, 2 }).SetRaw("title='test'").AsTable(a => "tb_topicAsTable").ToSql());
            Assert.Equal("UPDATE `tb_topicAsTable` SET title='test1' \r\nWHERE (`Id` = 1)", g.mysql.Update<Topic>(new Topic { Id = 1, Title = "test" }).SetRaw("title='test1'").AsTable(a => "tb_topicAsTable").ToSql());
            Assert.Equal("UPDATE `tb_topicAsTable` SET title='test1' \r\nWHERE (`Id` = 1 OR `Id` = 2)", g.mysql.Update<Topic>(new[] { new Topic { Id = 1, Title = "test" }, new Topic { Id = 2, Title = "test" } }).SetRaw("title='test1'").AsTable(a => "tb_topicAsTable").ToSql());
            Assert.Equal("UPDATE `tb_topicAsTable` SET title='test1' \r\nWHERE (`Id` = 1)", g.mysql.Update<Topic>(new { id = 1 }).SetRaw("title='test1'").AsTable(a => "tb_topicAsTable").ToSql());
        }
    }
}
