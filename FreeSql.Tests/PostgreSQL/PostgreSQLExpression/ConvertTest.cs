using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.PostgreSQLExpression {
	public class ConvertTest {

		ISelect<Topic> select => g.pgsql.Select<Topic>();

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
		public void ToBoolean() {
			var data = new List<object>();
			data.Add(select.Where(a => (Convert.ToBoolean(a.Clicks) ? 1 : 2) > 0).ToList());
			//SELECT a.`Id`, a.`Clicks`, a.`TestTypeInfoGuid`, a.`Title`, a.`CreateTime` 
			//FROM `tb_topic` a 
			//WHERE ((a.`Clicks` not in ('0','false')))
		}
		[Fact]
		public void ToByte() {
			var data = new List<object>();
			data.Add(select.Where(a => Convert.ToByte(a.Clicks) > 0).ToList());
		}
		[Fact]
		public void ToChar() {
			var data = new List<object>();
			data.Add(select.Where(a => Convert.ToBoolean(a.Clicks)).ToList());
		}
		[Fact]
		public void ToDateTime() {
			var data = new List<object>();
			data.Add(select.Where(a => Convert.ToDateTime(a.CreateTime.ToString()).Year > 0).ToList());
		}
		[Fact]
		public void ToDecimal() {
			var data = new List<object>();
			data.Add(select.Where(a => Convert.ToDecimal(a.Clicks) > 0).ToList());
		}
		[Fact]
		public void ToDouble() {
			var data = new List<object>();
			data.Add(select.Where(a => Convert.ToDouble(a.Clicks) > 0).ToList());
		}
		[Fact]
		public void ToInt16() {
			var data = new List<object>();
			data.Add(select.Where(a => Convert.ToInt16(a.Clicks) > 0).ToList());
		}
		[Fact]
		public void ToInt32() {
			var data = new List<object>();
			data.Add(select.Where(a => Convert.ToInt32(a.Clicks) > 0).ToList());
		}
		[Fact]
		public void ToInt64() {
			var data = new List<object>();
			data.Add(select.Where(a => Convert.ToInt64(a.Clicks) > 0).ToList());
		}
		[Fact]
		public void ToSByte() {
			var data = new List<object>();
			data.Add(select.Where(a => Convert.ToSByte(a.Clicks) > 0).ToList());
		}
		[Fact]
		public void ToSingle() {
			var data = new List<object>();
			data.Add(select.Where(a => Convert.ToSingle(a.Clicks) > 0).ToList());
		}
		[Fact]
		public void this_ToString() {
			var data = new List<object>();
			data.Add(select.Where(a => Convert.ToString(a.Clicks).Equals("")).ToList());
		}
		[Fact]
		public void ToUInt16() {
			var data = new List<object>();
			data.Add(select.Where(a => Convert.ToUInt16(a.Clicks) > 0).ToList());
		}
		[Fact]
		public void ToUInt32() {
			var data = new List<object>();
			data.Add(select.Where(a => Convert.ToUInt32(a.Clicks) > 0).ToList());
		}
		[Fact]
		public void ToUInt64() {
			var data = new List<object>();
			data.Add(select.Where(a => Convert.ToUInt64(a.Clicks) > 0).ToList());
		}
	}
}
