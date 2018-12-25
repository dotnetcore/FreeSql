using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.PostgreSQL {
	public class PostgreSQLInsertTest {

		IInsert<Topic> insert => g.pgsql.Insert<Topic>();

		[Table(Name = "tb_topic_insert")]
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
			Assert.Equal("INSERT INTO \"tb_topic_insert\"(\"clicks\", \"title\", \"createtime\") VALUES(@clicks0, @title0, @createtime0)", sql);

			sql = insert.AppendData(items).ToSql();
			Assert.Equal("INSERT INTO \"tb_topic_insert\"(\"clicks\", \"title\", \"createtime\") VALUES(@clicks0, @title0, @createtime0), (@clicks1, @title1, @createtime1), (@clicks2, @title2, @createtime2), (@clicks3, @title3, @createtime3), (@clicks4, @title4, @createtime4), (@clicks5, @title5, @createtime5), (@clicks6, @title6, @createtime6), (@clicks7, @title7, @createtime7), (@clicks8, @title8, @createtime8), (@clicks9, @title9, @createtime9)", sql);

			sql = insert.AppendData(items).InsertColumns(a => a.Title).ToSql();
			Assert.Equal("INSERT INTO \"tb_topic_insert\"(\"title\") VALUES(@title0), (@title1), (@title2), (@title3), (@title4), (@title5), (@title6), (@title7), (@title8), (@title9)", sql);

			sql = insert.AppendData(items).IgnoreColumns(a => a.CreateTime).ToSql();
			Assert.Equal("INSERT INTO \"tb_topic_insert\"(\"clicks\", \"title\") VALUES(@clicks0, @title0), (@clicks1, @title1), (@clicks2, @title2), (@clicks3, @title3), (@clicks4, @title4), (@clicks5, @title5), (@clicks6, @title6), (@clicks7, @title7), (@clicks8, @title8), (@clicks9, @title9)", sql);
		}

		[Fact]
		public void InsertColumns() {
			var items = new List<Topic>();
			for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

			var sql = insert.AppendData(items).InsertColumns(a => a.Title).ToSql();
			Assert.Equal("INSERT INTO \"tb_topic_insert\"(\"title\") VALUES(@title0), (@title1), (@title2), (@title3), (@title4), (@title5), (@title6), (@title7), (@title8), (@title9)", sql);

			sql = insert.AppendData(items).InsertColumns(a =>new { a.Title, a.Clicks }).ToSql();
			Assert.Equal("INSERT INTO \"tb_topic_insert\"(\"clicks\", \"title\") VALUES(@clicks0, @title0), (@clicks1, @title1), (@clicks2, @title2), (@clicks3, @title3), (@clicks4, @title4), (@clicks5, @title5), (@clicks6, @title6), (@clicks7, @title7), (@clicks8, @title8), (@clicks9, @title9)", sql);
		}
		[Fact]
		public void IgnoreColumns() {
			var items = new List<Topic>();
			for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

			var sql = insert.AppendData(items).IgnoreColumns(a => a.CreateTime).ToSql();
			Assert.Equal("INSERT INTO \"tb_topic_insert\"(\"clicks\", \"title\") VALUES(@clicks0, @title0), (@clicks1, @title1), (@clicks2, @title2), (@clicks3, @title3), (@clicks4, @title4), (@clicks5, @title5), (@clicks6, @title6), (@clicks7, @title7), (@clicks8, @title8), (@clicks9, @title9)", sql);

			sql = insert.AppendData(items).IgnoreColumns(a => new { a.Title, a.CreateTime }).ToSql();
			Assert.Equal("INSERT INTO \"tb_topic_insert\"(\"clicks\") VALUES(@clicks0), (@clicks1), (@clicks2), (@clicks3), (@clicks4), (@clicks5), (@clicks6), (@clicks7), (@clicks8), (@clicks9)", sql);
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

			insert.AppendData(items.First()).ExecuteInserted();
		}
	}
}
