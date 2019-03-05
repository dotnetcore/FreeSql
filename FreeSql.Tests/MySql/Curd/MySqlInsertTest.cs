using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.MySql {
	public class MySqlInsertTest {

		IInsert<Topic> insert => g.mysql.Insert<Topic>(); //��������

		[Table(Name = "tb_topic")]
		class Topic {
			[Column(IsIdentity = true, IsPrimary = true)]
			public int Id { get; set; }
			public int Clicks { get; set; }
			public TestTypeInfo Type { get; set; }
			public string Title { get; set; }
			public DateTime CreateTime { get; set; }
		}
 
		[Fact]
		public void AppendData() {
			var items = new List<Topic>();
			for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

			var sql = insert.AppendData(items.First()).ToSql();
			Assert.Equal("INSERT INTO `tb_topic`(`Clicks`, `Title`, `CreateTime`) VALUES(?Clicks0, ?Title0, ?CreateTime0)", sql);

			sql = insert.AppendData(items).ToSql();
			Assert.Equal("INSERT INTO `tb_topic`(`Clicks`, `Title`, `CreateTime`) VALUES(?Clicks0, ?Title0, ?CreateTime0), (?Clicks1, ?Title1, ?CreateTime1), (?Clicks2, ?Title2, ?CreateTime2), (?Clicks3, ?Title3, ?CreateTime3), (?Clicks4, ?Title4, ?CreateTime4), (?Clicks5, ?Title5, ?CreateTime5), (?Clicks6, ?Title6, ?CreateTime6), (?Clicks7, ?Title7, ?CreateTime7), (?Clicks8, ?Title8, ?CreateTime8), (?Clicks9, ?Title9, ?CreateTime9)", sql);

			sql = insert.AppendData(items).InsertColumns(a => a.Title).ToSql();
			Assert.Equal("INSERT INTO `tb_topic`(`Title`) VALUES(?Title0), (?Title1), (?Title2), (?Title3), (?Title4), (?Title5), (?Title6), (?Title7), (?Title8), (?Title9)", sql);

			sql = insert.AppendData(items).IgnoreColumns(a => a.CreateTime).ToSql();
			Assert.Equal("INSERT INTO `tb_topic`(`Clicks`, `Title`) VALUES(?Clicks0, ?Title0), (?Clicks1, ?Title1), (?Clicks2, ?Title2), (?Clicks3, ?Title3), (?Clicks4, ?Title4), (?Clicks5, ?Title5), (?Clicks6, ?Title6), (?Clicks7, ?Title7), (?Clicks8, ?Title8), (?Clicks9, ?Title9)", sql);
		}

		[Fact]
		public void InsertColumns() {
			var items = new List<Topic>();
			for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

			var sql = insert.AppendData(items).InsertColumns(a => a.Title).ToSql();
			Assert.Equal("INSERT INTO `tb_topic`(`Title`) VALUES(?Title0), (?Title1), (?Title2), (?Title3), (?Title4), (?Title5), (?Title6), (?Title7), (?Title8), (?Title9)", sql);

			sql = insert.AppendData(items).InsertColumns(a =>new { a.Title, a.Clicks }).ToSql();
			Assert.Equal("INSERT INTO `tb_topic`(`Clicks`, `Title`) VALUES(?Clicks0, ?Title0), (?Clicks1, ?Title1), (?Clicks2, ?Title2), (?Clicks3, ?Title3), (?Clicks4, ?Title4), (?Clicks5, ?Title5), (?Clicks6, ?Title6), (?Clicks7, ?Title7), (?Clicks8, ?Title8), (?Clicks9, ?Title9)", sql);
		}
		[Fact]
		public void IgnoreColumns() {
			var items = new List<Topic>();
			for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

			var sql = insert.AppendData(items).IgnoreColumns(a => a.CreateTime).ToSql();
			Assert.Equal("INSERT INTO `tb_topic`(`Clicks`, `Title`) VALUES(?Clicks0, ?Title0), (?Clicks1, ?Title1), (?Clicks2, ?Title2), (?Clicks3, ?Title3), (?Clicks4, ?Title4), (?Clicks5, ?Title5), (?Clicks6, ?Title6), (?Clicks7, ?Title7), (?Clicks8, ?Title8), (?Clicks9, ?Title9)", sql);

			sql = insert.AppendData(items).IgnoreColumns(a => new { a.Title, a.CreateTime }).ToSql();
			Assert.Equal("INSERT INTO `tb_topic`(`Clicks`) VALUES(?Clicks0), (?Clicks1), (?Clicks2), (?Clicks3), (?Clicks4), (?Clicks5), (?Clicks6), (?Clicks7), (?Clicks8), (?Clicks9)", sql);
		}
		[Fact]
		public void ExecuteAffrows() {
			var items = new List<Topic>();
			for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

			Assert.Equal(1, insert.AppendData(items.First()).ExecuteAffrows());
			Assert.Equal(10, insert.AppendData(items).ExecuteAffrows());
		}
		[Fact]
		public void ExecuteIdentity() {
			var items = new List<Topic>();
			for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

			Assert.NotEqual(0, insert.AppendData(items.First()).ExecuteIdentity());
		}
		[Fact]
		public void ExecuteInserted() {
			var items = new List<Topic>();
			for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

			//insert.AppendData(items.First()).ExecuteInserted();
		}

		[Fact]
		public void AsTable() {
			var items = new List<Topic>();
			for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newTitle{a}", Clicks = a * 100 });

			var sql = insert.AppendData(items.First()).AsTable(a => "Topic_InsertAsTable").ToSql();
			Assert.Equal("INSERT INTO `Topic_InsertAsTable`(`Clicks`, `Title`, `CreateTime`) VALUES(?Clicks0, ?Title0, ?CreateTime0)", sql);

			sql = insert.AppendData(items).AsTable(a => "Topic_InsertAsTable").ToSql();
			Assert.Equal("INSERT INTO `Topic_InsertAsTable`(`Clicks`, `Title`, `CreateTime`) VALUES(?Clicks0, ?Title0, ?CreateTime0), (?Clicks1, ?Title1, ?CreateTime1), (?Clicks2, ?Title2, ?CreateTime2), (?Clicks3, ?Title3, ?CreateTime3), (?Clicks4, ?Title4, ?CreateTime4), (?Clicks5, ?Title5, ?CreateTime5), (?Clicks6, ?Title6, ?CreateTime6), (?Clicks7, ?Title7, ?CreateTime7), (?Clicks8, ?Title8, ?CreateTime8), (?Clicks9, ?Title9, ?CreateTime9)", sql);

			sql = insert.AppendData(items).InsertColumns(a => a.Title).AsTable(a => "Topic_InsertAsTable").ToSql();
			Assert.Equal("INSERT INTO `Topic_InsertAsTable`(`Title`) VALUES(?Title0), (?Title1), (?Title2), (?Title3), (?Title4), (?Title5), (?Title6), (?Title7), (?Title8), (?Title9)", sql);

			sql = insert.AppendData(items).IgnoreColumns(a => a.CreateTime).AsTable(a => "Topic_InsertAsTable").ToSql();
			Assert.Equal("INSERT INTO `Topic_InsertAsTable`(`Clicks`, `Title`) VALUES(?Clicks0, ?Title0), (?Clicks1, ?Title1), (?Clicks2, ?Title2), (?Clicks3, ?Title3), (?Clicks4, ?Title4), (?Clicks5, ?Title5), (?Clicks6, ?Title6), (?Clicks7, ?Title7), (?Clicks8, ?Title8), (?Clicks9, ?Title9)", sql);

			sql = insert.AppendData(items).InsertColumns(a => a.Title).AsTable(a => "Topic_InsertAsTable").ToSql();
			Assert.Equal("INSERT INTO `Topic_InsertAsTable`(`Title`) VALUES(?Title0), (?Title1), (?Title2), (?Title3), (?Title4), (?Title5), (?Title6), (?Title7), (?Title8), (?Title9)", sql);

			sql = insert.AppendData(items).InsertColumns(a => new { a.Title, a.Clicks }).AsTable(a => "Topic_InsertAsTable").ToSql();
			Assert.Equal("INSERT INTO `Topic_InsertAsTable`(`Clicks`, `Title`) VALUES(?Clicks0, ?Title0), (?Clicks1, ?Title1), (?Clicks2, ?Title2), (?Clicks3, ?Title3), (?Clicks4, ?Title4), (?Clicks5, ?Title5), (?Clicks6, ?Title6), (?Clicks7, ?Title7), (?Clicks8, ?Title8), (?Clicks9, ?Title9)", sql);

			sql = insert.AppendData(items).IgnoreColumns(a => a.CreateTime).AsTable(a => "Topic_InsertAsTable").ToSql();
			Assert.Equal("INSERT INTO `Topic_InsertAsTable`(`Clicks`, `Title`) VALUES(?Clicks0, ?Title0), (?Clicks1, ?Title1), (?Clicks2, ?Title2), (?Clicks3, ?Title3), (?Clicks4, ?Title4), (?Clicks5, ?Title5), (?Clicks6, ?Title6), (?Clicks7, ?Title7), (?Clicks8, ?Title8), (?Clicks9, ?Title9)", sql);

			sql = insert.AppendData(items).IgnoreColumns(a => new { a.Title, a.CreateTime }).AsTable(a => "Topic_InsertAsTable").ToSql();
			Assert.Equal("INSERT INTO `Topic_InsertAsTable`(`Clicks`) VALUES(?Clicks0), (?Clicks1), (?Clicks2), (?Clicks3), (?Clicks4), (?Clicks5), (?Clicks6), (?Clicks7), (?Clicks8), (?Clicks9)", sql);
		}
	}
}
