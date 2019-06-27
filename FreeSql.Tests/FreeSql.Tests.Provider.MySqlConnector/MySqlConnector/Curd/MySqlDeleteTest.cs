using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.MySqlConnector
{
    public class MySqlDeleteTest
    {

        IDelete<Topic> delete => g.mysql.Delete<Topic>(); //��������

        [Table(Name = "tb_topic")]
        class Topic
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }
            public int Clicks { get; set; }
            public string Title { get; set; }
            public DateTime CreateTime { get; set; }
        }

        [Fact]
        public void Dywhere()
        {
            Assert.Null(g.mysql.Delete<Topic>().ToSql());
            var sql = g.mysql.Delete<Topic>(new[] { 1, 2 }).ToSql();
            Assert.Equal("DELETE FROM `tb_topic` WHERE (`Id` = 1 OR `Id` = 2)", sql);

            sql = g.mysql.Delete<Topic>(new Topic { Id = 1, Title = "test" }).ToSql();
            Assert.Equal("DELETE FROM `tb_topic` WHERE (`Id` = 1)", sql);

            sql = g.mysql.Delete<Topic>(new[] { new Topic { Id = 1, Title = "test" }, new Topic { Id = 2, Title = "test" } }).ToSql();
            Assert.Equal("DELETE FROM `tb_topic` WHERE (`Id` = 1 OR `Id` = 2)", sql);

            sql = g.mysql.Delete<Topic>(new { id = 1 }).ToSql();
            Assert.Equal("DELETE FROM `tb_topic` WHERE (`Id` = 1)", sql);
        }

        [Fact]
        public void Where()
        {
            var sql = delete.Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("DELETE FROM `tb_topic` WHERE (`Id` = 1)", sql);

            sql = delete.Where("id = @id", new { id = 1 }).ToSql().Replace("\r\n", "");
            Assert.Equal("DELETE FROM `tb_topic` WHERE (id = @id)", sql);

            var item = new Topic { Id = 1, Title = "newtitle" };
            sql = delete.Where(item).ToSql().Replace("\r\n", "");
            Assert.Equal("DELETE FROM `tb_topic` WHERE (`Id` = 1)", sql);

            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

            sql = delete.Where(items).ToSql().Replace("\r\n", "");
            Assert.Equal("DELETE FROM `tb_topic` WHERE (`Id` IN (1,2,3,4,5,6,7,8,9,10))", sql);
        }
        [Fact]
        public void WhereExists()
        {

        }
        [Fact]
        public void ExecuteAffrows()
        {

            var id = g.mysql.Insert<Topic>(new Topic { Title = "xxxx" }).ExecuteIdentity();
            Assert.Equal(1, delete.Where(a => a.Id == id).ExecuteAffrows());
        }
        [Fact]
        public void ExecuteDeleted()
        {

            //delete.Where(a => a.Id > 0).ExecuteDeleted();
        }

        [Fact]
        public void AsTable()
        {
            Assert.Null(g.mysql.Delete<Topic>().ToSql());
            var sql = g.mysql.Delete<Topic>(new[] { 1, 2 }).AsTable(a => "TopicAsTable").ToSql();
            Assert.Equal("DELETE FROM `TopicAsTable` WHERE (`Id` = 1 OR `Id` = 2)", sql);

            sql = g.mysql.Delete<Topic>(new Topic { Id = 1, Title = "test" }).AsTable(a => "TopicAsTable").ToSql();
            Assert.Equal("DELETE FROM `TopicAsTable` WHERE (`Id` = 1)", sql);

            sql = g.mysql.Delete<Topic>(new[] { new Topic { Id = 1, Title = "test" }, new Topic { Id = 2, Title = "test" } }).AsTable(a => "TopicAsTable").ToSql();
            Assert.Equal("DELETE FROM `TopicAsTable` WHERE (`Id` = 1 OR `Id` = 2)", sql);

            sql = g.mysql.Delete<Topic>(new { id = 1 }).AsTable(a => "TopicAsTable").ToSql();
            Assert.Equal("DELETE FROM `TopicAsTable` WHERE (`Id` = 1)", sql);
        }
    }
}
