using FirebirdSql.Data.FirebirdClient;
using FreeSql.Internal;
using FreeSql.Internal.Model;
using FreeSql.Internal.ObjectPool;
using System;
using System.Collections;
using System.Data.Common;
using System.Text;
using System.Threading;

namespace FreeSql.Firebird
{
    class FirebirdAdo : FreeSql.Internal.CommonProvider.AdoProvider
    {

        public FirebirdAdo() : base(DataType.Firebird, null, null) { }
        public FirebirdAdo(CommonUtils util, string masterConnectionString, string[] slaveConnectionStrings, Func<DbConnection> connectionFactory) : base(DataType.Firebird, masterConnectionString, slaveConnectionStrings)
        {
            base._util = util;
            if (connectionFactory != null)
            {
                var pool = new FreeSql.Internal.CommonProvider.DbConnectionPool(DataType.Firebird, connectionFactory);
                MasterPool = pool;
                _CreateCommandConnection = pool.TestConnection;
                return;
            }
            if (!string.IsNullOrEmpty(masterConnectionString))
                MasterPool = new FirebirdConnectionPool("主库", masterConnectionString, null, null);
            if (slaveConnectionStrings != null)
            {
                foreach (var slaveConnectionString in slaveConnectionStrings)
                {
                    var slavePool = new FirebirdConnectionPool($"从库{SlavePools.Count + 1}", slaveConnectionString, () => Interlocked.Decrement(ref slaveUnavailables), () => Interlocked.Increment(ref slaveUnavailables));
                    SlavePools.Add(slavePool);
                }
            }
        }

        public bool IsFirebird2_5 => ServerVersion.Contains("Firebird 2.5");
        public string ServerVersion
        {
            get
            {
                if (string.IsNullOrEmpty(_serverVersion) && MasterPool != null)
                    using (var conn = MasterPool.Get())
                    {
                        try
                        {
                            _serverVersion = conn.Value.ServerVersion;
                        }
                        catch
                        {
                            _serverVersion = "3.0.0";
                        }
                    }
                return _serverVersion;
            }
        }
        string _serverVersion;

        public override object AddslashesProcessParam(object param, Type mapType, ColumnInfo mapColumn)
        {
            if (param == null) return "NULL";
            if (mapType != null && mapType != param.GetType() && (param is IEnumerable == false))
                param = Utils.GetDataReaderValue(mapType, param);

            if (param is bool || param is bool?)
                return (bool)param ? "true" : "false";
            else if (param is string || param is char)
                return string.Concat("'", param.ToString().Replace("'", "''"), "'");
            else if (param is Enum)
                return ((Enum)param).ToInt64();
            else if (decimal.TryParse(string.Concat(param), out var trydec))
                return param;
            else if (param is DateTime || param is DateTime?)
                return string.Concat("timestamp '", ((DateTime)param).ToString("yyyy-MM-dd HH:mm:ss.fff"), "'");
            else if (param is TimeSpan || param is TimeSpan?)
                return ((TimeSpan)param).Ticks / 10;
            else if (param is byte[])
                return $"x'{CommonUtils.BytesSqlRaw(param as byte[])}'";
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
            return new FbCommand();
        }

        public override void ReturnConnection(IObjectPool<DbConnection> pool, Object<DbConnection> conn, Exception ex)
        {
            var rawPool = pool as FirebirdConnectionPool;
            if (rawPool != null) rawPool.Return(conn, ex);
            else pool.Return(conn);
        }

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj) => _util.GetDbParamtersByObject(sql, obj);
    }
}
