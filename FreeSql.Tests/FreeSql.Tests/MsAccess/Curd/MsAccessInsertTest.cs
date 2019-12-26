using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.MsAccess
{
    public class MsAccessInsertTest
    {
        IInsert<Topic> insert => g.msaccess.Insert<Topic>(); //��������

        [Table(Name = "tb_topic")]
        class Topic
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }
            public int? Clicks { get; set; }
            public int TypeGuid { get; set; }
            public TestTypeInfo Type { get; set; }
            public string Title { get; set; }
            public DateTime CreateTime { get; set; }
        }

        [Fact]
        public void AppendData()
        {
            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100, CreateTime = DateTime.Parse("2019-09-19 20:09:37") });

            var sql = insert.IgnoreColumns(a => a.TypeGuid).AppendData(items.First()).ToSql();
            Assert.Equal("INSERT INTO [tb_topic]([Clicks], [Title], [CreateTime]) VALUES(0, 'newtitle0', '2019-09-19 20:09:37')", sql);

            sql = insert.IgnoreColumns(a => a.TypeGuid).AppendData(items).ToSql();
            Assert.Equal("INSERT INTO [tb_topic]([Clicks], [Title], [CreateTime]) VALUES(0, 'newtitle0', '2019-09-19 20:09:37'), (100, 'newtitle1', '2019-09-19 20:09:37'), (200, 'newtitle2', '2019-09-19 20:09:37'), (300, 'newtitle3', '2019-09-19 20:09:37'), (400, 'newtitle4', '2019-09-19 20:09:37'), (500, 'newtitle5', '2019-09-19 20:09:37'), (600, 'newtitle6', '2019-09-19 20:09:37'), (700, 'newtitle7', '2019-09-19 20:09:37'), (800, 'newtitle8', '2019-09-19 20:09:37'), (900, 'newtitle9', '2019-09-19 20:09:37')", sql);

            sql = insert.AppendData(items).InsertColumns(a => a.Title).ToSql();
            Assert.Equal("INSERT INTO [tb_topic]([Title]) VALUES('newtitle0'), ('newtitle1'), ('newtitle2'), ('newtitle3'), ('newtitle4'), ('newtitle5'), ('newtitle6'), ('newtitle7'), ('newtitle8'), ('newtitle9')", sql);

            sql = insert.IgnoreColumns(a => new { a.CreateTime, a.TypeGuid }).AppendData(items).ToSql();
            Assert.Equal("INSERT INTO [tb_topic]([Clicks], [Title]) VALUES(0, 'newtitle0'), (100, 'newtitle1'), (200, 'newtitle2'), (300, 'newtitle3'), (400, 'newtitle4'), (500, 'newtitle5'), (600, 'newtitle6'), (700, 'newtitle7'), (800, 'newtitle8'), (900, 'newtitle9')", sql);
        }

        [Fact]
        public void InsertColumns()
        {
            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100, CreateTime = DateTime.Now });

            var sql = insert.AppendData(items).InsertColumns(a => a.Title).ToSql();
            Assert.Equal("INSERT INTO [tb_topic]([Title]) VALUES('newtitle0'), ('newtitle1'), ('newtitle2'), ('newtitle3'), ('newtitle4'), ('newtitle5'), ('newtitle6'), ('newtitle7'), ('newtitle8'), ('newtitle9')", sql);

            sql = insert.AppendData(items).InsertColumns(a => new { a.Title, a.Clicks }).ToSql();
            Assert.Equal("INSERT INTO [tb_topic]([Clicks], [Title]) VALUES(0, 'newtitle0'), (100, 'newtitle1'), (200, 'newtitle2'), (300, 'newtitle3'), (400, 'newtitle4'), (500, 'newtitle5'), (600, 'newtitle6'), (700, 'newtitle7'), (800, 'newtitle8'), (900, 'newtitle9')", sql);
        }
        [Fact]
        public void IgnoreColumns()
        {
            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100, CreateTime = DateTime.Now });

            var sql = insert.AppendData(items).IgnoreColumns(a => new { a.CreateTime, a.TypeGuid }).ToSql();
            Assert.Equal("INSERT INTO [tb_topic]([Clicks], [Title]) VALUES(0, 'newtitle0'), (100, 'newtitle1'), (200, 'newtitle2'), (300, 'newtitle3'), (400, 'newtitle4'), (500, 'newtitle5'), (600, 'newtitle6'), (700, 'newtitle7'), (800, 'newtitle8'), (900, 'newtitle9')", sql);

            sql = insert.AppendData(items).IgnoreColumns(a => new { a.Title, a.CreateTime, a.TypeGuid }).ToSql();
            Assert.Equal("INSERT INTO [tb_topic]([Clicks]) VALUES(0), (100), (200), (300), (400), (500), (600), (700), (800), (900)", sql);

            g.msaccess.Delete<TopicIgnore>().Where("1=1").ExecuteAffrows();
            var itemsIgnore = new List<TopicIgnore>();
            for (var a = 0; a < 32; a++) itemsIgnore.Add(new TopicIgnore { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100, CreateTime = DateTime.Now });
            g.msaccess.Insert<TopicIgnore>().AppendData(itemsIgnore).IgnoreColumns(a => new { a.Title }).ExecuteAffrows();
            Assert.Equal(32, itemsIgnore.Count);
            Assert.Equal(32, g.msaccess.Select<TopicIgnore>().Where(a => a.Title == null).Count());
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
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100, CreateTime = DateTime.Now });

            Assert.Equal(1, insert.AppendData(items.First()).ExecuteAffrows());
            Assert.Equal(10, insert.AppendData(items).ExecuteAffrows());

            //items = Enumerable.Range(0, 9989).Select(a => new Topic { Title = "newtitle" + a, CreateTime = DateTime.Now }).ToList();
            //Assert.Equal(9989, g.msaccess.Insert<Topic>(items).ExecuteAffrows());
        }
        [Fact]
        public void ExecuteIdentity()
        {
            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100, CreateTime = DateTime.Now });

            Assert.NotEqual(0, insert.AppendData(items.First()).ExecuteIdentity());


            //items = Enumerable.Range(0, 9999).Select(a => new Topic { Title = "newtitle" + a, CreateTime = DateTime.Now }).ToList();
            //var lastId = g.sqlite.Select<Topic>().Max(a => a.Id);
            //Assert.NotEqual(lastId, g.msaccess.Insert<Topic>(items).ExecuteIdentity());
        }

        [Fact]
        public void AsTable()
        {
            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100, CreateTime = DateTime.Parse("2019-09-19 20:01:51") });

            var sql = insert.IgnoreColumns(a => a.TypeGuid).AppendData(items.First()).AsTable(a => "tb_topicAsTable").ToSql();
            Assert.Equal("INSERT INTO [tb_topicAsTable]([Clicks], [Title], [CreateTime]) VALUES(0, 'newtitle0', '2019-09-19 20:01:51')", sql);

            sql = insert.IgnoreColumns(a => a.TypeGuid).AppendData(items).AsTable(a => "tb_topicAsTable").ToSql();
            Assert.Equal("INSERT INTO [tb_topicAsTable]([Clicks], [Title], [CreateTime]) VALUES(0, 'newtitle0', '2019-09-19 20:01:51'), (100, 'newtitle1', '2019-09-19 20:01:51'), (200, 'newtitle2', '2019-09-19 20:01:51'), (300, 'newtitle3', '2019-09-19 20:01:51'), (400, 'newtitle4', '2019-09-19 20:01:51'), (500, 'newtitle5', '2019-09-19 20:01:51'), (600, 'newtitle6', '2019-09-19 20:01:51'), (700, 'newtitle7', '2019-09-19 20:01:51'), (800, 'newtitle8', '2019-09-19 20:01:51'), (900, 'newtitle9', '2019-09-19 20:01:51')", sql);

            sql = insert.IgnoreColumns(a => a.TypeGuid).AppendData(items).InsertColumns(a => a.Title).AsTable(a => "tb_topicAsTable").ToSql();
            Assert.Equal("INSERT INTO [tb_topicAsTable]([Title]) VALUES('newtitle0'), ('newtitle1'), ('newtitle2'), ('newtitle3'), ('newtitle4'), ('newtitle5'), ('newtitle6'), ('newtitle7'), ('newtitle8'), ('newtitle9')", sql);

            sql = insert.IgnoreColumns(a => new { a.CreateTime, a.TypeGuid }).AppendData(items).AsTable(a => "tb_topicAsTable").ToSql();
            Assert.Equal("INSERT INTO [tb_topicAsTable]([Clicks], [Title]) VALUES(0, 'newtitle0'), (100, 'newtitle1'), (200, 'newtitle2'), (300, 'newtitle3'), (400, 'newtitle4'), (500, 'newtitle5'), (600, 'newtitle6'), (700, 'newtitle7'), (800, 'newtitle8'), (900, 'newtitle9')", sql);

            sql = insert.IgnoreColumns(a => new { a.Title, a.TypeGuid }).InsertColumns(a => a.Title).AppendData(items).AsTable(a => "tb_topicAsTable").ToSql();
            Assert.Equal("INSERT INTO [tb_topicAsTable]([Title]) VALUES('newtitle0'), ('newtitle1'), ('newtitle2'), ('newtitle3'), ('newtitle4'), ('newtitle5'), ('newtitle6'), ('newtitle7'), ('newtitle8'), ('newtitle9')", sql);

            sql = insert.IgnoreColumns(a => a.TypeGuid).AppendData(items).InsertColumns(a => new { a.Title, a.Clicks }).AsTable(a => "tb_topicAsTable").ToSql();
            Assert.Equal("INSERT INTO [tb_topicAsTable]([Clicks], [Title]) VALUES(0, 'newtitle0'), (100, 'newtitle1'), (200, 'newtitle2'), (300, 'newtitle3'), (400, 'newtitle4'), (500, 'newtitle5'), (600, 'newtitle6'), (700, 'newtitle7'), (800, 'newtitle8'), (900, 'newtitle9')", sql);

            sql = insert.IgnoreColumns(a => new { a.CreateTime, a.TypeGuid }).AppendData(items).AsTable(a => "tb_topicAsTable").ToSql();
            Assert.Equal("INSERT INTO [tb_topicAsTable]([Clicks], [Title]) VALUES(0, 'newtitle0'), (100, 'newtitle1'), (200, 'newtitle2'), (300, 'newtitle3'), (400, 'newtitle4'), (500, 'newtitle5'), (600, 'newtitle6'), (700, 'newtitle7'), (800, 'newtitle8'), (900, 'newtitle9')", sql);

            sql = insert.IgnoreColumns(a => new { a.CreateTime, a.Title, a.TypeGuid }).AppendData(items).AsTable(a => "tb_topicAsTable").ToSql();
            Assert.Equal("INSERT INTO [tb_topicAsTable]([Clicks]) VALUES(0), (100), (200), (300), (400), (500), (600), (700), (800), (900)", sql);
        }
    }
}
