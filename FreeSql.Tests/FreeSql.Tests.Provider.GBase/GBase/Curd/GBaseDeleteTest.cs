using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.GBase
{
    public class GBaseDeleteTest
    {

        IDelete<Topic> delete => g.gbase.Delete<Topic>();

        [Table(Name = "tb_topic_del")]
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
            Assert.Null(g.gbase.Delete<Topic>().ToSql());
            var sql = g.gbase.Delete<Topic>(new[] { 1, 2 }).ToSql();
            Assert.Equal("DELETE FROM tb_topic_del WHERE (Id IN (1,2))", sql);

            sql = g.gbase.Delete<Topic>(new Topic { Id = 1, Title = "test" }).ToSql();
            Assert.Equal("DELETE FROM tb_topic_del WHERE (Id = 1)", sql);

            sql = g.gbase.Delete<Topic>(new[] { new Topic { Id = 1, Title = "test" }, new Topic { Id = 2, Title = "test" } }).ToSql();
            Assert.Equal("DELETE FROM tb_topic_del WHERE (Id IN (1,2))", sql);

            sql = g.gbase.Delete<Topic>(new { id = 1 }).ToSql();
            Assert.Equal("DELETE FROM tb_topic_del WHERE (Id = 1)", sql);

            sql = g.gbase.Delete<MultiPkTopic>(new[] { new { Id1 = 1, Id2 = 10 }, new { Id1 = 2, Id2 = 20 } }).ToSql();
            Assert.Equal("DELETE FROM MultiPkTopic WHERE (Id1 = 1 AND Id2 = 10 OR Id1 = 2 AND Id2 = 20)", sql);
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
            Assert.Equal("DELETE FROM tb_topic_del WHERE (Id = 1)", sql);

            sql = delete.Where("id = @id", new { id = 1 }).ToSql().Replace("\r\n", "");
            Assert.Equal("DELETE FROM tb_topic_del WHERE (id = @id)", sql);

            var item = new Topic { Id = 1, Title = "newtitle" };
            sql = delete.Where(item).ToSql().Replace("\r\n", "");
            Assert.Equal("DELETE FROM tb_topic_del WHERE (Id = 1)", sql);

            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

            sql = delete.Where(items).ToSql().Replace("\r\n", "");
            Assert.Equal("DELETE FROM tb_topic_del WHERE (Id IN (1,2,3,4,5,6,7,8,9,10))", sql);
        }
        [Fact]
        public void ExecuteAffrows()
        {

            var id = g.gbase.Insert<Topic>(new Topic { Title = "xxxx" }).ExecuteIdentity();
            Assert.Equal(1, delete.Where(a => a.Id == id).ExecuteAffrows());
        }
        [Fact]
        public void ExecuteDeleted()
        {
            Assert.Throws<NotImplementedException>(() => delete.Where(a => a.Id > 0).ExecuteDeleted());
        }

        [Fact]
        public void AsTable()
        {
            //var connectionBuilder = new IfxConnectionStringBuilder
            //{
            //    Host = "192.168.164.134",
            //    Service = "9088",
            //    Server = "gbase01",
            //    Database = "testdb",
            //    UID = "gbasedbt",
            //    Pwd = "GBase123",
            //    DbLocale = "zh_CN.utf8",
            //    ClientLocale = "zh_CN.utf8",
            //    PersistSecurityInfo = true
            //};
            //using (IfxConnection conn = new IfxConnection(connectionBuilder.ConnectionString))
            //{
            //    conn.Open();

            //    var cmd = conn.CreateCommand();
            //    cmd.CommandText = "select 1 from dual";
            //    var val = cmd.ExecuteScalar();

            //    conn.Close();
            //}


            Assert.Null(g.gbase.Delete<Topic>().ToSql());
            var sql = g.gbase.Delete<Topic>(new[] { 1, 2 }).AsTable(a => "TopicAsTable").ToSql();
            Assert.Equal("DELETE FROM TopicAsTable WHERE (Id IN (1,2))", sql);

            sql = g.gbase.Delete<Topic>(new Topic { Id = 1, Title = "test" }).AsTable(a => "TopicAsTable").ToSql();
            Assert.Equal("DELETE FROM TopicAsTable WHERE (Id = 1)", sql);

            sql = g.gbase.Delete<Topic>(new[] { new Topic { Id = 1, Title = "test" }, new Topic { Id = 2, Title = "test" } }).AsTable(a => "TopicAsTable").ToSql();
            Assert.Equal("DELETE FROM TopicAsTable WHERE (Id IN (1,2))", sql);

            sql = g.gbase.Delete<Topic>(new { id = 1 }).AsTable(a => "TopicAsTable").ToSql();
            Assert.Equal("DELETE FROM TopicAsTable WHERE (Id = 1)", sql);
        }
    }
}
