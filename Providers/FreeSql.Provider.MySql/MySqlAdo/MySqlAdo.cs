using FreeSql.Internal;
using FreeSql.Internal.Model;
using FreeSql.Internal.ObjectPool;
using System;
using System.Collections;
using System.Data.Common;
using System.Threading;
using System.Linq;
using FreeSql.Internal.CommonProvider;
#if MySqlConnector
using MySqlConnector;
#else
using MySql.Data.MySqlClient;
#endif

namespace FreeSql.MySql
{
    class MySqlAdo : FreeSql.Internal.CommonProvider.AdoProvider
    {

        public MySqlAdo() : base(DataType.MySql, null, null) { }
        public MySqlAdo(CommonUtils util, string masterConnectionString, string[] slaveConnectionStrings, Func<DbConnection> connectionFactory) : base(DataType.MySql, masterConnectionString, slaveConnectionStrings)
        {
            base._util = util;
            if (connectionFactory != null)
            {
                var pool = new FreeSql.Internal.CommonProvider.DbConnectionPool(DataType.MySql, connectionFactory);
                ConnectionString = pool.TestConnection?.ConnectionString;
                MasterPool = pool;
                return;
            }

            var isAdoPool = masterConnectionString?.StartsWith("AdoConnectionPool,") ?? false;
            if (isAdoPool) masterConnectionString = masterConnectionString.Substring("AdoConnectionPool,".Length);
            if (!string.IsNullOrEmpty(masterConnectionString))
                MasterPool = isAdoPool ?
                    new DbConnectionStringPool(base.DataType, CoreErrorStrings.S_MasterDatabase, () => new MySqlConnection(masterConnectionString)) as IObjectPool<DbConnection> :
                    new MySqlConnectionPool(CoreErrorStrings.S_MasterDatabase, masterConnectionString, null, null);

            slaveConnectionStrings?.ToList().ForEach(slaveConnectionString =>
            {
                var slavePool = isAdoPool ?
                    new DbConnectionStringPool(base.DataType, $"{CoreErrorStrings.S_SlaveDatabase}{SlavePools.Count + 1}", () => new MySqlConnection(slaveConnectionString)) as IObjectPool<DbConnection> :
                    new MySqlConnectionPool($"{CoreErrorStrings.S_SlaveDatabase}{SlavePools.Count + 1}", slaveConnectionString, () => Interlocked.Decrement(ref slaveUnavailables), () => Interlocked.Increment(ref slaveUnavailables));
                SlavePools.Add(slavePool);
            });
        }
        public override object AddslashesProcessParam(object param, Type mapType, ColumnInfo mapColumn)
        {
            if (param == null) return "NULL";
            if (mapType != null && mapType != param.GetType() && (param is IEnumerable == false))
                param = Utils.GetDataReaderValue(mapType, param);

            if (param is bool || param is bool?)
                return (bool)param ? 1 : 0;
            else if (param is string)
                return string.Concat("'", param.ToString().Replace("'", "''").Replace("\\", "\\\\"), "'"); //只有 mysql 需要处理反斜杠
            else if (param is char)
                return string.Concat("'", param.ToString().Replace("'", "''").Replace("\\", "\\\\").Replace('\0', ' '), "'");
            else if (param is Enum)
                return AddslashesTypeHandler(param.GetType(), param) ?? string.Concat("'", param.ToString().Replace("'", "''").Replace("\\", "\\\\").Replace(", ", ","), "'"); //((Enum)val).ToInt64();
            else if (decimal.TryParse(string.Concat(param), out var trydec))
                return param;

            else if (param is DateTime)
                return AddslashesTypeHandler(typeof(DateTime), param) ?? string.Concat("'", ((DateTime)param).ToString("yyyy-MM-dd HH:mm:ss.fff"), "'");
            else if (param is DateTime?)
                return AddslashesTypeHandler(typeof(DateTime?), param) ?? string.Concat("'", ((DateTime)param).ToString("yyyy-MM-dd HH:mm:ss.fff"), "'");

#if net60
            else if (param is DateOnly || param is DateOnly?)
                return AddslashesTypeHandler(typeof(DateOnly), param) ?? string.Concat("'", ((DateOnly)param).ToString("yyyy-MM-dd"), "'");
            else if (param is TimeOnly || param is TimeOnly?)
            {
                var ts = (TimeOnly)param;
                return $"'{ts.Hour}:{ts.Minute}:{ts.Second}'";
            }
#endif

            else if (param is TimeSpan || param is TimeSpan?)
            {
                var ts = (TimeSpan)param;
                return $"'{Math.Floor(ts.TotalHours)}:{ts.Minutes}:{ts.Seconds}'";
            }
            else if (param is byte[])
                return $"0x{CommonUtils.BytesSqlRaw(param as byte[])}";
            else if (param is MygisGeometry)
                return string.Concat("ST_GeomFromText('", (param as MygisGeometry).AsText().Replace("'", "''").Replace("\\", "\\\\"), "')");
            else if (param is IEnumerable)
                return AddslashesIEnumerable(param, mapType, mapColumn);

            return string.Concat("'", param.ToString().Replace("'", "''").Replace("\\", "\\\\"), "'");
        }

        public override DbCommand CreateCommand()
        {
            return new MySqlCommand();
        }

        public override void ReturnConnection(IObjectPool<DbConnection> pool, Object<DbConnection> conn, Exception ex)
        {
            var rawPool = pool as MySqlConnectionPool;
            if (rawPool != null) rawPool.Return(conn, ex);
            else pool.Return(conn);
        }

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj) => _util.GetDbParamtersByObject(sql, obj);
    }
}
