using FreeSql.DataAnnotations;
using System;
using Xunit;

namespace FreeSql.Tests.Oracle {
	public class OracleAdoTest {
		[Fact]
		public void Pool() {
			var t1 = g.oracle.Ado.MasterPool.StatisticsFullily;
		}

		[Fact]
		public void SlavePools() {
			var t2 = g.oracle.Ado.SlavePools.Count;
		}

		[Fact]
		public void IsTracePerformance() {
			Assert.True(g.oracle.Ado.IsTracePerformance);
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
			var t3 = g.oracle.Ado.Query<xxx>("select * from \"song\"");

			var t4 = g.oracle.Ado.Query<(int, string, string)>("select * from \"song\"");

			var t5 = g.oracle.Ado.Query<dynamic>("select * from \"song\"");
		}

		class xxx {
			public int Id { get; set; }
			public string Path { get; set; }
			public string Title2 { get; set; }
		}
	}
}
