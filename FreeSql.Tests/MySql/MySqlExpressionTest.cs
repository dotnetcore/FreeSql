using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.MySql {
	public class MySqlExpressionTest {

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
		public void StartsWith() {
		}
		[Fact]
		public void EndsWith() {
		}
		[Fact]
		public void Contains() {
		}
		[Fact]
		public void ToLower() {
		}
		[Fact]
		public void ToUpper() {
			
		}
		[Fact]
		public void Substring() {
		}
		[Fact]
		public void Length() {
		}
		[Fact]
		public void IndexOf() {
		}
		[Fact]
		public void PadLeft() {
		}
		[Fact]
		public void PadRight() {
		}
		[Fact]
		public void Trim() {
		}
		[Fact]
		public void TrimStart() {
		}
		[Fact]
		public void TrimEnd() {
		}
		[Fact]
		public void Replace() {
		}
		[Fact]
		public void CompareTo() {
		}
	}
}
