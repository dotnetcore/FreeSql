using FreeSql.Internal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Npgsql;
using SafeObjectPool;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading;

namespace FreeSql.PostgreSQL {
	class PostgreSQLAdo : FreeSql.Internal.CommonProvider.AdoProvider {
		public PostgreSQLAdo() : base(null, null, DataType.PostgreSQL) { }
		public PostgreSQLAdo(CommonUtils util, ICache cache, ILogger log, string masterConnectionString, string[] slaveConnectionStrings) : base(cache, log, DataType.PostgreSQL) {
			base._util = util;
			if (!string.IsNullOrEmpty(masterConnectionString))
				MasterPool = new PostgreSQLConnectionPool("主库", masterConnectionString, null, null);
			if (slaveConnectionStrings != null) {
				foreach (var slaveConnectionString in slaveConnectionStrings) {
					var slavePool = new PostgreSQLConnectionPool($"从库{SlavePools.Count + 1}", slaveConnectionString, () => Interlocked.Decrement(ref slaveUnavailables), () => Interlocked.Increment(ref slaveUnavailables));
					SlavePools.Add(slavePool);
				}
			}
		}

		static DateTime dt1970 = new DateTime(1970, 1, 1);
		public override object AddslashesProcessParam(object param) {
			bool isdic = false;
			if (param == null) return "NULL";
			if (param is bool || param is bool?)
				return (bool)param ? "'t'" : "'f'";
			else if (param is string || param is char)
				return string.Concat("'", param.ToString().Replace("'", "''"), "'");
			else if (param is Enum)
				return ((Enum)param).ToInt64();
			else if (decimal.TryParse(string.Concat(param), out var trydec))
				return param;
			else if (param is DateTime || param is DateTime?)
				return string.Concat("'", ((DateTime)param).ToString("yyyy-MM-dd HH:mm:ss.ffffff"), "'");
			else if (param is TimeSpan || param is TimeSpan?)
				return ((TimeSpan)param).Ticks / 10;
			else if (param is JToken || param is JObject || param is JArray)
				return string.Concat("'", param.ToString().Replace("'", "''"), "'::jsonb");
			else if ((isdic = param is Dictionary<string, string>) ||
				param is IEnumerable<KeyValuePair<string, string>>) {
				var pgdics = isdic ? param as Dictionary<string, string> :
					param as IEnumerable<KeyValuePair<string, string>>;
				if (pgdics == null) return string.Concat("''::hstore");
				var pghstore = new StringBuilder();
				pghstore.Append("'");
				foreach (var dic in pgdics)
					pghstore.Append("\"").Append(dic.Key.Replace("'", "''")).Append("\"=>")
						.Append(dic.Key.Replace("'", "''")).Append(",");
				return pghstore.Append("'::hstore");
			} else if (param is IEnumerable) {
				var sb = new StringBuilder();
				var ie = param as IEnumerable;
				foreach (var z in ie) sb.Append(",").Append(AddslashesProcessParam(z));
				return sb.Length == 0 ? "(NULL)" : sb.Remove(0, 1).Insert(0, "(").Append(")").ToString();
			}else {
				return string.Concat("'", param.ToString().Replace("'", "''"), "'");
				//if (param is string) return string.Concat('N', nparms[a]);
			}
		}

		protected override DbCommand CreateCommand() {
			return new NpgsqlCommand();
		}

		protected override void ReturnConnection(ObjectPool<DbConnection> pool, Object<DbConnection> conn, Exception ex) {
			(pool as PostgreSQLConnectionPool).Return(conn, ex);
		}

		protected override DbParameter[] GetDbParamtersByObject(string sql, object obj) => _util.GetDbParamtersByObject(sql, obj);
	}
}