using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.Firebird
{
    public class FirebirdInsertTest
    {

        IInsert<Topic> insert => g.firebird.Insert<Topic>();

        [Table(Name = "TB_TOPIC_INSERT")]
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
            Assert.Equal("INSERT INTO \"TB_TOPIC_INSERT\"(\"CLICKS\", \"TITLE\", \"CREATETIME\") VALUES(@Clicks_0, @Title_0, @CreateTime_0)", sql);

            sql = insert.AppendData(items).ToSql();
            Assert.Equal(@"INSERT INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"", ""CREATETIME"") SELECT FIRST 1 @Clicks_0, @Title_0, @CreateTime_0 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_1, @Title_1, @CreateTime_1 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_2, @Title_2, @CreateTime_2 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_3, @Title_3, @CreateTime_3 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_4, @Title_4, @CreateTime_4 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_5, @Title_5, @CreateTime_5 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_6, @Title_6, @CreateTime_6 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_7, @Title_7, @CreateTime_7 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_8, @Title_8, @CreateTime_8 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_9, @Title_9, @CreateTime_9 FROM rdb$database", sql);

            sql = insert.AppendData(items).InsertColumns(a => a.Title).ToSql();
            Assert.Equal(@"INSERT INTO ""TB_TOPIC_INSERT""(""TITLE"") SELECT FIRST 1 @Title_0 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Title_1 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Title_2 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Title_3 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Title_4 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Title_5 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Title_6 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Title_7 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Title_8 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Title_9 FROM rdb$database", sql);

            sql = insert.AppendData(items).IgnoreColumns(a => a.CreateTime).ToSql();
            Assert.Equal(@"INSERT INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") SELECT FIRST 1 @Clicks_0, @Title_0 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_1, @Title_1 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_2, @Title_2 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_3, @Title_3 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_4, @Title_4 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_5, @Title_5 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_6, @Title_6 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_7, @Title_7 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_8, @Title_8 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_9, @Title_9 FROM rdb$database", sql);
        }

        [Fact]
        public void InsertColumns()
        {
            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

            var sql = insert.AppendData(items).InsertColumns(a => a.Title).ToSql();
            Assert.Equal(@"INSERT INTO ""TB_TOPIC_INSERT""(""TITLE"") SELECT FIRST 1 @Title_0 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Title_1 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Title_2 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Title_3 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Title_4 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Title_5 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Title_6 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Title_7 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Title_8 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Title_9 FROM rdb$database", sql);

            sql = insert.AppendData(items).InsertColumns(a => new { a.Title, a.Clicks }).ToSql();
            Assert.Equal(@"INSERT INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") SELECT FIRST 1 @Clicks_0, @Title_0 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_1, @Title_1 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_2, @Title_2 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_3, @Title_3 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_4, @Title_4 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_5, @Title_5 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_6, @Title_6 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_7, @Title_7 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_8, @Title_8 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_9, @Title_9 FROM rdb$database", sql);
        }
        [Fact]
        public void IgnoreColumns()
        {
            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

            var sql = insert.AppendData(items).IgnoreColumns(a => a.CreateTime).ToSql();
            Assert.Equal(@"INSERT INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") SELECT FIRST 1 @Clicks_0, @Title_0 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_1, @Title_1 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_2, @Title_2 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_3, @Title_3 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_4, @Title_4 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_5, @Title_5 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_6, @Title_6 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_7, @Title_7 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_8, @Title_8 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_9, @Title_9 FROM rdb$database", sql);

            sql = insert.AppendData(items).IgnoreColumns(a => new { a.Title, a.CreateTime }).ToSql();
            Assert.Equal(@"INSERT INTO ""TB_TOPIC_INSERT""(""CLICKS"") SELECT FIRST 1 @Clicks_0 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_1 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_2 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_3 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_4 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_5 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_6 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_7 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_8 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_9 FROM rdb$database", sql);

            g.firebird.Delete<TopicIgnore>().Where("1=1").ExecuteAffrows();
            var itemsIgnore = new List<TopicIgnore>();
            for (var a = 0; a < 2072; a++) itemsIgnore.Add(new TopicIgnore { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100, CreateTime = DateTime.Now });
            g.firebird.Insert<TopicIgnore>().AppendData(itemsIgnore).IgnoreColumns(a => new { a.Title }).ExecuteAffrows();
            Assert.Equal(2072, itemsIgnore.Count);
            Assert.Equal(2072, g.firebird.Select<TopicIgnore>().Where(a => a.Title == null).Count());
        }
        [Table(Name = "TB_TOPICIGNORECOLUMNS")]
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
            Assert.Equal(10, insert.NoneParameter().AppendData(items).ExecuteAffrows());

            Assert.Equal(10, g.firebird.Select<Topic>().Limit(10).InsertInto(null, a => new Topic
            {
                Title = a.Title
            }));
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

            var ret1 = insert.AppendData(items.First()).ExecuteInserted();
            var ret2 = insert.NoneParameter().AppendData(items).ExecuteInserted();
        }

        [Fact]
        public void AsTable()
        {
            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newTitle{a}", Clicks = a * 100 });

            var sql = insert.AppendData(items.First()).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal("INSERT INTO \"TOPIC_INSERTASTABLE\"(\"CLICKS\", \"TITLE\", \"CREATETIME\") VALUES(@Clicks_0, @Title_0, @CreateTime_0)", sql);

            sql = insert.AppendData(items).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal(@"INSERT INTO ""TOPIC_INSERTASTABLE""(""CLICKS"", ""TITLE"", ""CREATETIME"") SELECT FIRST 1 @Clicks_0, @Title_0, @CreateTime_0 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_1, @Title_1, @CreateTime_1 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_2, @Title_2, @CreateTime_2 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_3, @Title_3, @CreateTime_3 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_4, @Title_4, @CreateTime_4 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_5, @Title_5, @CreateTime_5 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_6, @Title_6, @CreateTime_6 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_7, @Title_7, @CreateTime_7 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_8, @Title_8, @CreateTime_8 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_9, @Title_9, @CreateTime_9 FROM rdb$database", sql);

            sql = insert.AppendData(items).InsertColumns(a => a.Title).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal(@"INSERT INTO ""TOPIC_INSERTASTABLE""(""TITLE"") SELECT FIRST 1 @Title_0 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Title_1 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Title_2 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Title_3 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Title_4 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Title_5 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Title_6 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Title_7 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Title_8 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Title_9 FROM rdb$database", sql);

            sql = insert.AppendData(items).IgnoreColumns(a => a.CreateTime).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal(@"INSERT INTO ""TOPIC_INSERTASTABLE""(""CLICKS"", ""TITLE"") SELECT FIRST 1 @Clicks_0, @Title_0 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_1, @Title_1 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_2, @Title_2 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_3, @Title_3 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_4, @Title_4 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_5, @Title_5 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_6, @Title_6 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_7, @Title_7 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_8, @Title_8 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_9, @Title_9 FROM rdb$database", sql);

            sql = insert.AppendData(items).InsertColumns(a => a.Title).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal(@"INSERT INTO ""TOPIC_INSERTASTABLE""(""TITLE"") SELECT FIRST 1 @Title_0 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Title_1 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Title_2 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Title_3 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Title_4 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Title_5 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Title_6 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Title_7 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Title_8 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Title_9 FROM rdb$database", sql);

            sql = insert.AppendData(items).InsertColumns(a => new { a.Title, a.Clicks }).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal(@"INSERT INTO ""TOPIC_INSERTASTABLE""(""CLICKS"", ""TITLE"") SELECT FIRST 1 @Clicks_0, @Title_0 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_1, @Title_1 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_2, @Title_2 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_3, @Title_3 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_4, @Title_4 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_5, @Title_5 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_6, @Title_6 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_7, @Title_7 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_8, @Title_8 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_9, @Title_9 FROM rdb$database", sql);

            sql = insert.AppendData(items).IgnoreColumns(a => a.CreateTime).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal(@"INSERT INTO ""TOPIC_INSERTASTABLE""(""CLICKS"", ""TITLE"") SELECT FIRST 1 @Clicks_0, @Title_0 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_1, @Title_1 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_2, @Title_2 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_3, @Title_3 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_4, @Title_4 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_5, @Title_5 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_6, @Title_6 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_7, @Title_7 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_8, @Title_8 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_9, @Title_9 FROM rdb$database", sql);

            sql = insert.AppendData(items).IgnoreColumns(a => new { a.Title, a.CreateTime }).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal(@"INSERT INTO ""TOPIC_INSERTASTABLE""(""CLICKS"") SELECT FIRST 1 @Clicks_0 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_1 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_2 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_3 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_4 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_5 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_6 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_7 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_8 FROM rdb$database 
UNION ALL
 SELECT FIRST 1 @Clicks_9 FROM rdb$database", sql);
        }
    }
}
