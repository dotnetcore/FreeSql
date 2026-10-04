using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.Internal.Model;
using FreeSql.Internal.ObjectPool;
using Newtonsoft.Json.Linq;
using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using static FreeSql.Internal.CommonExpression;
namespace FreeSql.PostgreSQL
{
    class PostgreSQLAdo : FreeSql.Internal.CommonProvider.AdoProvider
    {
        public PostgreSQLAdo() : base(DataType.PostgreSQL, null, null) { }
        public PostgreSQLAdo(CommonUtils util, string masterConnectionString, string[] slaveConnectionStrings, Func<DbConnection> connectionFactory) : base(DataType.PostgreSQL, masterConnectionString, slaveConnectionStrings)
        {
            base._util = util;
            if (connectionFactory != null)
            {
                var pool = new FreeSql.Internal.CommonProvider.DbConnectionPool(DataType.PostgreSQL, connectionFactory);
                ConnectionString = pool.TestConnection?.ConnectionString;
                MasterPool = pool;
                return;
            }

            var isAdoPool = masterConnectionString?.StartsWith("AdoConnectionPool,") ?? false;
            if (isAdoPool) masterConnectionString = masterConnectionString.Substring("AdoConnectionPool,".Length);
            if (!string.IsNullOrEmpty(masterConnectionString))
                MasterPool = isAdoPool ?
                    new DbConnectionStringPool(base.DataType, CoreErrorStrings.S_MasterDatabase, () => new NpgsqlConnection(masterConnectionString)) as IObjectPool<DbConnection> :
                    new PostgreSQLConnectionPool(CoreErrorStrings.S_MasterDatabase, masterConnectionString, null, null);

            slaveConnectionStrings?.ToList().ForEach(slaveConnectionString =>
            {
                var slavePool = isAdoPool ?
                    new DbConnectionStringPool(base.DataType, $"{CoreErrorStrings.S_SlaveDatabase}{SlavePools.Count + 1}", () => new NpgsqlConnection(slaveConnectionString)) as IObjectPool<DbConnection> :
                    new PostgreSQLConnectionPool($"{CoreErrorStrings.S_SlaveDatabase}{SlavePools.Count + 1}", slaveConnectionString, () => Interlocked.Decrement(ref slaveUnavailables), () => Interlocked.Increment(ref slaveUnavailables));
                SlavePools.Add(slavePool);
            });
        }

        public override object AddslashesProcessParam(object param, Type mapType, ColumnInfo mapColumn, CommonExpression.ExpressionStyle? style)
        {
            if (param == null) return "NULL";
            if (mapType != null && mapType != param.GetType() && (param is IEnumerable == false || param is JToken || param is JObject || param is JArray))
                param = Utils.GetDataReaderValue(mapType, param);

            bool isdic;
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

#if net60
            else if (param is DateOnly || param is DateOnly?)
                return AddslashesTypeHandler(typeof(DateOnly), param) ?? string.Concat("'", ((DateOnly)param).ToString("yyyy-MM-dd"), "'");
            else if (param is TimeOnly || param is TimeOnly?)
            {
                var ts = (TimeOnly)param;
                return $"'{ts.Hour}:{ts.Minute}:{ts.Second}'";
            }
#endif

            else if (param is TimeSpan || param is TimeSpan?)
            {
                var ts = (TimeSpan)param;
                return $"'{Math.Min(24, (int)Math.Floor(ts.TotalHours))}:{ts.Minutes}:{ts.Seconds}'";
            }
            else if (param is byte[])
                return $"'\\x{CommonUtils.BytesSqlRaw(param as byte[])}'";
            else if (param is JToken || param is JObject || param is JArray)
                return string.Concat("'", param.ToString().Replace("'", "''"), "'::jsonb");
            else if ((isdic = param is Dictionary<string, string>) ||
                param is IEnumerable<KeyValuePair<string, string>>)
            {
                var pgdics = isdic ? param as Dictionary<string, string> :
                    param as IEnumerable<KeyValuePair<string, string>>;

                var pghstore = new StringBuilder("'");
                var pairs = pgdics.ToArray();

                for (var i = 0; i < pairs.Length; i++)
                {
                    if (i != 0) pghstore.Append(",");

                    pghstore.AppendFormat("\"{0}\"=>", pairs[i].Key.Replace("'", "''"));

                    if (pairs[i].Value == null)
                        pghstore.Append("NULL");
                    else
                        pghstore.AppendFormat("\"{0}\"", pairs[i].Value.Replace("'", "''"));
                }

                return pghstore.Append("'::hstore");
            }
            else if (param is IEnumerable)
                return AddslashesIEnumerable(param, mapType, mapColumn, style);

            return string.Concat("'", param.ToString().Replace("'", "''"), "'");
        }

        protected override string AddslashesIEnumerable(object param, Type mapType, ColumnInfo mapColumn, ExpressionStyle? style = null)
        {
            var sb = new StringBuilder();
            var ie = param as IEnumerable;
            var idx = 0;
            foreach (var z in ie)
            {
                sb.Append(",");
                if (++idx > 500)
                {
                    sb.Append("   \r\n    \r\n"); //500元素分割, 3空格\r\n4空格
                    idx = 1;
                }
                var obj = AddslashesProcessParam(z, mapType, mapColumn, style);
                if (style == ExpressionStyle.Set && obj is string)
                {
                    obj = $"{obj}".Trim('\'');
                }
                sb.Append(string.Format(CultureInfo.InvariantCulture, "{0}", obj));
            }

            if (style == ExpressionStyle.Set)
                return sb.Length == 0 ? "(NULL)" : sb.Remove(0, 1).Insert(0, "'{").Append("}'").ToString();
            else
                return sb.Length == 0 ? "(NULL)" : sb.Remove(0, 1).Insert(0, "(").Append(")").ToString();
        }

        public override DbCommand CreateCommand()
        {
            return new NpgsqlCommand();
        }

        public override void ReturnConnection(IObjectPool<DbConnection> pool, Object<DbConnection> conn, Exception ex)
        {
            var rawPool = pool as PostgreSQLConnectionPool;
            if (rawPool != null) rawPool.Return(conn, ex);
            else pool.Return(conn);
        }

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj) => _util.GetDbParamtersByObject(sql, obj);
    }
}