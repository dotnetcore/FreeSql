using FreeSql.Internal;
using Microsoft.Extensions.Logging;
using SafeObjectPool;
using System;
using System.Collections;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.Threading;

namespace FreeSql.SqlServer {
	class SqlServerAdo : FreeSql.Internal.CommonProvider.AdoProvider {
		CommonUtils _util;

		public SqlServerAdo() : base(null, null) { }
		public SqlServerAdo(CommonUtils util, ICache cache, ILogger log, string masterConnectionString, string[] slaveConnectionStrings) : base(cache, log) {
			this._util = util;
			MasterPool = new SqlServerConnectionPool("主库", masterConnectionString, null, null);
			if (slaveConnectionStrings != null) {
				foreach (var slaveConnectionString in slaveConnectionStrings) {
					var slavePool = new SqlServerConnectionPool($"从库{SlavePools.Count + 1}", slaveConnectionString, () => Interlocked.Decrement(ref slaveUnavailables), () => Interlocked.Increment(ref slaveUnavailables));
					SlavePools.Add(slavePool);
				}
			}
		}
		public override object AddslashesProcessParam(object param) {
			if (param == null) return "NULL";
			if (param is bool || param is bool?)
				return (bool)param ? 1 : 0;
			else if (param is string || param is Enum)
				return string.Concat("'", param.ToString().Replace("'", "''"), "'");
			else if (decimal.TryParse(string.Concat(param), out var trydec))
				return param;
			else if (param is DateTime) {
				DateTime dt = (DateTime)param;
				return string.Concat("'", dt.ToString("yyyy-MM-dd HH:mm:ss.ffffff"), "'");
			} else if (param is DateTime?) {
				DateTime? dt = param as DateTime?;
				return string.Concat("'", dt.Value.ToString("yyyy-MM-dd HH:mm:ss.ffffff"), "'");
			} else if (param is IEnumerable) {
				var sb = new StringBuilder();
				var ie = param as IEnumerable;
				foreach (var z in ie) sb.Append(",").Append(AddslashesProcessParam(z));
				return sb.Length == 0 ? "(NULL)" : sb.Remove(0, 1).Insert(0, "(").Append(")").ToString();
			} else {
				return string.Concat("'", param.ToString().Replace("'", "''"), "'");
				//if (param is string) return string.Concat('N', nparms[a]);
			}
		}

		protected override DbCommand CreateCommand() {
			return new SqlCommand();
		}

		protected override void ReturnConnection(ObjectPool<DbConnection> pool, Object<DbConnection> conn, Exception ex) {
			(pool as SqlServerConnectionPool).Return(conn, ex);
		}

		protected override DbParameter[] GetDbParamtersByObject(string sql, object obj) => _util.GetDbParamtersByObject(sql, obj);
	}
}