using FreeSql.DataAnnotations;
using FreeSql.Tests.DataContext.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.SqlServer {
	[Collection("SqlServerCollection")]
	public class SqlServerInsertTest
	{
		SqlServerFixture _sqlserverFixture;

		public SqlServerInsertTest(SqlServerFixture sqlserverFixture)
		{
			_sqlserverFixture = sqlserverFixture;
		}

		IInsert<Topic> insert => _sqlserverFixture.SqlServer.Insert<Topic>(); //��������

		[Table(Name = "tb_topic")]
		class Topic {
			[Column(IsIdentity = true, IsPrimary = true)]
			public int Id { get; set; }
			public int? Clicks { get; set; }
			public int TypeGuid { get; set; }
			public TestTypeInfo Type { get; set; }
			public string Title { get; set; }
			public DateTime CreateTime { get; set; }
		}
 
		[Fact]
		public void AppendData() {
			var items = new List<Topic>();
			for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100, CreateTime = DateTime.Now });

			var sql = insert.IgnoreColumns(a => a.TypeGuid).AppendData(items.First()).ToSql();
			Assert.Equal("INSERT INTO [tb_topic]([Clicks], [Title], [CreateTime]) VALUES(@Clicks0, @Title0, @CreateTime0)", sql);

			sql = insert.IgnoreColumns(a => a.TypeGuid).AppendData(items).ToSql();
			Assert.Equal("INSERT INTO [tb_topic]([Clicks], [Title], [CreateTime]) VALUES(@Clicks0, @Title0, @CreateTime0), (@Clicks1, @Title1, @CreateTime1), (@Clicks2, @Title2, @CreateTime2), (@Clicks3, @Title3, @CreateTime3), (@Clicks4, @Title4, @CreateTime4), (@Clicks5, @Title5, @CreateTime5), (@Clicks6, @Title6, @CreateTime6), (@Clicks7, @Title7, @CreateTime7), (@Clicks8, @Title8, @CreateTime8), (@Clicks9, @Title9, @CreateTime9)", sql);

			sql = insert.AppendData(items).InsertColumns(a => a.Title).ToSql();
			Assert.Equal("INSERT INTO [tb_topic]([Title]) VALUES(@Title0), (@Title1), (@Title2), (@Title3), (@Title4), (@Title5), (@Title6), (@Title7), (@Title8), (@Title9)", sql);

			sql = insert.IgnoreColumns(a => new { a.CreateTime, a.TypeGuid }).AppendData(items).ToSql();
			Assert.Equal("INSERT INTO [tb_topic]([Clicks], [Title]) VALUES(@Clicks0, @Title0), (@Clicks1, @Title1), (@Clicks2, @Title2), (@Clicks3, @Title3), (@Clicks4, @Title4), (@Clicks5, @Title5), (@Clicks6, @Title6), (@Clicks7, @Title7), (@Clicks8, @Title8), (@Clicks9, @Title9)", sql);
		}

		[Fact]
		public void InsertColumns() {
			var items = new List<Topic>();
			for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100, CreateTime = DateTime.Now });

			var sql = insert.AppendData(items).InsertColumns(a => a.Title).ToSql();
			Assert.Equal("INSERT INTO [tb_topic]([Title]) VALUES(@Title0), (@Title1), (@Title2), (@Title3), (@Title4), (@Title5), (@Title6), (@Title7), (@Title8), (@Title9)", sql);

			sql = insert.AppendData(items).InsertColumns(a =>new { a.Title, a.Clicks }).ToSql();
			Assert.Equal("INSERT INTO [tb_topic]([Clicks], [Title]) VALUES(@Clicks0, @Title0), (@Clicks1, @Title1), (@Clicks2, @Title2), (@Clicks3, @Title3), (@Clicks4, @Title4), (@Clicks5, @Title5), (@Clicks6, @Title6), (@Clicks7, @Title7), (@Clicks8, @Title8), (@Clicks9, @Title9)", sql);
		}
		[Fact]
		public void IgnoreColumns() {
			var items = new List<Topic>();
			for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100, CreateTime = DateTime.Now });

			var sql = insert.AppendData(items).IgnoreColumns(a => new { a.CreateTime, a.TypeGuid }).ToSql();
			Assert.Equal("INSERT INTO [tb_topic]([Clicks], [Title]) VALUES(@Clicks0, @Title0), (@Clicks1, @Title1), (@Clicks2, @Title2), (@Clicks3, @Title3), (@Clicks4, @Title4), (@Clicks5, @Title5), (@Clicks6, @Title6), (@Clicks7, @Title7), (@Clicks8, @Title8), (@Clicks9, @Title9)", sql);

			sql = insert.AppendData(items).IgnoreColumns(a => new { a.Title, a.CreateTime, a.TypeGuid }).ToSql();
			Assert.Equal("INSERT INTO [tb_topic]([Clicks]) VALUES(@Clicks0), (@Clicks1), (@Clicks2), (@Clicks3), (@Clicks4), (@Clicks5), (@Clicks6), (@Clicks7), (@Clicks8), (@Clicks9)", sql);
		}
		[Fact]
		public void ExecuteAffrows() {
			var items = new List<Topic>();
			for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100, CreateTime = DateTime.Now });

			Assert.Equal(1, insert.AppendData(items.First()).ExecuteAffrows());
			Assert.Equal(10, insert.AppendData(items).ExecuteAffrows());

			//items = Enumerable.Range(0, 9989).Select(a => new Topic { Title = "newtitle" + a, CreateTime = DateTime.Now }).ToList();
			//Assert.Equal(9989, _sqlserverFixture.SqlServer.Insert<Topic>(items).ExecuteAffrows());
		}
		[Fact]
		public void ExecuteIdentity() {
			var items = new List<Topic>();
			for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100, CreateTime = DateTime.Now });

			Assert.NotEqual(0, insert.AppendData(items.First()).ExecuteIdentity());


			//items = Enumerable.Range(0, 9999).Select(a => new Topic { Title = "newtitle" + a, CreateTime = DateTime.Now }).ToList();
			//var lastId = g.sqlite.Select<Topic>().Max(a => a.Id);
			//Assert.NotEqual(lastId, _sqlserverFixture.SqlServer.Insert<Topic>(items).ExecuteIdentity());
		}
		[Fact]
		public void ExecuteInserted() {
			var items = new List<Topic>();
			for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100, CreateTime = DateTime.Now });

			var items2 = insert.AppendData(items).ExecuteInserted();

			items = Enumerable.Range(0, 90).Select(a => new Topic { Title = "newtitle" + a, CreateTime = DateTime.Now }).ToList();
			var itemsInserted = _sqlserverFixture.SqlServer.Insert<Topic>(items).ExecuteInserted();
			Assert.Equal(items.First().Title, itemsInserted.First().Title);
			Assert.Equal(items.Last().Title, itemsInserted.Last().Title);
		}

		[Fact]
		public void AsTable() {
			var items = new List<Topic>();
			for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100, CreateTime = DateTime.Now });

			var sql = insert.IgnoreColumns(a => a.TypeGuid).AppendData(items.First()).AsTable(a => "tb_topicAsTable").ToSql();
			Assert.Equal("INSERT INTO [tb_topicAsTable]([Clicks], [Title], [CreateTime]) VALUES(@Clicks0, @Title0, @CreateTime0)", sql);

			sql = insert.IgnoreColumns(a => a.TypeGuid).AppendData(items).AsTable(a => "tb_topicAsTable").ToSql();
			Assert.Equal("INSERT INTO [tb_topicAsTable]([Clicks], [Title], [CreateTime]) VALUES(@Clicks0, @Title0, @CreateTime0), (@Clicks1, @Title1, @CreateTime1), (@Clicks2, @Title2, @CreateTime2), (@Clicks3, @Title3, @CreateTime3), (@Clicks4, @Title4, @CreateTime4), (@Clicks5, @Title5, @CreateTime5), (@Clicks6, @Title6, @CreateTime6), (@Clicks7, @Title7, @CreateTime7), (@Clicks8, @Title8, @CreateTime8), (@Clicks9, @Title9, @CreateTime9)", sql);

			sql = insert.IgnoreColumns(a => a.TypeGuid).AppendData(items).InsertColumns(a => a.Title).AsTable(a => "tb_topicAsTable").ToSql();
			Assert.Equal("INSERT INTO [tb_topicAsTable]([Title]) VALUES(@Title0), (@Title1), (@Title2), (@Title3), (@Title4), (@Title5), (@Title6), (@Title7), (@Title8), (@Title9)", sql);

			sql = insert.IgnoreColumns(a => new { a.CreateTime, a.TypeGuid }).AppendData(items).AsTable(a => "tb_topicAsTable").ToSql();
			Assert.Equal("INSERT INTO [tb_topicAsTable]([Clicks], [Title]) VALUES(@Clicks0, @Title0), (@Clicks1, @Title1), (@Clicks2, @Title2), (@Clicks3, @Title3), (@Clicks4, @Title4), (@Clicks5, @Title5), (@Clicks6, @Title6), (@Clicks7, @Title7), (@Clicks8, @Title8), (@Clicks9, @Title9)", sql);

			sql = insert.IgnoreColumns(a => new { a.Title, a.TypeGuid }).InsertColumns(a => a.Title).AppendData(items).AsTable(a => "tb_topicAsTable").ToSql();
			Assert.Equal("INSERT INTO [tb_topicAsTable]([Title]) VALUES(@Title0), (@Title1), (@Title2), (@Title3), (@Title4), (@Title5), (@Title6), (@Title7), (@Title8), (@Title9)", sql);

			sql = insert.IgnoreColumns(a => a.TypeGuid).AppendData(items).InsertColumns(a => new { a.Title, a.Clicks }).AsTable(a => "tb_topicAsTable").ToSql();
			Assert.Equal("INSERT INTO [tb_topicAsTable]([Clicks], [Title]) VALUES(@Clicks0, @Title0), (@Clicks1, @Title1), (@Clicks2, @Title2), (@Clicks3, @Title3), (@Clicks4, @Title4), (@Clicks5, @Title5), (@Clicks6, @Title6), (@Clicks7, @Title7), (@Clicks8, @Title8), (@Clicks9, @Title9)", sql);

			sql = insert.IgnoreColumns(a => new { a.CreateTime, a.TypeGuid }).AppendData(items).AsTable(a => "tb_topicAsTable").ToSql();
			Assert.Equal("INSERT INTO [tb_topicAsTable]([Clicks], [Title]) VALUES(@Clicks0, @Title0), (@Clicks1, @Title1), (@Clicks2, @Title2), (@Clicks3, @Title3), (@Clicks4, @Title4), (@Clicks5, @Title5), (@Clicks6, @Title6), (@Clicks7, @Title7), (@Clicks8, @Title8), (@Clicks9, @Title9)", sql);

			sql = insert.IgnoreColumns(a => new { a.CreateTime, a.Title, a.TypeGuid }).AppendData(items).AsTable(a => "tb_topicAsTable").ToSql();
			Assert.Equal("INSERT INTO [tb_topicAsTable]([Clicks]) VALUES(@Clicks0), (@Clicks1), (@Clicks2), (@Clicks3), (@Clicks4), (@Clicks5), (@Clicks6), (@Clicks7), (@Clicks8), (@Clicks9)", sql);
		}
	}
}
