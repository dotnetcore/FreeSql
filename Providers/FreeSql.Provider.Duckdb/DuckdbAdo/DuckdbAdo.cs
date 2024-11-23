using FreeSql.Internal;
using FreeSql.Internal.Model;
using FreeSql.Internal.ObjectPool;
using System;
using System.Collections;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Linq;
using FreeSql.Internal.CommonProvider;
using DuckDB.NET.Data;
using static DuckDB.NET.Native.NativeMethods;
using ColumnInfo = FreeSql.Internal.Model.ColumnInfo;

namespace FreeSql.Duckdb
{
    class DuckdbAdo : FreeSql.Internal.CommonProvider.AdoProvider
    {
        public DuckdbAdo() : base(DataType.DuckDB, null, null) { }
        public DuckdbAdo(CommonUtils util, string masterConnectionString, string[] slaveConnectionStrings, Func<DbConnection> connectionFactory) : base(DataType.DuckDB, masterConnectionString, slaveConnectionStrings)
        {
            base._util = util;
            if (connectionFactory != null)
            {
                var pool = new FreeSql.Internal.CommonProvider.DbConnectionPool(DataType.DuckDB, connectionFactory);
                ConnectionString = pool.TestConnection?.ConnectionString;
                MasterPool = pool;
                _CreateCommandConnection = pool.TestConnection;
                return;
            }

            var isAdoPool = masterConnectionString?.StartsWith("AdoConnectionPool,") ?? false;
            if (isAdoPool) masterConnectionString = masterConnectionString.Substring("AdoConnectionPool,".Length);
            if (!string.IsNullOrEmpty(masterConnectionString))
                MasterPool = isAdoPool ?
                    new DbConnectionStringPool(base.DataType, CoreErrorStrings.S_MasterDatabase, () => DuckdbConnectionPool.CreateConnection(masterConnectionString)) as IObjectPool<DbConnection> :
                    new DuckdbConnectionPool(CoreErrorStrings.S_MasterDatabase, masterConnectionString, null, null);

            slaveConnectionStrings?.ToList().ForEach(slaveConnectionString =>
            {
                var slavePool = isAdoPool ?
                    new DbConnectionStringPool(base.DataType, $"{CoreErrorStrings.S_SlaveDatabase}{SlavePools.Count + 1}", () => DuckdbConnectionPool.CreateConnection(slaveConnectionString)) as IObjectPool<DbConnection> :
                    new DuckdbConnectionPool($"{CoreErrorStrings.S_SlaveDatabase}{SlavePools.Count + 1}", slaveConnectionString, () => Interlocked.Decrement(ref slaveUnavailables), () => Interlocked.Increment(ref slaveUnavailables));
                SlavePools.Add(slavePool);
            });
        }
        public override object AddslashesProcessParam(object param, Type mapType, ColumnInfo mapColumn)
        {
            if (param == null) return "NULL";
            if (mapType != null && mapType != param.GetType() && (param is IEnumerable == false))
                param = Utils.GetDataReaderValue(mapType, param);

            if (param is bool || param is bool?)
                return (bool)param ? "true" : "false";
            else if (param is string)
                return string.Concat("'", param.ToString().Replace("'", "''"), "'");
            else if (param is char)
                return string.Concat("'", param.ToString().Replace("'", "''").Replace('\0', ' '), "'");
            else if (param is Enum)
                return AddslashesTypeHandler(param.GetType(), param) ?? ((Enum)param).ToInt64();
            else if (decimal.TryParse(string.Concat(param), out var trydec))
                return param;

            else if (param is DateTime)
                return AddslashesTypeHandler(typeof(DateTime), param) ?? string.Concat("timestamp '", ((DateTime)param).ToString("yyyy-MM-dd HH:mm:ss.ffffff"), "'");
            else if (param is DateTime?)
                return AddslashesTypeHandler(typeof(DateTime?), param) ?? string.Concat("timestamp '", ((DateTime)param).ToString("yyyy-MM-dd HH:mm:ss.ffffff"), "'");

#if net60
            else if (param is DateOnly || param is DateOnly?)
                return AddslashesTypeHandler(typeof(DateOnly), param) ?? string.Concat("date '", ((DateOnly)param).ToString("yyyy-MM-dd"), "'");
            else if (param is TimeOnly || param is TimeOnly?)
            {
                var ts = (TimeOnly)param;
                return $"time '{ts.Hour}:{ts.Minute}:{ts.Second}'";
            }
#endif

            else if (param is TimeSpan || param is TimeSpan?)
            {
                var ts = (TimeSpan)param;
                return $"time '{Math.Min(24, (int)Math.Floor(ts.TotalHours))}:{ts.Minutes}:{ts.Seconds}.{ts.Milliseconds}'";
            }
            else if (param is byte[])
            {
                var sb = new StringBuilder().Append("'");
                foreach (var vc in param as byte[]) sb.Append("\\x").Append(vc.ToString("X2"));
                return sb.Append("'::blob").ToString();
            }
            else if (param is IEnumerable)
                return AddslashesIEnumerable(param, mapType, mapColumn);

            return string.Concat("'", param.ToString().Replace("'", "''"), "'");
        }

        DbConnection _CreateCommandConnection;
        public override DbCommand CreateCommand()
        {
            if (_CreateCommandConnection != null)
            {
                var cmd = _CreateCommandConnection.CreateCommand();
                cmd.Connection = null;
                return cmd;
            }
            return new DuckDBCommand();
        }

        public override void ReturnConnection(IObjectPool<DbConnection> pool, Object<DbConnection> conn, Exception ex)
        {
            var rawPool = pool as DuckdbConnectionPool;
            if (rawPool != null) rawPool.Return(conn, ex);
            else pool.Return(conn);
        }

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj) => _util.GetDbParamtersByObject(sql, obj);
    }
}
