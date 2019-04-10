using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.PostgreSQL.Curd;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Npgsql.LegacyPostgis;
using NpgsqlTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Net;
using System.Net.NetworkInformation;

namespace FreeSql.PostgreSQL {

	class PostgreSQLProvider<TMark> : IFreeSql<TMark> {

		static PostgreSQLProvider() {
			Utils.dicExecuteArrayRowReadClassOrTuple[typeof(BitArray)] = true;
			Utils.dicExecuteArrayRowReadClassOrTuple[typeof(NpgsqlPoint)] = true;
			Utils.dicExecuteArrayRowReadClassOrTuple[typeof(NpgsqlLine)] = true;
			Utils.dicExecuteArrayRowReadClassOrTuple[typeof(NpgsqlLSeg)] = true;
			Utils.dicExecuteArrayRowReadClassOrTuple[typeof(NpgsqlBox)] = true;
			Utils.dicExecuteArrayRowReadClassOrTuple[typeof(NpgsqlPath)] = true;
			Utils.dicExecuteArrayRowReadClassOrTuple[typeof(NpgsqlPolygon)] = true;
			Utils.dicExecuteArrayRowReadClassOrTuple[typeof(NpgsqlCircle)] = true;
			Utils.dicExecuteArrayRowReadClassOrTuple[typeof((IPAddress Address, int Subnet))] = true;
			Utils.dicExecuteArrayRowReadClassOrTuple[typeof(IPAddress)] = true;
			Utils.dicExecuteArrayRowReadClassOrTuple[typeof(PhysicalAddress)] = true;
			Utils.dicExecuteArrayRowReadClassOrTuple[typeof(NpgsqlRange<int>)] = true;
			Utils.dicExecuteArrayRowReadClassOrTuple[typeof(NpgsqlRange<long>)] = true;
			Utils.dicExecuteArrayRowReadClassOrTuple[typeof(NpgsqlRange<decimal>)] = true;
			Utils.dicExecuteArrayRowReadClassOrTuple[typeof(NpgsqlRange<DateTime>)] = true;
			Utils.dicExecuteArrayRowReadClassOrTuple[typeof(PostgisPoint)] = true;
			Utils.dicExecuteArrayRowReadClassOrTuple[typeof(PostgisLineString)] = true;
			Utils.dicExecuteArrayRowReadClassOrTuple[typeof(PostgisPolygon)] = true;
			Utils.dicExecuteArrayRowReadClassOrTuple[typeof(PostgisMultiPoint)] = true;
			Utils.dicExecuteArrayRowReadClassOrTuple[typeof(PostgisMultiLineString)] = true;
			Utils.dicExecuteArrayRowReadClassOrTuple[typeof(PostgisMultiPolygon)] = true;
			Utils.dicExecuteArrayRowReadClassOrTuple[typeof(PostgisGeometry)] = true;
			Utils.dicExecuteArrayRowReadClassOrTuple[typeof(PostgisGeometryCollection)] = true;
			Utils.dicExecuteArrayRowReadClassOrTuple[typeof(Dictionary<string, string>)] = true;
			Utils.dicExecuteArrayRowReadClassOrTuple[typeof(JToken)] = true;
			Utils.dicExecuteArrayRowReadClassOrTuple[typeof(JObject)] = true;
			Utils.dicExecuteArrayRowReadClassOrTuple[typeof(JArray)] = true;
		}

		public ISelect<T1> Select<T1>() where T1 : class => new PostgreSQLSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, null);
		public ISelect<T1> Select<T1>(object dywhere) where T1 : class => new PostgreSQLSelect<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
		public IInsert<T1> Insert<T1>() where T1 : class => new PostgreSQLInsert<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression);
		public IInsert<T1> Insert<T1>(T1 source) where T1 : class => this.Insert<T1>().AppendData(source);
		public IInsert<T1> Insert<T1>(T1[] source) where T1 : class => this.Insert<T1>().AppendData(source);
		public IInsert<T1> Insert<T1>(IEnumerable<T1> source) where T1 : class => this.Insert<T1>().AppendData(source);
		public IUpdate<T1> Update<T1>() where T1 : class => new PostgreSQLUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, null);
		public IUpdate<T1> Update<T1>(object dywhere) where T1 : class => new PostgreSQLUpdate<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);
		public IDelete<T1> Delete<T1>() where T1 : class => new PostgreSQLDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, null);
		public IDelete<T1> Delete<T1>(object dywhere) where T1 : class => new PostgreSQLDelete<T1>(this, this.InternalCommonUtils, this.InternalCommonExpression, dywhere);

		public IAdo Ado { get; }
		public IAop Aop { get; }
		public ICache Cache { get; }
		public ICodeFirst CodeFirst { get; }
		public IDbFirst DbFirst { get; }
		public PostgreSQLProvider(IDistributedCache cache, ILogger log, string masterConnectionString, string[] slaveConnectionString) {
			if (log == null) log = new LoggerFactory(new[] { new Microsoft.Extensions.Logging.Debug.DebugLoggerProvider() }).CreateLogger("FreeSql.PostgreSQL");

			this.InternalCommonUtils = new PostgreSQLUtils(this);
			this.InternalCommonExpression = new PostgreSQLExpression(this.InternalCommonUtils);

			this.Cache = new CacheProvider(cache, log);
			this.Ado = new PostgreSQLAdo(this.InternalCommonUtils, this.Cache, log, masterConnectionString, slaveConnectionString);
			this.Aop = new AopProvider();

			this.DbFirst = new PostgreSQLDbFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
			this.CodeFirst = new PostgreSQLCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
		}

		internal CommonUtils InternalCommonUtils { get; }
		internal CommonExpression InternalCommonExpression { get; }

		public void Transaction(Action handler) => Ado.Transaction(handler);

		public void Transaction(Action handler, TimeSpan timeout) => Ado.Transaction(handler, timeout);
	}
}
