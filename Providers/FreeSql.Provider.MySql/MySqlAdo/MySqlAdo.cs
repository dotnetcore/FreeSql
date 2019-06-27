using FreeSql.Internal;
using MySql.Data.MySqlClient;
using SafeObjectPool;
using System;
using System.Collections;
using System.Data.Common;
using System.Text;
using System.Threading;

namespace FreeSql.MySql
{
    class MySqlAdo : FreeSql.Internal.CommonProvider.AdoProvider
    {

        public MySqlAdo() : base(DataType.MySql) { }
        public MySqlAdo(CommonUtils util, string masterConnectionString, string[] slaveConnectionStrings) : base(DataType.MySql)
        {
            base._util = util;
            if (!string.IsNullOrEmpty(masterConnectionString))
                MasterPool = new MySqlConnectionPool("主库", masterConnectionString, null, null);
            if (slaveConnectionStrings != null)
            {
                foreach (var slaveConnectionString in slaveConnectionStrings)
                {
                    var slavePool = new MySqlConnectionPool($"从库{SlavePools.Count + 1}", slaveConnectionString, () => Interlocked.Decrement(ref slaveUnavailables), () => Interlocked.Increment(ref slaveUnavailables));
                    SlavePools.Add(slavePool);
                }
            }
        }
        static DateTime dt1970 = new DateTime(1970, 1, 1);
        public override object AddslashesProcessParam(object param, Type mapType)
        {
            if (param == null) return "NULL";
            if (mapType != null && mapType != param.GetType())
                param = Utils.GetDataReaderValue(mapType, param);
            if (param is bool || param is bool?)
                return (bool)param ? 1 : 0;
            else if (param is string || param is char)
                return string.Concat("'", param.ToString().Replace("'", "''"), "'");
            else if (param is Enum)
                return string.Concat("'", param.ToString().Replace("'", "''"), "'"); //((Enum)val).ToInt64();
            else if (decimal.TryParse(string.Concat(param), out var trydec))
                return param;
            else if (param is DateTime || param is DateTime?)
                return string.Concat("'", ((DateTime)param).ToString("yyyy-MM-dd HH:mm:ss.fff"), "'");
            else if (param is TimeSpan || param is TimeSpan?)
                return ((TimeSpan)param).Ticks / 10;
            else if (param is MygisGeometry)
                return string.Concat("ST_GeomFromText('", (param as MygisGeometry).AsText().Replace("'", "''"), "')");
            else if (param is IEnumerable)
            {
                var sb = new StringBuilder();
                var ie = param as IEnumerable;
                foreach (var z in ie) sb.Append(",").Append(AddslashesProcessParam(z, mapType));
                return sb.Length == 0 ? "(NULL)" : sb.Remove(0, 1).Insert(0, "(").Append(")").ToString();
            }
            return string.Concat("'", param.ToString().Replace("'", "''"), "'");
        }

        protected override DbCommand CreateCommand()
        {
            return new MySqlCommand();
        }

        protected override void ReturnConnection(ObjectPool<DbConnection> pool, Object<DbConnection> conn, Exception ex)
        {
            (pool as MySqlConnectionPool).Return(conn, ex);
        }

        protected override DbParameter[] GetDbParamtersByObject(string sql, object obj) => _util.GetDbParamtersByObject(sql, obj);
    }
}
