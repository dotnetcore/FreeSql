using FreeSql.Internal;
using FreeSql.Internal.Model;
using FreeSql.Internal.ObjectPool;
using System;
using System.Collections;
using System.Data.Common;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace FreeSql.Custom.Oracle
{
    class CustomOracleAdo : FreeSql.Internal.CommonProvider.AdoProvider
    {
        DbProviderFactory Factory => FreeSqlCustomAdapterGlobalExtensions.GetDbProviderFactory(_util._orm);

        public CustomOracleAdo() : base(DataType.OdbcOracle, null, null) { }
        public CustomOracleAdo(CommonUtils util, string masterConnectionString, string[] slaveConnectionStrings, Func<DbConnection> connectionFactory) : base(DataType.OdbcOracle, masterConnectionString, slaveConnectionStrings)
        {
            base._util = util;
            if (connectionFactory != null)
            {
                var pool = new FreeSql.Internal.CommonProvider.DbConnectionPool(DataType.SqlServer, connectionFactory);
                ConnectionString = pool.TestConnection?.ConnectionString;
                MasterPool = pool;
                using (var conn = pool.Get())
                    UserId = CustomOracleAdo.GetUserId(conn.Value.ConnectionString);
                return;
            }
            throw new Exception(CoreStrings.S_CustomAdapter_OnlySuppport_UseConnectionFactory);
        }

        internal string UserId { get; set; }
        public static string GetUserId(string connectionString)
        {
            var userIdMatch = Regex.Match(connectionString, @"(User\s+Id|Uid)\s*=\s*([^;]+)", RegexOptions.IgnoreCase);
            if (userIdMatch.Success == false) throw new Exception(@"从 ConnectionString 中无法匹配 (User\s+Id|Uid)\s*=\s*([^;]+)");
            return userIdMatch.Groups[2].Value.Trim().ToUpper();
        }

        public override object AddslashesProcessParam(object param, Type mapType, ColumnInfo mapColumn)
        {
            if (param == null) return "NULL";
            if (mapType != null && mapType != param.GetType() && (param is IEnumerable == false))
                param = Utils.GetDataReaderValue(mapType, param);

            if (param is byte[])
                return $"hextoraw('{CommonUtils.BytesSqlRaw(param as byte[])}')";
            else if (param is bool || param is bool?)
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
                return string.Concat("to_timestamp('", ((DateTime)param).ToString("yyyy-MM-dd HH:mm:ss.ffffff"), "','YYYY-MM-DD HH24:MI:SS.FF6')");
            else if (param is TimeSpan || param is TimeSpan?)
                return $"numtodsinterval({((TimeSpan)param).Ticks * 1.0 / 10000000},'second')";
            else if (param is IEnumerable)
                return AddslashesIEnumerable(param, mapType, mapColumn);

            return string.Concat("'", param.ToString().Replace("'", "''"), "'");
            //if (param is string) return string.Concat('N', nparms[a]);
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
