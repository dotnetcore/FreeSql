using FreeSql.DataAnnotations;
using FreeSql.Tests.DataContext.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
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

        IUpdate<Topic> update => g.sqlserver.Update<Topic>();

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
            Assert.Null(g.sqlserver.Update<Topic>().ToSql());
            Assert.Equal("UPDATE [tb_topic] SET title='test' \r\nWHERE ([Id] IN (1,2))", g.sqlserver.Update<Topic>(new[] { 1, 2 }).SetRaw("title='test'").ToSql());
            Assert.Equal("UPDATE [tb_topic] SET title='test1' \r\nWHERE ([Id] = 1)", g.sqlserver.Update<Topic>(new Topic { Id = 1, Title = "test" }).SetRaw("title='test1'").ToSql());
            Assert.Equal("UPDATE [tb_topic] SET title='test1' \r\nWHERE ([Id] IN (1,2))", g.sqlserver.Update<Topic>(new[] { new Topic { Id = 1, Title = "test" }, new Topic { Id = 2, Title = "test" } }).SetRaw("title='test1'").ToSql());
            Assert.Equal("UPDATE [tb_topic] SET title='test1' \r\nWHERE ([Id] = 1)", g.sqlserver.Update<Topic>(new { id = 1 }).SetRaw("title='test1'").ToSql());
        }

        [Fact]
        public void SetSource()
        {
            var sql = update.SetSource(new Topic { Id = 1, Title = "newtitle" }).IgnoreColumns(a => a.TypeGuid).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET [Clicks] = @p_0, [Title] = @p_1, [CreateTime] = @p_2 WHERE ([Id] = 1)", sql);

            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });
            items[0].Clicks = null;

            sql = update.SetSource(items).IgnoreColumns(a => a.TypeGuid).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET [Clicks] = CASE [Id] WHEN 1 THEN @p_0 WHEN 2 THEN @p_1 WHEN 3 THEN @p_2 WHEN 4 THEN @p_3 WHEN 5 THEN @p_4 WHEN 6 THEN @p_5 WHEN 7 THEN @p_6 WHEN 8 THEN @p_7 WHEN 9 THEN @p_8 WHEN 10 THEN @p_9 END, [Title] = CASE [Id] WHEN 1 THEN @p_10 WHEN 2 THEN @p_11 WHEN 3 THEN @p_12 WHEN 4 THEN @p_13 WHEN 5 THEN @p_14 WHEN 6 THEN @p_15 WHEN 7 THEN @p_16 WHEN 8 THEN @p_17 WHEN 9 THEN @p_18 WHEN 10 THEN @p_19 END, [CreateTime] = CASE [Id] WHEN 1 THEN @p_20 WHEN 2 THEN @p_21 WHEN 3 THEN @p_22 WHEN 4 THEN @p_23 WHEN 5 THEN @p_24 WHEN 6 THEN @p_25 WHEN 7 THEN @p_26 WHEN 8 THEN @p_27 WHEN 9 THEN @p_28 WHEN 10 THEN @p_29 END WHERE ([Id] IN (1,2,3,4,5,6,7,8,9,10))", sql);

            sql = update.SetSource(items).IgnoreColumns(a => new { a.Clicks, a.CreateTime, a.TypeGuid }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET [Title] = CASE [Id] WHEN 1 THEN @p_0 WHEN 2 THEN @p_1 WHEN 3 THEN @p_2 WHEN 4 THEN @p_3 WHEN 5 THEN @p_4 WHEN 6 THEN @p_5 WHEN 7 THEN @p_6 WHEN 8 THEN @p_7 WHEN 9 THEN @p_8 WHEN 10 THEN @p_9 END WHERE ([Id] IN (1,2,3,4,5,6,7,8,9,10))", sql);

            sql = update.SetSource(items).IgnoreColumns(a => a.TypeGuid).Set(a => a.CreateTime, new DateTime(2020, 1, 1)).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET [CreateTime] = @p_0 WHERE ([Id] IN (1,2,3,4,5,6,7,8,9,10))", sql);

            sql = g.sqlserver.Update<ts_source_mpk>().SetSource(new[] {
                new ts_source_mpk { id1 = 1, id2 = 7, xx = "a1" }, 
                new ts_source_mpk { id1 = 1, id2 = 8, xx = "b122" } 
            }).NoneParameter().ToSql().Replace("\r\n", "");
        }
        public class ts_source_mpk
        {
            [Column(IsPrimary = true)]
            public int id1 { get; set; }
            [Column(IsPrimary = true)]
            public int id2 { get; set; }
            public string xx { get; set; }
        }
        [Fact]
        public void SetSourceIgnore()
        {
            Assert.Equal("UPDATE [tssi01] SET [tint] = 10 WHERE ([id] = '00000000-0000-0000-0000-000000000000')",
                g.sqlserver.Update<tssi01>().NoneParameter()
                    .SetSourceIgnore(new tssi01 { id = Guid.Empty, tint = 10 }, col => col == null).ToSql().Replace("\r\n", ""));
        }
        public class tssi01
        {
            [Column(CanUpdate = false)]
            public Guid id { get; set; }
            public int tint { get; set; }
            public string title { get; set; }
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

            var dt2000 = DateTime.Parse("2000-01-01");
            sql = update.Set(a => a.Clicks == (a.CreateTime > dt2000 ? 1 : 2)).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET [Clicks] = case when [CreateTime] > '2000-01-01 00:00:00.000' then 1 else 2 end WHERE ([Id] = 1)", sql);

            sql = update.Set(a => a.Id == 10).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET [Id] = 10 WHERE ([Id] = 1)", sql);

            sql = update.Set(a => a.Clicks == null).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET [Clicks] = NULL WHERE ([Id] = 1)", sql);
        }
        [Fact]
        public void SetRaw()
        {
            var sql = update.Where(a => a.Id == 1).SetRaw("clicks = clicks + @incrClick", new { incrClick = 1 }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET clicks = clicks + @incrClick WHERE ([Id] = 1)", sql);
        }
        [Fact]
        public void SetDto()
        {
            var sql = update.SetDto(new { clicks = 1, title = "xxx" }).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET [Clicks] = @p_0, [Title] = @p_1 WHERE ([Id] = 1)", sql);
            sql = update.NoneParameter().SetDto(new { clicks = 1, title = "xxx" }).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET [Clicks] = 1, [Title] = N'xxx' WHERE ([Id] = 1)", sql);

            sql = update.SetDto(new Dictionary<string, object> { ["clicks"] = 1, ["title"] = "xxx" }).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET [Clicks] = @p_0, [Title] = @p_1 WHERE ([Id] = 1)", sql);
            sql = update.NoneParameter().SetDto(new Dictionary<string, object> { ["clicks"] = 1, ["title"] = "xxx" }).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE [tb_topic] SET [Clicks] = 1, [Title] = N'xxx' WHERE ([Id] = 1)", sql);
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
        public void ExecuteAffrows()
        {
            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

            var time = DateTime.Now;
            var items222 = g.sqlserver.Select<Topic>().Where(a => a.CreateTime > time).Limit(10).ToList();

            update.SetSource(items.First()).NoneParameter().ExecuteAffrows();
            update.SetSource(items).NoneParameter().ExecuteAffrows();
        }
        [Fact]
        public void ExecuteUpdated()
        {
            g.sqlserver.Insert<Topic>().AppendData(new Topic()).ExecuteAffrows();
            g.sqlserver.Insert<Topic>().AppendData(new Topic()).ExecuteAffrows();
            g.sqlserver.Insert<Topic>().AppendData(new Topic()).ExecuteAffrows();
            g.sqlserver.Insert<Topic>().AppendData(new Topic()).ExecuteAffrows();

            var items = g.sqlserver.Select<Topic>().Limit(2).ToList();
            g.sqlserver.Update<Topic>(items).SetRaw("Title='test'").ExecuteUpdated();

            items = g.sqlserver.Select<Topic>().Limit(2).ToList();
            var result = g.sqlserver.Update<Topic>(items).SetRaw("Title='test'").ExecuteUpdatedAsync().Result;
        }

        [Fact]
        public void AsTable()
        {
            Assert.Null(g.sqlserver.Update<Topic>().ToSql());
            Assert.Equal("UPDATE [tb_topicAsTable] SET title='test' \r\nWHERE ([Id] IN (1,2))", g.sqlserver.Update<Topic>(new[] { 1, 2 }).SetRaw("title='test'").AsTable(a => "tb_topicAsTable").ToSql());
            Assert.Equal("UPDATE [tb_topicAsTable] SET title='test1' \r\nWHERE ([Id] = 1)", g.sqlserver.Update<Topic>(new Topic { Id = 1, Title = "test" }).SetRaw("title='test1'").AsTable(a => "tb_topicAsTable").ToSql());
            Assert.Equal("UPDATE [tb_topicAsTable] SET title='test1' \r\nWHERE ([Id] IN (1,2))", g.sqlserver.Update<Topic>(new[] { new Topic { Id = 1, Title = "test" }, new Topic { Id = 2, Title = "test" } }).SetRaw("title='test1'").AsTable(a => "tb_topicAsTable").ToSql());
            Assert.Equal("UPDATE [tb_topicAsTable] SET title='test1' \r\nWHERE ([Id] = 1)", g.sqlserver.Update<Topic>(new { id = 1 }).SetRaw("title='test1'").AsTable(a => "tb_topicAsTable").ToSql());
        }
    }
}
