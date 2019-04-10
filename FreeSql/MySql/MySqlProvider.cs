using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.MySql.Curd;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace FreeSql.MySql {

	class MySqlProvider<TMark> : IFreeSql<TMark> {

		static MySqlProvider() {
			Utils.dicExecuteArrayRowReadClassOrTuple[typeof(MygisPoint)] = true;
			Utils.dicExecuteArrayRowReadClassOrTuple[typeof(MygisLineString)] = true;
			Utils.dicExecuteArrayRowReadClassOrTuple[typeof(MygisPolygon)] = true;
			Utils.dicExecuteArrayRowReadClassOrTuple[typeof(MygisMultiPoint)] = true;
			Utils.dicExecuteArrayRowReadClassOrTuple[typeof(MygisMultiLineString)] = true;
			Utils.dicExecuteArrayRowReadClassOrTuple[typeof(MygisMultiPolygon)] = true;
		}

		public ISelect<T1> Select<T1>() where T1 : class => new MySqlSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, null);
		public ISelect<T1> Select<T1>(object dywhere) where T1 : class => new MySqlSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
		public IInsert<T1> Insert<T1>() where T1 : class => new MySqlInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);
		public IInsert<T1> Insert<T1>(T1 source) where T1 : class => this.Insert<T1>().AppendData(source);
		public IInsert<T1> Insert<T1>(T1[] source) where T1 : class => this.Insert<T1>().AppendData(source);
		public IInsert<T1> Insert<T1>(IEnumerable<T1> source) where T1 : class => this.Insert<T1>().AppendData(source);
		public IUpdate<T1> Update<T1>() where T1 : class => new MySqlUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, null);
		public IUpdate<T1> Update<T1>(object dywhere) where T1 : class => new MySqlUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
		public IDelete<T1> Delete<T1>() where T1 : class => new MySqlDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, null);
		public IDelete<T1> Delete<T1>(object dywhere) where T1 : class => new MySqlDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);

		public IAdo Ado { get; }
		public IAop Aop { get; }
		public ICache Cache { get; }
		public ICodeFirst CodeFirst { get; }
		public IDbFirst DbFirst { get; }
		public MySqlProvider(IDistributedCache cache, ILogger log, string masterConnectionString, string[] slaveConnectionString) {
			if (log == null) log = new LoggerFactory(new[] { new Microsoft.Extensions.Logging.Debug.DebugLoggerProvider() }).CreateLogger("FreeSql.MySql");

			this.InternalCommonUtils = new MySqlUtils(this);
			this.InternalCommonExpression = new MySqlExpression(this.InternalCommonUtils);

			this.Cache = new CacheProvider(cache, log);
			this.Ado = new MySqlAdo(this.InternalCommonUtils, this.Cache, log, masterConnectionString, slaveConnectionString);
			this.Aop = new AopProvider();

			this.DbFirst = new MySqlDbFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
			this.CodeFirst = new MySqlCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
		}

		internal CommonUtils InternalCommonUtils { get; }
		internal CommonExpression InternalCommonExpression { get; }

		public void Transaction(Action handler) => Ado.Transaction(handler);

		public void Transaction(Action handler, TimeSpan timeout) => Ado.Transaction(handler, timeout);
	}
}
