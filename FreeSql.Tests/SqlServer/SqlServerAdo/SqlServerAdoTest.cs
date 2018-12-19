using FreeSql.DataAnnotations;
using System;
using Xunit;

namespace FreeSql.Tests.SqlServer {
	public class SqlServerAdoTest {
		[Fact]
		public void Pool() {
			var t1 = g.sqlserver.Ado.MasterPool.StatisticsFullily;
		}

		[Fact]
		public void SlavePools() {
			var t2 = g.sqlserver.Ado.SlavePools.Count;
		}

		[Fact]
		public void IsTracePerformance() {
			Assert.True(g.sqlserver.Ado.IsTracePerformance);
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
			var t3 = g.sqlserver.Ado.Query<xxx>("select * from song");

			var t4 = g.sqlserver.Ado.Query<(int, string, string, DateTime)>("select * from song");

			var t5 = g.sqlserver.Ado.Query<dynamic>("select * from song");
		}

		class xxx {
			public int Id { get; set; }
			public string Title { get; set; }
			public string Url { get; set; }
			public DateTime Create_time { get; set; }
		}
	}
}
