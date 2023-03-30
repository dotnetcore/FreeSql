using FreeSql.Internal;
using FreeSql.Internal.Model;
using FreeSql.Internal.ObjectPool;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using XuguClient;

namespace FreeSql.Xugu
{
    class XuguAdo : FreeSql.Internal.CommonProvider.AdoProvider
    {
        public XuguAdo() : base(DataType.PostgreSQL, null, null) { }
        public XuguAdo(CommonUtils util, string masterConnectionString, string[] slaveConnectionStrings, Func<DbConnection> connectionFactory) : base(DataType.PostgreSQL, masterConnectionString, slaveConnectionStrings)
        {
            base._util = util; 
            if (connectionFactory != null)
            {
                MasterPool = new FreeSql.Internal.CommonProvider.DbConnectionPool(DataType.PostgreSQL, connectionFactory);
                return;
            }
            if (!string.IsNullOrEmpty(masterConnectionString))
                MasterPool = new XuguConnectionPool(CoreStrings.S_MasterDatabase, masterConnectionString, null, null);
            if (slaveConnectionStrings != null)
            {
                foreach (var slaveConnectionString in slaveConnectionStrings)
                {
                    var slavePool = new XuguConnectionPool($"{CoreStrings.S_SlaveDatabase}{SlavePools.Count + 1}", slaveConnectionString, () => Interlocked.Decrement(ref slaveUnavailables), () => Interlocked.Increment(ref slaveUnavailables));
                    SlavePools.Add(slavePool);
                }
            }
        }

        public override object AddslashesProcessParam(object param, Type mapType, ColumnInfo mapColumn)
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
                return ((Enum)param).ToInt64();
            else if (decimal.TryParse(string.Concat(param), out var trydec))
                return param;
            else if (param is DateTime || param is DateTime?)
                return string.Concat("'", ((DateTime)param).ToString("yyyy-MM-dd HH:mm:ss.ffffff"), "'");
            else if (param is TimeSpan || param is TimeSpan?)
                return ((TimeSpan)param).Ticks / 10;
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
                return AddslashesIEnumerable(param, mapType, mapColumn);

            return string.Concat("'", param.ToString().Replace("'", "''"), "'");
        }

        public override DbCommand CreateCommand()
        {
            return new XGCommand();
        }

        public override void ReturnConnection(IObjectPool<DbConnection> pool, Object<DbConnection> conn, Exception ex)
        {
            var rawPool = pool as XuguConnectionPool;
            if (rawPool != null) rawPool.Return(conn, ex);
            else pool.Return(conn);
        }

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj) => _util.GetDbParamtersByObject(sql, obj);
    }
}