using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.MySql.Expression {
	public class MathTest {

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
		public void PI() {
			var data = new List<object>();
			data.Add(select.Where(a => Math.PI + a.Clicks > 0).ToSql());
		}
		[Fact]
		public void Abs() {
		}
		[Fact]
		public void Sign() {
		}
		[Fact]
		public void Floor() {
		}
		[Fact]
		public void Ceiling() {
		}
		[Fact]
		public void Round() {
		}
		[Fact]
		public void Exp() {
		}
		[Fact]
		public void Log() {
		}
		[Fact]
		public void Log10() {
		}
		[Fact]
		public void Pow() {
		}
		[Fact]
		public void Sqrt() {
		}
		[Fact]
		public void Cos() {
		}
		[Fact]
		public void Sin() {
		}
		[Fact]
		public void Tan() {
		}
		[Fact]
		public void Acos() {
		}
		[Fact]
		public void Asin() {
		}
		[Fact]
		public void Atan() {
		}
		[Fact]
		public void Atan2() {
		}
		[Fact]
		public void Truncate() {
		}
	}
}
