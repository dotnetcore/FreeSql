using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.Internal.Model;
using FreeSql.Internal.ObjectPool;
using System;
using System.Collections;
using System.Data.Common;
using System.Linq;
using System.Threading;
using TDengine.Data.Client;

namespace FreeSql.TDengine
{
    internal class TDengineAdo : AdoProvider
    {
        public TDengineAdo() : base(DataType.TDengine, null, null) { }

        public TDengineAdo(CommonUtils util, string masterConnectionString, string[] slaveConnectionStrings,
            Func<DbConnection> connectionFactory) : base(DataType.TDengine, masterConnectionString,
            slaveConnectionStrings)
        {
            base._util = util;
            if (connectionFactory != null)
            {
                var pool = new DbConnectionPool(DataType.TDengine, connectionFactory);
                ConnectionString = pool.TestConnection?.ConnectionString;
                MasterPool = pool;
                return;
            }

            var isAdoPool = masterConnectionString?.StartsWith("AdoConnectionPool,") ?? false;
            if (isAdoPool) masterConnectionString = masterConnectionString.Substring("AdoConnectionPool,".Length);
            if (!string.IsNullOrEmpty(masterConnectionString))
                MasterPool = isAdoPool
                    ? new DbConnectionStringPool(base.DataType, CoreErrorStrings.S_MasterDatabase,
                        () => new TDengineConnection(masterConnectionString)) as IObjectPool<DbConnection>
                    : new TDengineConnectionPool(CoreErrorStrings.S_MasterDatabase, masterConnectionString, null, null);

            slaveConnectionStrings?.ToList().ForEach(slaveConnectionString =>
            {
                var slavePool = isAdoPool
                    ? new DbConnectionStringPool(base.DataType, $"{CoreErrorStrings.S_SlaveDatabase}{SlavePools.Count + 1}",
                        () => new TDengineConnection(slaveConnectionString)) as IObjectPool<DbConnection>
                    : new TDengineConnectionPool($"{CoreErrorStrings.S_SlaveDatabase}{SlavePools.Count + 1}",
                        slaveConnectionString, () => Interlocked.Decrement(ref slaveUnavailables),
                        () => Interlocked.Increment(ref slaveUnavailables));
                SlavePools.Add(slavePool);
            });
        }

        public override object AddslashesProcessParam(object param, Type mapType, ColumnInfo mapColumn)
        {
            if (param == null) return "NULL";

            if (mapType != null && mapType != param.GetType() && (param is IEnumerable == false))
                param = Utils.GetDataReaderValue(mapType, param);

            if (param is bool paramBool)
                return paramBool ? "true" : "false";
            else if (param is string paramStr)
                return string.Concat("'", paramStr?.Replace("'", "\\'"), "'");
            else if (param is char)
                return string.Concat("'", param.ToString()?.Replace("'", "''").Replace('\0', ' '), "'");
            else if (param is Enum @enum)
                return AddslashesTypeHandler(@enum.GetType(), @enum) ?? @enum.ToInt64();
            else if (decimal.TryParse(string.Concat(param), out _))
                return param;

            else if (param is DateTime time)
                return AddslashesTypeHandler(typeof(DateTime), time) ??
                       string.Concat("'", time.ToString("yyyy-MM-dd HH:mm:ss.fffffff"), "'");

#if NET6_0_OR_GREATER

            else if (param is DateOnly dateOnly)
                return AddslashesTypeHandler(typeof(DateOnly), dateOnly) ??
                       string.Concat("'", dateOnly.ToString("yyyy-MM-dd"), "'");
            else if (param is TimeOnly timeOnly)
            {
                return $"'{timeOnly.Hour}:{timeOnly.Minute}:{timeOnly.Second}'";
            }
#endif

            else if (param is TimeSpan timeSpan)
            {
                return $"'{Math.Floor(timeSpan.TotalHours)}:{timeSpan.Minutes}:{timeSpan.Seconds}'";
            }
            else if (param is byte[] bytes)
                return $"0x{CommonUtils.BytesSqlRaw(bytes)}";

            else if (param is IEnumerable)
                return AddslashesIEnumerable(param, mapType, mapColumn);

            return string.Concat("'", param.ToString()?.Replace("\\", "\\\\").Replace("'", "\\'"), "'");
        }

        public override DbCommand CreateCommand()
        {
            return new TDengineCommand();
        }

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj) =>
            _util.GetDbParamtersByObject(sql, obj);

        public override void ReturnConnection(IObjectPool<DbConnection> pool, Object<DbConnection> conn, Exception ex)
        {
            if (pool is TDengineConnectionPool rawPool) rawPool.Return(conn, ex);
            else pool.Return(conn);
        }
    }
}