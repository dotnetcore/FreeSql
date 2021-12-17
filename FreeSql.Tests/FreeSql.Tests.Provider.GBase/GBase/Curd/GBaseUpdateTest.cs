using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.GBase
{
    public class GBaseUpdateTest
    {
        IUpdate<Topic> update => g.gbase.Update<Topic>();

        [Table(Name = "tb_topic_insert")]
        class Topic
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }
            public int? Clicks { get; set; }
            public string Title { get; set; }
            public DateTime CreateTime { get; set; }
        }
        [Table(Name = "tb_topic_setsource")]
        class Topic22
        {
            [Column(IsPrimary = true)]
            public int Id { get; set; }
            public int? Clicks { get; set; }
            public int TypeGuid { get; set; }
            public string Title { get; set; }
            public DateTime CreateTime { get; set; }
        }

        [Fact]
        public void Dywhere()
        {
            Assert.Null(g.gbase.Update<Topic>().ToSql());
            Assert.Equal("UPDATE tb_topic_insert SET Title='test' \r\nWHERE (Id IN (1,2))", g.gbase.Update<Topic>(new[] { 1, 2 }).SetRaw("Title='test'").ToSql());
            Assert.Equal("UPDATE tb_topic_insert SET Title='test1' \r\nWHERE (Id = 1)", g.gbase.Update<Topic>(new Topic { Id = 1, Title = "test" }).SetRaw("Title='test1'").ToSql());
            Assert.Equal("UPDATE tb_topic_insert SET Title='test1' \r\nWHERE (Id IN (1,2))", g.gbase.Update<Topic>(new[] { new Topic { Id = 1, Title = "test" }, new Topic { Id = 2, Title = "test" } }).SetRaw("Title='test1'").ToSql());
            Assert.Equal("UPDATE tb_topic_insert SET Title='test1' \r\nWHERE (Id = 1)", g.gbase.Update<Topic>(new { id = 1 }).SetRaw("Title='test1'").ToSql());
        }

        [Fact]
        public void SetSource()
        {
            var sql = update.SetSource(new Topic { Id = 1, Title = "newtitle" }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE tb_topic_insert SET Clicks = NULL, Title = 'newtitle', CreateTime = '0001-01-01 00:00:00.000' WHERE (Id = 1)", sql);

            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });
            items[0].Clicks = null;

            sql = update.SetSource(items).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE tb_topic_insert SET Clicks = CASE Id WHEN 1 THEN NULL WHEN 2 THEN 100 WHEN 3 THEN 200 WHEN 4 THEN 300 WHEN 5 THEN 400 WHEN 6 THEN 500 WHEN 7 THEN 600 WHEN 8 THEN 700 WHEN 9 THEN 800 WHEN 10 THEN 900 END, Title = CASE Id WHEN 1 THEN 'newtitle0' WHEN 2 THEN 'newtitle1' WHEN 3 THEN 'newtitle2' WHEN 4 THEN 'newtitle3' WHEN 5 THEN 'newtitle4' WHEN 6 THEN 'newtitle5' WHEN 7 THEN 'newtitle6' WHEN 8 THEN 'newtitle7' WHEN 9 THEN 'newtitle8' WHEN 10 THEN 'newtitle9' END, CreateTime = CASE Id WHEN 1 THEN '0001-01-01 00:00:00.000' WHEN 2 THEN '0001-01-01 00:00:00.000' WHEN 3 THEN '0001-01-01 00:00:00.000' WHEN 4 THEN '0001-01-01 00:00:00.000' WHEN 5 THEN '0001-01-01 00:00:00.000' WHEN 6 THEN '0001-01-01 00:00:00.000' WHEN 7 THEN '0001-01-01 00:00:00.000' WHEN 8 THEN '0001-01-01 00:00:00.000' WHEN 9 THEN '0001-01-01 00:00:00.000' WHEN 10 THEN '0001-01-01 00:00:00.000' END WHERE (Id IN (1,2,3,4,5,6,7,8,9,10))", sql);

            sql = update.SetSource(items).IgnoreColumns(a => new { a.Clicks, a.CreateTime }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE tb_topic_insert SET Title = CASE Id WHEN 1 THEN 'newtitle0' WHEN 2 THEN 'newtitle1' WHEN 3 THEN 'newtitle2' WHEN 4 THEN 'newtitle3' WHEN 5 THEN 'newtitle4' WHEN 6 THEN 'newtitle5' WHEN 7 THEN 'newtitle6' WHEN 8 THEN 'newtitle7' WHEN 9 THEN 'newtitle8' WHEN 10 THEN 'newtitle9' END WHERE (Id IN (1,2,3,4,5,6,7,8,9,10))", sql);

            sql = update.SetSource(items).Set(a => a.CreateTime, new DateTime(2020, 1, 1)).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE tb_topic_insert SET CreateTime = '2020-01-01 00:00:00.000' WHERE (Id IN (1,2,3,4,5,6,7,8,9,10))", sql);

            sql = g.gbase.Update<ts_source_mpk>().SetSource(new[] {
                new ts_source_mpk { id1 = 1, id2 = 7, xx = "a1" },
                new ts_source_mpk { id1 = 1, id2 = 8, xx = "b122" }
            }).NoneParameter().ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE ts_source_mpk SET xx = CASE (id1 || '+' || id2) WHEN (1 || '+' || 7) THEN 'a1' WHEN (1 || '+' || 8) THEN 'b122' END WHERE ((id1 = 1 AND id2 = 7) OR (id1 = 1 AND id2 = 8))", sql);
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
        public void SetSourceNoIdentity()
        {
            var fsql = g.gbase;
            fsql.Delete<Topic22>().Where("1=1").ExecuteAffrows();
            var sql = fsql.Update<Topic22>().SetSource(new Topic22 { Id = 1, Title = "newtitle" }).IgnoreColumns(a => a.TypeGuid).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE tb_topic_setsource SET Clicks = NULL, Title = 'newtitle', CreateTime = '0001-01-01 00:00:00.000' WHERE (Id = 1)", sql);

            var items = new List<Topic22>();
            for (var a = 0; a < 10; a++) items.Add(new Topic22 { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });
            Assert.Equal(10, fsql.Insert(items).ExecuteAffrows());
            items[0].Clicks = null;

            sql = fsql.Update<Topic22>().SetSource(items).IgnoreColumns(a => a.TypeGuid).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE tb_topic_setsource SET Clicks = CASE Id WHEN 1 THEN NULL WHEN 2 THEN 100 WHEN 3 THEN 200 WHEN 4 THEN 300 WHEN 5 THEN 400 WHEN 6 THEN 500 WHEN 7 THEN 600 WHEN 8 THEN 700 WHEN 9 THEN 800 WHEN 10 THEN 900 END, Title = CASE Id WHEN 1 THEN 'newtitle0' WHEN 2 THEN 'newtitle1' WHEN 3 THEN 'newtitle2' WHEN 4 THEN 'newtitle3' WHEN 5 THEN 'newtitle4' WHEN 6 THEN 'newtitle5' WHEN 7 THEN 'newtitle6' WHEN 8 THEN 'newtitle7' WHEN 9 THEN 'newtitle8' WHEN 10 THEN 'newtitle9' END, CreateTime = CASE Id WHEN 1 THEN '0001-01-01 00:00:00.000' WHEN 2 THEN '0001-01-01 00:00:00.000' WHEN 3 THEN '0001-01-01 00:00:00.000' WHEN 4 THEN '0001-01-01 00:00:00.000' WHEN 5 THEN '0001-01-01 00:00:00.000' WHEN 6 THEN '0001-01-01 00:00:00.000' WHEN 7 THEN '0001-01-01 00:00:00.000' WHEN 8 THEN '0001-01-01 00:00:00.000' WHEN 9 THEN '0001-01-01 00:00:00.000' WHEN 10 THEN '0001-01-01 00:00:00.000' END WHERE (Id IN (1,2,3,4,5,6,7,8,9,10))", sql);

            sql = fsql.Update<Topic22>().SetSource(items).IgnoreColumns(a => new { a.Clicks, a.CreateTime, a.TypeGuid }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE tb_topic_setsource SET Title = CASE Id WHEN 1 THEN 'newtitle0' WHEN 2 THEN 'newtitle1' WHEN 3 THEN 'newtitle2' WHEN 4 THEN 'newtitle3' WHEN 5 THEN 'newtitle4' WHEN 6 THEN 'newtitle5' WHEN 7 THEN 'newtitle6' WHEN 8 THEN 'newtitle7' WHEN 9 THEN 'newtitle8' WHEN 10 THEN 'newtitle9' END WHERE (Id IN (1,2,3,4,5,6,7,8,9,10))", sql);

            sql = fsql.Update<Topic22>().SetSource(items).IgnoreColumns(a => a.TypeGuid).Set(a => a.CreateTime, new DateTime(2020, 1, 1)).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE tb_topic_setsource SET CreateTime = '2020-01-01 00:00:00.000' WHERE (Id IN (1,2,3,4,5,6,7,8,9,10))", sql);
        }
        [Fact]
        public void SetSourceIgnore()
        {
            Assert.Equal("UPDATE tssi01 SET tint = 10 WHERE (id = '00000000-0000-0000-0000-000000000000')",
                g.gbase.Update<tssi01>().NoneParameter()
                    .SetSourceIgnore(new tssi01 { id = Guid.Empty, tint = 10 }, col => col == null).ToSql().Replace("\r\n", ""));
        }
        public class tssi01
        {
            [Column(CanUpdate = false)]
            public Guid id { get; set; }
            public int tint { get; set; }
            public string Title { get; set; }
        }
        [Fact]
        public void IgnoreColumns()
        {
            var sql = update.SetSource(new Topic { Id = 1, Title = "newtitle" }).IgnoreColumns(a => new { a.Clicks, a.CreateTime }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE tb_topic_insert SET Title = 'newtitle' WHERE (Id = 1)", sql);

            sql = update.SetSource(new Topic { Id = 1, Title = "newtitle" }).IgnoreColumns(a => new object[] { a.Clicks, a.CreateTime }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE tb_topic_insert SET Title = 'newtitle' WHERE (Id = 1)", sql);

            sql = update.SetSource(new Topic { Id = 1, Title = "newtitle" }).IgnoreColumns(a => new[] { "Clicks", "CreateTime" }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE tb_topic_insert SET Title = 'newtitle' WHERE (Id = 1)", sql);

            var cols = new[] { "Clicks", "CreateTime" };
            sql = update.SetSource(new Topic { Id = 1, Title = "newtitle" }).IgnoreColumns(a => cols).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE tb_topic_insert SET Title = 'newtitle' WHERE (Id = 1)", sql);

            cols = new[] { "Clicks", "CreateTime" };
            sql = update.SetSource(new Topic { Id = 1, Title = "newtitle" }).IgnoreColumns(cols).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE tb_topic_insert SET Title = 'newtitle' WHERE (Id = 1)", sql);
        }
        [Fact]
        public void UpdateColumns()
        {
            var sql = update.SetSource(new Topic { Id = 1, Title = "newtitle" }).UpdateColumns(a => a.Title).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE tb_topic_insert SET Title = 'newtitle' WHERE (Id = 1)", sql);
        }
        [Fact]
        public void Set()
        {
            var sql = update.Where(a => a.Id == 1).Set(a => a.Title, "newtitle").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE tb_topic_insert SET Title = 'newtitle' WHERE (Id = 1)", sql);

            sql = update.Where(a => a.Id == 1).Set(a => a.Title, "newtitle").Set(a => a.CreateTime, new DateTime(2020, 1, 1)).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE tb_topic_insert SET Title = 'newtitle', CreateTime = '2020-01-01 00:00:00.000' WHERE (Id = 1)", sql);

            sql = update.Set(a => a.Clicks * 10 / 1).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE tb_topic_insert SET Clicks = trunc(nvl(Clicks, 0) * 10/1) WHERE (Id = 1)", sql);

            sql = update.Set(a => a.Id - 10).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE tb_topic_insert SET Id = (Id - 10) WHERE (Id = 1)", sql);

            int incrv = 10;
            sql = update.Set(a => a.Clicks * incrv / 1).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE tb_topic_insert SET Clicks = trunc(nvl(Clicks, 0) * 10/1) WHERE (Id = 1)", sql);

            sql = update.Set(a => a.Id - incrv).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE tb_topic_insert SET Id = (Id - 10) WHERE (Id = 1)", sql);

            sql = update.Set(a => a.Clicks == a.Clicks * 10 / 1).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE tb_topic_insert SET Clicks = trunc(Clicks * 10/1) WHERE (Id = 1)", sql);

            var dt2000 = DateTime.Parse("2000-01-01");
            sql = update.Set(a => a.Clicks == (a.CreateTime > dt2000 ? 1 : 2)).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE tb_topic_insert SET Clicks = case when CreateTime > '2000-01-01 00:00:00.000' then 1 else 2 end WHERE (Id = 1)", sql);

            sql = update.Set(a => a.Id == 10).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE tb_topic_insert SET Id = 10 WHERE (Id = 1)", sql);

            sql = update.Set(a => a.Clicks == null).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE tb_topic_insert SET Clicks = NULL WHERE (Id = 1)", sql);

            sql = update.Set(a => a.Clicks == null).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE tb_topic_insert SET Clicks = NULL WHERE (Id = 1)", sql);
        }
        [Fact]
        public void SetRaw()
        {
            var sql = update.Where(a => a.Id == 1).SetRaw("clicks = clicks + @incrClick", new { incrClick = 1 }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE tb_topic_insert SET clicks = clicks + @incrClick WHERE (Id = 1)", sql);
        }
        [Fact]
        public void SetDto()
        {
            var sql = update.SetDto(new { clicks = 1, Title = "xxx" }).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE tb_topic_insert SET Clicks = 1, Title = 'xxx' WHERE (Id = 1)", sql);

            sql = update.NoneParameter().SetDto(new Dictionary<string, object> { ["clicks"] = 1, ["Title"] = "xxx" }).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE tb_topic_insert SET Clicks = 1, Title = 'xxx' WHERE (Id = 1)", sql);
        }
        [Fact]
        public void Where()
        {
            var sql = update.Where(a => a.Id == 1).SetRaw("Title='newtitle'").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE tb_topic_insert SET Title='newtitle' WHERE (Id = 1)", sql);

            sql = update.Where("id = @id", new { id = 1 }).SetRaw("Title='newtitle'").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE tb_topic_insert SET Title='newtitle' WHERE (id = @id)", sql);

            var item = new Topic { Id = 1, Title = "newtitle" };
            sql = update.Where(item).SetRaw("Title='newtitle'").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE tb_topic_insert SET Title='newtitle' WHERE (Id = 1)", sql);

            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });
            sql = update.Where(items).SetRaw("Title='newtitle'").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE tb_topic_insert SET Title='newtitle' WHERE (Id IN (1,2,3,4,5,6,7,8,9,10))", sql);
        }
        [Fact]
        public void ExecuteAffrows()
        {
            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

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
            Assert.Null(g.gbase.Update<Topic>().ToSql());
            Assert.Equal("UPDATE tb_topicAsTable SET Title='test' \r\nWHERE (Id IN (1,2))", g.gbase.Update<Topic>(new[] { 1, 2 }).SetRaw("Title='test'").AsTable(a => "tb_topicAsTable").ToSql());
            Assert.Equal("UPDATE tb_topicAsTable SET Title='test1' \r\nWHERE (Id = 1)", g.gbase.Update<Topic>(new Topic { Id = 1, Title = "test" }).SetRaw("Title='test1'").AsTable(a => "tb_topicAsTable").ToSql());
            Assert.Equal("UPDATE tb_topicAsTable SET Title='test1' \r\nWHERE (Id IN (1,2))", g.gbase.Update<Topic>(new[] { new Topic { Id = 1, Title = "test" }, new Topic { Id = 2, Title = "test" } }).SetRaw("Title='test1'").AsTable(a => "tb_topicAsTable").ToSql());
            Assert.Equal("UPDATE tb_topicAsTable SET Title='test1' \r\nWHERE (Id = 1)", g.gbase.Update<Topic>(new { id = 1 }).SetRaw("Title='test1'").AsTable(a => "tb_topicAsTable").ToSql());
        }
    }
}
