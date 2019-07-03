using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.PostgreSQL
{
    public class PostgreSQLInsertTest
    {

        IInsert<Topic> insert => g.pgsql.Insert<Topic>();

        [Table(Name = "tb_topic_insert")]
        class Topic
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }
            public int Clicks { get; set; }
            public TestTypeInfo Type { get; set; }
            public string Title { get; set; }
            public DateTime CreateTime { get; set; }
        }

        [Fact]
        public void AppendData()
        {
            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

            var sql = insert.AppendData(items.First()).ToSql();
            Assert.Equal("INSERT INTO \"tb_topic_insert\"(\"clicks\", \"title\", \"createtime\") VALUES(@clicks_0, @title_0, @createtime_0)", sql);

            sql = insert.AppendData(items).ToSql();
            Assert.Equal("INSERT INTO \"tb_topic_insert\"(\"clicks\", \"title\", \"createtime\") VALUES(@clicks_0, @title_0, @createtime_0), (@clicks_1, @title_1, @createtime_1), (@clicks_2, @title_2, @createtime_2), (@clicks_3, @title_3, @createtime_3), (@clicks_4, @title_4, @createtime_4), (@clicks_5, @title_5, @createtime_5), (@clicks_6, @title_6, @createtime_6), (@clicks_7, @title_7, @createtime_7), (@clicks_8, @title_8, @createtime_8), (@clicks_9, @title_9, @createtime_9)", sql);

            sql = insert.AppendData(items).InsertColumns(a => a.Title).ToSql();
            Assert.Equal("INSERT INTO \"tb_topic_insert\"(\"title\") VALUES(@title_0), (@title_1), (@title_2), (@title_3), (@title_4), (@title_5), (@title_6), (@title_7), (@title_8), (@title_9)", sql);

            sql = insert.AppendData(items).IgnoreColumns(a => a.CreateTime).ToSql();
            Assert.Equal("INSERT INTO \"tb_topic_insert\"(\"clicks\", \"title\") VALUES(@clicks_0, @title_0), (@clicks_1, @title_1), (@clicks_2, @title_2), (@clicks_3, @title_3), (@clicks_4, @title_4), (@clicks_5, @title_5), (@clicks_6, @title_6), (@clicks_7, @title_7), (@clicks_8, @title_8), (@clicks_9, @title_9)", sql);
        }

        [Fact]
        public void InsertColumns()
        {
            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

            var sql = insert.AppendData(items).InsertColumns(a => a.Title).ToSql();
            Assert.Equal("INSERT INTO \"tb_topic_insert\"(\"title\") VALUES(@title_0), (@title_1), (@title_2), (@title_3), (@title_4), (@title_5), (@title_6), (@title_7), (@title_8), (@title_9)", sql);

            sql = insert.AppendData(items).InsertColumns(a => new { a.Title, a.Clicks }).ToSql();
            Assert.Equal("INSERT INTO \"tb_topic_insert\"(\"clicks\", \"title\") VALUES(@clicks_0, @title_0), (@clicks_1, @title_1), (@clicks_2, @title_2), (@clicks_3, @title_3), (@clicks_4, @title_4), (@clicks_5, @title_5), (@clicks_6, @title_6), (@clicks_7, @title_7), (@clicks_8, @title_8), (@clicks_9, @title_9)", sql);
        }
        [Fact]
        public void IgnoreColumns()
        {
            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

            var sql = insert.AppendData(items).IgnoreColumns(a => a.CreateTime).ToSql();
            Assert.Equal("INSERT INTO \"tb_topic_insert\"(\"clicks\", \"title\") VALUES(@clicks_0, @title_0), (@clicks_1, @title_1), (@clicks_2, @title_2), (@clicks_3, @title_3), (@clicks_4, @title_4), (@clicks_5, @title_5), (@clicks_6, @title_6), (@clicks_7, @title_7), (@clicks_8, @title_8), (@clicks_9, @title_9)", sql);

            sql = insert.AppendData(items).IgnoreColumns(a => new { a.Title, a.CreateTime }).ToSql();
            Assert.Equal("INSERT INTO \"tb_topic_insert\"(\"clicks\") VALUES(@clicks_0), (@clicks_1), (@clicks_2), (@clicks_3), (@clicks_4), (@clicks_5), (@clicks_6), (@clicks_7), (@clicks_8), (@clicks_9)", sql);

            g.pgsql.Delete<TopicIgnore>().Where("1=1").ExecuteAffrows();
            var itemsIgnore = new List<TopicIgnore>();
            for (var a = 0; a < 2072; a++) itemsIgnore.Add(new TopicIgnore { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100, CreateTime = DateTime.Now });
            g.pgsql.Insert<TopicIgnore>().AppendData(itemsIgnore).IgnoreColumns(a => new { a.Title }).ExecuteAffrows();
            Assert.Equal(2072, itemsIgnore.Count);
            Assert.Equal(2072, g.pgsql.Select<TopicIgnore>().Where(a => a.Title == null).Count());
        }
        [Table(Name = "tb_topicIgnoreColumns")]
        class TopicIgnore
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }
            public int Clicks { get; set; }
            public string Title { get; set; }
            public DateTime CreateTime { get; set; }
        }
        [Fact]
        public void ExecuteAffrows()
        {
            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

            Assert.Equal(1, insert.AppendData(items.First()).ExecuteAffrows());
            Assert.Equal(10, insert.AppendData(items).ExecuteAffrows());
        }
        [Fact]
        public void ExecuteIdentity()
        {
            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

            Assert.NotEqual(0, insert.AppendData(items.First()).ExecuteIdentity());
        }
        [Fact]
        public void ExecuteInserted()
        {
            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

            insert.AppendData(items.First()).ExecuteInserted();
        }

        [Fact]
        public void AsTable()
        {
            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newTitle{a}", Clicks = a * 100 });

            var sql = insert.AppendData(items.First()).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal("INSERT INTO \"Topic_InsertAsTable\"(\"clicks\", \"title\", \"createtime\") VALUES(@clicks_0, @title_0, @createtime_0)", sql);

            sql = insert.AppendData(items).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal("INSERT INTO \"Topic_InsertAsTable\"(\"clicks\", \"title\", \"createtime\") VALUES(@clicks_0, @title_0, @createtime_0), (@clicks_1, @title_1, @createtime_1), (@clicks_2, @title_2, @createtime_2), (@clicks_3, @title_3, @createtime_3), (@clicks_4, @title_4, @createtime_4), (@clicks_5, @title_5, @createtime_5), (@clicks_6, @title_6, @createtime_6), (@clicks_7, @title_7, @createtime_7), (@clicks_8, @title_8, @createtime_8), (@clicks_9, @title_9, @createtime_9)", sql);

            sql = insert.AppendData(items).InsertColumns(a => a.Title).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal("INSERT INTO \"Topic_InsertAsTable\"(\"title\") VALUES(@title_0), (@title_1), (@title_2), (@title_3), (@title_4), (@title_5), (@title_6), (@title_7), (@title_8), (@title_9)", sql);

            sql = insert.AppendData(items).IgnoreColumns(a => a.CreateTime).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal("INSERT INTO \"Topic_InsertAsTable\"(\"clicks\", \"title\") VALUES(@clicks_0, @title_0), (@clicks_1, @title_1), (@clicks_2, @title_2), (@clicks_3, @title_3), (@clicks_4, @title_4), (@clicks_5, @title_5), (@clicks_6, @title_6), (@clicks_7, @title_7), (@clicks_8, @title_8), (@clicks_9, @title_9)", sql);

            sql = insert.AppendData(items).InsertColumns(a => a.Title).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal("INSERT INTO \"Topic_InsertAsTable\"(\"title\") VALUES(@title_0), (@title_1), (@title_2), (@title_3), (@title_4), (@title_5), (@title_6), (@title_7), (@title_8), (@title_9)", sql);

            sql = insert.AppendData(items).InsertColumns(a => new { a.Title, a.Clicks }).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal("INSERT INTO \"Topic_InsertAsTable\"(\"clicks\", \"title\") VALUES(@clicks_0, @title_0), (@clicks_1, @title_1), (@clicks_2, @title_2), (@clicks_3, @title_3), (@clicks_4, @title_4), (@clicks_5, @title_5), (@clicks_6, @title_6), (@clicks_7, @title_7), (@clicks_8, @title_8), (@clicks_9, @title_9)", sql);

            sql = insert.AppendData(items).IgnoreColumns(a => a.CreateTime).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal("INSERT INTO \"Topic_InsertAsTable\"(\"clicks\", \"title\") VALUES(@clicks_0, @title_0), (@clicks_1, @title_1), (@clicks_2, @title_2), (@clicks_3, @title_3), (@clicks_4, @title_4), (@clicks_5, @title_5), (@clicks_6, @title_6), (@clicks_7, @title_7), (@clicks_8, @title_8), (@clicks_9, @title_9)", sql);

            sql = insert.AppendData(items).IgnoreColumns(a => new { a.Title, a.CreateTime }).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal("INSERT INTO \"Topic_InsertAsTable\"(\"clicks\") VALUES(@clicks_0), (@clicks_1), (@clicks_2), (@clicks_3), (@clicks_4), (@clicks_5), (@clicks_6), (@clicks_7), (@clicks_8), (@clicks_9)", sql);
        }
    }
}
