using FreeSql.DataAnnotations;
using FreeSql.Tests.DataContext.SqlServer;
using System;
using System.Collections.Generic;
using Xunit;

namespace FreeSql.Tests.SqlServer
{
    [Collection("SqlServerCollection")]
    public class SqlServerUpdateTest
    {

        SqlServerFixture _sqlserverFixture;

        public SqlServerUpdateTest(SqlServerFixture sqlserverFixture)
        {
            _sqlserverFixture = sqlserverFixture;
        }

        IUpdate<Topic> update => _sqlserverFixture.SqlServer.Update<Topic>();

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
            Assert.Null(_sqlserverFixture.SqlServer.Update<Topic>().ToSql());
            Assert.Equal("UPDATE [tb_topic] SET title='test' \r\nWHERE ([Id] = 1 OR [Id] = 2)", _sqlserverFixture.SqlServer.Update<Topic>(new[] { 1, 2 }).SetRaw("title='test'").ToSql());
            Assert.Equal("UPDATE [tb_topic] SET title='test1' \r\nWHERE ([Id] = 1)", _sqlserverFixture.SqlServer.Update<Topic>(new Topic { Id = 1, Title = "test" }).SetRaw("title='test1'").ToSql());
            Assert.Equal("UPDATE [tb_topic] SET title='test1' \r\nWHERE ([Id] = 1 OR [Id] = 2)", _sqlserverFixture.SqlServer.Update<Topic>(new[] { new Topic { Id = 1, Title = "test" }, new Topic { Id = 2, Title = "test" } }).SetRaw("title='test1'").ToSql());
            Assert.Equal("UPDATE [tb_topic] SET title='test1' \r\nWHERE ([Id] = 1)", _sqlserverFixture.SqlServer.Update<Topic>(new { id = 1 }).SetRaw("title='test1'").ToSql());
        }

