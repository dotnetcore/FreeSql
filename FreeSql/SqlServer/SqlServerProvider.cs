using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.SqlServer.Curd;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace FreeSql.SqlServer {

	class SqlServerProvider : IFreeSql {

		public ISelect<T1> Select<T1>() where T1 : class => new SqlServerSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, null);
		public ISelect<T1> Select<T1>(object dywhere) where T1 : class => new SqlServerSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
		public IInsert<T1> Insert<T1>() where T1 : class => new SqlServerInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);
		public IInsert<T1> Insert<T1>(T1 source) where T1 : class => this.Insert<T1>().AppendData(source);
		public IInsert<T1> Insert<T1>(IEnumerable<T1> source) where T1 : class => this.Insert<T1>().AppendData(source);
		public IUpdate<T1> Update<T1>() where T1 : class => new SqlServerUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, null);
		public IUpdate<T1> Update<T1>(object dywhere) where T1 : class => new SqlServerUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
		public IDelete<T1> Delete<T1>() where T1 : class => new SqlServerDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, null);
		public IDelete<T1> Delete<T1>(object dywhere) where T1 : class => new SqlServerDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);

		public IAdo Ado { get; }
		public ICache Cache { get; }
		public ICodeFirst CodeFirst { get; }
		public IDbFirst DbFirst { get; }
		public SqlServerProvider(IDistributedCache cache, IConfiguration cacheStrategy, string masterConnectionString, string[] slaveConnectionString, ILogger log) {
			CacheStrategy = cacheStrategy;
			if (log == null) log = new LoggerFactory(new[] { new Microsoft.Extensions.Logging.Debug.DebugLoggerProvider() }).CreateLogger("FreeSql.SqlServer");

			this.InternalCommonUtils = new SqlServerUtils(this);
			this.InternalCommonExpression = new SqlServerExpression(this.InternalCommonUtils);

			this.Cache = new CacheProvider(cache, log);
			this.Ado = new SqlServerAdo(this.InternalCommonUtils, this.Cache, log, masterConnectionString, slaveConnectionString);

			this.DbFirst = new SqlServerDbFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
			this.InternalCommonUtils.CodeFirst = this.CodeFirst = new SqlServerCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
		}

		internal CommonUtils InternalCommonUtils { get; }
		internal CommonExpression InternalCommonExpression { get; }
		internal IConfiguration CacheStrategy { get; private set; }

		public void Transaction(Action handler) => Ado.Transaction(handler);

		public void Transaction(Action handler, TimeSpan timeout) => Ado.Transaction(handler, timeout);
	}
}
