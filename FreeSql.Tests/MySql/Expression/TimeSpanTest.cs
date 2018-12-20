using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.MySql.Expression {
	public class TimeSpanTest {

		ISelect<Topic> select => g.mysql.Select<Topic>();

		[Table(Name = "tb_topic")]
		class Topic {
			[Column(IsIdentity = true, IsPrimary = true)]
			public int Id { get; set; }
			public int Clicks { get; set; }
			public int TestTypeInfoGuid { get; set; }
			public TestTypeInfo Type { get; set; }
			public string Title { get; set; }
			public DateTime CreateTime { get; set; }
		}
		class TestTypeInfo {
			public int Guid { get; set; }
			public int ParentId { get; set; }
			public TestTypeParentInfo Parent { get; set; }
			public string Name { get; set; }
		}
		class TestTypeParentInfo {
			public int Id { get; set; }
			public string Name { get; set; }

			public List<TestTypeInfo> Types { get; set; }
		}

		[Fact]
		public void Days() {
		}
		[Fact]
		public void Hours() {
		}
		[Fact]
		public void Milliseconds() {
		}
		[Fact]
		public void Minutes() {
		}
		[Fact]
		public void Seconds() {
		}
		[Fact]
		public void Ticks() {
		}
		[Fact]
		public void TotalDays() {
		}
		[Fact]
		public void TotalHours() {
		}
		[Fact]
		public void TotalMilliseconds() {
		}
		[Fact]
		public void TotalMinutes() {
		}
		[Fact]
		public void TotalSeconds() {
		}
		[Fact]
		public void Add() {
		}
		[Fact]
		public void Subtract() {
		}
	}
}
