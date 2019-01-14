using FreeSql.DataAnnotations;
using System;
using Xunit;

namespace FreeSql.Tests.Sqlite {
	public class SqliteAdoTest {
		[Fact]
		public void Pool() {
			var t1 = g.sqlite.Ado.MasterPool.StatisticsFullily;
		}

		[Fact]
		public void SlavePools() {
			var t2 = g.sqlite.Ado.SlavePools.Count;
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
			var t3 = g.sqlite.Ado.Query<xxx>("select * from \"song\"");

			var t4 = g.sqlite.Ado.Query<(int, string, string)>("select * from \"song\"");

			var t5 = g.sqlite.Ado.Query<dynamic>("select * from \"song\"");
		}

		class xxx {
			public int Id { get; set; }
			public string Path { get; set; }
			public string Title2 { get; set; }
		}
	}
}
