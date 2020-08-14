using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.ShenTong
{
    public class ShenTongDeleteTest
    {

        IDelete<Topic> delete => g.shentong.Delete<Topic>();

        [Table(Name = "tb_topic_del")]
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
        public void Dywhere()
        {
            Assert.Null(g.shentong.Delete<Topic>().ToSql());
            var sql = g.shentong.Delete<Topic>(new[] { 1, 2 }).ToSql();
            Assert.Equal("DELETE FROM \"TB_TOPIC_DEL\" WHERE (\"ID\" IN (1,2))", sql);

            sql = g.shentong.Delete<Topic>(new Topic { Id = 1, Title = "test" }).ToSql();
            Assert.Equal("DELETE FROM \"TB_TOPIC_DEL\" WHERE (\"ID\" = 1)", sql);

            sql = g.shentong.Delete<Topic>(new[] { new Topic { Id = 1, Title = "test" }, new Topic { Id = 2, Title = "test" } }).ToSql();
            Assert.Equal("DELETE FROM \"TB_TOPIC_DEL\" WHERE (\"ID\" IN (1,2))", sql);

            sql = g.shentong.Delete<Topic>(new { id = 1 }).ToSql();
            Assert.Equal("DELETE FROM \"TB_TOPIC_DEL\" WHERE (\"ID\" = 1)", sql);

            sql = g.shentong.Delete<MultiPkTopic>(new[] { new { Id1 = 1, Id2 = 10 }, new { Id1 = 2, Id2 = 20 } }).ToSql();
            Assert.Equal("DELETE FROM \"MULTIPKTOPIC\" WHERE (\"ID1\" = 1 AND \"ID2\" = 10 OR \"ID1\" = 2 AND \"ID2\" = 20)", sql);
        }
        class MultiPkTopic
        {
            [Column(IsPrimary = true)]
            public int Id1 { get; set; }
            [Column(IsPrimary = true)]
            public int Id2 { get; set; }
            public int Clicks { get; set; }
            public string Title { get; set; }
            public DateTime CreateTime { get; set; }
        }

        [Fact]
        public void Where()
        {
            var sql = delete.Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("DELETE FROM \"TB_TOPIC_DEL\" WHERE (\"ID\" = 1)", sql);

            sql = delete.Where("id = @id", new { id = 1 }).ToSql().Replace("\r\n", "");
            Assert.Equal("DELETE FROM \"TB_TOPIC_DEL\" WHERE (id = @id)", sql);

            var item = new Topic { Id = 1, Title = "newtitle" };
            sql = delete.Where(item).ToSql().Replace("\r\n", "");
            Assert.Equal("DELETE FROM \"TB_TOPIC_DEL\" WHERE (\"ID\" = 1)", sql);

            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

            sql = delete.Where(items).ToSql().Replace("\r\n", "");
            Assert.Equal("DELETE FROM \"TB_TOPIC_DEL\" WHERE (\"ID\" IN (1,2,3,4,5,6,7,8,9,10))", sql);
        }
        [Fact]
        public void ExecuteAffrows()
        {

            var id = g.shentong.Insert<Topic>(new Topic { Title = "xxxx" }).ExecuteIdentity();
            Assert.Equal(1, delete.Where(a => a.Id == id).ExecuteAffrows());
        }
        [Fact]
        public void ExecuteDeleted()
        {

            delete.Where(a => a.Id > 0).ExecuteDeleted();
        }

        [Fact]
        public void AsTable()
        {
            Assert.Null(g.shentong.Delete<Topic>().ToSql());
            var sql = g.shentong.Delete<Topic>(new[] { 1, 2 }).AsTable(a => "TopicAsTable").ToSql();
            Assert.Equal("DELETE FROM \"TOPICASTABLE\" WHERE (\"ID\" IN (1,2))", sql);

            sql = g.shentong.Delete<Topic>(new Topic { Id = 1, Title = "test" }).AsTable(a => "TopicAsTable").ToSql();
            Assert.Equal("DELETE FROM \"TOPICASTABLE\" WHERE (\"ID\" = 1)", sql);

            sql = g.shentong.Delete<Topic>(new[] { new Topic { Id = 1, Title = "test" }, new Topic { Id = 2, Title = "test" } }).AsTable(a => "TopicAsTable").ToSql();
            Assert.Equal("DELETE FROM \"TOPICASTABLE\" WHERE (\"ID\" IN (1,2))", sql);

            sql = g.shentong.Delete<Topic>(new { id = 1 }).AsTable(a => "TopicAsTable").ToSql();
            Assert.Equal("DELETE FROM \"TOPICASTABLE\" WHERE (\"ID\" = 1)", sql);
        }
    }
}
