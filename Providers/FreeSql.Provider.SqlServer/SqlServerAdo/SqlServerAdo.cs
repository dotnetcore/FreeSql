using FreeSql.Internal;
using SafeObjectPool;
using System;
using System.Collections;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.Threading;

namespace FreeSql.SqlServer
{
    class SqlServerAdo : FreeSql.Internal.CommonProvider.AdoProvider
    {
        public SqlServerAdo() : base(DataType.SqlServer) { }
        public SqlServerAdo(CommonUtils util, string masterConnectionString, string[] slaveConnectionStrings) : base(DataType.SqlServer)
        {
            base._util = util;
            if (!string.IsNullOrEmpty(masterConnectionString))
                MasterPool = new SqlServerConnectionPool("主库", masterConnectionString, null, null);
            if (slaveConnectionStrings != null)
            {
                foreach (var slaveConnectionString in slaveConnectionStrings)
                {
                    var slavePool = new SqlServerConnectionPool($"从库{SlavePools.Count + 1}", slaveConnectionString, () => Interlocked.Decrement(ref slaveUnavailables), () => Interlocked.Increment(ref slaveUnavailables));
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
            else if (param is string)
                return string.Concat("N'", param.ToString().Replace("'", "''"), "'");
            else if (param is char)
                return string.Concat("'", param.ToString().Replace("'", "''"), "'");
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
            return new SqlCommand();
        }

        protected override void ReturnConnection(ObjectPool<DbConnection> pool, Object<DbConnection> conn, Exception ex)
        {
            (pool as SqlServerConnectionPool).Return(conn, ex);
        }

        protected override DbParameter[] GetDbParamtersByObject(string sql, object obj) => _util.GetDbParamtersByObject(sql, obj);
    }
}