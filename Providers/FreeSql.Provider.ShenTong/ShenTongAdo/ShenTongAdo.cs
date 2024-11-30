using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.Internal.Model;
using FreeSql.Internal.ObjectPool;
using System;
using System.Collections;
using System.Data.Common;
using System.Data.OscarClient;
using System.Linq;
using System.Threading;

namespace FreeSql.ShenTong
{
    class ShenTongAdo : FreeSql.Internal.CommonProvider.AdoProvider
    {
        public ShenTongAdo() : base(DataType.ShenTong, null, null) { }
        public ShenTongAdo(CommonUtils util, string masterConnectionString, string[] slaveConnectionStrings, Func<DbConnection> connectionFactory) : base(DataType.ShenTong, masterConnectionString, slaveConnectionStrings)
        {
            base._util = util; 
            if (connectionFactory != null)
            {
                var pool = new FreeSql.Internal.CommonProvider.DbConnectionPool(DataType.ShenTong, connectionFactory);
                ConnectionString = pool.TestConnection?.ConnectionString;
                MasterPool = pool;
                return;
            }

            var isAdoPool = masterConnectionString?.StartsWith("AdoConnectionPool,") ?? false;
            if (isAdoPool) masterConnectionString = masterConnectionString.Substring("AdoConnectionPool,".Length);
            if (!string.IsNullOrEmpty(masterConnectionString))
                MasterPool = isAdoPool ?
                    new DbConnectionStringPool(base.DataType, CoreErrorStrings.S_MasterDatabase, () => new OscarConnection(masterConnectionString)) as IObjectPool<DbConnection> :
                    new ShenTongConnectionPool(CoreErrorStrings.S_MasterDatabase, masterConnectionString, null, null);

            slaveConnectionStrings?.ToList().ForEach(slaveConnectionString =>
            {
                var slavePool = isAdoPool ?
                    new DbConnectionStringPool(base.DataType, $"{CoreErrorStrings.S_SlaveDatabase}{SlavePools.Count + 1}", () => new OscarConnection(slaveConnectionString)) as IObjectPool<DbConnection> :
                    new ShenTongConnectionPool($"{CoreErrorStrings.S_SlaveDatabase}{SlavePools.Count + 1}", slaveConnectionString, () => Interlocked.Decrement(ref slaveUnavailables), () => Interlocked.Increment(ref slaveUnavailables));
                SlavePools.Add(slavePool);
            });
        }

        public override object AddslashesProcessParam(object param, Type mapType, ColumnInfo mapColumn)
        {
            if (param == null) return "NULL";
            if (mapType != null && mapType != param.GetType() && (param is IEnumerable == false))
                param = Utils.GetDataReaderValue(mapType, param);

            if (param is bool || param is bool?)
                return (bool)param ? "'t'" : "'f'";
            else if (param is string)
                return string.Concat("'", param.ToString().Replace("'", "''"), "'");
            else if (param is char)
                return string.Concat("'", param.ToString().Replace("'", "''").Replace('\0', ' '), "'");
            else if (param is Enum)
                return AddslashesTypeHandler(param.GetType(), param) ?? ((Enum)param).ToInt64();
            else if (decimal.TryParse(string.Concat(param), out var trydec))
                return param;

            else if (param is DateTime)
                return AddslashesTypeHandler(typeof(DateTime), param) ?? string.Concat("'", ((DateTime)param).ToString("yyyy-MM-dd HH:mm:ss.ffffff"), "'");
            else if (param is DateTime?)
                return AddslashesTypeHandler(typeof(DateTime?), param) ?? string.Concat("'", ((DateTime)param).ToString("yyyy-MM-dd HH:mm:ss.ffffff"), "'");

            else if (param is TimeSpan || param is TimeSpan?)
            {
                var ts = (TimeSpan)param;
                var hh = Math.Min(24, (int)Math.Floor(ts.TotalHours));
                if (hh >= 24) hh = 0;
                return $"'{hh}:{ts.Minutes}:{ts.Seconds}.{ts.Milliseconds}'";
            }
            else if (param is byte[])
                return $"0x{CommonUtils.BytesSqlRaw(param as byte[])}";
            else if (param is IEnumerable)
                return AddslashesIEnumerable(param, mapType, mapColumn);

            return string.Concat("'", param.ToString().Replace("'", "''"), "'");
        }

        public override DbCommand CreateCommand()
        {
            return new OscarCommand();
        }

        public override void ReturnConnection(IObjectPool<DbConnection> pool, Object<DbConnection> conn, Exception ex)
        {
            var rawPool = pool as ShenTongConnectionPool;
            if (rawPool != null) rawPool.Return(conn, ex);
            else pool.Return(conn);
        }

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj) => _util.GetDbParamtersByObject(sql, obj);
    }
}