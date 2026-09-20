using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.Internal.Model;
using FreeSql.Internal.ObjectPool;
using System;
using System.Collections;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;
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
                MasterPool = new DbConnectionStringPool(base.DataType, "主数据库", () => new SndbConnection(masterConnectionString));

            slaveConnectionStrings?.ToList().ForEach(slaveConnectionString =>
            {
                if (slaveConnectionString?.StartsWith("AdoConnectionPool,") == true)
                    slaveConnectionString = slaveConnectionString.Substring("AdoConnectionPool,".Length);
                SlavePools.Add(new DbConnectionStringPool(base.DataType, $"从数据库{SlavePools.Count + 1}", () => new SndbConnection(slaveConnectionString)));
            });
        }

        public override object AddslashesProcessParam(object param, Type mapType, ColumnInfo mapColumn)
        {
            if (param == null) return "NULL";
            if (param is global::SonnetDB.Model.GeoPoint geoPoint)
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "POINT({0:R}, {1:R})",
                    geoPoint.Lat,
                    geoPoint.Lon);
            }
            if (SonnetDBUtils.TryGetVectorValue(param, out var vector)
                && (mapColumn == null || SonnetDBUtils.IsVectorColumn(mapColumn)))
                return SonnetDBUtils.FormatVectorLiteral(vector);
            if (mapType != null && mapType != param.GetType() && (param is IEnumerable == false || param is string))
                param = Utils.GetDataReaderValue(mapType, param);

            if (param is ulong unsignedValue)
                return SonnetDBUtils.ConvertUInt64ToInt64(unsignedValue);
            if (param is bool b) return b ? "true" : "false";
            if (param is string s) return string.Concat("'", s.Replace("'", "''"), "'");
            if (param is char c) return string.Concat("'", c.ToString().Replace("'", "''").Replace('\0', ' '), "'");
            if (param is Guid g) return string.Concat("'", g.ToString("n"), "'");
            if (param is Enum e)
            {
                var typeHandlerValue = AddslashesTypeHandler(param.GetType(), param);
                if (typeHandlerValue != null) return typeHandlerValue;
                if (Enum.GetUnderlyingType(e.GetType()) == typeof(ulong))
                    return SonnetDBUtils.ConvertUInt64ToInt64(Convert.ToUInt64(e, CultureInfo.InvariantCulture));
                return e.ToInt64();
            }
            if (param is DateTime dt)
            {
                return ToUnixTimeMilliseconds(dt).ToString(CultureInfo.InvariantCulture);
            }
            if (param is DateTimeOffset dto)
            {
                return dto.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture);
            }
            if (param is byte[] bytes) return string.Concat("'", Convert.ToBase64String(bytes).Replace("'", "''"), "'");
            if (param is JsonDocument document) return string.Concat("'", document.RootElement.GetRawText().Replace("'", "''"), "'");
            if (param is JsonElement element)
            {
                var json = element.ValueKind == JsonValueKind.Undefined ? "null" : element.GetRawText();
                return string.Concat("'", json.Replace("'", "''"), "'");
            }
            if (param is JsonNode node) return string.Concat("'", node.ToJsonString().Replace("'", "''"), "'");
            if (param is BigInteger)
                throw SonnetDBUtils.UnsupportedBigInteger(param.GetType());
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

        /// <summary>
        /// 将 SonnetDB 返回的时间列转换为 DateTimeOffset。
        /// 时序测量的时间列按 Unix 毫秒整数返回，关系表 DATETIME
        /// 则通常按 DateTime 返回；两种形态都在这里统一处理。
        /// </summary>
        public static DateTimeOffset ReadDateTimeOffset(DbDataReader reader, int ordinal)
        {
            return SonnetDBUtils.ConvertDateTimeOffsetValue(reader.GetValue(ordinal));
        }

        /// <summary>
        /// 将 SonnetDB 返回的时间列转换为 DateTime。
        /// 时序测量的时间列按 Unix 毫秒整数返回，关系表 DATETIME
        /// 则通常按 DateTime 返回。
        /// </summary>
        public static DateTime ReadDateTime(DbDataReader reader, int ordinal)
        {
            return SonnetDBUtils.ConvertDateTimeValue(reader.GetValue(ordinal));
        }

        public override void ReturnConnection(IObjectPool<DbConnection> pool, Object<DbConnection> conn, Exception ex) => pool.Return(conn, ex != null);

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj) => _util.GetDbParamtersByObject(sql, obj);
    }
}
