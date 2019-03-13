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

	class PostgreSQLProvider : IFreeSql {

		static PostgreSQLProvider() {
			Utils.dicExecuteArrayRowReadClassOrTuple.Add(typeof(BitArray), true);
			Utils.dicExecuteArrayRowReadClassOrTuple.Add(typeof(NpgsqlPoint), true);
			Utils.dicExecuteArrayRowReadClassOrTuple.Add(typeof(NpgsqlLine), true);
			Utils.dicExecuteArrayRowReadClassOrTuple.Add(typeof(NpgsqlLSeg), true);
			Utils.dicExecuteArrayRowReadClassOrTuple.Add(typeof(NpgsqlBox), true);
			Utils.dicExecuteArrayRowReadClassOrTuple.Add(typeof(NpgsqlPath), true);
			Utils.dicExecuteArrayRowReadClassOrTuple.Add(typeof(NpgsqlPolygon), true);
			Utils.dicExecuteArrayRowReadClassOrTuple.Add(typeof(NpgsqlCircle), true);
			Utils.dicExecuteArrayRowReadClassOrTuple.Add(typeof((IPAddress Address, int Subnet)), true);
			Utils.dicExecuteArrayRowReadClassOrTuple.Add(typeof(IPAddress), true);
			Utils.dicExecuteArrayRowReadClassOrTuple.Add(typeof(PhysicalAddress), true);
			Utils.dicExecuteArrayRowReadClassOrTuple.Add(typeof(NpgsqlRange<int>), true);
			Utils.dicExecuteArrayRowReadClassOrTuple.Add(typeof(NpgsqlRange<long>), true);
			Utils.dicExecuteArrayRowReadClassOrTuple.Add(typeof(NpgsqlRange<decimal>), true);
			Utils.dicExecuteArrayRowReadClassOrTuple.Add(typeof(NpgsqlRange<DateTime>), true);
			Utils.dicExecuteArrayRowReadClassOrTuple.Add(typeof(PostgisPoint), true);
			Utils.dicExecuteArrayRowReadClassOrTuple.Add(typeof(PostgisLineString), true);
			Utils.dicExecuteArrayRowReadClassOrTuple.Add(typeof(PostgisPolygon), true);
			Utils.dicExecuteArrayRowReadClassOrTuple.Add(typeof(PostgisMultiPoint), true);
			Utils.dicExecuteArrayRowReadClassOrTuple.Add(typeof(PostgisMultiLineString), true);
			Utils.dicExecuteArrayRowReadClassOrTuple.Add(typeof(PostgisMultiPolygon), true);
			Utils.dicExecuteArrayRowReadClassOrTuple.Add(typeof(PostgisGeometry), true);
			Utils.dicExecuteArrayRowReadClassOrTuple.Add(typeof(PostgisGeometryCollection), true);
			Utils.dicExecuteArrayRowReadClassOrTuple.Add(typeof(Dictionary<string, string>), true);
			Utils.dicExecuteArrayRowReadClassOrTuple.Add(typeof(JToken), true);
			Utils.dicExecuteArrayRowReadClassOrTuple.Add(typeof(JObject), true);
			Utils.dicExecuteArrayRowReadClassOrTuple.Add(typeof(JArray), true);
		}

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
			this.CodeFirst = new PostgreSQLCodeFirst(this, this.InternalCommonUtils, this.InternalCommonExpression);
		}

		internal CommonUtils InternalCommonUtils { get; }
		internal CommonExpression InternalCommonExpression { get; }

		public void Transaction(Action handler) => Ado.Transaction(handler);

		public void Transaction(Action handler, TimeSpan timeout) => Ado.Transaction(handler, timeout);
	}
}