        [Fact]
        public void SetSource()
        {
            var sql = update.SetSource(new Topic { Id = 1, Title = "newtitle" }).IgnoreColumns(a => a.TypeGuid).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET [Clicks] = @p_0, [Title] = @p_1, [CreateTime] = @p_2 WHERE ([Id] = 1)", sql);

            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

            sql = update.SetSource(items).IgnoreColumns(a => a.TypeGuid).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET [Clicks] = CASE [Id] WHEN 1 THEN @p_0 WHEN 2 THEN @p_1 WHEN 3 THEN @p_2 WHEN 4 THEN @p_3 WHEN 5 THEN @p_4 WHEN 6 THEN @p_5 WHEN 7 THEN @p_6 WHEN 8 THEN @p_7 WHEN 9 THEN @p_8 WHEN 10 THEN @p_9 END, [Title] = CASE [Id] WHEN 1 THEN @p_10 WHEN 2 THEN @p_11 WHEN 3 THEN @p_12 WHEN 4 THEN @p_13 WHEN 5 THEN @p_14 WHEN 6 THEN @p_15 WHEN 7 THEN @p_16 WHEN 8 THEN @p_17 WHEN 9 THEN @p_18 WHEN 10 THEN @p_19 END, [CreateTime] = CASE [Id] WHEN 1 THEN @p_20 WHEN 2 THEN @p_21 WHEN 3 THEN @p_22 WHEN 4 THEN @p_23 WHEN 5 THEN @p_24 WHEN 6 THEN @p_25 WHEN 7 THEN @p_26 WHEN 8 THEN @p_27 WHEN 9 THEN @p_28 WHEN 10 THEN @p_29 END WHERE ([Id] IN (1,2,3,4,5,6,7,8,9,10))", sql);

            sql = update.SetSource(items).IgnoreColumns(a => new { a.Clicks, a.CreateTime, a.TypeGuid }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET [Title] = CASE [Id] WHEN 1 THEN @p_0 WHEN 2 THEN @p_1 WHEN 3 THEN @p_2 WHEN 4 THEN @p_3 WHEN 5 THEN @p_4 WHEN 6 THEN @p_5 WHEN 7 THEN @p_6 WHEN 8 THEN @p_7 WHEN 9 THEN @p_8 WHEN 10 THEN @p_9 END WHERE ([Id] IN (1,2,3,4,5,6,7,8,9,10))", sql);

            sql = update.SetSource(items).IgnoreColumns(a => a.TypeGuid).Set(a => a.CreateTime, new DateTime(2020, 1, 1)).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET [CreateTime] = @p_0 WHERE ([Id] IN (1,2,3,4,5,6,7,8,9,10))", sql);
        }
        [Fact]
        public void IgnoreColumns()
        {
            var sql = update.SetSource(new Topic { Id = 1, Title = "newtitle" }).IgnoreColumns(a => new { a.Clicks, a.CreateTime, a.TypeGuid }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET [Title] = @p_0 WHERE ([Id] = 1)", sql);
        }
        [Fact]
        public void UpdateColumns()
        {
            var sql = update.SetSource(new Topic { Id = 1, Title = "newtitle" }).UpdateColumns(a => a.Title).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET [Title] = @p_0 WHERE ([Id] = 1)", sql);
        }
        [Fact]
        public void Set()
        {
            var sql = update.Where(a => a.Id == 1).Set(a => a.Title, "newtitle").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET [Title] = @p_0 WHERE ([Id] = 1)", sql);

            sql = update.Where(a => a.Id == 1).Set(a => a.Title, "newtitle").Set(a => a.CreateTime, new DateTime(2020, 1, 1)).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET [Title] = @p_0, [CreateTime] = @p_1 WHERE ([Id] = 1)", sql);

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

            sql = update.Set(a => a.Id == 10).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET [Id] = 10 WHERE ([Id] = 1)", sql);
        }
        [Fact]
        public void SetRaw()
        {
            var sql = update.Where(a => a.Id == 1).SetRaw("clicks = clicks + @incrClick", new { incrClick = 1 }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET clicks = clicks + @incrClick WHERE ([Id] = 1)", sql);
        }
        [Fact]
        public void Where()
        {
            var sql = update.Where(a => a.Id == 1).SetRaw("title='newtitle'").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET title='newtitle' WHERE ([Id] = 1)", sql);

            sql = update.Where("id = @id", new { id = 1 }).SetRaw("title='newtitle'").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET title='newtitle' WHERE (id = @id)", sql);

            var item = new Topic { Id = 1, Title = "newtitle" };
            sql = update.Where(item).SetRaw("title='newtitle'").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET title='newtitle' WHERE ([Id] = 1)", sql);

            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });
            sql = update.Where(items).SetRaw("title='newtitle'").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET title='newtitle' WHERE ([Id] IN (1,2,3,4,5,6,7,8,9,10))", sql);
        }
        [Fact]
        public void WhereExists()
        {

        }
        [Fact]
        public void ExecuteAffrows()
        {

        }
        [Fact]
        public void ExecuteUpdated()
        {
            _sqlserverFixture.SqlServer.Insert<Topic>().AppendData(new Topic()).ExecuteAffrows();
            _sqlserverFixture.SqlServer.Insert<Topic>().AppendData(new Topic()).ExecuteAffrows();
            _sqlserverFixture.SqlServer.Insert<Topic>().AppendData(new Topic()).ExecuteAffrows();
            _sqlserverFixture.SqlServer.Insert<Topic>().AppendData(new Topic()).ExecuteAffrows();

            var items = _sqlserverFixture.SqlServer.Select<Topic>().Limit(2).ToList();
            _sqlserverFixture.SqlServer.Update<Topic>(items).SetRaw("title='test'").ExecuteUpdated();

            items = _sqlserverFixture.SqlServer.Select<Topic>().Limit(2).ToList();
            var result = _sqlserverFixture.SqlServer.Update<Topic>(items).SetRaw("title='test'").ExecuteUpdatedAsync().Result;
        }

        [Fact]
        public void AsTable()
        {
            Assert.Null(_sqlserverFixture.SqlServer.Update<Topic>().ToSql());
            Assert.Equal("UPDATE [tb_topicAsTable] SET title='test' \r\nWHERE ([Id] = 1 OR [Id] = 2)", _sqlserverFixture.SqlServer.Update<Topic>(new[] { 1, 2 }).SetRaw("title='test'").AsTable(a => "tb_topicAsTable").ToSql());
            Assert.Equal("UPDATE [tb_topicAsTable] SET title='test1' \r\nWHERE ([Id] = 1)", _sqlserverFixture.SqlServer.Update<Topic>(new Topic { Id = 1, Title = "test" }).SetRaw("title='test1'").AsTable(a => "tb_topicAsTable").ToSql());
            Assert.Equal("UPDATE [tb_topicAsTable] SET title='test1' \r\nWHERE ([Id] = 1 OR [Id] = 2)", _sqlserverFixture.SqlServer.Update<Topic>(new[] { new Topic { Id = 1, Title = "test" }, new Topic { Id = 2, Title = "test" } }).SetRaw("title='test1'").AsTable(a => "tb_topicAsTable").ToSql());
            Assert.Equal("UPDATE [tb_topicAsTable] SET title='test1' \r\nWHERE ([Id] = 1)", _sqlserverFixture.SqlServer.Update<Topic>(new { id = 1 }).SetRaw("title='test1'").AsTable(a => "tb_topicAsTable").ToSql());
        }
    }
}
