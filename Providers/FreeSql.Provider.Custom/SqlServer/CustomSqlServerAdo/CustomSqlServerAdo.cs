using FreeSql.Internal;
using FreeSql.Internal.Model;
using FreeSql.Internal.ObjectPool;
using System;
using System.Collections;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;

namespace FreeSql.Custom.SqlServer
{
    class CustomSqlServerAdo : FreeSql.Internal.CommonProvider.AdoProvider
    {
        DbProviderFactory Factory => FreeSqlCustomAdapterGlobalExtensions.GetDbProviderFactory(_util._orm);

        public CustomSqlServerAdo() : base(DataType.OdbcSqlServer, null, null) { }
        public CustomSqlServerAdo(CommonUtils util, string masterConnectionString, string[] slaveConnectionStrings, Func<DbConnection> connectionFactory) : base(DataType.OdbcSqlServer, masterConnectionString, slaveConnectionStrings)
        {
            base._util = util;
            if (connectionFactory != null)
            {
                var pool = new FreeSql.Internal.CommonProvider.DbConnectionPool(DataType.SqlServer, connectionFactory);
                MasterPool = pool;
                return;
            }
            throw new Exception(CoreStrings.S_CustomAdapter_OnlySuppport_UseConnectionFactory);
        }

        string[] ncharDbTypes = new[] { "NVARCHAR", "NCHAR", "NTEXT" };
        public override object AddslashesProcessParam(object param, Type mapType, ColumnInfo mapColumn)
        {
            if (param == null) return "NULL";
            if (mapType != null && mapType != param.GetType() && (param is IEnumerable == false))
                param = Utils.GetDataReaderValue(mapType, param);

            if (param is bool || param is bool?)
                return (bool)param ? 1 : 0;
            else if (param is string)
            {
                if (mapColumn != null && mapColumn.CsType.NullableTypeOrThis() == typeof(string) && ncharDbTypes.Any(a => mapColumn.Attribute.DbType.Contains(a)) == false)
                    return string.Concat("'", param.ToString().Replace("'", "''"), "'");
                return string.Concat("N'", param.ToString().Replace("'", "''"), "'");
            }
            else if (param is char)
                return string.Concat("'", param.ToString().Replace("'", "''").Replace('\0', ' '), "'");
            else if (param is Enum)
                return ((Enum)param).ToInt64();
            else if (decimal.TryParse(string.Concat(param), out var trydec))
                return param;
            else if (param is DateTime || param is DateTime?)
            {
                if (param.Equals(DateTime.MinValue) == true) param = new DateTime(1970, 1, 1);
                return string.Concat("'", ((DateTime)param).ToString("yyyy-MM-dd HH:mm:ss.fff"), "'");
            }
            else if (param is DateTimeOffset || param is DateTimeOffset?)
            {
                if (param.Equals(DateTimeOffset.MinValue) == true) param = new DateTimeOffset(new DateTime(1970, 1, 1), TimeSpan.Zero);
                return string.Concat("'", ((DateTimeOffset)param).ToString("yyyy-MM-dd HH:mm:ss.fff zzzz"), "'");
            }
            else if (param is TimeSpan || param is TimeSpan?)
                return ((TimeSpan)param).TotalSeconds;
            else if (param is byte[])
                return $"0x{CommonUtils.BytesSqlRaw(param as byte[])}";
            else if (param is IEnumerable) 
                return AddslashesIEnumerable(param, mapType, mapColumn);

            return string.Concat("'", param.ToString().Replace("'", "''"), "'");
        }

        public override DbCommand CreateCommand()
        {
            return Factory.CreateCommand();
        }

        public override void ReturnConnection(IObjectPool<DbConnection> pool, Object<DbConnection> conn, Exception ex)
        {
            pool.Return(conn);
        }

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj) => _util.GetDbParamtersByObject(sql, obj);
    }
}