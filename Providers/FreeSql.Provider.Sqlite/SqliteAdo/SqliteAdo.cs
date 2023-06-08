using FreeSql.Internal;
using FreeSql.Internal.Model;
using FreeSql.Internal.ObjectPool;
using System;
using System.Collections;
using System.Data.Common;
#if MicrosoftData
using Microsoft.Data.Sqlite;
#else
using System.Data.SQLite;
#endif
using System.Text;
using System.Threading;
using System.Linq;
using FreeSql.Internal.CommonProvider;

namespace FreeSql.Sqlite
{
    class SqliteAdo : FreeSql.Internal.CommonProvider.AdoProvider
    {
        public SqliteAdo() : base(DataType.Sqlite, null, null) { }
        public SqliteAdo(CommonUtils util, string masterConnectionString, string[] slaveConnectionStrings, Func<DbConnection> connectionFactory) : base(DataType.Sqlite, masterConnectionString, slaveConnectionStrings)
        {
            base._util = util;
            if (connectionFactory != null)
            {
                var pool = new FreeSql.Internal.CommonProvider.DbConnectionPool(DataType.Sqlite, connectionFactory);
                ConnectionString = pool.TestConnection?.ConnectionString;
                MasterPool = pool;
                _CreateCommandConnection = pool.TestConnection;
                return;
            }

            var isAdoPool = masterConnectionString?.StartsWith("AdoConnectionPool,") ?? false;
            if (isAdoPool) masterConnectionString = masterConnectionString.Substring("AdoConnectionPool,".Length);
            if (!string.IsNullOrEmpty(masterConnectionString))
                MasterPool = isAdoPool ?
                    new DbConnectionStringPool(base.DataType, CoreStrings.S_MasterDatabase, () => SqliteConnectionPool.CreateConnection(masterConnectionString)) as IObjectPool<DbConnection> :
                    new SqliteConnectionPool(CoreStrings.S_MasterDatabase, masterConnectionString, null, null);

            slaveConnectionStrings?.ToList().ForEach(slaveConnectionString =>
            {
                var slavePool = isAdoPool ?
                        new DbConnectionStringPool(base.DataType, $"{CoreStrings.S_SlaveDatabase}{SlavePools.Count + 1}", () => SqliteConnectionPool.CreateConnection(slaveConnectionString)) as IObjectPool<DbConnection> :
                        new SqliteConnectionPool($"{CoreStrings.S_SlaveDatabase}{SlavePools.Count + 1}", slaveConnectionString, () => Interlocked.Decrement(ref slaveUnavailables), () => Interlocked.Increment(ref slaveUnavailables));
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
                return string.Concat("'", param.ToString().Replace("'", "''"), "'");
            else if (param is char)
                return string.Concat("'", param.ToString().Replace("'", "''").Replace('\0', ' '), "'");
            else if (param is Enum)
                return ((Enum)param).ToInt64();
            else if (decimal.TryParse(string.Concat(param), out var trydec))
                return param;
            else if (param is DateTime || param is DateTime?)
                return string.Concat("'", ((DateTime)param).ToString("yyyy-MM-dd HH:mm:ss"), "'");
            else if (param is TimeSpan || param is TimeSpan?)
                return ((TimeSpan)param).TotalSeconds;
            else if (param is byte[])
                return string.Concat("'", Encoding.UTF8.GetString(param as byte[]), "'");
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
#if MicrosoftData
                return new SqliteCommand();
#else
                return new SQLiteCommand();
#endif

        }

        public override void ReturnConnection(IObjectPool<DbConnection> pool, Object<DbConnection> conn, Exception ex)
        {
            var rawPool = pool as SqliteConnectionPool;
            if (rawPool != null) rawPool.Return(conn, ex);
            else pool.Return(conn);
        }

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj) => _util.GetDbParamtersByObject(sql, obj);
    }
}
