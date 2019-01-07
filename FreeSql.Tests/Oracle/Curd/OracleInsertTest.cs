using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.Oracle {
	public class OracleInsertTest {

		IInsert<Topic> insert => g.oracle.Insert<Topic>(); //��������

		[Table(Name = "tb_topic")]
		class Topic {
			[Column(IsIdentity = true, IsPrimary = true)]
			public int Id { get; set; }
			public int? Clicks { get; set; }
			public TestTypeInfo Type { get; set; }
			public string Title { get; set; }
			public DateTime CreateTime { get; set; }
		}
 
		[Fact]
		public void AppendData() {
			var items = new List<Topic>();
			for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100, CreateTime = DateTime.Now });

			var data = new List<object>();
			var sql = insert.AppendData(items.First()).ToSql();
			Assert.Equal("INSERT INTO \"tb_topic\"(\"Clicks\", \"Title\", \"CreateTime\") VALUES(:Clicks0, :Title0, :CreateTime0)", sql);
			data.Add(insert.AppendData(items.First()).ExecuteIdentity());

			sql = insert.AppendData(items).ToSql();
			Assert.Equal(@"INSERT ALL
INTO ""tb_topic""(""Clicks"", ""Title"", ""CreateTime"") VALUES(:Clicks0, :Title0, :CreateTime0)
INTO ""tb_topic""(""Clicks"", ""Title"", ""CreateTime"") VALUES(:Clicks1, :Title1, :CreateTime1)
INTO ""tb_topic""(""Clicks"", ""Title"", ""CreateTime"") VALUES(:Clicks2, :Title2, :CreateTime2)
INTO ""tb_topic""(""Clicks"", ""Title"", ""CreateTime"") VALUES(:Clicks3, :Title3, :CreateTime3)
INTO ""tb_topic""(""Clicks"", ""Title"", ""CreateTime"") VALUES(:Clicks4, :Title4, :CreateTime4)
INTO ""tb_topic""(""Clicks"", ""Title"", ""CreateTime"") VALUES(:Clicks5, :Title5, :CreateTime5)
INTO ""tb_topic""(""Clicks"", ""Title"", ""CreateTime"") VALUES(:Clicks6, :Title6, :CreateTime6)
INTO ""tb_topic""(""Clicks"", ""Title"", ""CreateTime"") VALUES(:Clicks7, :Title7, :CreateTime7)
INTO ""tb_topic""(""Clicks"", ""Title"", ""CreateTime"") VALUES(:Clicks8, :Title8, :CreateTime8)
INTO ""tb_topic""(""Clicks"", ""Title"", ""CreateTime"") VALUES(:Clicks9, :Title9, :CreateTime9)
 SELECT 1 FROM DUAL", sql);
			data.Add(insert.AppendData(items.First()).ExecuteIdentity());

			sql = insert.AppendData(items).InsertColumns(a => a.Title).ToSql();
			Assert.Equal(@"INSERT ALL
INTO ""tb_topic""(""Title"") VALUES(:Title0)
INTO ""tb_topic""(""Title"") VALUES(:Title1)
INTO ""tb_topic""(""Title"") VALUES(:Title2)
INTO ""tb_topic""(""Title"") VALUES(:Title3)
INTO ""tb_topic""(""Title"") VALUES(:Title4)
INTO ""tb_topic""(""Title"") VALUES(:Title5)
INTO ""tb_topic""(""Title"") VALUES(:Title6)
INTO ""tb_topic""(""Title"") VALUES(:Title7)
INTO ""tb_topic""(""Title"") VALUES(:Title8)
INTO ""tb_topic""(""Title"") VALUES(:Title9)
 SELECT 1 FROM DUAL", sql);
			data.Add(insert.AppendData(items.First()).ExecuteIdentity());

			sql = insert.AppendData(items).IgnoreColumns(a => a.CreateTime).ToSql();
			Assert.Equal(@"INSERT ALL
INTO ""tb_topic""(""Clicks"", ""Title"") VALUES(:Clicks0, :Title0)
INTO ""tb_topic""(""Clicks"", ""Title"") VALUES(:Clicks1, :Title1)
INTO ""tb_topic""(""Clicks"", ""Title"") VALUES(:Clicks2, :Title2)
INTO ""tb_topic""(""Clicks"", ""Title"") VALUES(:Clicks3, :Title3)
INTO ""tb_topic""(""Clicks"", ""Title"") VALUES(:Clicks4, :Title4)
INTO ""tb_topic""(""Clicks"", ""Title"") VALUES(:Clicks5, :Title5)
INTO ""tb_topic""(""Clicks"", ""Title"") VALUES(:Clicks6, :Title6)
INTO ""tb_topic""(""Clicks"", ""Title"") VALUES(:Clicks7, :Title7)
INTO ""tb_topic""(""Clicks"", ""Title"") VALUES(:Clicks8, :Title8)
INTO ""tb_topic""(""Clicks"", ""Title"") VALUES(:Clicks9, :Title9)
 SELECT 1 FROM DUAL", sql);
			data.Add(insert.AppendData(items.First()).ExecuteIdentity());
		}

		[Fact]
		public void InsertColumns() {
			var items = new List<Topic>();
			for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100, CreateTime = DateTime.Now });

			var data = new List<object>();
			var sql = insert.AppendData(items).InsertColumns(a => a.Title).ToSql();
			Assert.Equal(@"INSERT ALL
INTO ""tb_topic""(""Title"") VALUES(:Title0)
INTO ""tb_topic""(""Title"") VALUES(:Title1)
INTO ""tb_topic""(""Title"") VALUES(:Title2)
INTO ""tb_topic""(""Title"") VALUES(:Title3)
INTO ""tb_topic""(""Title"") VALUES(:Title4)
INTO ""tb_topic""(""Title"") VALUES(:Title5)
INTO ""tb_topic""(""Title"") VALUES(:Title6)
INTO ""tb_topic""(""Title"") VALUES(:Title7)
INTO ""tb_topic""(""Title"") VALUES(:Title8)
INTO ""tb_topic""(""Title"") VALUES(:Title9)
 SELECT 1 FROM DUAL", sql);
			data.Add(insert.AppendData(items.First()).ExecuteIdentity());

			sql = insert.AppendData(items).InsertColumns(a =>new { a.Title, a.Clicks }).ToSql();
			Assert.Equal(@"INSERT ALL
INTO ""tb_topic""(""Clicks"", ""Title"") VALUES(:Clicks0, :Title0)
INTO ""tb_topic""(""Clicks"", ""Title"") VALUES(:Clicks1, :Title1)
INTO ""tb_topic""(""Clicks"", ""Title"") VALUES(:Clicks2, :Title2)
INTO ""tb_topic""(""Clicks"", ""Title"") VALUES(:Clicks3, :Title3)
INTO ""tb_topic""(""Clicks"", ""Title"") VALUES(:Clicks4, :Title4)
INTO ""tb_topic""(""Clicks"", ""Title"") VALUES(:Clicks5, :Title5)
INTO ""tb_topic""(""Clicks"", ""Title"") VALUES(:Clicks6, :Title6)
INTO ""tb_topic""(""Clicks"", ""Title"") VALUES(:Clicks7, :Title7)
INTO ""tb_topic""(""Clicks"", ""Title"") VALUES(:Clicks8, :Title8)
INTO ""tb_topic""(""Clicks"", ""Title"") VALUES(:Clicks9, :Title9)
 SELECT 1 FROM DUAL", sql);
			data.Add(insert.AppendData(items.First()).ExecuteIdentity());
		}
		[Fact]
		public void IgnoreColumns() {
			var items = new List<Topic>();
			for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100, CreateTime = DateTime.Now });

			var data = new List<object>();
			var sql = insert.AppendData(items).IgnoreColumns(a => a.CreateTime).ToSql();
			Assert.Equal(@"INSERT ALL
INTO ""tb_topic""(""Clicks"", ""Title"") VALUES(:Clicks0, :Title0)
INTO ""tb_topic""(""Clicks"", ""Title"") VALUES(:Clicks1, :Title1)
INTO ""tb_topic""(""Clicks"", ""Title"") VALUES(:Clicks2, :Title2)
INTO ""tb_topic""(""Clicks"", ""Title"") VALUES(:Clicks3, :Title3)
INTO ""tb_topic""(""Clicks"", ""Title"") VALUES(:Clicks4, :Title4)
INTO ""tb_topic""(""Clicks"", ""Title"") VALUES(:Clicks5, :Title5)
INTO ""tb_topic""(""Clicks"", ""Title"") VALUES(:Clicks6, :Title6)
INTO ""tb_topic""(""Clicks"", ""Title"") VALUES(:Clicks7, :Title7)
INTO ""tb_topic""(""Clicks"", ""Title"") VALUES(:Clicks8, :Title8)
INTO ""tb_topic""(""Clicks"", ""Title"") VALUES(:Clicks9, :Title9)
 SELECT 1 FROM DUAL", sql);
			data.Add(insert.AppendData(items.First()).ExecuteIdentity());

			sql = insert.AppendData(items).IgnoreColumns(a => new { a.Title, a.CreateTime }).ToSql();
			Assert.Equal(@"INSERT ALL
INTO ""tb_topic""(""Clicks"") VALUES(:Clicks0)
INTO ""tb_topic""(""Clicks"") VALUES(:Clicks1)
INTO ""tb_topic""(""Clicks"") VALUES(:Clicks2)
INTO ""tb_topic""(""Clicks"") VALUES(:Clicks3)
INTO ""tb_topic""(""Clicks"") VALUES(:Clicks4)
INTO ""tb_topic""(""Clicks"") VALUES(:Clicks5)
INTO ""tb_topic""(""Clicks"") VALUES(:Clicks6)
INTO ""tb_topic""(""Clicks"") VALUES(:Clicks7)
INTO ""tb_topic""(""Clicks"") VALUES(:Clicks8)
INTO ""tb_topic""(""Clicks"") VALUES(:Clicks9)
 SELECT 1 FROM DUAL", sql);
			data.Add(insert.AppendData(items.First()).ExecuteIdentity());
		}
		[Fact]
		public void ExecuteAffrows() {
			var items = new List<Topic>();
			for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100, CreateTime = DateTime.Now });

			Assert.Equal(1, insert.AppendData(items.First()).ExecuteAffrows());
			Assert.Equal(10, insert.AppendData(items).ExecuteAffrows());
		}
		[Fact]
		public void ExecuteIdentity() {
			var items = new List<Topic>();
			for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100, CreateTime = DateTime.Now });

			Assert.NotEqual(0, insert.AppendData(items.First()).ExecuteIdentity());
		}
		[Fact]
		public void ExecuteInserted() {
			//var items = new List<Topic>();
			//for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100, CreateTime = DateTime.Now });

			//var items2 = insert.AppendData(items).ExecuteInserted();
		}
	}
}
