using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.Dameng
{
    public class DamengInsertTest
    {

        IInsert<Topic> insert => g.dameng.Insert<Topic>().NoneParameter();

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
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100, CreateTime = DateTime.Parse("2019-09-19 22:25:38.697071") });

            var data = new List<object>();
            var sql = insert.AppendData(items.First()).ToSql();
            Assert.Equal("INSERT INTO \"TB_TOPIC_INSERT\"(\"CLICKS\", \"TITLE\", \"CREATETIME\") VALUES(0, 'newtitle0', to_timestamp('2019-09-19 22:25:38.697071','YYYY-MM-DD HH24:MI:SS.FF6'))", sql);
            data.Add(insert.AppendData(items.First()).ExecuteIdentity());

            sql = insert.AppendData(items).ToSql();
            Assert.Equal(@"INSERT ALL
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(0, 'newtitle0', to_timestamp('2019-09-19 22:25:38.697071','YYYY-MM-DD HH24:MI:SS.FF6'))
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(100, 'newtitle1', to_timestamp('2019-09-19 22:25:38.697071','YYYY-MM-DD HH24:MI:SS.FF6'))
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(200, 'newtitle2', to_timestamp('2019-09-19 22:25:38.697071','YYYY-MM-DD HH24:MI:SS.FF6'))
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(300, 'newtitle3', to_timestamp('2019-09-19 22:25:38.697071','YYYY-MM-DD HH24:MI:SS.FF6'))
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(400, 'newtitle4', to_timestamp('2019-09-19 22:25:38.697071','YYYY-MM-DD HH24:MI:SS.FF6'))
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(500, 'newtitle5', to_timestamp('2019-09-19 22:25:38.697071','YYYY-MM-DD HH24:MI:SS.FF6'))
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(600, 'newtitle6', to_timestamp('2019-09-19 22:25:38.697071','YYYY-MM-DD HH24:MI:SS.FF6'))
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(700, 'newtitle7', to_timestamp('2019-09-19 22:25:38.697071','YYYY-MM-DD HH24:MI:SS.FF6'))
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(800, 'newtitle8', to_timestamp('2019-09-19 22:25:38.697071','YYYY-MM-DD HH24:MI:SS.FF6'))
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(900, 'newtitle9', to_timestamp('2019-09-19 22:25:38.697071','YYYY-MM-DD HH24:MI:SS.FF6'))
 SELECT 1 FROM DUAL", sql);
            data.Add(insert.AppendData(items.First()).ExecuteIdentity());

            sql = insert.AppendData(items).InsertColumns(a => a.Title).ToSql();
            Assert.Equal(@"INSERT ALL
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES('newtitle0')
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES('newtitle1')
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES('newtitle2')
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES('newtitle3')
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES('newtitle4')
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES('newtitle5')
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES('newtitle6')
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES('newtitle7')
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES('newtitle8')
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES('newtitle9')
 SELECT 1 FROM DUAL", sql);
            data.Add(insert.AppendData(items.First()).ExecuteIdentity());

            sql = insert.AppendData(items).IgnoreColumns(a => a.CreateTime).ToSql();
            Assert.Equal(@"INSERT ALL
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(0, 'newtitle0')
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(100, 'newtitle1')
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(200, 'newtitle2')
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(300, 'newtitle3')
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(400, 'newtitle4')
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(500, 'newtitle5')
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(600, 'newtitle6')
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(700, 'newtitle7')
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(800, 'newtitle8')
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(900, 'newtitle9')
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
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES('newtitle0')
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES('newtitle1')
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES('newtitle2')
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES('newtitle3')
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES('newtitle4')
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES('newtitle5')
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES('newtitle6')
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES('newtitle7')
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES('newtitle8')
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES('newtitle9')
 SELECT 1 FROM DUAL", sql);
            data.Add(insert.AppendData(items.First()).ExecuteIdentity());

            sql = insert.AppendData(items).InsertColumns(a => new { a.Title, a.Clicks }).ToSql();
            Assert.Equal(@"INSERT ALL
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(0, 'newtitle0')
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(100, 'newtitle1')
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(200, 'newtitle2')
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(300, 'newtitle3')
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(400, 'newtitle4')
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(500, 'newtitle5')
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(600, 'newtitle6')
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(700, 'newtitle7')
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(800, 'newtitle8')
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(900, 'newtitle9')
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
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(0, 'newtitle0')
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(100, 'newtitle1')
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(200, 'newtitle2')
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(300, 'newtitle3')
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(400, 'newtitle4')
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(500, 'newtitle5')
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(600, 'newtitle6')
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(700, 'newtitle7')
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(800, 'newtitle8')
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(900, 'newtitle9')
 SELECT 1 FROM DUAL", sql);
            data.Add(insert.AppendData(items.First()).ExecuteIdentity());

            sql = insert.AppendData(items).IgnoreColumns(a => new { a.Title, a.CreateTime }).ToSql();
            Assert.Equal(@"INSERT ALL
INTO ""TB_TOPIC_INSERT""(""CLICKS"") VALUES(0)
INTO ""TB_TOPIC_INSERT""(""CLICKS"") VALUES(100)
INTO ""TB_TOPIC_INSERT""(""CLICKS"") VALUES(200)
INTO ""TB_TOPIC_INSERT""(""CLICKS"") VALUES(300)
INTO ""TB_TOPIC_INSERT""(""CLICKS"") VALUES(400)
INTO ""TB_TOPIC_INSERT""(""CLICKS"") VALUES(500)
INTO ""TB_TOPIC_INSERT""(""CLICKS"") VALUES(600)
INTO ""TB_TOPIC_INSERT""(""CLICKS"") VALUES(700)
INTO ""TB_TOPIC_INSERT""(""CLICKS"") VALUES(800)
INTO ""TB_TOPIC_INSERT""(""CLICKS"") VALUES(900)
 SELECT 1 FROM DUAL", sql);
            data.Add(insert.AppendData(items.First()).ExecuteIdentity());

            g.dameng.Delete<TopicIgnore>().Where("1=1").ExecuteAffrows();
            var itemsIgnore = new List<TopicIgnore>();
            for (var a = 0; a < 2072; a++) itemsIgnore.Add(new TopicIgnore { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100, CreateTime = DateTime.Now });
            g.dameng.Insert<TopicIgnore>().AppendData(itemsIgnore).IgnoreColumns(a => new { a.Title }).ExecuteAffrows();
            Assert.Equal(2072, itemsIgnore.Count);
            Assert.Equal(2072, g.dameng.Select<TopicIgnore>().Where(a => a.Title == null).Count());
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

            Assert.Equal(10, g.dameng.Select<Topic>().Limit(10).InsertInto(null, a => new Topic
            {
                Title = a.Title
            }));
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

        //[Fact]
        //public void ExecuteDmBulkCopy()
        //{
        //    var items = new List<Topic>();
        //    for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100, CreateTime = DateTime.Now });

        //    insert.AppendData(items).InsertIdentity().ExecuteDmBulkCopy();
        //    //Dm.DmException:¡°The fastloading dll not loading!¡±
        //}

        [Fact]
        public void AsTable()
        {
            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newTitle{a}", Clicks = a * 100 });

            var sql = insert.AppendData(items.First()).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal("INSERT INTO \"TOPIC_INSERTASTABLE\"(\"CLICKS\", \"TITLE\", \"CREATETIME\") VALUES(0, 'newTitle0', to_timestamp('0001-01-01 00:00:00.000000','YYYY-MM-DD HH24:MI:SS.FF6'))", sql);

            sql = insert.AppendData(items).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal(@"INSERT ALL
INTO ""TOPIC_INSERTASTABLE""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(0, 'newTitle0', to_timestamp('0001-01-01 00:00:00.000000','YYYY-MM-DD HH24:MI:SS.FF6'))
INTO ""TOPIC_INSERTASTABLE""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(100, 'newTitle1', to_timestamp('0001-01-01 00:00:00.000000','YYYY-MM-DD HH24:MI:SS.FF6'))
INTO ""TOPIC_INSERTASTABLE""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(200, 'newTitle2', to_timestamp('0001-01-01 00:00:00.000000','YYYY-MM-DD HH24:MI:SS.FF6'))
INTO ""TOPIC_INSERTASTABLE""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(300, 'newTitle3', to_timestamp('0001-01-01 00:00:00.000000','YYYY-MM-DD HH24:MI:SS.FF6'))
INTO ""TOPIC_INSERTASTABLE""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(400, 'newTitle4', to_timestamp('0001-01-01 00:00:00.000000','YYYY-MM-DD HH24:MI:SS.FF6'))
INTO ""TOPIC_INSERTASTABLE""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(500, 'newTitle5', to_timestamp('0001-01-01 00:00:00.000000','YYYY-MM-DD HH24:MI:SS.FF6'))
INTO ""TOPIC_INSERTASTABLE""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(600, 'newTitle6', to_timestamp('0001-01-01 00:00:00.000000','YYYY-MM-DD HH24:MI:SS.FF6'))
INTO ""TOPIC_INSERTASTABLE""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(700, 'newTitle7', to_timestamp('0001-01-01 00:00:00.000000','YYYY-MM-DD HH24:MI:SS.FF6'))
INTO ""TOPIC_INSERTASTABLE""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(800, 'newTitle8', to_timestamp('0001-01-01 00:00:00.000000','YYYY-MM-DD HH24:MI:SS.FF6'))
INTO ""TOPIC_INSERTASTABLE""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(900, 'newTitle9', to_timestamp('0001-01-01 00:00:00.000000','YYYY-MM-DD HH24:MI:SS.FF6'))
 SELECT 1 FROM DUAL", sql);

            sql = insert.AppendData(items).IgnoreColumns(a => a.Title).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal(@"INSERT ALL
INTO ""TOPIC_INSERTASTABLE""(""CLICKS"", ""CREATETIME"") VALUES(0, to_timestamp('0001-01-01 00:00:00.000000','YYYY-MM-DD HH24:MI:SS.FF6'))
INTO ""TOPIC_INSERTASTABLE""(""CLICKS"", ""CREATETIME"") VALUES(100, to_timestamp('0001-01-01 00:00:00.000000','YYYY-MM-DD HH24:MI:SS.FF6'))
INTO ""TOPIC_INSERTASTABLE""(""CLICKS"", ""CREATETIME"") VALUES(200, to_timestamp('0001-01-01 00:00:00.000000','YYYY-MM-DD HH24:MI:SS.FF6'))
INTO ""TOPIC_INSERTASTABLE""(""CLICKS"", ""CREATETIME"") VALUES(300, to_timestamp('0001-01-01 00:00:00.000000','YYYY-MM-DD HH24:MI:SS.FF6'))
INTO ""TOPIC_INSERTASTABLE""(""CLICKS"", ""CREATETIME"") VALUES(400, to_timestamp('0001-01-01 00:00:00.000000','YYYY-MM-DD HH24:MI:SS.FF6'))
INTO ""TOPIC_INSERTASTABLE""(""CLICKS"", ""CREATETIME"") VALUES(500, to_timestamp('0001-01-01 00:00:00.000000','YYYY-MM-DD HH24:MI:SS.FF6'))
INTO ""TOPIC_INSERTASTABLE""(""CLICKS"", ""CREATETIME"") VALUES(600, to_timestamp('0001-01-01 00:00:00.000000','YYYY-MM-DD HH24:MI:SS.FF6'))
INTO ""TOPIC_INSERTASTABLE""(""CLICKS"", ""CREATETIME"") VALUES(700, to_timestamp('0001-01-01 00:00:00.000000','YYYY-MM-DD HH24:MI:SS.FF6'))
INTO ""TOPIC_INSERTASTABLE""(""CLICKS"", ""CREATETIME"") VALUES(800, to_timestamp('0001-01-01 00:00:00.000000','YYYY-MM-DD HH24:MI:SS.FF6'))
INTO ""TOPIC_INSERTASTABLE""(""CLICKS"", ""CREATETIME"") VALUES(900, to_timestamp('0001-01-01 00:00:00.000000','YYYY-MM-DD HH24:MI:SS.FF6'))
 SELECT 1 FROM DUAL", sql);

            sql = insert.AppendData(items).IgnoreColumns(a => new { a.Clicks, a.CreateTime }).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal(@"INSERT ALL
INTO ""TOPIC_INSERTASTABLE""(""TITLE"") VALUES('newTitle0')
INTO ""TOPIC_INSERTASTABLE""(""TITLE"") VALUES('newTitle1')
INTO ""TOPIC_INSERTASTABLE""(""TITLE"") VALUES('newTitle2')
INTO ""TOPIC_INSERTASTABLE""(""TITLE"") VALUES('newTitle3')
INTO ""TOPIC_INSERTASTABLE""(""TITLE"") VALUES('newTitle4')
INTO ""TOPIC_INSERTASTABLE""(""TITLE"") VALUES('newTitle5')
INTO ""TOPIC_INSERTASTABLE""(""TITLE"") VALUES('newTitle6')
INTO ""TOPIC_INSERTASTABLE""(""TITLE"") VALUES('newTitle7')
INTO ""TOPIC_INSERTASTABLE""(""TITLE"") VALUES('newTitle8')
INTO ""TOPIC_INSERTASTABLE""(""TITLE"") VALUES('newTitle9')
 SELECT 1 FROM DUAL", sql);

            sql = insert.AppendData(items).InsertColumns(a => a.Title).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal(@"INSERT ALL
INTO ""TOPIC_INSERTASTABLE""(""TITLE"") VALUES('newTitle0')
INTO ""TOPIC_INSERTASTABLE""(""TITLE"") VALUES('newTitle1')
INTO ""TOPIC_INSERTASTABLE""(""TITLE"") VALUES('newTitle2')
INTO ""TOPIC_INSERTASTABLE""(""TITLE"") VALUES('newTitle3')
INTO ""TOPIC_INSERTASTABLE""(""TITLE"") VALUES('newTitle4')
INTO ""TOPIC_INSERTASTABLE""(""TITLE"") VALUES('newTitle5')
INTO ""TOPIC_INSERTASTABLE""(""TITLE"") VALUES('newTitle6')
INTO ""TOPIC_INSERTASTABLE""(""TITLE"") VALUES('newTitle7')
INTO ""TOPIC_INSERTASTABLE""(""TITLE"") VALUES('newTitle8')
INTO ""TOPIC_INSERTASTABLE""(""TITLE"") VALUES('newTitle9')
 SELECT 1 FROM DUAL", sql);

            sql = insert.AppendData(items).InsertColumns(a => new { a.Title, a.Clicks }).AsTable(a => "Topic_InsertAsTable").ToSql();
            Assert.Equal(@"INSERT ALL
INTO ""TOPIC_INSERTASTABLE""(""CLICKS"", ""TITLE"") VALUES(0, 'newTitle0')
INTO ""TOPIC_INSERTASTABLE""(""CLICKS"", ""TITLE"") VALUES(100, 'newTitle1')
INTO ""TOPIC_INSERTASTABLE""(""CLICKS"", ""TITLE"") VALUES(200, 'newTitle2')
INTO ""TOPIC_INSERTASTABLE""(""CLICKS"", ""TITLE"") VALUES(300, 'newTitle3')
INTO ""TOPIC_INSERTASTABLE""(""CLICKS"", ""TITLE"") VALUES(400, 'newTitle4')
INTO ""TOPIC_INSERTASTABLE""(""CLICKS"", ""TITLE"") VALUES(500, 'newTitle5')
INTO ""TOPIC_INSERTASTABLE""(""CLICKS"", ""TITLE"") VALUES(600, 'newTitle6')
INTO ""TOPIC_INSERTASTABLE""(""CLICKS"", ""TITLE"") VALUES(700, 'newTitle7')
INTO ""TOPIC_INSERTASTABLE""(""CLICKS"", ""TITLE"") VALUES(800, 'newTitle8')
INTO ""TOPIC_INSERTASTABLE""(""CLICKS"", ""TITLE"") VALUES(900, 'newTitle9')
 SELECT 1 FROM DUAL", sql);
        }
    }
}
