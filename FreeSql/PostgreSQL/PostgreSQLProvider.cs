using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.PostgreSQL.Curd;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace FreeSql.PostgreSQL {

	class PostgreSQLProvider : IFreeSql {

		public ISelect<T1> Select<T1>() where T1 : class => new PostgreSQLSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, null);
		public ISelect<T1> Select<T1>(object dywhere) where T1 : class => new PostgreSQLSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
		public IInsert<T1> Insert<T1>() where T1 : class => new PostgreSQLInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);
		public IInsert<T1> Insert<T1>(T1 source) where T1 : class => this.Insert<T1>().AppendData(source);
		public IInsert<T1> Insert<T1>(IEnumerable<T1> source) where T1 : class => this.Insert<T1>().AppendData(source);
		public IUpdate<T1> Update<T1>() where T1 : class => new PostgreSQLUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, null);
		public IUpdate<T1> Update<T1>(object dywhere) where T1 : class => new PostgreSQLUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
		public IDelete<T1> Delete<T1>() where T1 : class => new PostgreSQLDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, null);
		public IDelete<T1> Delete<T1>(object dywhere) where T1 : class => new PostgreSQLDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);

		public IAdo Ado { get; }
		public ICache Cache { get; }
		public ICodeFirst CodeFirst { get; }
		public IDbFirst DbFirst { get; }
		public PostgreSQLProvider(IDistributedCache cache, ILogger log, string masterConnectionString, string[] slaveConnectionString) {
			if (log == null) log = new LoggerFactory(new[] { new Microsoft.Extensions.Logging.Debug.DebugLoggerProvider() }).CreateLogger("FreeSql.PostgreSQL");

			this.InternalCommonUtils = new PostgreSQLUtils(this);
			this.InternalCommonExpression = new PostgreSQLExpression(this.InternalCommonUtils);

			this.Cache = new CacheProvider(cache, log);
			this.Ado = new PostgreSQLAdo(this.InternalCommonUtils, this.Cache, log, masterConnectionString, slaveConnectionString);

			this.DbFirst = new PostgreSQLDbFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
			this.InternalCommonUtils.CodeFirst = this.CodeFirst = new PostgreSQLCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
		}

		internal CommonUtils InternalCommonUtils { get; }
		internal CommonExpression InternalCommonExpression { get; }

		public void Transaction(Action handler) => Ado.Transaction(handler);

		public void Transaction(Action handler, TimeSpan timeout) => Ado.Transaction(handler, timeout);
	}
}
