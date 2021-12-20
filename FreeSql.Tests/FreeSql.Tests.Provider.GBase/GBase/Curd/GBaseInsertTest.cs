using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.GBase
{
    public class GBaseInsertTest
    {

        IInsert<Topic> insert => g.gbase.Insert<Topic>();

        [Table(Name = "TB_TOPIC_INSERT")]
        class Topic
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }
            public int Clicks { get; set; }
            public string Title { get; set; }
            public DateTime CreateTime { get; set; }
        }

        [Fact]
        public void AppendData()
        {
            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

            var sql = insert.AppendData(items.First()).ToSql();
            Assert.Equal("INSERT INTO TB_TOPIC_INSERT(Clicks, Title, CreateTime) VALUES(0, 'newtitle0', '0001-01-01 00:00:00.000')", sql);

            sql = insert.AppendData(items).ToSql();
            Assert.Equal(@"INSERT INTO TB_TOPIC_INSERT(Clicks, Title, CreateTime)
SELECT * FROM (
 SELECT 0, 'newtitle0', '0001-01-01 00:00:00.000' FROM dual 
UNION ALL
 SELECT 100, 'newtitle1', '0001-01-01 00:00:00.000' FROM dual 
UNION ALL
 SELECT 200, 'newtitle2', '0001-01-01 00:00:00.000' FROM dual 
UNION ALL
 SELECT 300, 'newtitle3', '0001-01-01 00:00:00.000' FROM dual 
UNION ALL
 SELECT 400, 'newtitle4', '0001-01-01 00:00:00.000' FROM dual 
UNION ALL
 SELECT 500, 'newtitle5', '0001-01-01 00:00:00.000' FROM dual 
UNION ALL
 SELECT 600, 'newtitle6', '0001-01-01 00:00:00.000' FROM dual 
UNION ALL
 SELECT 700, 'newtitle7', '0001-01-01 00:00:00.000' FROM dual 
UNION ALL
 SELECT 800, 'newtitle8', '0001-01-01 00:00:00.000' FROM dual 
UNION ALL
 SELECT 900, 'newtitle9', '0001-01-01 00:00:00.000' FROM dual
) ftbtmp", sql);

            sql = insert.AppendData(items).InsertColumns(a => a.Title).ToSql();
            Assert.Equal(@"INSERT INTO TB_TOPIC_INSERT(Title)
SELECT * FROM (
 SELECT 'newtitle0' FROM dual 
UNION ALL
 SELECT 'newtitle1' FROM dual 
UNION ALL
 SELECT 'newtitle2' FROM dual 
UNION ALL
 SELECT 'newtitle3' FROM dual 
UNION ALL
 SELECT 'newtitle4' FROM dual 
UNION ALL
 SELECT 'newtitle5' FROM dual 
UNION ALL
 SELECT 'newtitle6' FROM dual 
UNION ALL
 SELECT 'newtitle7' FROM dual 
UNION ALL
 SELECT 'newtitle8' FROM dual 
UNION ALL
 SELECT 'newtitle9' FROM dual
) ftbtmp", sql);

            sql = insert.AppendData(items).IgnoreColumns(a => a.CreateTime).ToSql();
            Assert.Equal(@"INSERT INTO TB_TOPIC_INSERT(Clicks, Title)
SELECT * FROM (
 SELECT 0, 'newtitle0' FROM dual 
UNION ALL
 SELECT 100, 'newtitle1' FROM dual 
UNION ALL
 SELECT 200, 'newtitle2' FROM dual 
UNION ALL
 SELECT 300, 'newtitle3' FROM dual 
UNION ALL
 SELECT 400, 'newtitle4' FROM dual 
UNION ALL
 SELECT 500, 'newtitle5' FROM dual 
UNION ALL
 SELECT 600, 'newtitle6' FROM dual 
UNION ALL
 SELECT 700, 'newtitle7' FROM dual 
UNION ALL
 SELECT 800, 'newtitle8' FROM dual 
UNION ALL
 SELECT 900, 'newtitle9' FROM dual
) ftbtmp", sql);
        }

        [Fact]
        public void InsertColumns()
        {
            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

            var sql = insert.AppendData(items).InsertColumns(a => a.Title).ToSql();
            Assert.Equal(@"INSERT INTO TB_TOPIC_INSERT(Title)
SELECT * FROM (
 SELECT 'newtitle0' FROM dual 
UNION ALL
 SELECT 'newtitle1' FROM dual 
UNION ALL
 SELECT 'newtitle2' FROM dual 
UNION ALL
 SELECT 'newtitle3' FROM dual 
UNION ALL
 SELECT 'newtitle4' FROM dual 
UNION ALL
 SELECT 'newtitle5' FROM dual 
UNION ALL
 SELECT 'newtitle6' FROM dual 
UNION ALL
 SELECT 'newtitle7' FROM dual 
UNION ALL
 SELECT 'newtitle8' FROM dual 
UNION ALL
 SELECT 'newtitle9' FROM dual
) ftbtmp", sql);

            sql = insert.AppendData(items).InsertColumns(a => new { a.Title, a.Clicks }).ToSql();
            Assert.Equal(@"INSERT INTO TB_TOPIC_INSERT(Clicks, Title)
SELECT * FROM (
 SELECT 0, 'newtitle0' FROM dual 
UNION ALL
 SELECT 100, 'newtitle1' FROM dual 
UNION ALL
 SELECT 200, 'newtitle2' FROM dual 
UNION ALL
 SELECT 300, 'newtitle3' FROM dual 
UNION ALL
 SELECT 400, 'newtitle4' FROM dual 
UNION ALL
 SELECT 500, 'newtitle5' FROM dual 
UNION ALL
 SELECT 600, 'newtitle6' FROM dual 
UNION ALL
 SELECT 700, 'newtitle7' FROM dual 
UNION ALL
 SELECT 800, 'newtitle8' FROM dual 
UNION ALL
 SELECT 900, 'newtitle9' FROM dual
) ftbtmp", sql);
        }
        [Fact]
        public void IgnoreColumns()
        {
            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

            var sql = insert.AppendData(items).IgnoreColumns(a => a.CreateTime).ToSql();
            Assert.Equal(@"INSERT INTO TB_TOPIC_INSERT(Clicks, Title)
SELECT * FROM (
 SELECT 0, 'newtitle0' FROM dual 
UNION ALL
 SELECT 100, 'newtitle1' FROM dual 
UNION ALL
 SELECT 200, 'newtitle2' FROM dual 
UNION ALL
 SELECT 300, 'newtitle3' FROM dual 
UNION ALL
 SELECT 400, 'newtitle4' FROM dual 
UNION ALL
 SELECT 500, 'newtitle5' FROM dual 
UNION ALL
 SELECT 600, 'newtitle6' FROM dual 
UNION ALL
 SELECT 700, 'newtitle7' FROM dual 
UNION ALL
 SELECT 800, 'newtitle8' FROM dual 
UNION ALL
 SELECT 900, 'newtitle9' FROM dual
) ftbtmp", sql);

            sql = insert.AppendData(items).IgnoreColumns(a => new { a.Title, a.CreateTime }).ToSql();
            Assert.Equal(@"INSERT INTO TB_TOPIC_INSERT(Clicks)
SELECT * FROM (
 SELECT 0 FROM dual 
UNION ALL
 SELECT 100 FROM dual 
UNION ALL
 SELECT 200 FROM dual 
UNION ALL
 SELECT 300 FROM dual 
UNION ALL
 SELECT 400 FROM dual 
UNION ALL
 SELECT 500 FROM dual 
UNION ALL
 SELECT 600 FROM dual 
UNION ALL
 SELECT 700 FROM dual 
UNION ALL
 SELECT 800 FROM dual 
UNION ALL
 SELECT 900 FROM dual
) ftbtmp", sql);

            g.gbase.Delete<TopicIgnore>().Where("1=1").ExecuteAffrows();
            var itemsIgnore = new List<TopicIgnore>();
            for (var a = 0; a < 2072; a++) itemsIgnore.Add(new TopicIgnore { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100, CreateTime = DateTime.Now });
            g.gbase.Insert<TopicIgnore>().AppendData(itemsIgnore).IgnoreColumns(a => new { a.Title }).ExecuteAffrows();
            Assert.Equal(2072, itemsIgnore.Count);
            Assert.Equal(2072, g.gbase.Select<TopicIgnore>().Where(a => a.Title == null).Count());
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

            Assert.Equal(10, g.gbase.Select<Topic>().Limit(10).InsertInto(null, a => new Topic
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

            Assert.Throws<NotImplementedException>(() => insert.AppendData(items.First()).ExecuteInserted());
        }

        [Fact]
        public void AsTable()
        {
            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newTitle{a}", Clicks = a * 100 });

            var sql = insert.AppendData(items.First()).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal("INSERT INTO Topic_InsertAsTable(Clicks, Title, CreateTime) VALUES(0, 'newTitle0', '0001-01-01 00:00:00.000')", sql);
            
            sql = insert.AppendData(items).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal(@"INSERT INTO Topic_InsertAsTable(Clicks, Title, CreateTime)
SELECT * FROM (
 SELECT 0, 'newTitle0', '0001-01-01 00:00:00.000' FROM dual 
UNION ALL
 SELECT 100, 'newTitle1', '0001-01-01 00:00:00.000' FROM dual 
UNION ALL
 SELECT 200, 'newTitle2', '0001-01-01 00:00:00.000' FROM dual 
UNION ALL
 SELECT 300, 'newTitle3', '0001-01-01 00:00:00.000' FROM dual 
UNION ALL
 SELECT 400, 'newTitle4', '0001-01-01 00:00:00.000' FROM dual 
UNION ALL
 SELECT 500, 'newTitle5', '0001-01-01 00:00:00.000' FROM dual 
UNION ALL
 SELECT 600, 'newTitle6', '0001-01-01 00:00:00.000' FROM dual 
UNION ALL
 SELECT 700, 'newTitle7', '0001-01-01 00:00:00.000' FROM dual 
UNION ALL
 SELECT 800, 'newTitle8', '0001-01-01 00:00:00.000' FROM dual 
UNION ALL
 SELECT 900, 'newTitle9', '0001-01-01 00:00:00.000' FROM dual
) ftbtmp", sql);

            sql = insert.AppendData(items).InsertColumns(a => a.Title).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal(@"INSERT INTO Topic_InsertAsTable(Title)
SELECT * FROM (
 SELECT 'newTitle0' FROM dual 
UNION ALL
 SELECT 'newTitle1' FROM dual 
UNION ALL
 SELECT 'newTitle2' FROM dual 
UNION ALL
 SELECT 'newTitle3' FROM dual 
UNION ALL
 SELECT 'newTitle4' FROM dual 
UNION ALL
 SELECT 'newTitle5' FROM dual 
UNION ALL
 SELECT 'newTitle6' FROM dual 
UNION ALL
 SELECT 'newTitle7' FROM dual 
UNION ALL
 SELECT 'newTitle8' FROM dual 
UNION ALL
 SELECT 'newTitle9' FROM dual
) ftbtmp", sql);

            sql = insert.AppendData(items).IgnoreColumns(a => a.CreateTime).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal(@"INSERT INTO Topic_InsertAsTable(Clicks, Title)
SELECT * FROM (
 SELECT 0, 'newTitle0' FROM dual 
UNION ALL
 SELECT 100, 'newTitle1' FROM dual 
UNION ALL
 SELECT 200, 'newTitle2' FROM dual 
UNION ALL
 SELECT 300, 'newTitle3' FROM dual 
UNION ALL
 SELECT 400, 'newTitle4' FROM dual 
UNION ALL
 SELECT 500, 'newTitle5' FROM dual 
UNION ALL
 SELECT 600, 'newTitle6' FROM dual 
UNION ALL
 SELECT 700, 'newTitle7' FROM dual 
UNION ALL
 SELECT 800, 'newTitle8' FROM dual 
UNION ALL
 SELECT 900, 'newTitle9' FROM dual
) ftbtmp", sql);

            sql = insert.AppendData(items).InsertColumns(a => a.Title).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal(@"INSERT INTO Topic_InsertAsTable(Title)
SELECT * FROM (
 SELECT 'newTitle0' FROM dual 
UNION ALL
 SELECT 'newTitle1' FROM dual 
UNION ALL
 SELECT 'newTitle2' FROM dual 
UNION ALL
 SELECT 'newTitle3' FROM dual 
UNION ALL
 SELECT 'newTitle4' FROM dual 
UNION ALL
 SELECT 'newTitle5' FROM dual 
UNION ALL
 SELECT 'newTitle6' FROM dual 
UNION ALL
 SELECT 'newTitle7' FROM dual 
UNION ALL
 SELECT 'newTitle8' FROM dual 
UNION ALL
 SELECT 'newTitle9' FROM dual
) ftbtmp", sql);

            sql = insert.AppendData(items).InsertColumns(a => new { a.Title, a.Clicks }).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal(@"INSERT INTO Topic_InsertAsTable(Clicks, Title)
SELECT * FROM (
 SELECT 0, 'newTitle0' FROM dual 
UNION ALL
 SELECT 100, 'newTitle1' FROM dual 
UNION ALL
 SELECT 200, 'newTitle2' FROM dual 
UNION ALL
 SELECT 300, 'newTitle3' FROM dual 
UNION ALL
 SELECT 400, 'newTitle4' FROM dual 
UNION ALL
 SELECT 500, 'newTitle5' FROM dual 
UNION ALL
 SELECT 600, 'newTitle6' FROM dual 
UNION ALL
 SELECT 700, 'newTitle7' FROM dual 
UNION ALL
 SELECT 800, 'newTitle8' FROM dual 
UNION ALL
 SELECT 900, 'newTitle9' FROM dual
) ftbtmp", sql);

            sql = insert.AppendData(items).IgnoreColumns(a => a.CreateTime).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal(@"INSERT INTO Topic_InsertAsTable(Clicks, Title)
SELECT * FROM (
 SELECT 0, 'newTitle0' FROM dual 
UNION ALL
 SELECT 100, 'newTitle1' FROM dual 
UNION ALL
 SELECT 200, 'newTitle2' FROM dual 
UNION ALL
 SELECT 300, 'newTitle3' FROM dual 
UNION ALL
 SELECT 400, 'newTitle4' FROM dual 
UNION ALL
 SELECT 500, 'newTitle5' FROM dual 
UNION ALL
 SELECT 600, 'newTitle6' FROM dual 
UNION ALL
 SELECT 700, 'newTitle7' FROM dual 
UNION ALL
 SELECT 800, 'newTitle8' FROM dual 
UNION ALL
 SELECT 900, 'newTitle9' FROM dual
) ftbtmp", sql);

            sql = insert.AppendData(items).IgnoreColumns(a => new { a.Title, a.CreateTime }).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal(@"INSERT INTO Topic_InsertAsTable(Clicks)
SELECT * FROM (
 SELECT 0 FROM dual 
UNION ALL
 SELECT 100 FROM dual 
UNION ALL
 SELECT 200 FROM dual 
UNION ALL
 SELECT 300 FROM dual 
UNION ALL
 SELECT 400 FROM dual 
UNION ALL
 SELECT 500 FROM dual 
UNION ALL
 SELECT 600 FROM dual 
UNION ALL
 SELECT 700 FROM dual 
UNION ALL
 SELECT 800 FROM dual 
UNION ALL
 SELECT 900 FROM dual
) ftbtmp", sql);
        }
    }
}
