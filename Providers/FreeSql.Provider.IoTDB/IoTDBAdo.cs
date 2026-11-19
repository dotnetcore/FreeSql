using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.Internal.Model;
using FreeSql.Internal.ObjectPool;
using Apache.IoTDB.Data;

namespace FreeSql.IoTDB
{
    internal class IoTDBAdo : AdoProvider
    {
        public string Database { get; }

        public IoTDBAdo() : base(DataType.IoTDB, null, null) { }

        public IoTDBAdo(CommonUtils util, string masterConnectionString, string[] slaveConnectionStrings,
            Func<DbConnection> connectionFactory) : base(DataType.IoTDB, masterConnectionString, slaveConnectionStrings)
        {
            _util = util;
            var master = IoTDBOptions.Parse(masterConnectionString);
            Database = master.Database;
            ConnectionString = master.ConnectionString;

            if (connectionFactory != null)
            {
                var pool = new DbConnectionPool(DataType.IoTDB, connectionFactory);
                ConnectionString = pool.TestConnection?.ConnectionString;
                MasterPool = pool;
                return;
            }

            if (!string.IsNullOrEmpty(master.ConnectionString))
            {
                MasterPool = new DbConnectionStringPool(DataType.IoTDB, CoreErrorStrings.S_MasterDatabase,
                    () => new Apache.IoTDB.Data.IoTDBConnection(master.ConnectionString));
            }

            foreach (var connectionString in slaveConnectionStrings ?? Array.Empty<string>())
            {
                var slave = IoTDBOptions.Parse(connectionString);
                SlavePools.Add(new DbConnectionStringPool(DataType.IoTDB,
                    $"{CoreErrorStrings.S_SlaveDatabase}{SlavePools.Count + 1}",
                    () => new Apache.IoTDB.Data.IoTDBConnection(slave.ConnectionString)));
            }
        }

        public override object AddslashesProcessParam(object param, Type mapType, ColumnInfo mapColumn)
        {
            if (mapType != null && param != null && mapType != param.GetType() && !(param is System.Collections.IEnumerable))
                param = Utils.GetDataReaderValue(mapType, param);
            return IoTDBSqlParameterBinder.ToLiteral(param);
        }

        public override DbCommand CreateCommand() => new IoTDBCommand();

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj)
        {
            return Utils.GetDbParamtersByObject<DbParameter>(sql, obj, "@", (name, type, value) =>
            {
                var parameter = new IoTDBParameter("@" + name, value);
                var dbType = _util._orm.CodeFirst.GetDbInfo(type)?.type;
                if (dbType != null) parameter.DbType = (DbType)dbType.Value;
                return parameter;
            });
        }

        public override void ReturnConnection(IObjectPool<DbConnection> pool, Object<DbConnection> conn, Exception ex)
        {
            if (ex == null) pool.Return(conn);
            else pool.Return(conn, true);
        }
    }
}
