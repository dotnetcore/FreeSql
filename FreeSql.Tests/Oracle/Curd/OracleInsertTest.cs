using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.Oracle {
	public class OracleInsertTest {

		IInsert<Topic> insert => g.oracle.Insert<Topic>(); //��������

		[Table(Name = "tb_topic_insert")]
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
			Assert.Equal("INSERT INTO \"TB_TOPIC_INSERT\"(\"CLICKS\", \"TITLE\", \"CREATETIME\") VALUES(:Clicks0, :Title0, :CreateTime0)", sql);
			data.Add(insert.AppendData(items.First()).ExecuteIdentity());

			sql = insert.AppendData(items).ToSql();
			Assert.Equal(@"INSERT ALL
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(:Clicks0, :Title0, :CreateTime0)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(:Clicks1, :Title1, :CreateTime1)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(:Clicks2, :Title2, :CreateTime2)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(:Clicks3, :Title3, :CreateTime3)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(:Clicks4, :Title4, :CreateTime4)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(:Clicks5, :Title5, :CreateTime5)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(:Clicks6, :Title6, :CreateTime6)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(:Clicks7, :Title7, :CreateTime7)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(:Clicks8, :Title8, :CreateTime8)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"", ""CREATETIME"") VALUES(:Clicks9, :Title9, :CreateTime9)
 SELECT 1 FROM DUAL", sql);
			data.Add(insert.AppendData(items.First()).ExecuteIdentity());

			sql = insert.AppendData(items).InsertColumns(a => a.Title).ToSql();
			Assert.Equal(@"INSERT ALL
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title0)
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title1)
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title2)
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title3)
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title4)
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title5)
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title6)
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title7)
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title8)
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title9)
 SELECT 1 FROM DUAL", sql);
			data.Add(insert.AppendData(items.First()).ExecuteIdentity());

			sql = insert.AppendData(items).IgnoreColumns(a => a.CreateTime).ToSql();
			Assert.Equal(@"INSERT ALL
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks0, :Title0)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks1, :Title1)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks2, :Title2)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks3, :Title3)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks4, :Title4)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks5, :Title5)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks6, :Title6)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks7, :Title7)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks8, :Title8)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks9, :Title9)
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
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title0)
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title1)
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title2)
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title3)
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title4)
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title5)
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title6)
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title7)
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title8)
INTO ""TB_TOPIC_INSERT""(""TITLE"") VALUES(:Title9)
 SELECT 1 FROM DUAL", sql);
			data.Add(insert.AppendData(items.First()).ExecuteIdentity());

			sql = insert.AppendData(items).InsertColumns(a =>new { a.Title, a.Clicks }).ToSql();
			Assert.Equal(@"INSERT ALL
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks0, :Title0)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks1, :Title1)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks2, :Title2)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks3, :Title3)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks4, :Title4)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks5, :Title5)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks6, :Title6)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks7, :Title7)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks8, :Title8)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks9, :Title9)
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
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks0, :Title0)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks1, :Title1)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks2, :Title2)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks3, :Title3)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks4, :Title4)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks5, :Title5)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks6, :Title6)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks7, :Title7)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks8, :Title8)
INTO ""TB_TOPIC_INSERT""(""CLICKS"", ""TITLE"") VALUES(:Clicks9, :Title9)
 SELECT 1 FROM DUAL", sql);
			data.Add(insert.AppendData(items.First()).ExecuteIdentity());

			sql = insert.AppendData(items).IgnoreColumns(a => new { a.Title, a.CreateTime }).ToSql();
			Assert.Equal(@"INSERT ALL
INTO ""TB_TOPIC_INSERT""(""CLICKS"") VALUES(:Clicks0)
INTO ""TB_TOPIC_INSERT""(""CLICKS"") VALUES(:Clicks1)
INTO ""TB_TOPIC_INSERT""(""CLICKS"") VALUES(:Clicks2)
INTO ""TB_TOPIC_INSERT""(""CLICKS"") VALUES(:Clicks3)
INTO ""TB_TOPIC_INSERT""(""CLICKS"") VALUES(:Clicks4)
INTO ""TB_TOPIC_INSERT""(""CLICKS"") VALUES(:Clicks5)
INTO ""TB_TOPIC_INSERT""(""CLICKS"") VALUES(:Clicks6)
INTO ""TB_TOPIC_INSERT""(""CLICKS"") VALUES(:Clicks7)
INTO ""TB_TOPIC_INSERT""(""CLICKS"") VALUES(:Clicks8)
INTO ""TB_TOPIC_INSERT""(""CLICKS"") VALUES(:Clicks9)
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
