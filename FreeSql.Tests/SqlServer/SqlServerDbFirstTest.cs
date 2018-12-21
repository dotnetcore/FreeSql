using FreeSql.DataAnnotations;
using System;
using Xunit;

namespace FreeSql.Tests.SqlServer {
	public class SqlServerDbFirstTest {
		[Fact]
		public void GetDatabases() {

			var t1 = g.sqlserver.DbFirst.GetDatabases();

		}

		[Fact]
		public void GetTablesByDatabase() {

			var t2 = g.sqlserver.DbFirst.GetTablesByDatabase(g.sqlserver.DbFirst.GetDatabases()[0]);

		}
	}
}
