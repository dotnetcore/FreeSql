using FreeSql.DataAnnotations;
using System;
using Xunit;

namespace FreeSql.Tests.Sqlite3 {
	public class SqliteAdoTest {
		[Fact]
		public void Pool() {
			var t1 = g.sqlite3.Ado.MasterPool.StatisticsFullily;
		}

		[Fact]
		public void SlavePools() {
			var t2 = g.sqlite3.Ado.SlavePools.Count;
		}

		[Fact]
		public void IsTracePerformance() {
			Assert.True(g.sqlite3.Ado.IsTracePerformance);
		}

		[Fact]
		public void ExecuteReader() {
			
		}
		[Fact]
		public void ExecuteArray() {
			
		}
		[Fact]
		public void ExecuteNonQuery() {
			
		}
		[Fact]
		public void ExecuteScalar() {
			
		}

		[Fact]
		public void Query() {
			var t3 = g.sqlite3.Ado.Query<xxx>("select * from \"song\"");

			var t4 = g.sqlite3.Ado.Query<(int, string, string)>("select * from \"song\"");

			var t5 = g.sqlite3.Ado.Query<dynamic>("select * from \"song\"");
		}

		class xxx {
			public int Id { get; set; }
			public string Path { get; set; }
			public string Title2 { get; set; }
		}
	}
}
