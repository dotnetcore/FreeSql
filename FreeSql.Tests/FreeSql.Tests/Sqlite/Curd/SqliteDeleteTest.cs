using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.Sqlite
{
    public class SqliteDeleteTest
    {

        IDelete<Topic> delete => g.sqlite.Delete<Topic>(); //��������

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
            Assert.Null(g.sqlite.Delete<Topic>().ToSql());
            var sql = g.sqlite.Delete<Topic>(new[] { 1, 2 }).ToSql();
            Assert.Equal("DELETE FROM \"tb_topic22211\" WHERE (\"Id\" = 1 OR \"Id\" = 2)", sql);

            sql = g.sqlite.Delete<Topic>(new Topic { Id = 1, Title = "test" }).ToSql();
            Assert.Equal("DELETE FROM \"tb_topic22211\" WHERE (\"Id\" = 1)", sql);

            sql = g.sqlite.Delete<Topic>(new[] { new Topic { Id = 1, Title = "test" }, new Topic { Id = 2, Title = "test" } }).ToSql();
            Assert.Equal("DELETE FROM \"tb_topic22211\" WHERE (\"Id\" = 1 OR \"Id\" = 2)", sql);

            sql = g.sqlite.Delete<Topic>(new { id = 1 }).ToSql();
            Assert.Equal("DELETE FROM \"tb_topic22211\" WHERE (\"Id\" = 1)", sql);
        }

        [Fact]
        public void Where()
        {
            var sql = delete.Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("DELETE FROM \"tb_topic22211\" WHERE (\"Id\" = 1)", sql);

            sql = delete.Where("id = @id", new { id = 1 }).ToSql().Replace("\r\n", "");
            Assert.Equal("DELETE FROM \"tb_topic22211\" WHERE (id = @id)", sql);

            var item = new Topic { Id = 1, Title = "newtitle" };
            sql = delete.Where(item).ToSql().Replace("\r\n", "");
            Assert.Equal("DELETE FROM \"tb_topic22211\" WHERE (\"Id\" = 1)", sql);

            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

            sql = delete.Where(items).ToSql().Replace("\r\n", "");
            Assert.Equal("DELETE FROM \"tb_topic22211\" WHERE (\"Id\" IN (1,2,3,4,5,6,7,8,9,10))", sql);
        }
        [Fact]
        public void WhereExists()
        {

        }
        [Fact]
        public void ExecuteAffrows()
        {

            var id = g.sqlite.Insert<Topic>(new Topic { Title = "xxxx", CreateTime = DateTime.Now }).ExecuteIdentity();
            Assert.Equal(1, delete.Where(a => a.Id == id).ExecuteAffrows());
        }
        [Fact]
        public void ExecuteDeleted()
        {

            //var item = g.Sqlite.Delete<Topic>(new Topic { Title = "xxxx", CreateTime = DateTime.Now }).ExecuteInserted();
            //Assert.Equal(item[0].Id, delete.Where(a => a.Id == item[0].Id).ExecuteDeleted()[0].Id);
        }

        [Fact]
        public void AsTable()
        {
            Assert.Null(g.sqlite.Delete<Topic>().AsTable(a => "TopicAsTable").ToSql());
            var sql = g.sqlite.Delete<Topic>(new[] { 1, 2 }).AsTable(a => "TopicAsTable").ToSql();
            Assert.Equal("DELETE FROM \"TopicAsTable\" WHERE (\"Id\" = 1 OR \"Id\" = 2)", sql);

            sql = g.sqlite.Delete<Topic>(new Topic { Id = 1, Title = "test" }).AsTable(a => "TopicAsTable").ToSql();
            Assert.Equal("DELETE FROM \"TopicAsTable\" WHERE (\"Id\" = 1)", sql);

            sql = g.sqlite.Delete<Topic>(new[] { new Topic { Id = 1, Title = "test" }, new Topic { Id = 2, Title = "test" } }).AsTable(a => "TopicAsTable").ToSql();
            Assert.Equal("DELETE FROM \"TopicAsTable\" WHERE (\"Id\" = 1 OR \"Id\" = 2)", sql);

            sql = g.sqlite.Delete<Topic>(new { id = 1 }).AsTable(a => "TopicAsTable").ToSql();
            Assert.Equal("DELETE FROM \"TopicAsTable\" WHERE (\"Id\" = 1)", sql);
        }
    }
}
