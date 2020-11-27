using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.Firebird
{
    public class FirebirdUpdateTest
    {
        IUpdate<Topic> update => g.firebird.Update<Topic>();

        [Table(Name = "tb_topic_insert")]
        class Topic
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }
            public int? Clicks { get; set; }
            public TestTypeInfo Type { get; set; }
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
            public TestTypeInfo Type { get; set; }
            public string Title { get; set; }
            public DateTime CreateTime { get; set; }
        }

        [Fact]
        public void Dywhere()
        {
            Assert.Null(g.firebird.Update<Topic>().ToSql());
            Assert.Equal("UPDATE \"TB_TOPIC_INSERT\" SET title='test' \r\nWHERE (\"ID\" IN (1,2))", g.firebird.Update<Topic>(new[] { 1, 2 }).SetRaw("title='test'").ToSql());
            Assert.Equal("UPDATE \"TB_TOPIC_INSERT\" SET title='test1' \r\nWHERE (\"ID\" = 1)", g.firebird.Update<Topic>(new Topic { Id = 1, Title = "test" }).SetRaw("title='test1'").ToSql());
            Assert.Equal("UPDATE \"TB_TOPIC_INSERT\" SET title='test1' \r\nWHERE (\"ID\" IN (1,2))", g.firebird.Update<Topic>(new[] { new Topic { Id = 1, Title = "test" }, new Topic { Id = 2, Title = "test" } }).SetRaw("title='test1'").ToSql());
            Assert.Equal("UPDATE \"TB_TOPIC_INSERT\" SET title='test1' \r\nWHERE (\"ID\" = 1)", g.firebird.Update<Topic>(new { id = 1 }).SetRaw("title='test1'").ToSql());
        }

        [Fact]
        public void SetSource()
        {
            var sql = update.SetSource(new Topic { Id = 1, Title = "newtitle" }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC_INSERT\" SET \"CLICKS\" = @p_0, \"TITLE\" = @p_1, \"CREATETIME\" = @p_2 WHERE (\"ID\" = 1)", sql);

            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });
            items[0].Clicks = null;

            sql = update.SetSource(items).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC_INSERT\" SET \"CLICKS\" = CASE \"ID\" WHEN 1 THEN @p_0 WHEN 2 THEN @p_1 WHEN 3 THEN @p_2 WHEN 4 THEN @p_3 WHEN 5 THEN @p_4 WHEN 6 THEN @p_5 WHEN 7 THEN @p_6 WHEN 8 THEN @p_7 WHEN 9 THEN @p_8 WHEN 10 THEN @p_9 END, \"TITLE\" = CASE \"ID\" WHEN 1 THEN @p_10 WHEN 2 THEN @p_11 WHEN 3 THEN @p_12 WHEN 4 THEN @p_13 WHEN 5 THEN @p_14 WHEN 6 THEN @p_15 WHEN 7 THEN @p_16 WHEN 8 THEN @p_17 WHEN 9 THEN @p_18 WHEN 10 THEN @p_19 END, \"CREATETIME\" = CASE \"ID\" WHEN 1 THEN @p_20 WHEN 2 THEN @p_21 WHEN 3 THEN @p_22 WHEN 4 THEN @p_23 WHEN 5 THEN @p_24 WHEN 6 THEN @p_25 WHEN 7 THEN @p_26 WHEN 8 THEN @p_27 WHEN 9 THEN @p_28 WHEN 10 THEN @p_29 END WHERE (\"ID\" IN (1,2,3,4,5,6,7,8,9,10))", sql);

            sql = update.SetSource(items).IgnoreColumns(a => new { a.Clicks, a.CreateTime }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC_INSERT\" SET \"TITLE\" = CASE \"ID\" WHEN 1 THEN @p_0 WHEN 2 THEN @p_1 WHEN 3 THEN @p_2 WHEN 4 THEN @p_3 WHEN 5 THEN @p_4 WHEN 6 THEN @p_5 WHEN 7 THEN @p_6 WHEN 8 THEN @p_7 WHEN 9 THEN @p_8 WHEN 10 THEN @p_9 END WHERE (\"ID\" IN (1,2,3,4,5,6,7,8,9,10))", sql);

            sql = update.SetSource(items).Set(a => a.CreateTime, new DateTime(2020, 1, 1)).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC_INSERT\" SET \"CREATETIME\" = @p_0 WHERE (\"ID\" IN (1,2,3,4,5,6,7,8,9,10))", sql);

            sql = g.firebird.Update<ts_source_mpk>().SetSource(new[] {
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
        public void SetSourceNoIdentity()
        {
            var fsql = g.firebird;
            fsql.Delete<Topic22>().Where("1=1").ExecuteAffrows();
            var sql = fsql.Update<Topic22>().SetSource(new Topic22 { Id = 1, Title = "newtitle" }).IgnoreColumns(a => a.TypeGuid).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC_SETSOURCE\" SET \"CLICKS\" = @p_0, \"TITLE\" = @p_1, \"CREATETIME\" = @p_2 WHERE (\"ID\" = 1)", sql);

            var items = new List<Topic22>();
            for (var a = 0; a < 10; a++) items.Add(new Topic22 { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });
            Assert.Equal(10, fsql.Insert(items).ExecuteAffrows());
            items[0].Clicks = null;

            sql = fsql.Update<Topic22>().SetSource(items).IgnoreColumns(a => a.TypeGuid).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC_SETSOURCE\" SET \"CLICKS\" = CASE \"ID\" WHEN 1 THEN @p_0 WHEN 2 THEN @p_1 WHEN 3 THEN @p_2 WHEN 4 THEN @p_3 WHEN 5 THEN @p_4 WHEN 6 THEN @p_5 WHEN 7 THEN @p_6 WHEN 8 THEN @p_7 WHEN 9 THEN @p_8 WHEN 10 THEN @p_9 END, \"TITLE\" = CASE \"ID\" WHEN 1 THEN @p_10 WHEN 2 THEN @p_11 WHEN 3 THEN @p_12 WHEN 4 THEN @p_13 WHEN 5 THEN @p_14 WHEN 6 THEN @p_15 WHEN 7 THEN @p_16 WHEN 8 THEN @p_17 WHEN 9 THEN @p_18 WHEN 10 THEN @p_19 END, \"CREATETIME\" = CASE \"ID\" WHEN 1 THEN @p_20 WHEN 2 THEN @p_21 WHEN 3 THEN @p_22 WHEN 4 THEN @p_23 WHEN 5 THEN @p_24 WHEN 6 THEN @p_25 WHEN 7 THEN @p_26 WHEN 8 THEN @p_27 WHEN 9 THEN @p_28 WHEN 10 THEN @p_29 END WHERE (\"ID\" IN (1,2,3,4,5,6,7,8,9,10))", sql);

            sql = fsql.Update<Topic22>().SetSource(items).IgnoreColumns(a => new { a.Clicks, a.CreateTime, a.TypeGuid }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC_SETSOURCE\" SET \"TITLE\" = CASE \"ID\" WHEN 1 THEN @p_0 WHEN 2 THEN @p_1 WHEN 3 THEN @p_2 WHEN 4 THEN @p_3 WHEN 5 THEN @p_4 WHEN 6 THEN @p_5 WHEN 7 THEN @p_6 WHEN 8 THEN @p_7 WHEN 9 THEN @p_8 WHEN 10 THEN @p_9 END WHERE (\"ID\" IN (1,2,3,4,5,6,7,8,9,10))", sql);

            sql = fsql.Update<Topic22>().SetSource(items).IgnoreColumns(a => a.TypeGuid).Set(a => a.CreateTime, new DateTime(2020, 1, 1)).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC_SETSOURCE\" SET \"CREATETIME\" = @p_0 WHERE (\"ID\" IN (1,2,3,4,5,6,7,8,9,10))", sql);
        }
        [Fact]
        public void SetSourceIgnore()
        {
            Assert.Equal("UPDATE \"TSSI01\" SET \"TINT\" = 10 WHERE (\"ID\" = '00000000-0000-0000-0000-000000000000')",
                g.firebird.Update<tssi01>().NoneParameter()
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
            Assert.Equal("UPDATE \"TB_TOPIC_INSERT\" SET \"TITLE\" = @p_0 WHERE (\"ID\" = 1)", sql);

            sql = update.SetSource(new Topic { Id = 1, Title = "newtitle" }).IgnoreColumns(a => new object[] { a.Clicks, a.CreateTime }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC_INSERT\" SET \"TITLE\" = @p_0 WHERE (\"ID\" = 1)", sql);

            sql = update.SetSource(new Topic { Id = 1, Title = "newtitle" }).IgnoreColumns(a => new[] { "Clicks", "CreateTime" }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC_INSERT\" SET \"TITLE\" = @p_0 WHERE (\"ID\" = 1)", sql);

            var cols = new[] { "Clicks", "CreateTime" };
            sql = update.SetSource(new Topic { Id = 1, Title = "newtitle" }).IgnoreColumns(a => cols).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC_INSERT\" SET \"TITLE\" = @p_0 WHERE (\"ID\" = 1)", sql);

            cols = new[] { "Clicks", "CreateTime" };
            sql = update.SetSource(new Topic { Id = 1, Title = "newtitle" }).IgnoreColumns(cols).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC_INSERT\" SET \"TITLE\" = @p_0 WHERE (\"ID\" = 1)", sql);
        }
        [Fact]
        public void UpdateColumns()
        {
            var sql = update.SetSource(new Topic { Id = 1, Title = "newtitle" }).UpdateColumns(a => a.Title).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC_INSERT\" SET \"TITLE\" = @p_0 WHERE (\"ID\" = 1)", sql);
        }
        [Fact]
        public void Set()
        {
            var sql = update.Where(a => a.Id == 1).Set(a => a.Title, "newtitle").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC_INSERT\" SET \"TITLE\" = @p_0 WHERE (\"ID\" = 1)", sql);

            sql = update.Where(a => a.Id == 1).Set(a => a.Title, "newtitle").Set(a => a.CreateTime, new DateTime(2020, 1, 1)).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC_INSERT\" SET \"TITLE\" = @p_0, \"CREATETIME\" = @p_1 WHERE (\"ID\" = 1)", sql);

            sql = update.Set(a => a.Clicks * 10 / 1).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC_INSERT\" SET \"CLICKS\" = trunc(coalesce(\"CLICKS\", 0) * 10/1) WHERE (\"ID\" = 1)", sql);

            sql = update.Set(a => a.Id - 10).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC_INSERT\" SET \"ID\" = (\"ID\" - 10) WHERE (\"ID\" = 1)", sql);

            int incrv = 10;
            sql = update.Set(a => a.Clicks * incrv / 1).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC_INSERT\" SET \"CLICKS\" = trunc(coalesce(\"CLICKS\", 0) * 10/1) WHERE (\"ID\" = 1)", sql);

            sql = update.Set(a => a.Id - incrv).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC_INSERT\" SET \"ID\" = (\"ID\" - 10) WHERE (\"ID\" = 1)", sql);

            sql = update.Set(a => a.Clicks == a.Clicks * 10 / 1).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC_INSERT\" SET \"CLICKS\" = trunc(\"CLICKS\" * 10/1) WHERE (\"ID\" = 1)", sql);

            var dt2000 = DateTime.Parse("2000-01-01");
            sql = update.Set(a => a.Clicks == (a.CreateTime > dt2000 ? 1 : 2)).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC_INSERT\" SET \"CLICKS\" = case when \"CREATETIME\" > timestamp '2000-01-01 00:00:00.000' then 1 else 2 end WHERE (\"ID\" = 1)", sql);

            sql = update.Set(a => a.Id == 10).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC_INSERT\" SET \"ID\" = 10 WHERE (\"ID\" = 1)", sql);

            sql = update.Set(a => a.Clicks == null).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC_INSERT\" SET \"CLICKS\" = NULL WHERE (\"ID\" = 1)", sql);

            sql = update.Set(a => a.Clicks == null).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC_INSERT\" SET \"CLICKS\" = NULL WHERE (\"ID\" = 1)", sql);
        }
        [Fact]
        public void SetRaw()
        {
            var sql = update.Where(a => a.Id == 1).SetRaw("clicks = clicks + @incrClick", new { incrClick = 1 }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC_INSERT\" SET clicks = clicks + @incrClick WHERE (\"ID\" = 1)", sql);
        }
        [Fact]
        public void SetDto()
        {
            var sql = update.SetDto(new { clicks = 1, title = "xxx" }).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC_INSERT\" SET \"CLICKS\" = @p_0, \"TITLE\" = @p_1 WHERE (\"ID\" = 1)", sql);
            sql = update.NoneParameter().SetDto(new { clicks = 1, title = "xxx" }).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC_INSERT\" SET \"CLICKS\" = 1, \"TITLE\" = 'xxx' WHERE (\"ID\" = 1)", sql);

            sql = update.SetDto(new Dictionary<string, object> { ["clicks"] = 1, ["title"] = "xxx" }).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC_INSERT\" SET \"CLICKS\" = @p_0, \"TITLE\" = @p_1 WHERE (\"ID\" = 1)", sql);
            sql = update.NoneParameter().SetDto(new Dictionary<string, object> { ["clicks"] = 1, ["title"] = "xxx" }).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC_INSERT\" SET \"CLICKS\" = 1, \"TITLE\" = 'xxx' WHERE (\"ID\" = 1)", sql);
        }
        [Fact]
        public void Where()
        {
            var sql = update.Where(a => a.Id == 1).SetRaw("title='newtitle'").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC_INSERT\" SET title='newtitle' WHERE (\"ID\" = 1)", sql);

            sql = update.Where("id = @id", new { id = 1 }).SetRaw("title='newtitle'").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC_INSERT\" SET title='newtitle' WHERE (id = @id)", sql);

            var item = new Topic { Id = 1, Title = "newtitle" };
            sql = update.Where(item).SetRaw("title='newtitle'").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC_INSERT\" SET title='newtitle' WHERE (\"ID\" = 1)", sql);

            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });
            sql = update.Where(items).SetRaw("title='newtitle'").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC_INSERT\" SET title='newtitle' WHERE (\"ID\" IN (1,2,3,4,5,6,7,8,9,10))", sql);
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
            Assert.Null(g.firebird.Update<Topic>().ToSql());
            Assert.Equal("UPDATE \"TB_TOPICASTABLE\" SET title='test' \r\nWHERE (\"ID\" IN (1,2))", g.firebird.Update<Topic>(new[] { 1, 2 }).SetRaw("title='test'").AsTable(a => "tb_topicAsTable").ToSql());
            Assert.Equal("UPDATE \"TB_TOPICASTABLE\" SET title='test1' \r\nWHERE (\"ID\" = 1)", g.firebird.Update<Topic>(new Topic { Id = 1, Title = "test" }).SetRaw("title='test1'").AsTable(a => "tb_topicAsTable").ToSql());
            Assert.Equal("UPDATE \"TB_TOPICASTABLE\" SET title='test1' \r\nWHERE (\"ID\" IN (1,2))", g.firebird.Update<Topic>(new[] { new Topic { Id = 1, Title = "test" }, new Topic { Id = 2, Title = "test" } }).SetRaw("title='test1'").AsTable(a => "tb_topicAsTable").ToSql());
            Assert.Equal("UPDATE \"TB_TOPICASTABLE\" SET title='test1' \r\nWHERE (\"ID\" = 1)", g.firebird.Update<Topic>(new { id = 1 }).SetRaw("title='test1'").AsTable(a => "tb_topicAsTable").ToSql());
        }
    }
}
