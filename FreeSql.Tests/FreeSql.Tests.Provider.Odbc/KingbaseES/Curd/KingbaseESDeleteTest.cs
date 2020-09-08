using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.Odbc.KingbaseES
{
    public class KingbaseESDeleteTest
    {

        IDelete<Topic> delete => g.kingbaseES.Delete<Topic>(); //��������

        [Table(Name = "tb_topic22211")]
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
            Assert.Null(g.kingbaseES.Delete<Topic>().ToSql());
            var sql = g.kingbaseES.Delete<Topic>(new[] { 1, 2 }).ToSql();
            Assert.Equal("DELETE FROM \"TB_TOPIC22211\" WHERE (\"ID\" IN (1,2))", sql);

            sql = g.kingbaseES.Delete<Topic>(new Topic { Id = 1, Title = "test" }).ToSql();
            Assert.Equal("DELETE FROM \"TB_TOPIC22211\" WHERE (\"ID\" = 1)", sql);

            sql = g.kingbaseES.Delete<Topic>(new[] { new Topic { Id = 1, Title = "test" }, new Topic { Id = 2, Title = "test" } }).ToSql();
            Assert.Equal("DELETE FROM \"TB_TOPIC22211\" WHERE (\"ID\" IN (1,2))", sql);

            sql = g.kingbaseES.Delete<Topic>(new { id = 1 }).ToSql();
            Assert.Equal("DELETE FROM \"TB_TOPIC22211\" WHERE (\"ID\" = 1)", sql);

            sql = g.kingbaseES.Delete<MultiPkTopic>(new[] { new { Id1 = 1, Id2 = 10 }, new { Id1 = 2, Id2 = 20 } }).ToSql();
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
            Assert.Equal("DELETE FROM \"TB_TOPIC22211\" WHERE (\"ID\" = 1)", sql);

            sql = delete.Where("id = :id", new { id = 1 }).ToSql().Replace("\r\n", "");
            Assert.Equal("DELETE FROM \"TB_TOPIC22211\" WHERE (id = :id)", sql);

            var item = new Topic { Id = 1, Title = "newtitle" };
            sql = delete.Where(item).ToSql().Replace("\r\n", "");
            Assert.Equal("DELETE FROM \"TB_TOPIC22211\" WHERE (\"ID\" = 1)", sql);

            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

            sql = delete.Where(items).ToSql().Replace("\r\n", "");
            Assert.Equal("DELETE FROM \"TB_TOPIC22211\" WHERE (\"ID\" IN (1,2,3,4,5,6,7,8,9,10))", sql);
        }
        [Fact]
        public void ExecuteAffrows()
        {

            var id = g.kingbaseES.Insert<Topic>(new Topic { Title = "xxxx", CreateTime = DateTime.Now }).ExecuteIdentity();
            Assert.Equal(1, delete.Where(a => a.Id == id).ExecuteAffrows());
        }
        [Fact]
        public void ExecuteDeleted()
        {

            //var item = g.kingbaseES.Insert<Topic>(new Topic { Title = "xxxx", CreateTime = DateTime.Now }).ExecuteInserted();
            //Assert.Equal(item[0].Id, delete.Where(a => a.Id == item[0].Id).ExecuteDeleted()[0].Id);
        }

        [Fact]
        public void AsTable()
        {
            Assert.Null(g.kingbaseES.Delete<Topic>().ToSql());
            var sql = g.kingbaseES.Delete<Topic>(new[] { 1, 2 }).AsTable(a => "TopicAsTable").ToSql();
            Assert.Equal("DELETE FROM \"TOPICASTABLE\" WHERE (\"ID\" IN (1,2))", sql);

            sql = g.kingbaseES.Delete<Topic>(new Topic { Id = 1, Title = "test" }).AsTable(a => "TopicAsTable").ToSql();
            Assert.Equal("DELETE FROM \"TOPICASTABLE\" WHERE (\"ID\" = 1)", sql);

            sql = g.kingbaseES.Delete<Topic>(new[] { new Topic { Id = 1, Title = "test" }, new Topic { Id = 2, Title = "test" } }).AsTable(a => "TopicAsTable").ToSql();
            Assert.Equal("DELETE FROM \"TOPICASTABLE\" WHERE (\"ID\" IN (1,2))", sql);

            sql = g.kingbaseES.Delete<Topic>(new { id = 1 }).AsTable(a => "TopicAsTable").ToSql();
            Assert.Equal("DELETE FROM \"TOPICASTABLE\" WHERE (\"ID\" = 1)", sql);
        }
    }
}
