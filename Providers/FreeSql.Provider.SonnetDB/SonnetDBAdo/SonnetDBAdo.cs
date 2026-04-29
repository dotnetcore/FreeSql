using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.Internal.Model;
using FreeSql.Internal.ObjectPool;
using System;
using System.Collections;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using SndbCommand = global::SonnetDB.Data.SndbCommand;
using SndbConnection = global::SonnetDB.Data.SndbConnection;

namespace FreeSql.SonnetDB
{
    public class SonnetDBAdo : AdoProvider
    {
        public SonnetDBAdo() : base(DataType.SonnetDB, null, null) { }

        public SonnetDBAdo(CommonUtils util, string masterConnectionString, string[] slaveConnectionStrings, Func<DbConnection> connectionFactory)
            : base(DataType.SonnetDB, masterConnectionString, slaveConnectionStrings)
        {
            base._util = util;
            if (connectionFactory != null)
            {
                var pool = new DbConnectionPool(DataType.SonnetDB, connectionFactory);
                ConnectionString = pool.TestConnection?.ConnectionString;
                MasterPool = pool;
                return;
            }

            var isAdoPool = masterConnectionString?.StartsWith("AdoConnectionPool,") ?? false;
            if (isAdoPool) masterConnectionString = masterConnectionString.Substring("AdoConnectionPool,".Length);
            if (!string.IsNullOrEmpty(masterConnectionString))
                MasterPool = new DbConnectionStringPool(base.DataType, CoreErrorStrings.S_MasterDatabase, () => new SndbConnection(masterConnectionString));

            slaveConnectionStrings?.ToList().ForEach(slaveConnectionString =>
            {
                if (slaveConnectionString?.StartsWith("AdoConnectionPool,") == true)
                    slaveConnectionString = slaveConnectionString.Substring("AdoConnectionPool,".Length);
                SlavePools.Add(new DbConnectionStringPool(base.DataType, $"{CoreErrorStrings.S_SlaveDatabase}{SlavePools.Count + 1}", () => new SndbConnection(slaveConnectionString)));
            });
        }

        public override object AddslashesProcessParam(object param, Type mapType, ColumnInfo mapColumn)
        {
            if (param == null) return "NULL";
            if (mapType != null && mapType != param.GetType() && (param is IEnumerable == false || param is string))
                param = Utils.GetDataReaderValue(mapType, param);

            if (param is bool b) return b ? "true" : "false";
            if (param is string s) return string.Concat("'", s.Replace("'", "''"), "'");
            if (param is char c) return string.Concat("'", c.ToString().Replace("'", "''").Replace('\0', ' '), "'");
            if (param is Guid g) return string.Concat("'", g.ToString("n"), "'");
            if (param is Enum e) return AddslashesTypeHandler(param.GetType(), param) ?? e.ToInt64();
            if (param is DateTime dt) return ToUnixTimeMilliseconds(dt).ToString(CultureInfo.InvariantCulture);
            if (param is DateTimeOffset dto) return dto.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture);
            if (param is byte[] bytes) return string.Concat("'", Convert.ToBase64String(bytes).Replace("'", "''"), "'");
            if (param is IEnumerable) return AddslashesIEnumerable(param, mapType, mapColumn);
            if (decimal.TryParse(string.Concat(param), NumberStyles.Any, CultureInfo.InvariantCulture, out _)) return param;

            return string.Concat("'", param.ToString().Replace("'", "''"), "'");
        }

        static long ToUnixTimeMilliseconds(DateTime value)
        {
            if (value.Kind == DateTimeKind.Unspecified) value = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            return new DateTimeOffset(value).ToUnixTimeMilliseconds();
        }

        public override DbCommand CreateCommand() => new SndbCommand();

        public override void ReturnConnection(IObjectPool<DbConnection> pool, Object<DbConnection> conn, Exception ex) => pool.Return(conn, ex != null);

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj) => _util.GetDbParamtersByObject(sql, obj);
    }
}
