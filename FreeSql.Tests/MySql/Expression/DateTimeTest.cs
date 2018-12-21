using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.MySql.Expression {
	public class DateTimeTest {

		ISelect<Topic> select => g.mysql.Select<Topic>();

		[Table(Name = "tb_topic111333")]
		class Topic {
			[Column(IsIdentity = true, IsPrimary = true)]
			public int Id { get; set; }
			public int Clicks { get; set; }
			public int TestTypeInfoGuid { get; set; }
			public TestTypeInfo Type { get; set; }
			public string Title { get; set; }
			public DateTime CreateTime { get; set; }
		}
		[Table(Name = "TestTypeInfo333")]
		class TestTypeInfo {
			public int Guid { get; set; }
			public int ParentId { get; set; }
			public TestTypeParentInfo Parent { get; set; }
			public string Name { get; set; }
			public DateTime Time { get; set; }
		}
		[Table(Name = "TestTypeParentInfo23123")]
		class TestTypeParentInfo {
			public int Id { get; set; }
			public string Name { get; set; }

			public List<TestTypeInfo> Types { get; set; }
			public DateTime Time2 { get; set; }
		}

		[Fact]
		public void DayOfWeek() {
			var data = new List<object>();
			data.Add(select.Where(a => a.CreateTime.DayOfWeek > DateTime.Now.DayOfWeek).ToSql());
			data.Add(select.Where(a => a.Type.Time.DayOfWeek > DateTime.Now.DayOfWeek).ToSql());
			data.Add(select.Where(a => a.Type.Parent.Time2.DayOfWeek > DateTime.Now.DayOfWeek).ToSql());
			//SELECT a.`Id` as1, a.`Clicks` as2, a.`TestTypeInfoGuid` as3, a.`Title` as4, a.`CreateTime` as5 
			//FROM `tb_topic111333` a 
			//WHERE ((dayofweek(a.`CreateTime`) - 1) > (dayofweek(now()) - 1));

			//SELECT a.`Id` as1, a.`Clicks` as2, a.`TestTypeInfoGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
			//FROM `tb_topic111333` a, `TestTypeInfo333` a__Type 
			//WHERE ((dayofweek(a__Type.`Time`) - 1) > (dayofweek(now()) - 1));

			//SELECT a.`Id` as1, a.`Clicks` as2, a.`TestTypeInfoGuid` as3, a__Type.`Guid` as4, a__Type.`ParentId` as5, a__Type.`Name` as6, a__Type.`Time` as7, a.`Title` as8, a.`CreateTime` as9 
			//FROM `tb_topic111333` a, `TestTypeInfo333` a__Type, `TestTypeParentInfo23123` a__Type__Parent 
			//WHERE ((dayofweek(a__Type__Parent.`Time2`) - 1) > (dayofweek(now()) - 1))
		}
		[Fact]
		public void Day() {
		}
		[Fact]
		public void DayOfYear() {
		}
		[Fact]
		public void Month() {
		}
		[Fact]
		public void Year() {
		}
		[Fact]
		public void Hour() {
		}
		[Fact]
		public void Minute() {
		}
		[Fact]
		public void Second() {
		}
		[Fact]
		public void Millisecond() {
		}
		[Fact]
		public void Ticks() {
		}
		[Fact]
		public void Add() {
		}
		[Fact]
		public void AddDays() {
		}
		[Fact]
		public void AddHours() {
		}
		[Fact]
		public void AddMilliseconds() {
		}
		[Fact]
		public void AddMinutes() {
		}
		[Fact]
		public void AddMonths() {
		}
		[Fact]
		public void AddSeconds() {
		}
		[Fact]
		public void AddTicks() {
		}
		[Fact]
		public void AddYears() {
		}
		[Fact]
		public void Subtract() {
		}
	}
}
