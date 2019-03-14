using System;
using System.Collections.Generic;
using System.Text;

namespace FreeSql.Tests.DataContext.SqlServer
{
	public class SqlServerFixture : IDisposable
	{
		public SqlServerFixture()
		{
			sqlServerLazy = new Lazy<IFreeSql>(() => new FreeSql.FreeSqlBuilder()
			  .UseConnectionString(FreeSql.DataType.SqlServer, "Data Source=.;Integrated Security=True;Initial Catalog=cms;Pooling=true;Max Pool Size=10")
			  //.UseConnectionString(FreeSql.DataType.SqlServer, "Data Source=(localdb)\\mssqllocaldb;Integrated Security=True;Initial Catalog=cms;Pooling=true;Max Pool Size=10")
			  .UseAutoSyncStructure(true)
			  .UseLazyLoading(true)
			  .Build());

			// ... initialize data in the test database ...
		}

		public void Dispose()
		{
			// ... clean up test data from the database ...
		}

		private Lazy<IFreeSql> sqlServerLazy;
		public IFreeSql SqlServer => sqlServerLazy.Value;
	}
}
