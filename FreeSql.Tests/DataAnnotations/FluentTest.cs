using FreeSql.DataAnnotations;
using Xunit;

namespace FreeSql.Tests.DataAnnotations {
	public class FluentTest {
		[Fact]
		public void Fluent() {
			g.mysql.CodeFirst
				//.ConfigEntity<TestFluenttb1>(a => {
				//	a.Name("xxdkdkdk1").SelectFilter("a.Id22 > 0");
				//	a.Property(b => b.Id).Name("Id22").IsIdentity(true);
				//	a.Property(b => b.name).DbType("varchar(100)").IsNullable(true);
				//})

				.ConfigEntity(typeof(TestFluenttb1), a => {
					a.Name("xxdkdkdk1222").SelectFilter("a.Id22 > 1");
					a.Property("Id").Name("Id22dd").IsIdentity(true);
					a.Property("Name").DbType("varchar(101)").IsNullable(true);
				})

				.ConfigEntity<TestFluenttb2>(a => {
					a.Name("xxdkdkdk2").SelectFilter("a.Idx > 0");
					a.Property(b => b.Id).Name("Id22").IsIdentity(true);
					a.Property(b => b.name).DbType("varchar(100)").IsNullable(true);
				})
				;

			var ddl1 = g.mysql.CodeFirst.GetComparisonDDLStatements<TestFluenttb1>();
			var ddl2 = g.mysql.CodeFirst.GetComparisonDDLStatements<TestFluenttb2>();

			var t1id = g.mysql.Insert<TestFluenttb1>().AppendData(new TestFluenttb1 { }).ExecuteIdentity();
			var t1 = g.mysql.Select<TestFluenttb1>(t1id).ToOne();

			var t2lastId = g.mysql.Select<TestFluenttb2>().Max(a => a.Id);
			var t2affrows = g.mysql.Insert<TestFluenttb2>().AppendData(new TestFluenttb2 { Id = t2lastId + 1 }).ExecuteAffrows();
			var t2 = g.mysql.Select<TestFluenttb2>(t2lastId + 1).ToOne();
		}
	}

	class TestFluenttb1 {
		public int Id { get; set; }

		public string name { get; set; } = "defaultValue";
	}

	[Table(Name = "cccccdddwww")]
	class TestFluenttb2 {
		[Column(Name = "Idx", IsPrimary = true, IsIdentity = false)]
		public int Id { get; set; }

		public string name { get; set; } = "defaultValue";
	}
}
