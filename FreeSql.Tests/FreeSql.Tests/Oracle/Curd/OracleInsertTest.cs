using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.Oracle
{
    public class OracleInsertTest
    {

        IInsert<Topic> insert => g.oracle.Insert<Topic>(); //��������

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

        [Fact]
        public void AppendData()
        {
            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100, CreateTime = DateTime.Now });

            var data = new List<object>();
            var sql = insert.AppendData(items.First()).ToSql();
            Assert.Equal("INSERT INTO \"TB_TOPIC_INSERT\"(\"CLICKS\", \"TITLE\", \"CREATETIME\") VALUES(:Clicks_0, :Title_0, :CreateTime_0)", sql);
            data.Add(insert.AppendData(items.First()).ExecuteIdentity());

            sql = insert.AppendData(items).ToSql();
            Assert.Equal(@"INSERT ALL
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(:Clicks_0, :Title_0, :CreateTime_0)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(:Clicks_1, :Title_1, :CreateTime_1)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(:Clicks_2, :Title_2, :CreateTime_2)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(:Clicks_3, :Title_3, :CreateTime_3)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(:Clicks_4, :Title_4, :CreateTime_4)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(:Clicks_5, :Title_5, :CreateTime_5)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(:Clicks_6, :Title_6, :CreateTime_6)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(:Clicks_7, :Title_7, :CreateTime_7)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(:Clicks_8, :Title_8, :CreateTime_8)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(:Clicks_9, :Title_9, :CreateTime_9)
 SELECT 1 FROM DUAL", sql);
            data.Add(insert.AppendData(items.First()).ExecuteIdentity());

            sql = insert.AppendData(items).InsertColumns(a => a.Title).ToSql();
            Assert.Equal(@"INSERT ALL
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title_0)
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title_1)
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title_2)
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title_3)
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title_4)
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title_5)
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title_6)
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title_7)
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title_8)
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title_9)
 SELECT 1 FROM DUAL", sql);
            data.Add(insert.AppendData(items.First()).ExecuteIdentity());

            sql = insert.AppendData(items).IgnoreColumns(a => a.CreateTime).ToSql();
            Assert.Equal(@"INSERT ALL
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks_0, :Title_0)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks_1, :Title_1)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks_2, :Title_2)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks_3, :Title_3)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks_4, :Title_4)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks_5, :Title_5)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks_6, :Title_6)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks_7, :Title_7)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks_8, :Title_8)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks_9, :Title_9)
 SELECT 1 FROM DUAL", sql);
            data.Add(insert.AppendData(items.First()).ExecuteIdentity());
        }

        [Fact]
        public void InsertColumns()
        {
            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100, CreateTime = DateTime.Now });

            var data = new List<object>();
            var sql = insert.AppendData(items).InsertColumns(a => a.Title).ToSql();
            Assert.Equal(@"INSERT ALL
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title_0)
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title_1)
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title_2)
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title_3)
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title_4)
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title_5)
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title_6)
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title_7)
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title_8)
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title_9)
 SELECT 1 FROM DUAL", sql);
            data.Add(insert.AppendData(items.First()).ExecuteIdentity());

            sql = insert.AppendData(items).InsertColumns(a => new { a.Title, a.Clicks }).ToSql();
            Assert.Equal(@"INSERT ALL
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks_0, :Title_0)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks_1, :Title_1)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks_2, :Title_2)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks_3, :Title_3)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks_4, :Title_4)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks_5, :Title_5)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks_6, :Title_6)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks_7, :Title_7)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks_8, :Title_8)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks_9, :Title_9)
 SELECT 1 FROM DUAL", sql);
            data.Add(insert.AppendData(items.First()).ExecuteIdentity());
        }
        [Fact]
        public void IgnoreColumns()
        {
            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100, CreateTime = DateTime.Now });

            var data = new List<object>();
            var sql = insert.AppendData(items).IgnoreColumns(a => a.CreateTime).ToSql();
            Assert.Equal(@"INSERT ALL
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks_0, :Title_0)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks_1, :Title_1)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks_2, :Title_2)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks_3, :Title_3)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks_4, :Title_4)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks_5, :Title_5)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks_6, :Title_6)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks_7, :Title_7)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks_8, :Title_8)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks_9, :Title_9)
 SELECT 1 FROM DUAL", sql);
            data.Add(insert.AppendData(items.First()).ExecuteIdentity());

            sql = insert.AppendData(items).IgnoreColumns(a => new { a.Title, a.CreateTime }).ToSql();
            Assert.Equal(@"INSERT ALL
INTO ""TB_TOPIC_INSERT""(""CLICKS"") VALUES(:Clicks_0)
INTO ""TB_TOPIC_INSERT""(""CLICKS"") VALUES(:Clicks_1)
INTO ""TB_TOPIC_INSERT""(""CLICKS"") VALUES(:Clicks_2)
INTO ""TB_TOPIC_INSERT""(""CLICKS"") VALUES(:Clicks_3)
INTO ""TB_TOPIC_INSERT""(""CLICKS"") VALUES(:Clicks_4)
INTO ""TB_TOPIC_INSERT""(""CLICKS"") VALUES(:Clicks_5)
INTO ""TB_TOPIC_INSERT""(""CLICKS"") VALUES(:Clicks_6)
INTO ""TB_TOPIC_INSERT""(""CLICKS"") VALUES(:Clicks_7)
INTO ""TB_TOPIC_INSERT""(""CLICKS"") VALUES(:Clicks_8)
INTO ""TB_TOPIC_INSERT""(""CLICKS"") VALUES(:Clicks_9)
 SELECT 1 FROM DUAL", sql);
            data.Add(insert.AppendData(items.First()).ExecuteIdentity());

            g.oracle.Delete<TopicIgnore>().Where("1=1").ExecuteAffrows();
            var itemsIgnore = new List<TopicIgnore>();
            for (var a = 0; a < 2072; a++) itemsIgnore.Add(new TopicIgnore { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100, CreateTime = DateTime.Now });
            g.oracle.Insert<TopicIgnore>().AppendData(itemsIgnore).IgnoreColumns(a => new { a.Title }).ExecuteAffrows();
            Assert.Equal(2072, itemsIgnore.Count);
            Assert.Equal(2072, g.oracle.Select<TopicIgnore>().Where(a => a.Title == null).Count());
        }
        [Table(Name = "tb_topicICs")]
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
        }
        [Fact]
        public void ExecuteIdentity()
        {
            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100, CreateTime = DateTime.Now });

            Assert.NotEqual(0, insert.AppendData(items.First()).ExecuteIdentity());
        }
        [Fact]
        public void ExecuteInserted()
        {
            //var items = new List<Topic>();
            //for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100, CreateTime = DateTime.Now });

            //var items2 = insert.AppendData(items).ExecuteInserted();
        }

        [Fact]
        public void AsTable()
        {
            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newTitle{a}", Clicks = a * 100 });

            var sql = insert.AppendData(items.First()).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal("INSERT INTO \"Topic_InsertAsTable\"(\"CLICKS\", \"TITLE\", \"CREATETIME\") VALUES(:Clicks_0, :Title_0, :CreateTime_0)", sql);

            sql = insert.AppendData(items).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal(@"INSERT ALL
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(:Clicks_0, :Title_0, :CreateTime_0)
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(:Clicks_1, :Title_1, :CreateTime_1)
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(:Clicks_2, :Title_2, :CreateTime_2)
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(:Clicks_3, :Title_3, :CreateTime_3)
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(:Clicks_4, :Title_4, :CreateTime_4)
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(:Clicks_5, :Title_5, :CreateTime_5)
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(:Clicks_6, :Title_6, :CreateTime_6)
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(:Clicks_7, :Title_7, :CreateTime_7)
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(:Clicks_8, :Title_8, :CreateTime_8)
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(:Clicks_9, :Title_9, :CreateTime_9)
 SELECT 1 FROM DUAL", sql);

            sql = insert.AppendData(items).InsertColumns(a => a.Title).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal(@"INSERT ALL
INTO ""Topic_InsertAsTable""(""TITLE"") VALUES(:Title_0)
INTO ""Topic_InsertAsTable""(""TITLE"") VALUES(:Title_1)
INTO ""Topic_InsertAsTable""(""TITLE"") VALUES(:Title_2)
INTO ""Topic_InsertAsTable""(""TITLE"") VALUES(:Title_3)
INTO ""Topic_InsertAsTable""(""TITLE"") VALUES(:Title_4)
INTO ""Topic_InsertAsTable""(""TITLE"") VALUES(:Title_5)
INTO ""Topic_InsertAsTable""(""TITLE"") VALUES(:Title_6)
INTO ""Topic_InsertAsTable""(""TITLE"") VALUES(:Title_7)
INTO ""Topic_InsertAsTable""(""TITLE"") VALUES(:Title_8)
INTO ""Topic_InsertAsTable""(""TITLE"") VALUES(:Title_9)
 SELECT 1 FROM DUAL", sql);

            sql = insert.AppendData(items).IgnoreColumns(a => a.CreateTime).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal(@"INSERT ALL
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"") VALUES(:Clicks_0, :Title_0)
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"") VALUES(:Clicks_1, :Title_1)
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"") VALUES(:Clicks_2, :Title_2)
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"") VALUES(:Clicks_3, :Title_3)
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"") VALUES(:Clicks_4, :Title_4)
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"") VALUES(:Clicks_5, :Title_5)
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"") VALUES(:Clicks_6, :Title_6)
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"") VALUES(:Clicks_7, :Title_7)
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"") VALUES(:Clicks_8, :Title_8)
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"") VALUES(:Clicks_9, :Title_9)
 SELECT 1 FROM DUAL", sql);

            sql = insert.AppendData(items).InsertColumns(a => a.Title).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal(@"INSERT ALL
INTO ""Topic_InsertAsTable""(""TITLE"") VALUES(:Title_0)
INTO ""Topic_InsertAsTable""(""TITLE"") VALUES(:Title_1)
INTO ""Topic_InsertAsTable""(""TITLE"") VALUES(:Title_2)
INTO ""Topic_InsertAsTable""(""TITLE"") VALUES(:Title_3)
INTO ""Topic_InsertAsTable""(""TITLE"") VALUES(:Title_4)
INTO ""Topic_InsertAsTable""(""TITLE"") VALUES(:Title_5)
INTO ""Topic_InsertAsTable""(""TITLE"") VALUES(:Title_6)
INTO ""Topic_InsertAsTable""(""TITLE"") VALUES(:Title_7)
INTO ""Topic_InsertAsTable""(""TITLE"") VALUES(:Title_8)
INTO ""Topic_InsertAsTable""(""TITLE"") VALUES(:Title_9)
 SELECT 1 FROM DUAL", sql);

            sql = insert.AppendData(items).InsertColumns(a => new { a.Title, a.Clicks }).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal(@"INSERT ALL
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"") VALUES(:Clicks_0, :Title_0)
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"") VALUES(:Clicks_1, :Title_1)
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"") VALUES(:Clicks_2, :Title_2)
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"") VALUES(:Clicks_3, :Title_3)
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"") VALUES(:Clicks_4, :Title_4)
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"") VALUES(:Clicks_5, :Title_5)
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"") VALUES(:Clicks_6, :Title_6)
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"") VALUES(:Clicks_7, :Title_7)
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"") VALUES(:Clicks_8, :Title_8)
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"") VALUES(:Clicks_9, :Title_9)
 SELECT 1 FROM DUAL", sql);

            sql = insert.AppendData(items).IgnoreColumns(a => a.CreateTime).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal(@"INSERT ALL
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"") VALUES(:Clicks_0, :Title_0)
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"") VALUES(:Clicks_1, :Title_1)
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"") VALUES(:Clicks_2, :Title_2)
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"") VALUES(:Clicks_3, :Title_3)
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"") VALUES(:Clicks_4, :Title_4)
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"") VALUES(:Clicks_5, :Title_5)
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"") VALUES(:Clicks_6, :Title_6)
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"") VALUES(:Clicks_7, :Title_7)
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"") VALUES(:Clicks_8, :Title_8)
INTO ""Topic_InsertAsTable""(""CLICKS"", ""TITLE"") VALUES(:Clicks_9, :Title_9)
 SELECT 1 FROM DUAL", sql);

            sql = insert.AppendData(items).IgnoreColumns(a => new { a.Title, a.CreateTime }).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal(@"INSERT ALL
INTO ""Topic_InsertAsTable""(""CLICKS"") VALUES(:Clicks_0)
INTO ""Topic_InsertAsTable""(""CLICKS"") VALUES(:Clicks_1)
INTO ""Topic_InsertAsTable""(""CLICKS"") VALUES(:Clicks_2)
INTO ""Topic_InsertAsTable""(""CLICKS"") VALUES(:Clicks_3)
INTO ""Topic_InsertAsTable""(""CLICKS"") VALUES(:Clicks_4)
INTO ""Topic_InsertAsTable""(""CLICKS"") VALUES(:Clicks_5)
INTO ""Topic_InsertAsTable""(""CLICKS"") VALUES(:Clicks_6)
INTO ""Topic_InsertAsTable""(""CLICKS"") VALUES(:Clicks_7)
INTO ""Topic_InsertAsTable""(""CLICKS"") VALUES(:Clicks_8)
INTO ""Topic_InsertAsTable""(""CLICKS"") VALUES(:Clicks_9)
 SELECT 1 FROM DUAL", sql);
        }
    }
}
