using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.Odbc.MySql
{
    public class MySqlInsertTest
    {

        IInsert<Topic> insert => g.mysql.Insert<Topic>(); //��������

        [Table(Name = "tb_topic")]
        class Topic
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }
            public int Clicks { get; set; }
            public string Title { get; set; }
            public DateTime CreateTime { get; set; }
        }
        class TestEnumInsertTb
        {
            [Column(IsIdentity = true)]
            public int id { get; set; }
            public TestEnumInserTbType type { get; set; }
            public DateTime time { get; set; } = new DateTime();
        }
        enum TestEnumInserTbType { str1, biggit, sum211 }

        [Fact]
        public void AppendData()
        {
            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

            var sql = insert.AppendData(items.First()).ToSql();
            Assert.Equal("INSERT INTO `tb_topic`(`Clicks`, `Title`, `CreateTime`) VALUES(0, 'newtitle0', '0001-01-01 00:00:00.000')", sql);

            sql = insert.AppendData(items).ToSql();
            Assert.Equal("INSERT INTO `tb_topic`(`Clicks`, `Title`, `CreateTime`) VALUES(0, 'newtitle0', '0001-01-01 00:00:00.000'), (100, 'newtitle1', '0001-01-01 00:00:00.000'), (200, 'newtitle2', '0001-01-01 00:00:00.000'), (300, 'newtitle3', '0001-01-01 00:00:00.000'), (400, 'newtitle4', '0001-01-01 00:00:00.000'), (500, 'newtitle5', '0001-01-01 00:00:00.000'), (600, 'newtitle6', '0001-01-01 00:00:00.000'), (700, 'newtitle7', '0001-01-01 00:00:00.000'), (800, 'newtitle8', '0001-01-01 00:00:00.000'), (900, 'newtitle9', '0001-01-01 00:00:00.000')", sql);

            sql = insert.AppendData(items).InsertColumns(a => a.Title).ToSql();
            Assert.Equal("INSERT INTO `tb_topic`(`Title`) VALUES('newtitle0'), ('newtitle1'), ('newtitle2'), ('newtitle3'), ('newtitle4'), ('newtitle5'), ('newtitle6'), ('newtitle7'), ('newtitle8'), ('newtitle9')", sql);

            sql = insert.AppendData(items).IgnoreColumns(a => a.CreateTime).ToSql();
            Assert.Equal("INSERT INTO `tb_topic`(`Clicks`, `Title`) VALUES(0, 'newtitle0'), (100, 'newtitle1'), (200, 'newtitle2'), (300, 'newtitle3'), (400, 'newtitle4'), (500, 'newtitle5'), (600, 'newtitle6'), (700, 'newtitle7'), (800, 'newtitle8'), (900, 'newtitle9')", sql);

            sql = g.mysql.Insert<TestEnumInsertTb>().AppendData(new TestEnumInsertTb { type = TestEnumInserTbType.sum211, time = DateTime.Parse("2019-09-19 21:26:51.030") }).ToSql();
            Assert.Equal("INSERT INTO `TestEnumInsertTb`(`type`, `time`) VALUES('sum211', '2019-09-19 21:26:51.030')", sql);

            sql = g.mysql.Insert<TestEnumInsertTb>().AppendData(new TestEnumInsertTb { type = TestEnumInserTbType.sum211 }).NoneParameter().ToSql();
            Assert.Equal("INSERT INTO `TestEnumInsertTb`(`type`, `time`) VALUES('sum211', '0001-01-01 00:00:00.000')", sql);
        }

        [Fact]
        public void InsertColumns()
        {
            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

            var sql = insert.AppendData(items).InsertColumns(a => a.Title).ToSql();
            Assert.Equal("INSERT INTO `tb_topic`(`Title`) VALUES('newtitle0'), ('newtitle1'), ('newtitle2'), ('newtitle3'), ('newtitle4'), ('newtitle5'), ('newtitle6'), ('newtitle7'), ('newtitle8'), ('newtitle9')", sql);

            sql = insert.AppendData(items).InsertColumns(a => new { a.Title, a.Clicks }).ToSql();
            Assert.Equal("INSERT INTO `tb_topic`(`Clicks`, `Title`) VALUES(0, 'newtitle0'), (100, 'newtitle1'), (200, 'newtitle2'), (300, 'newtitle3'), (400, 'newtitle4'), (500, 'newtitle5'), (600, 'newtitle6'), (700, 'newtitle7'), (800, 'newtitle8'), (900, 'newtitle9')", sql);
        }
        [Fact]
        public void IgnoreColumns()
        {
            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

            var sql = insert.AppendData(items).IgnoreColumns(a => a.CreateTime).ToSql();
            Assert.Equal("INSERT INTO `tb_topic`(`Clicks`, `Title`) VALUES(0, 'newtitle0'), (100, 'newtitle1'), (200, 'newtitle2'), (300, 'newtitle3'), (400, 'newtitle4'), (500, 'newtitle5'), (600, 'newtitle6'), (700, 'newtitle7'), (800, 'newtitle8'), (900, 'newtitle9')", sql);

            sql = insert.AppendData(items).IgnoreColumns(a => new { a.Title, a.CreateTime }).ToSql();
            Assert.Equal("INSERT INTO `tb_topic`(`Clicks`) VALUES(0), (100), (200), (300), (400), (500), (600), (700), (800), (900)", sql);

            g.mysql.Delete<TopicIgnore>().Where("1=1").ExecuteAffrows();
            var itemsIgnore = new List<TopicIgnore>();
            for (var a = 0; a < 2072; a++) itemsIgnore.Add(new TopicIgnore { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100, CreateTime = DateTime.Now });
            g.mysql.Insert<TopicIgnore>().AppendData(itemsIgnore).IgnoreColumns(a => new { a.Title }).ExecuteAffrows();
            Assert.Equal(2072, itemsIgnore.Count);
            Assert.Equal(2072, g.mysql.Select<TopicIgnore>().Where(a => a.Title == null).Count());
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

            Assert.Equal(1, g.mysql.Insert<TestEnumInsertTb>().AppendData(new TestEnumInsertTb { type = TestEnumInserTbType.sum211 }).ExecuteAffrows());
            Assert.Equal(1, g.mysql.Insert<TestEnumInsertTb>().AppendData(new TestEnumInsertTb { type = TestEnumInserTbType.sum211 }).NoneParameter().ExecuteAffrows());
        }
        [Fact]
        public void ExecuteIdentity()
        {
            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

            Assert.NotEqual(0, insert.AppendData(items.First()).ExecuteIdentity());

            var id = g.mysql.Insert<TestEnumInsertTb>().AppendData(new TestEnumInsertTb { type = TestEnumInserTbType.sum211 }).ExecuteIdentity();
            Assert.Equal(TestEnumInserTbType.sum211, g.mysql.Select<TestEnumInsertTb>().Where(a => a.id == id).First()?.type);
            id = g.mysql.Insert<TestEnumInsertTb>().AppendData(new TestEnumInsertTb { type = TestEnumInserTbType.sum211 }).NoneParameter().ExecuteIdentity();
            Assert.Equal(TestEnumInserTbType.sum211, g.mysql.Select<TestEnumInsertTb>().Where(a => a.id == id).First()?.type);
        }
        [Fact]
        public void ExecuteInserted()
        {
            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

            //insert.AppendData(items.First()).ExecuteInserted();
        }

        [Fact]
        public void AsTable()
        {
            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newTitle{a}", Clicks = a * 100 });

            var sql = insert.AppendData(items.First()).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal("INSERT INTO `Topic_InsertAsTable`(`Clicks`, `Title`, `CreateTime`) VALUES(0, 'newTitle0', '0001-01-01 00:00:00.000')", sql);

            sql = insert.AppendData(items).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal("INSERT INTO `Topic_InsertAsTable`(`Clicks`, `Title`, `CreateTime`) VALUES(0, 'newTitle0', '0001-01-01 00:00:00.000'), (100, 'newTitle1', '0001-01-01 00:00:00.000'), (200, 'newTitle2', '0001-01-01 00:00:00.000'), (300, 'newTitle3', '0001-01-01 00:00:00.000'), (400, 'newTitle4', '0001-01-01 00:00:00.000'), (500, 'newTitle5', '0001-01-01 00:00:00.000'), (600, 'newTitle6', '0001-01-01 00:00:00.000'), (700, 'newTitle7', '0001-01-01 00:00:00.000'), (800, 'newTitle8', '0001-01-01 00:00:00.000'), (900, 'newTitle9', '0001-01-01 00:00:00.000')", sql);

            sql = insert.AppendData(items).InsertColumns(a => a.Title).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal("INSERT INTO `Topic_InsertAsTable`(`Title`) VALUES('newTitle0'), ('newTitle1'), ('newTitle2'), ('newTitle3'), ('newTitle4'), ('newTitle5'), ('newTitle6'), ('newTitle7'), ('newTitle8'), ('newTitle9')", sql);

            sql = insert.AppendData(items).IgnoreColumns(a => a.CreateTime).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal("INSERT INTO `Topic_InsertAsTable`(`Clicks`, `Title`) VALUES(0, 'newTitle0'), (100, 'newTitle1'), (200, 'newTitle2'), (300, 'newTitle3'), (400, 'newTitle4'), (500, 'newTitle5'), (600, 'newTitle6'), (700, 'newTitle7'), (800, 'newTitle8'), (900, 'newTitle9')", sql);

            sql = insert.AppendData(items).InsertColumns(a => a.Title).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal("INSERT INTO `Topic_InsertAsTable`(`Title`) VALUES('newTitle0'), ('newTitle1'), ('newTitle2'), ('newTitle3'), ('newTitle4'), ('newTitle5'), ('newTitle6'), ('newTitle7'), ('newTitle8'), ('newTitle9')", sql);

            sql = insert.AppendData(items).InsertColumns(a => new { a.Title, a.Clicks }).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal("INSERT INTO `Topic_InsertAsTable`(`Clicks`, `Title`) VALUES(0, 'newTitle0'), (100, 'newTitle1'), (200, 'newTitle2'), (300, 'newTitle3'), (400, 'newTitle4'), (500, 'newTitle5'), (600, 'newTitle6'), (700, 'newTitle7'), (800, 'newTitle8'), (900, 'newTitle9')", sql);

            sql = insert.AppendData(items).IgnoreColumns(a => a.CreateTime).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal("INSERT INTO `Topic_InsertAsTable`(`Clicks`, `Title`) VALUES(0, 'newTitle0'), (100, 'newTitle1'), (200, 'newTitle2'), (300, 'newTitle3'), (400, 'newTitle4'), (500, 'newTitle5'), (600, 'newTitle6'), (700, 'newTitle7'), (800, 'newTitle8'), (900, 'newTitle9')", sql);

            sql = insert.AppendData(items).IgnoreColumns(a => new { a.Title, a.CreateTime }).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal("INSERT INTO `Topic_InsertAsTable`(`Clicks`) VALUES(0), (100), (200), (300), (400), (500), (600), (700), (800), (900)", sql);
        }
    }
}
