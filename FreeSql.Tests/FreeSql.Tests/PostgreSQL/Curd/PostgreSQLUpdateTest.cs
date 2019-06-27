using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using Xunit;

namespace FreeSql.Tests.PostgreSQL
{
    public class PostgreSQLUpdateTest
    {
        IUpdate<Topic> update => g.pgsql.Update<Topic>();

        [Table(Name = "tb_topic")]
        class Topic
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }
            public int? Clicks { get; set; }
            public TestTypeInfo Type { get; set; }
            public string Title { get; set; }
            public DateTime CreateTime { get; set; }
        }

        [Fact]
        public void Dywhere()
        {
            Assert.Null(g.pgsql.Update<Topic>().ToSql());
            Assert.Equal("UPDATE \"tb_topic\" SET title='test' \r\nWHERE (\"id\" = 1 OR \"id\" = 2)", g.pgsql.Update<Topic>(new[] { 1, 2 }).SetRaw("title='test'").ToSql());
            Assert.Equal("UPDATE \"tb_topic\" SET title='test1' \r\nWHERE (\"id\" = 1)", g.pgsql.Update<Topic>(new Topic { Id = 1, Title = "test" }).SetRaw("title='test1'").ToSql());
            Assert.Equal("UPDATE \"tb_topic\" SET title='test1' \r\nWHERE (\"id\" = 1 OR \"id\" = 2)", g.pgsql.Update<Topic>(new[] { new Topic { Id = 1, Title = "test" }, new Topic { Id = 2, Title = "test" } }).SetRaw("title='test1'").ToSql());
            Assert.Equal("UPDATE \"tb_topic\" SET title='test1' \r\nWHERE (\"id\" = 1)", g.pgsql.Update<Topic>(new { id = 1 }).SetRaw("title='test1'").ToSql());
        }

        [Fact]
        public void SetSource()
        {
            var sql = update.SetSource(new Topic { Id = 1, Title = "newtitle" }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"tb_topic\" SET \"clicks\" = @p_0, \"title\" = @p_1, \"createtime\" = @p_2 WHERE (\"id\" = 1)", sql);

            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

            sql = update.SetSource(items).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"tb_topic\" SET \"clicks\" = CASE \"id\" WHEN 1 THEN @p_0 WHEN 2 THEN @p_1 WHEN 3 THEN @p_2 WHEN 4 THEN @p_3 WHEN 5 THEN @p_4 WHEN 6 THEN @p_5 WHEN 7 THEN @p_6 WHEN 8 THEN @p_7 WHEN 9 THEN @p_8 WHEN 10 THEN @p_9 END, \"title\" = CASE \"id\" WHEN 1 THEN @p_10 WHEN 2 THEN @p_11 WHEN 3 THEN @p_12 WHEN 4 THEN @p_13 WHEN 5 THEN @p_14 WHEN 6 THEN @p_15 WHEN 7 THEN @p_16 WHEN 8 THEN @p_17 WHEN 9 THEN @p_18 WHEN 10 THEN @p_19 END, \"createtime\" = CASE \"id\" WHEN 1 THEN @p_20 WHEN 2 THEN @p_21 WHEN 3 THEN @p_22 WHEN 4 THEN @p_23 WHEN 5 THEN @p_24 WHEN 6 THEN @p_25 WHEN 7 THEN @p_26 WHEN 8 THEN @p_27 WHEN 9 THEN @p_28 WHEN 10 THEN @p_29 END WHERE (\"id\" IN (1,2,3,4,5,6,7,8,9,10))", sql);

            sql = update.SetSource(items).IgnoreColumns(a => new { a.Clicks, a.CreateTime }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"tb_topic\" SET \"title\" = CASE \"id\" WHEN 1 THEN @p_0 WHEN 2 THEN @p_1 WHEN 3 THEN @p_2 WHEN 4 THEN @p_3 WHEN 5 THEN @p_4 WHEN 6 THEN @p_5 WHEN 7 THEN @p_6 WHEN 8 THEN @p_7 WHEN 9 THEN @p_8 WHEN 10 THEN @p_9 END WHERE (\"id\" IN (1,2,3,4,5,6,7,8,9,10))", sql);

            sql = update.SetSource(items).Set(a => a.CreateTime, new DateTime(2020, 1, 1)).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"tb_topic\" SET \"createtime\" = @p_0 WHERE (\"id\" IN (1,2,3,4,5,6,7,8,9,10))", sql);
        }
        [Fact]
        public void IgnoreColumns()
        {
            var sql = update.SetSource(new Topic { Id = 1, Title = "newtitle" }).IgnoreColumns(a => new { a.Clicks, a.CreateTime }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"tb_topic\" SET \"title\" = @p_0 WHERE (\"id\" = 1)", sql);
        }
        [Fact]
        public void UpdateColumns()
        {
            var sql = update.SetSource(new Topic { Id = 1, Title = "newtitle" }).UpdateColumns(a => a.Title).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"tb_topic\" SET \"title\" = @p_0 WHERE (\"id\" = 1)", sql);
        }
        [Fact]
        public void Set()
        {
            var sql = update.Where(a => a.Id == 1).Set(a => a.Title, "newtitle").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"tb_topic\" SET \"title\" = @p_0 WHERE (\"id\" = 1)", sql);

            sql = update.Where(a => a.Id == 1).Set(a => a.Title, "newtitle").Set(a => a.CreateTime, new DateTime(2020, 1, 1)).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"tb_topic\" SET \"title\" = @p_0, \"createtime\" = @p_1 WHERE (\"id\" = 1)", sql);

            sql = update.Set(a => a.Clicks * 10 / 1).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"tb_topic\" SET \"clicks\" = coalesce(\"clicks\", 0) * 10 / 1 WHERE (\"id\" = 1)", sql);

            sql = update.Set(a => a.Id - 10).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"tb_topic\" SET \"id\" = (\"id\" - 10) WHERE (\"id\" = 1)", sql);

            int incrv = 10;
            sql = update.Set(a => a.Clicks * incrv / 1).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"tb_topic\" SET \"clicks\" = coalesce(\"clicks\", 0) * 10 / 1 WHERE (\"id\" = 1)", sql);

            sql = update.Set(a => a.Id - incrv).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"tb_topic\" SET \"id\" = (\"id\" - 10) WHERE (\"id\" = 1)", sql);

            sql = update.Set(a => a.Clicks == a.Clicks * 10 / 1).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"tb_topic\" SET \"clicks\" = \"clicks\" * 10 / 1 WHERE (\"id\" = 1)", sql);

            sql = update.Set(a => a.Id == 10).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"tb_topic\" SET \"id\" = 10 WHERE (\"id\" = 1)", sql);
        }
        [Fact]
        public void SetRaw()
        {
            var sql = update.Where(a => a.Id == 1).SetRaw("clicks = clicks + @incrClick", new { incrClick = 1 }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"tb_topic\" SET clicks = clicks + @incrClick WHERE (\"id\" = 1)", sql);
        }
        [Fact]
        public void Where()
        {
            var sql = update.Where(a => a.Id == 1).SetRaw("title='newtitle'").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"tb_topic\" SET title='newtitle' WHERE (\"id\" = 1)", sql);

            sql = update.Where("id = @id", new { id = 1 }).SetRaw("title='newtitle'").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"tb_topic\" SET title='newtitle' WHERE (id = @id)", sql);

            var item = new Topic { Id = 1, Title = "newtitle" };
            sql = update.Where(item).SetRaw("title='newtitle'").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"tb_topic\" SET title='newtitle' WHERE (\"id\" = 1)", sql);

            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });
            sql = update.Where(items).SetRaw("title='newtitle'").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"tb_topic\" SET title='newtitle' WHERE (\"id\" IN (1,2,3,4,5,6,7,8,9,10))", sql);
        }
        [Fact]
        public void WhereExists()
        {

        }
        [Fact]
        public void ExecuteAffrows()
        {

        }
        [Fact]
        public void ExecuteUpdated()
        {

        }

        [Fact]
        public void AsTable()
        {
            Assert.Null(g.pgsql.Update<Topic>().ToSql());
            Assert.Equal("UPDATE \"tb_topicAsTable\" SET title='test' \r\nWHERE (\"id\" = 1 OR \"id\" = 2)", g.pgsql.Update<Topic>(new[] { 1, 2 }).SetRaw("title='test'").AsTable(a => "tb_topicAsTable").ToSql());
            Assert.Equal("UPDATE \"tb_topicAsTable\" SET title='test1' \r\nWHERE (\"id\" = 1)", g.pgsql.Update<Topic>(new Topic { Id = 1, Title = "test" }).SetRaw("title='test1'").AsTable(a => "tb_topicAsTable").ToSql());
            Assert.Equal("UPDATE \"tb_topicAsTable\" SET title='test1' \r\nWHERE (\"id\" = 1 OR \"id\" = 2)", g.pgsql.Update<Topic>(new[] { new Topic { Id = 1, Title = "test" }, new Topic { Id = 2, Title = "test" } }).SetRaw("title='test1'").AsTable(a => "tb_topicAsTable").ToSql());
            Assert.Equal("UPDATE \"tb_topicAsTable\" SET title='test1' \r\nWHERE (\"id\" = 1)", g.pgsql.Update<Topic>(new { id = 1 }).SetRaw("title='test1'").AsTable(a => "tb_topicAsTable").ToSql());
        }
    }
}
