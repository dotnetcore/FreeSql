using FreeSql.Internal;
using FreeSql.Internal.Model;
using FreeSql.Internal.ObjectPool;
using System;
using System.Collections;
using System.Data.Common;
using System.Text;
using System.Threading;

namespace FreeSql.Custom
{
    class CustomAdo : FreeSql.Internal.CommonProvider.AdoProvider
    {
        public CustomAdo() : base(DataType.Custom, null, null) { }
        public CustomAdo(CommonUtils util, string masterConnectionString, string[] slaveConnectionStrings, Func<DbConnection> connectionFactory) : base(DataType.Custom, masterConnectionString, slaveConnectionStrings)
        {
            base._util = util;
            if (connectionFactory != null)
            {
                var pool = new FreeSql.Internal.CommonProvider.DbConnectionPool(DataType.SqlServer, connectionFactory);
                MasterPool = pool;
                _CreateCommandConnection = pool.TestConnection;
                _CreateParameterCommand = CreateCommand();
                return;
            }
            throw new Exception("FreeSql.Provider.CustomAdapter 仅支持 UseConnectionFactory 方式构建 IFreeSql");
        }
        CustomAdapter Adapter => (_util == null ? FreeSqlCustomAdapterGlobalExtensions.DefaultAdapter : _util._orm.GetCustomAdapter());

        public override object AddslashesProcessParam(object param, Type mapType, ColumnInfo mapColumn)
        {
            if (param == null) return "NULL";
            if (mapType != null && mapType != param.GetType() && (param is IEnumerable == false))
                param = Utils.GetDataReaderValue(mapType, param);

            if (param is bool || param is bool?)
                return (bool)param ? 1 : 0;
            else if (param is string)
                return Adapter.UnicodeStringRawSql(param, mapColumn);
            else if (param is char)
                return string.Concat("'", param.ToString().Replace("'", "''").Replace('\0', ' '), "'");
            else if (param is Enum)
                return ((Enum)param).ToInt64();
            else if (decimal.TryParse(string.Concat(param), out var trydec))
                return param;
            else if (param is DateTime || param is DateTime?)
                return Adapter.DateTimeRawSql(param);
            else if (param is TimeSpan || param is TimeSpan?)
                return Adapter.TimeSpanRawSql(param);
            else if (param is byte[])
                return Adapter.ByteRawSql(param as byte[]);
            else if (param is IEnumerable)
                return AddslashesIEnumerable(param, mapType, mapColumn);

            return string.Concat("'", param.ToString().Replace("'", "''"), "'");
        }

        DbConnection _CreateCommandConnection;
        DbCommand _CreateParameterCommand;
        public override DbCommand CreateCommand()
        {
            if (_CreateCommandConnection != null)
            {
                var cmd = _CreateCommandConnection.CreateCommand();
                cmd.Connection = null;
                return cmd;
            }
            throw new Exception("FreeSql.Provider.CustomAdapter 无法使用 CreateCommand");
        }
        public DbParameter CreateParameter()
        {
            return _CreateParameterCommand.CreateParameter();
        }

        public override void ReturnConnection(IObjectPool<DbConnection> pool, Object<DbConnection> conn, Exception ex)
        {
            pool.Return(conn);
        }

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj) => _util.GetDbParamtersByObject(sql, obj);
    }
}