using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.Odbc.Default
{
    public class OdbcUpdateTest
    {
        IUpdate<Topic> update => g.odbc.Update<Topic>();

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
        public void Dywhere()
        {
            Assert.Null(g.odbc.Update<Topic>().ToSql());
            Assert.Equal("UPDATE [tb_topic] SET title='test' \r\nWHERE ([Id] = 1 OR [Id] = 2)", g.odbc.Update<Topic>(new[] { 1, 2 }).SetRaw("title='test'").ToSql());
            Assert.Equal("UPDATE [tb_topic] SET title='test1' \r\nWHERE ([Id] = 1)", g.odbc.Update<Topic>(new Topic { Id = 1, Title = "test" }).SetRaw("title='test1'").ToSql());
            Assert.Equal("UPDATE [tb_topic] SET title='test1' \r\nWHERE ([Id] = 1 OR [Id] = 2)", g.odbc.Update<Topic>(new[] { new Topic { Id = 1, Title = "test" }, new Topic { Id = 2, Title = "test" } }).SetRaw("title='test1'").ToSql());
            Assert.Equal("UPDATE [tb_topic] SET title='test1' \r\nWHERE ([Id] = 1)", g.odbc.Update<Topic>(new { id = 1 }).SetRaw("title='test1'").ToSql());
        }

        [Fact]
        public void SetSource()
        {
            var sql = update.SetSource(new Topic { Id = 1, Title = "newtitle" }).IgnoreColumns(a => a.TypeGuid).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET [Clicks] = NULL, [Title] = N'newtitle', [CreateTime] = '1970-01-01 00:00:00' WHERE ([Id] = 1)", sql);

            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });
            items[0].Clicks = null;

            sql = update.SetSource(items).IgnoreColumns(a => a.TypeGuid).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET [Clicks] = CASE [Id] WHEN 1 THEN NULL WHEN 2 THEN 100 WHEN 3 THEN 200 WHEN 4 THEN 300 WHEN 5 THEN 400 WHEN 6 THEN 500 WHEN 7 THEN 600 WHEN 8 THEN 700 WHEN 9 THEN 800 WHEN 10 THEN 900 END, [Title] = CASE [Id] WHEN 1 THEN N'newtitle0' WHEN 2 THEN N'newtitle1' WHEN 3 THEN N'newtitle2' WHEN 4 THEN N'newtitle3' WHEN 5 THEN N'newtitle4' WHEN 6 THEN N'newtitle5' WHEN 7 THEN N'newtitle6' WHEN 8 THEN N'newtitle7' WHEN 9 THEN N'newtitle8' WHEN 10 THEN N'newtitle9' END, [CreateTime] = CASE [Id] WHEN 1 THEN '1970-01-01 00:00:00' WHEN 2 THEN '1970-01-01 00:00:00' WHEN 3 THEN '1970-01-01 00:00:00' WHEN 4 THEN '1970-01-01 00:00:00' WHEN 5 THEN '1970-01-01 00:00:00' WHEN 6 THEN '1970-01-01 00:00:00' WHEN 7 THEN '1970-01-01 00:00:00' WHEN 8 THEN '1970-01-01 00:00:00' WHEN 9 THEN '1970-01-01 00:00:00' WHEN 10 THEN '1970-01-01 00:00:00' END WHERE ([Id] IN (1,2,3,4,5,6,7,8,9,10))", sql);

            sql = update.SetSource(items).IgnoreColumns(a => new { a.Clicks, a.CreateTime, a.TypeGuid }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET [Title] = CASE [Id] WHEN 1 THEN N'newtitle0' WHEN 2 THEN N'newtitle1' WHEN 3 THEN N'newtitle2' WHEN 4 THEN N'newtitle3' WHEN 5 THEN N'newtitle4' WHEN 6 THEN N'newtitle5' WHEN 7 THEN N'newtitle6' WHEN 8 THEN N'newtitle7' WHEN 9 THEN N'newtitle8' WHEN 10 THEN N'newtitle9' END WHERE ([Id] IN (1,2,3,4,5,6,7,8,9,10))", sql);

            sql = update.SetSource(items).IgnoreColumns(a => a.TypeGuid).Set(a => a.CreateTime, new DateTime(2020, 1, 1)).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET [CreateTime] = '2020-01-01 00:00:00' WHERE ([Id] IN (1,2,3,4,5,6,7,8,9,10))", sql);
        }
        [Fact]
        public void IgnoreColumns()
        {
            var sql = update.SetSource(new Topic { Id = 1, Title = "newtitle" }).IgnoreColumns(a => new { a.Clicks, a.CreateTime, a.TypeGuid }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET [Title] = N'newtitle' WHERE ([Id] = 1)", sql);
        }
        [Fact]
        public void UpdateColumns()
        {
            var sql = update.SetSource(new Topic { Id = 1, Title = "newtitle" }).UpdateColumns(a => a.Title).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET [Title] = N'newtitle' WHERE ([Id] = 1)", sql);
        }
        [Fact]
        public void Set()
        {
            var sql = update.Where(a => a.Id == 1).Set(a => a.Title, "newtitle").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET [Title] = N'newtitle' WHERE ([Id] = 1)", sql);

            sql = update.Where(a => a.Id == 1).Set(a => a.Title, "newtitle").Set(a => a.CreateTime, new DateTime(2020, 1, 1)).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET [Title] = N'newtitle', [CreateTime] = '2020-01-01 00:00:00' WHERE ([Id] = 1)", sql);

            sql = update.Set(a => a.Clicks * 10 / 1).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET [Clicks] = isnull([Clicks], 0) * 10 / 1 WHERE ([Id] = 1)", sql);

            sql = update.Set(a => a.Id - 10).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET [Id] = ([Id] - 10) WHERE ([Id] = 1)", sql);

            int incrv = 10;
            sql = update.Set(a => a.Clicks * incrv / 1).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET [Clicks] = isnull([Clicks], 0) * 10 / 1 WHERE ([Id] = 1)", sql);

            sql = update.Set(a => a.Id - incrv).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET [Id] = ([Id] - 10) WHERE ([Id] = 1)", sql);

            sql = update.Set(a => a.Clicks == a.Clicks * 10 / 1).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET [Clicks] = [Clicks] * 10 / 1 WHERE ([Id] = 1)", sql);

            var dt2000 = DateTime.Parse("2000-01-01");
            sql = update.Set(a => a.Clicks == (a.CreateTime > dt2000 ? 1 : 2)).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET [Clicks] = case when [CreateTime] > '2000-01-01 00:00:00' then 1 else 2 end WHERE ([Id] = 1)", sql);

            sql = update.Set(a => a.Id == 10).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET [Id] = 10 WHERE ([Id] = 1)", sql);
        }
        [Fact]
        public void SetRaw()
        {
            var sql = update.Where(a => a.Id == 1).SetRaw("clicks = clicks + ?", new { incrClick = 1 }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET clicks = clicks + ? WHERE ([Id] = 1)", sql);
        }
        [Fact]
        public void Where()
        {
            var sql = update.Where(a => a.Id == 1).SetRaw("title='newtitle'").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET title='newtitle' WHERE ([Id] = 1)", sql);

            sql = update.Where("id = ?", new { id = 1 }).SetRaw("title='newtitle'").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET title='newtitle' WHERE (id = ?)", sql);

            var item = new Topic { Id = 1, Title = "newtitle" };
            sql = update.Where(item).SetRaw("title='newtitle'").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET title='newtitle' WHERE ([Id] = 1)", sql);

            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });
            sql = update.Where(items).SetRaw("title='newtitle'").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET title='newtitle' WHERE ([Id] IN (1,2,3,4,5,6,7,8,9,10))", sql);
        }
        [Fact]
        public void ExecuteAffrows()
        {
            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

            var time = DateTime.Now;
            var items222 = g.odbc.Select<Topic>().Where(a => a.CreateTime > time).Limit(10).ToList();

            update.SetSource(items.First()).NoneParameter().ExecuteAffrows();
            update.SetSource(items).NoneParameter().ExecuteAffrows();
        }

        [Fact]
        public void AsTable()
        {
            Assert.Null(g.odbc.Update<Topic>().ToSql());
            Assert.Equal("UPDATE [tb_topicAsTable] SET title='test' \r\nWHERE ([Id] = 1 OR [Id] = 2)", g.odbc.Update<Topic>(new[] { 1, 2 }).SetRaw("title='test'").AsTable(a => "tb_topicAsTable").ToSql());
            Assert.Equal("UPDATE [tb_topicAsTable] SET title='test1' \r\nWHERE ([Id] = 1)", g.odbc.Update<Topic>(new Topic { Id = 1, Title = "test" }).SetRaw("title='test1'").AsTable(a => "tb_topicAsTable").ToSql());
            Assert.Equal("UPDATE [tb_topicAsTable] SET title='test1' \r\nWHERE ([Id] = 1 OR [Id] = 2)", g.odbc.Update<Topic>(new[] { new Topic { Id = 1, Title = "test" }, new Topic { Id = 2, Title = "test" } }).SetRaw("title='test1'").AsTable(a => "tb_topicAsTable").ToSql());
            Assert.Equal("UPDATE [tb_topicAsTable] SET title='test1' \r\nWHERE ([Id] = 1)", g.odbc.Update<Topic>(new { id = 1 }).SetRaw("title='test1'").AsTable(a => "tb_topicAsTable").ToSql());
        }
    }
}
