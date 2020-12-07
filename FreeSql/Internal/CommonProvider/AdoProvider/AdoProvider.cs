using FreeSql.Internal.Model;
using FreeSql.Internal.ObjectPool;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.Internal.CommonProvider
{
    public abstract partial class AdoProvider : IAdo, IDisposable
    {

        public abstract void ReturnConnection(IObjectPool<DbConnection> pool, Object<DbConnection> conn, Exception ex);
        public abstract DbCommand CreateCommand();
        public abstract DbParameter[] GetDbParamtersByObject(string sql, object obj);
        public DbParameter[] GetDbParamtersByObject(object obj) => GetDbParamtersByObject("*", obj);

        protected bool IsTracePerformance => _util?._orm?.Aop.CommandAfterHandler != null;

        public IObjectPool<DbConnection> MasterPool { get; protected set; }
        public List<IObjectPool<DbConnection>> SlavePools { get; } = new List<IObjectPool<DbConnection>>();
        public DataType DataType { get; }
        public string ConnectionString { get; protected set; }
        public string[] SlaveConnectionStrings { get; protected set; }
        public Guid Identifier { get; protected set; }

        public CommonUtils _util { get; set; }
        protected int slaveUnavailables = 0;
        private object slaveLock = new object();
        private Random slaveRandom = new Random();
        protected Func<DbTransaction> ResolveTransaction;

        public AdoProvider(DataType dataType, string connectionString, string[] slaveConnectionStrings)
        {
            this.DataType = dataType;
            this.ConnectionString = connectionString;
            this.SlaveConnectionStrings = slaveConnectionStrings;
            this.Identifier = Guid.NewGuid();
        }

        void LoggerException(IObjectPool<DbConnection> pool, PrepareCommandResult pc, Exception ex, DateTime dt, StringBuilder logtxt, bool isThrowException = true)
        {
            var cmd = pc.cmd;
            if (pc.isclose) pc.cmd.Connection.Close();
            if (IsTracePerformance)
            {
                TimeSpan ts = DateTime.Now.Subtract(dt);
                if (ex == null && ts.TotalMilliseconds > 100)
                    Trace.WriteLine(logtxt.Insert(0, $"{pool?.Policy.Name}（执行SQL）语句耗时过长{ts.TotalMilliseconds}ms\r\n{cmd.CommandText}\r\n").ToString());
                else
                    logtxt.Insert(0, $"{pool?.Policy.Name}（执行SQL）耗时{ts.TotalMilliseconds}ms\r\n{cmd.CommandText}\r\n").ToString();
            }

            if (ex == null)
            {
                _util?._orm?.Aop.CommandAfterHandler?.Invoke(_util._orm, new Aop.CommandAfterEventArgs(pc.before, ex, logtxt.ToString()));
                return;
            }

            StringBuilder log = new StringBuilder();
            log.Append(pool?.Policy.Name).Append("数据库出错（执行SQL）〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓\r\n").Append(cmd.CommandText).Append("\r\n");
            foreach (DbParameter parm in cmd.Parameters)
                log.Append(parm.ParameterName.PadRight(20, ' ')).Append(" = ").Append((parm.Value ?? DBNull.Value) == DBNull.Value ? "NULL" : parm.Value).Append("\r\n");

            log.Append(ex.Message);
            Trace.WriteLine(log.ToString());

            if (cmd.Transaction != null)
            {
                var curTran = TransactionCurrentThread;
                if (cmd.Transaction != TransactionCurrentThread)
                {
                    //cmd.Transaction.Rollback();
                }
                else
                    RollbackTransaction(ex);
            }

            _util?._orm?.Aop.CommandAfterHandler?.Invoke(_util._orm, new Aop.CommandAfterEventArgs(pc.before, ex, logtxt.ToString()));

            cmd.Parameters.Clear();
            if (isThrowException)
            {
                if (DataType == DataType.Sqlite) cmd.Dispose();
                throw new Exception(ex.Message, ex);
            }
        }

        internal Dictionary<string, PropertyInfo> GetQueryTypeProperties(Type type)
        {
            return type.GetPropertiesDictIgnoreCase(); //与 ExecuteArrayRowReadClassOrTuple 顺序同步，防止【延时属性】获取到位置不对的问题
            //var tb = _util.GetTableByEntity(type);
            //var props = tb?.Properties ?? type.GetPropertiesDictIgnoreCase();
            //return props;
        }

        public AdoCommandFluent CommandFluent(string cmdText, object parms = null) => new AdoCommandFluent(this, cmdText, parms);

        public bool ExecuteConnectTest(int commandTimeout = 0)
        {
            try
            {
                switch (DataType)
                {
                    case DataType.Oracle:
                    case DataType.OdbcOracle:
                        ExecuteNonQuery(null, null, CommandType.Text, " SELECT 1 FROM dual", commandTimeout);
                        return true;
                    case DataType.Firebird:
                        ExecuteNonQuery(null, null, CommandType.Text, " SELECT FIRST 1 1 FROM rdb$database", commandTimeout);
                        return true;
                }
                ExecuteNonQuery(null, null, CommandType.Text, " SELECT 1", commandTimeout);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public T QuerySingle<T>(string cmdText, object parms = null) => Query<T>(cmdText, parms).FirstOrDefault();
        public T QuerySingle<T>(CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => Query<T>(cmdType, cmdText, cmdParms).FirstOrDefault();
        public List<T> Query<T>(string cmdText, object parms = null) => Query<T>(null, null, null, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms));
        public List<T> Query<T>(DbTransaction transaction, string cmdText, object parms = null) => Query<T>(null, null, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms));
        public List<T> Query<T>(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null) => Query<T>(null, connection, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms));
        public List<T> Query<T>(CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => Query<T>(null, null, null, cmdType, cmdText, 0, cmdParms);
        public List<T> Query<T>(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => Query<T>(null, null, transaction, cmdType, cmdText, 0, cmdParms);
        public List<T> Query<T>(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, params DbParameter[] cmdParms) => Query<T>(null, connection, transaction, cmdType, cmdText, cmdTimeout, cmdParms);
        public List<T> Query<T>(Type resultType, DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, params DbParameter[] cmdParms)
        {
            var ret = new List<T>();
            if (string.IsNullOrEmpty(cmdText)) return ret;
            var type = typeof(T);
            if (resultType != null && type != resultType) type = resultType;
            string flag = null;
            int[] indexes = null;
            var props = GetQueryTypeProperties(type);
            ExecuteReader(connection, transaction, fetch =>
            {
                if (indexes == null)
                {
                    var sbflag = new StringBuilder().Append("adoQuery");
                    var dic = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
                    for (var a = 0; a < fetch.Object.FieldCount; a++)
                    {
                        var name = fetch.Object.GetName(a);
                        if (dic.ContainsKey(name)) continue;
                        sbflag.Append(name).Append(":").Append(a).Append(",");
                        dic.Add(name, a);
                    }
                    indexes = props.Select(a => dic.TryGetValue(a.Key, out var tryint) ? tryint : -1).ToArray();
                    flag = sbflag.ToString();
                }
                ret.Add((T)Utils.ExecuteArrayRowReadClassOrTuple(flag, type, indexes, fetch.Object, 0, _util).Value);
            }, cmdType, cmdText, cmdTimeout, cmdParms);
            return ret;
        }
        #region query multi
        public NativeTuple<List<T1>, List<T2>> Query<T1, T2>(string cmdText, object parms = null) => Query<T1, T2>(null, null, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms));
        public NativeTuple<List<T1>, List<T2>> Query<T1, T2>(DbTransaction transaction, string cmdText, object parms = null) => Query<T1, T2>(null, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms));
        public NativeTuple<List<T1>, List<T2>> Query<T1, T2>(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null) => Query<T1, T2>(connection, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms));
        public NativeTuple<List<T1>, List<T2>> Query<T1, T2>(CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => Query<T1, T2>(null, null, cmdType, cmdText, 0, cmdParms);
        public NativeTuple<List<T1>, List<T2>> Query<T1, T2>(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => Query<T1, T2>(null, transaction, cmdType, cmdText, 0, cmdParms);
        public NativeTuple<List<T1>, List<T2>> Query<T1, T2>(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, params DbParameter[] cmdParms)
        {
            if (string.IsNullOrEmpty(cmdText)) return NativeTuple.Create(new List<T1>(), new List<T2>());
            var ret1 = new List<T1>();
            var type1 = typeof(T1);
            string flag1 = null;
            int[] indexes1 = null;
            var props1 = GetQueryTypeProperties(type1);

            var ret2 = new List<T2>();
            var type2 = typeof(T2);
            string flag2 = null;
            int[] indexes2 = null;
            var props2 = GetQueryTypeProperties(type2);
            ExecuteReaderMultiple(2, connection, transaction, (fetch, result) =>
            {
                switch (result)
                {
                    case 0:
                        if (indexes1 == null)
                        {
                            var sbflag = new StringBuilder().Append("adoQuery");
                            var dic = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
                            for (var a = 0; a < fetch.Object.FieldCount; a++)
                            {
                                var name = fetch.Object.GetName(a);
                                if (dic.ContainsKey(name)) continue;
                                sbflag.Append(name).Append(":").Append(a).Append(",");
                                dic.Add(name, a);
                            }
                            indexes1 = props1.Select(a => dic.TryGetValue(a.Key, out var tryint) ? tryint : -1).ToArray();
                            flag1 = sbflag.ToString();
                        }
                        ret1.Add((T1)Utils.ExecuteArrayRowReadClassOrTuple(flag1, type1, indexes1, fetch.Object, 0, _util).Value);
                        break;
                    case 1:
                        if (indexes2 == null)
                        {
                            var sbflag = new StringBuilder().Append("adoQuery");
                            var dic = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
                            for (var a = 0; a < fetch.Object.FieldCount; a++)
                            {
                                var name = fetch.Object.GetName(a);
                                if (dic.ContainsKey(name)) continue;
                                sbflag.Append(name).Append(":").Append(a).Append(",");
                                dic.Add(name, a);
                            }
                            indexes2 = props2.Select(a => dic.TryGetValue(a.Key, out var tryint) ? tryint : -1).ToArray();
                            flag2 = sbflag.ToString();
                        }
                        ret2.Add((T2)Utils.ExecuteArrayRowReadClassOrTuple(flag2, type2, indexes2, fetch.Object, 0, _util).Value);
                        break;
                }
            }, null, cmdType, cmdText, cmdTimeout, cmdParms);
            return NativeTuple.Create(ret1, ret2);
        }

        public NativeTuple<List<T1>, List<T2>, List<T3>> Query<T1, T2, T3>(string cmdText, object parms = null) => Query<T1, T2, T3>(null, null, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms));
        public NativeTuple<List<T1>, List<T2>, List<T3>> Query<T1, T2, T3>(DbTransaction transaction, string cmdText, object parms = null) => Query<T1, T2, T3>(null, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms));
        public NativeTuple<List<T1>, List<T2>, List<T3>> Query<T1, T2, T3>(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null) => Query<T1, T2, T3>(connection, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms));
        public NativeTuple<List<T1>, List<T2>, List<T3>> Query<T1, T2, T3>(CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => Query<T1, T2, T3>(null, null, cmdType, cmdText, 0, cmdParms);
        public NativeTuple<List<T1>, List<T2>, List<T3>> Query<T1, T2, T3>(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => Query<T1, T2, T3>(null, transaction, cmdType, cmdText, 0, cmdParms);
        public NativeTuple<List<T1>, List<T2>, List<T3>> Query<T1, T2, T3>(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, params DbParameter[] cmdParms)
        {
            if (string.IsNullOrEmpty(cmdText)) return NativeTuple.Create(new List<T1>(), new List<T2>(), new List<T3>());
            var ret1 = new List<T1>();
            var type1 = typeof(T1);
            string flag1 = null;
            int[] indexes1 = null;
            var props1 = GetQueryTypeProperties(type1);

            var ret2 = new List<T2>();
            var type2 = typeof(T2);
            string flag2 = null;
            int[] indexes2 = null;
            var props2 = GetQueryTypeProperties(type2);

            var ret3 = new List<T3>();
            var type3 = typeof(T3);
            string flag3 = null;
            int[] indexes3 = null;
            var props3 = GetQueryTypeProperties(type3);
            ExecuteReaderMultiple(3, connection, transaction, (fetch, result) =>
            {
                switch (result)
                {
                    case 0:
                        if (indexes1 == null)
                        {
                            var sbflag = new StringBuilder().Append("adoQuery");
                            var dic = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
                            for (var a = 0; a < fetch.Object.FieldCount; a++)
                            {
                                var name = fetch.Object.GetName(a);
                                if (dic.ContainsKey(name)) continue;
                                sbflag.Append(name).Append(":").Append(a).Append(",");
                                dic.Add(name, a);
                            }
                            indexes1 = props1.Select(a => dic.TryGetValue(a.Key, out var tryint) ? tryint : -1).ToArray();
                            flag1 = sbflag.ToString();
                        }
                        ret1.Add((T1)Utils.ExecuteArrayRowReadClassOrTuple(flag1, type1, indexes1, fetch.Object, 0, _util).Value);
                        break;
                    case 1:
                        if (indexes2 == null)
                        {
                            var sbflag = new StringBuilder().Append("adoQuery");
                            var dic = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
                            for (var a = 0; a < fetch.Object.FieldCount; a++)
                            {
                                var name = fetch.Object.GetName(a);
                                if (dic.ContainsKey(name)) continue;
                                sbflag.Append(name).Append(":").Append(a).Append(",");
                                dic.Add(name, a);
                            }
                            indexes2 = props2.Select(a => dic.TryGetValue(a.Key, out var tryint) ? tryint : -1).ToArray();
                            flag2 = sbflag.ToString();
                        }
                        ret2.Add((T2)Utils.ExecuteArrayRowReadClassOrTuple(flag2, type2, indexes2, fetch.Object, 0, _util).Value);
                        break;
                    case 2:
                        if (indexes3 == null)
                        {
                            var sbflag = new StringBuilder().Append("adoQuery");
                            var dic = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
                            for (var a = 0; a < fetch.Object.FieldCount; a++)
                            {
                                var name = fetch.Object.GetName(a);
                                if (dic.ContainsKey(name)) continue;
                                sbflag.Append(name).Append(":").Append(a).Append(",");
                                dic.Add(name, a);
                            }
                            indexes3 = props3.Select(a => dic.TryGetValue(a.Key, out var tryint) ? tryint : -1).ToArray();
                            flag3 = sbflag.ToString();
                        }
                        ret3.Add((T3)Utils.ExecuteArrayRowReadClassOrTuple(flag3, type3, indexes3, fetch.Object, 0, _util).Value);
                        break;
                }
            }, null, cmdType, cmdText, cmdTimeout, cmdParms);
            return NativeTuple.Create(ret1, ret2, ret3);
        }

        public NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>> Query<T1, T2, T3, T4>(string cmdText, object parms = null) => Query<T1, T2, T3, T4>(null, null, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms));
        public NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>> Query<T1, T2, T3, T4>(DbTransaction transaction, string cmdText, object parms = null) => Query<T1, T2, T3, T4>(null, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms));
        public NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>> Query<T1, T2, T3, T4>(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null) => Query<T1, T2, T3, T4>(connection, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms));
        public NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>> Query<T1, T2, T3, T4>(CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => Query<T1, T2, T3, T4>(null, null, cmdType, cmdText, 0, cmdParms);
        public NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>> Query<T1, T2, T3, T4>(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => Query<T1, T2, T3, T4>(null, transaction, cmdType, cmdText, 0, cmdParms);
        public NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>> Query<T1, T2, T3, T4>(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, params DbParameter[] cmdParms)
        {
            if (string.IsNullOrEmpty(cmdText)) return NativeTuple.Create(new List<T1>(), new List<T2>(), new List<T3>(), new List<T4>());
            var ret1 = new List<T1>();
            var type1 = typeof(T1);
            string flag1 = null;
            int[] indexes1 = null;
            var props1 = GetQueryTypeProperties(type1);

            var ret2 = new List<T2>();
            var type2 = typeof(T2);
            string flag2 = null;
            int[] indexes2 = null;
            var props2 = GetQueryTypeProperties(type2);

            var ret3 = new List<T3>();
            var type3 = typeof(T3);
            string flag3 = null;
            int[] indexes3 = null;
            var props3 = GetQueryTypeProperties(type3);

            var ret4 = new List<T4>();
            var type4 = typeof(T4);
            string flag4 = null;
            int[] indexes4 = null;
            var props4 = GetQueryTypeProperties(type4);
            ExecuteReaderMultiple(4, connection, transaction, (fetch, result) =>
            {
                switch (result)
                {
                    case 0:
                        if (indexes1 == null)
                        {
                            var sbflag = new StringBuilder().Append("adoQuery");
                            var dic = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
                            for (var a = 0; a < fetch.Object.FieldCount; a++)
                            {
                                var name = fetch.Object.GetName(a);
                                if (dic.ContainsKey(name)) continue;
                                sbflag.Append(name).Append(":").Append(a).Append(",");
                                dic.Add(name, a);
                            }
                            indexes1 = props1.Select(a => dic.TryGetValue(a.Key, out var tryint) ? tryint : -1).ToArray();
                            flag1 = sbflag.ToString();
                        }
                        ret1.Add((T1)Utils.ExecuteArrayRowReadClassOrTuple(flag1, type1, indexes1, fetch.Object, 0, _util).Value);
                        break;
                    case 1:
                        if (indexes2 == null)
                        {
                            var sbflag = new StringBuilder().Append("adoQuery");
                            var dic = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
                            for (var a = 0; a < fetch.Object.FieldCount; a++)
                            {
                                var name = fetch.Object.GetName(a);
                                if (dic.ContainsKey(name)) continue;
                                sbflag.Append(name).Append(":").Append(a).Append(",");
                                dic.Add(name, a);
                            }
                            indexes2 = props2.Select(a => dic.TryGetValue(a.Key, out var tryint) ? tryint : -1).ToArray();
                            flag2 = sbflag.ToString();
                        }
                        ret2.Add((T2)Utils.ExecuteArrayRowReadClassOrTuple(flag2, type2, indexes2, fetch.Object, 0, _util).Value);
                        break;
                    case 2:
                        if (indexes3 == null)
                        {
                            var sbflag = new StringBuilder().Append("adoQuery");
                            var dic = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
                            for (var a = 0; a < fetch.Object.FieldCount; a++)
                            {
                                var name = fetch.Object.GetName(a);
                                if (dic.ContainsKey(name)) continue;
                                sbflag.Append(name).Append(":").Append(a).Append(",");
                                dic.Add(name, a);
                            }
                            indexes3 = props3.Select(a => dic.TryGetValue(a.Key, out var tryint) ? tryint : -1).ToArray();
                            flag3 = sbflag.ToString();
                        }
                        ret3.Add((T3)Utils.ExecuteArrayRowReadClassOrTuple(flag3, type3, indexes3, fetch.Object, 0, _util).Value);
                        break;
                    case 3:
                        if (indexes4 == null)
                        {
                            var sbflag = new StringBuilder().Append("adoQuery");
                            var dic = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
                            for (var a = 0; a < fetch.Object.FieldCount; a++)
                            {
                                var name = fetch.Object.GetName(a);
                                if (dic.ContainsKey(name)) continue;
                                sbflag.Append(name).Append(":").Append(a).Append(",");
                                dic.Add(name, a);
                            }
                            indexes4 = props4.Select(a => dic.TryGetValue(a.Key, out var tryint) ? tryint : -1).ToArray();
                            flag4 = sbflag.ToString();
                        }
                        ret4.Add((T4)Utils.ExecuteArrayRowReadClassOrTuple(flag4, type4, indexes4, fetch.Object, 0, _util).Value);
                        break;
                }
            }, null, cmdType, cmdText, cmdTimeout, cmdParms);
            return NativeTuple.Create(ret1, ret2, ret3, ret4);
        }

        public NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>> Query<T1, T2, T3, T4, T5>(string cmdText, object parms = null) => Query<T1, T2, T3, T4, T5>(null, null, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms));
        public NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>> Query<T1, T2, T3, T4, T5>(DbTransaction transaction, string cmdText, object parms = null) => Query<T1, T2, T3, T4, T5>(null, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms));
        public NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>> Query<T1, T2, T3, T4, T5>(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null) => Query<T1, T2, T3, T4, T5>(connection, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms));
        public NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>> Query<T1, T2, T3, T4, T5>(CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => Query<T1, T2, T3, T4, T5>(null, null, cmdType, cmdText, 0, cmdParms);
        public NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>> Query<T1, T2, T3, T4, T5>(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => Query<T1, T2, T3, T4, T5>(null, transaction, cmdType, cmdText, 0, cmdParms);
        public NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>> Query<T1, T2, T3, T4, T5>(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, params DbParameter[] cmdParms)
        {
            if (string.IsNullOrEmpty(cmdText)) return NativeTuple.Create(new List<T1>(), new List<T2>(), new List<T3>(), new List<T4>(), new List<T5>());
            var ret1 = new List<T1>();
            var type1 = typeof(T1);
            string flag1 = null;
            int[] indexes1 = null;
            var props1 = GetQueryTypeProperties(type1);

            var ret2 = new List<T2>();
            var type2 = typeof(T2);
            string flag2 = null;
            int[] indexes2 = null;
            var props2 = GetQueryTypeProperties(type2);

            var ret3 = new List<T3>();
            var type3 = typeof(T3);
            string flag3 = null;
            int[] indexes3 = null;
            var props3 = GetQueryTypeProperties(type3);

            var ret4 = new List<T4>();
            var type4 = typeof(T4);
            string flag4 = null;
            int[] indexes4 = null;
            var props4 = GetQueryTypeProperties(type4);

            var ret5 = new List<T5>();
            var type5 = typeof(T5);
            string flag5 = null;
            int[] indexes5 = null;
            var props5 = GetQueryTypeProperties(type5);
            ExecuteReaderMultiple(5, connection, transaction, (fetch, result) =>
            {
                switch (result)
                {
                    case 0:
                        if (indexes1 == null)
                        {
                            var sbflag = new StringBuilder().Append("adoQuery");
                            var dic = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
                            for (var a = 0; a < fetch.Object.FieldCount; a++)
                            {
                                var name = fetch.Object.GetName(a);
                                if (dic.ContainsKey(name)) continue;
                                sbflag.Append(name).Append(":").Append(a).Append(",");
                                dic.Add(name, a);
                            }
                            indexes1 = props1.Select(a => dic.TryGetValue(a.Key, out var tryint) ? tryint : -1).ToArray();
                            flag1 = sbflag.ToString();
                        }
                        ret1.Add((T1)Utils.ExecuteArrayRowReadClassOrTuple(flag1, type1, indexes1, fetch.Object, 0, _util).Value);
                        break;
                    case 1:
                        if (indexes2 == null)
                        {
                            var sbflag = new StringBuilder().Append("adoQuery");
                            var dic = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
                            for (var a = 0; a < fetch.Object.FieldCount; a++)
                            {
                                var name = fetch.Object.GetName(a);
                                if (dic.ContainsKey(name)) continue;
                                sbflag.Append(name).Append(":").Append(a).Append(",");
                                dic.Add(name, a);
                            }
                            indexes2 = props2.Select(a => dic.TryGetValue(a.Key, out var tryint) ? tryint : -1).ToArray();
                            flag2 = sbflag.ToString();
                        }
                        ret2.Add((T2)Utils.ExecuteArrayRowReadClassOrTuple(flag2, type2, indexes2, fetch.Object, 0, _util).Value);
                        break;
                    case 2:
                        if (indexes3 == null)
                        {
                            var sbflag = new StringBuilder().Append("adoQuery");
                            var dic = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
                            for (var a = 0; a < fetch.Object.FieldCount; a++)
                            {
                                var name = fetch.Object.GetName(a);
                                if (dic.ContainsKey(name)) continue;
                                sbflag.Append(name).Append(":").Append(a).Append(",");
                                dic.Add(name, a);
                            }
                            indexes3 = props3.Select(a => dic.TryGetValue(a.Key, out var tryint) ? tryint : -1).ToArray();
                            flag3 = sbflag.ToString();
                        }
                        ret3.Add((T3)Utils.ExecuteArrayRowReadClassOrTuple(flag3, type3, indexes3, fetch.Object, 0, _util).Value);
                        break;
                    case 3:
                        if (indexes4 == null)
                        {
                            var sbflag = new StringBuilder().Append("adoQuery");
                            var dic = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
                            for (var a = 0; a < fetch.Object.FieldCount; a++)
                            {
                                var name = fetch.Object.GetName(a);
                                if (dic.ContainsKey(name)) continue;
                                sbflag.Append(name).Append(":").Append(a).Append(",");
                                dic.Add(name, a);
                            }
                            indexes4 = props4.Select(a => dic.TryGetValue(a.Key, out var tryint) ? tryint : -1).ToArray();
                            flag4 = sbflag.ToString();
                        }
                        ret4.Add((T4)Utils.ExecuteArrayRowReadClassOrTuple(flag4, type4, indexes4, fetch.Object, 0, _util).Value);
                        break;
                    case 4:
                        if (indexes5 == null)
                        {
                            var sbflag = new StringBuilder().Append("adoQuery");
                            var dic = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
                            for (var a = 0; a < fetch.Object.FieldCount; a++)
                            {
                                var name = fetch.Object.GetName(a);
                                if (dic.ContainsKey(name)) continue;
                                sbflag.Append(name).Append(":").Append(a).Append(",");
                                dic.Add(name, a);
                            }
                            indexes5 = props5.Select(a => dic.TryGetValue(a.Key, out var tryint) ? tryint : -1).ToArray();
                            flag5 = sbflag.ToString();
                        }
                        ret5.Add((T5)Utils.ExecuteArrayRowReadClassOrTuple(flag5, type5, indexes5, fetch.Object, 0, _util).Value);
                        break;
                }
            }, null, cmdType, cmdText, cmdTimeout, cmdParms);
            return NativeTuple.Create(ret1, ret2, ret3, ret4, ret5);
        }
        #endregion

        public void ExecuteReader(Action<FetchCallbackArgs<DbDataReader>> fetchHandler, string cmdText, object parms = null) => ExecuteReader(null, null, fetchHandler, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms));
        public void ExecuteReader(DbTransaction transaction, Action<FetchCallbackArgs<DbDataReader>> fetchHandler, string cmdText, object parms = null) => ExecuteReader(null, transaction, fetchHandler, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms));
        public void ExecuteReader(DbConnection connection, DbTransaction transaction, Action<FetchCallbackArgs<DbDataReader>> fetchHandler, string cmdText, object parms = null) => ExecuteReader(connection, transaction, fetchHandler, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms));
        public void ExecuteReader(Action<FetchCallbackArgs<DbDataReader>> fetchHandler, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => ExecuteReader(null, null, fetchHandler, cmdType, cmdText, 0, cmdParms);
        public void ExecuteReader(DbTransaction transaction, Action<FetchCallbackArgs<DbDataReader>> fetchHandler, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => ExecuteReader(null, transaction, fetchHandler, cmdType, cmdText, 0, cmdParms);
        public void ExecuteReader(DbConnection connection, DbTransaction transaction, Action<FetchCallbackArgs<DbDataReader>> fetchHandler, CommandType cmdType, string cmdText, int cmdTimeout, params DbParameter[] cmdParms) => ExecuteReaderMultiple(1, connection, transaction, (fetch, result) => fetchHandler(fetch), null, cmdType, cmdText, cmdTimeout, cmdParms);
        void ExecuteReaderMultiple(int multipleResult, DbConnection connection, DbTransaction transaction, Action<FetchCallbackArgs<DbDataReader>, int> fetchHandler, Action<DbDataReader, int> schemaHandler, CommandType cmdType, string cmdText, int cmdTimeout, params DbParameter[] cmdParms)
        {
            if (string.IsNullOrEmpty(cmdText)) return;
            var dt = DateTime.Now;
            var logtxt = new StringBuilder();
            var logtxt_dt = DateTime.Now;
            var pool = this.MasterPool;
            var isSlave = false;

            if (transaction == null && connection == null)
            {
                //读写分离规则
                if (this.SlavePools.Any() && IsFromSlave(cmdText))
                {
                    var availables = slaveUnavailables == 0 ?
                        //查从库
                        this.SlavePools : (
                        //查主库
                        slaveUnavailables == this.SlavePools.Count ? new List<IObjectPool<DbConnection>>() :
                        //查从库可用
                        this.SlavePools.Where(sp => sp.IsAvailable).ToList());
                    if (availables.Any())
                    {
                        isSlave = true;
                        pool = availables.Count == 1 ? availables[0] : availables[slaveRandom.Next(availables.Count)];
                    }
                }
            }

            Object<DbConnection> conn = null;
            var pc = PrepareCommand(connection, transaction, cmdType, cmdText, cmdTimeout, cmdParms, logtxt);
            if (IsTracePerformance)
            {
                logtxt.Append("PrepareCommand: ").Append(DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds).Append("ms Total: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms\r\n");
                logtxt_dt = DateTime.Now;
            }
            Exception ex = null;
            try
            {
                if (isSlave)
                {
                    //从库查询切换，恢复
                    bool isSlaveFail = false;
                    try
                    {
                        if (pc.cmd.Connection == null) pc.cmd.Connection = (conn = pool.Get()).Value;
                        //if (slaveRandom.Next(100) % 2 == 0) throw new Exception("测试从库抛出异常");
                    }
                    catch
                    {
                        isSlaveFail = true;
                    }
                    if (isSlaveFail)
                    {
                        if (conn != null)
                        {
                            if (IsTracePerformance) logtxt_dt = DateTime.Now;
                            ReturnConnection(pool, conn, ex); //pool.Return(conn, ex);
                            if (IsTracePerformance) logtxt.Append("Pool.Return: ").Append(DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds).Append("ms Total: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms");
                        }
                        LoggerException(pool, pc, new Exception($"连接失败，准备切换其他可用服务器"), dt, logtxt, false);
                        pc.cmd.Parameters.Clear();
                        if (DataType == DataType.Sqlite) pc.cmd.Dispose();
                        ExecuteReaderMultiple(multipleResult, connection, transaction, fetchHandler, schemaHandler, cmdType, cmdText, cmdTimeout, cmdParms);
                        return;
                    }
                }
                else
                {
                    //主库查询
                    if (pc.cmd.Connection == null) pc.cmd.Connection = (conn = pool.Get()).Value;
                }
                if (IsTracePerformance)
                {
                    logtxt.Append("Pool.Get: ").Append(DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds).Append("ms Total: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms\r\n");
                    logtxt_dt = DateTime.Now;
                }
                using (var dr = pc.cmd.ExecuteReader())
                {
                    int resultIndex = 0;
                    var fetch = new FetchCallbackArgs<DbDataReader> { Object = dr };
                    while (true)
                    {
                        bool isfirst = true;
                        while (true)
                        {
                            bool isread = dr.Read();
                            if (schemaHandler != null && isfirst)
                            {
                                isfirst = false;
                                schemaHandler(dr, resultIndex);
                            }
                            if (isread == false) break;

                            if (fetchHandler != null)
                            {
                                fetchHandler(fetch, resultIndex);
                                if (fetch.IsBreak)
                                {
                                    resultIndex = multipleResult;
                                    break;
                                }
                            }
                        }
                        if (++resultIndex >= multipleResult || dr.NextResult() == false) break;
                    }
                    dr.Close();
                }
                if (IsTracePerformance)
                {
                    logtxt.Append("ExecuteReader: ").Append(DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds).Append("ms Total: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms\r\n");
                    logtxt_dt = DateTime.Now;
                }
            }
            catch (Exception ex2)
            {
                ex = ex2;
            }

            if (conn != null)
            {
                ReturnConnection(pool, conn, ex); //pool.Return(conn, ex);
                if (IsTracePerformance)
                {
                    logtxt.Append("Pool.Return: ").Append(DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds).Append("ms Total: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms");
                    logtxt_dt = DateTime.Now;
                }
            }
            LoggerException(pool, pc, ex, dt, logtxt);
            pc.cmd.Parameters.Clear();
            if (DataType == DataType.Sqlite) pc.cmd.Dispose();
        }
        public object[][] ExecuteArray(string cmdText, object parms = null) => ExecuteArray(null, null, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms));
        public object[][] ExecuteArray(DbTransaction transaction, string cmdText, object parms = null) => ExecuteArray(null, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms));
        public object[][] ExecuteArray(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null) => ExecuteArray(connection, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms));
        public object[][] ExecuteArray(CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => ExecuteArray(null, null, cmdType, cmdText, 0, cmdParms);
        public object[][] ExecuteArray(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => ExecuteArray(null, transaction, cmdType, cmdText, 0, cmdParms);
        public object[][] ExecuteArray(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, params DbParameter[] cmdParms)
        {
            List<object[]> ret = new List<object[]>();
            ExecuteReader(connection, transaction, fetch =>
            {
                object[] values = new object[fetch.Object.FieldCount];
                fetch.Object.GetValues(values);
                ret.Add(values);
            }, cmdType, cmdText, cmdTimeout, cmdParms);
            return ret.ToArray();
        }
        public DataSet ExecuteDataSet(string cmdText, object parms = null) => ExecuteDataSet(null, null, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms));
        public DataSet ExecuteDataSet(DbTransaction transaction, string cmdText, object parms = null) => ExecuteDataSet(null, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms));
        public DataSet ExecuteDataSet(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null) => ExecuteDataSet(connection, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms));
        public DataSet ExecuteDataSet(CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => ExecuteDataSet(null, null, cmdType, cmdText, 0, cmdParms);
        public DataSet ExecuteDataSet(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => ExecuteDataSet(null, transaction, cmdType, cmdText, 0, cmdParms);
        public DataSet ExecuteDataSet(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, params DbParameter[] cmdParms)
        {
            var ret = new DataSet();
            DataTable dt = null;
            ExecuteReaderMultiple(16, connection, transaction, (fetch, result) =>
            {
                object[] values = new object[dt.Columns.Count];
                fetch.Object.GetValues(values);
                dt.Rows.Add(values);
            }, (dr, result) =>
            {
                dt = ret.Tables.Add();
                for (var a = 0; a < dr.FieldCount; a++)
                {
                    var name = dr.GetName(a);
                    if (dt.Columns.Contains(name)) name = $"{name}_{Guid.NewGuid().ToString("N").Substring(0, 4)}";
                    dt.Columns.Add(name, dr.GetFieldType(a));
                }
            }, cmdType, cmdText, cmdTimeout, cmdParms);
            return ret;
        }
        public DataTable ExecuteDataTable(string cmdText, object parms = null) => ExecuteDataTable(null, null, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms));
        public DataTable ExecuteDataTable(DbTransaction transaction, string cmdText, object parms = null) => ExecuteDataTable(null, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms));
        public DataTable ExecuteDataTable(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null) => ExecuteDataTable(connection, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms));
        public DataTable ExecuteDataTable(CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => ExecuteDataTable(null, null, cmdType, cmdText, 0, cmdParms);
        public DataTable ExecuteDataTable(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => ExecuteDataTable(null, transaction, cmdType, cmdText, 0, cmdParms);
        public DataTable ExecuteDataTable(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, params DbParameter[] cmdParms)
        {
            var ret = new DataTable();
            ExecuteReaderMultiple(1, connection, transaction, (fetch, result) =>
            {
                object[] values = new object[ret.Columns.Count];
                fetch.Object.GetValues(values);
                ret.Rows.Add(values);
            }, (dr, result) =>
            {
                for (var a = 0; a < dr.FieldCount; a++)
                {
                    var name = dr.GetName(a);
                    if (ret.Columns.Contains(name)) name = $"{name}_{Guid.NewGuid().ToString("N").Substring(0, 4)}";
                    ret.Columns.Add(name, dr.GetFieldType(a));
                }
            }, cmdType, cmdText, cmdTimeout, cmdParms);
            return ret;
        }
        public int ExecuteNonQuery(string cmdText, object parms = null) => ExecuteNonQuery(null, null, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms));
        public int ExecuteNonQuery(DbTransaction transaction, string cmdText, object parms = null) => ExecuteNonQuery(null, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms));
        public int ExecuteNonQuery(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null) => ExecuteNonQuery(connection, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms));
        public int ExecuteNonQuery(CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => ExecuteNonQuery(null, null, cmdType, cmdText, 0, cmdParms);
        public int ExecuteNonQuery(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => ExecuteNonQuery(null, transaction, cmdType, cmdText, 0, cmdParms);
        public int ExecuteNonQuery(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, params DbParameter[] cmdParms)
        {
            if (string.IsNullOrEmpty(cmdText)) return 0;
            var dt = DateTime.Now;
            var logtxt = new StringBuilder();
            var logtxt_dt = DateTime.Now;
            Object<DbConnection> conn = null;
            var pc = PrepareCommand(connection, transaction, cmdType, cmdText, cmdTimeout, cmdParms, logtxt);
            int val = 0;
            Exception ex = null;
            try
            {
                if (pc.cmd.Connection == null) pc.cmd.Connection = (conn = this.MasterPool.Get()).Value;
                val = pc.cmd.ExecuteNonQuery();
            }
            catch (Exception ex2)
            {
                ex = ex2;
            }

            if (conn != null)
            {
                if (IsTracePerformance) logtxt_dt = DateTime.Now;
                ReturnConnection(MasterPool, conn, ex); //this.MasterPool.Return(conn, ex);
                if (IsTracePerformance) logtxt.Append("Pool.Return: ").Append(DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds).Append("ms Total: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms");
            }
            LoggerException(this.MasterPool, pc, ex, dt, logtxt);
            pc.cmd.Parameters.Clear();
            if (DataType == DataType.Sqlite) pc.cmd.Dispose();
            return val;
        }
        public object ExecuteScalar(string cmdText, object parms = null) => ExecuteScalar(null, null, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms));
        public object ExecuteScalar(DbTransaction transaction, string cmdText, object parms = null) => ExecuteScalar(null, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms));
        public object ExecuteScalar(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null) => ExecuteScalar(connection, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms));
        public object ExecuteScalar(CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => ExecuteScalar(null, null, cmdType, cmdText, 0, cmdParms);
        public object ExecuteScalar(DbTransaction transaction, CommandType cmdType, string cmdText, params DbParameter[] cmdParms) => ExecuteScalar(null, transaction, cmdType, cmdText, 0, cmdParms);
        public object ExecuteScalar(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, params DbParameter[] cmdParms)
        {
            if (string.IsNullOrEmpty(cmdText)) return null;
            var dt = DateTime.Now;
            var logtxt = new StringBuilder();
            var logtxt_dt = DateTime.Now;
            Object<DbConnection> conn = null;
            var pc = PrepareCommand(connection, transaction, cmdType, cmdText, cmdTimeout, cmdParms, logtxt);
            object val = null;
            Exception ex = null;
            try
            {
                if (pc.cmd.Connection == null) pc.cmd.Connection = (conn = this.MasterPool.Get()).Value;
                val = pc.cmd.ExecuteScalar();
            }
            catch (Exception ex2)
            {
                ex = ex2;
            }

            if (conn != null)
            {
                if (IsTracePerformance) logtxt_dt = DateTime.Now;
                ReturnConnection(MasterPool, conn, ex); //this.MasterPool.Return(conn, ex);
                if (IsTracePerformance) logtxt.Append("Pool.Return: ").Append(DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds).Append("ms Total: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms");
            }
            LoggerException(this.MasterPool, pc, ex, dt, logtxt);
            pc.cmd.Parameters.Clear();
            if (DataType == DataType.Sqlite) pc.cmd.Dispose();
            return val;
        }

        class PrepareCommandResult
        {
            public Aop.CommandBeforeEventArgs before { get; }
            public DbCommand cmd { get; }
            public bool isclose { get; }
            public PrepareCommandResult(Aop.CommandBeforeEventArgs before, DbCommand cmd, bool isclose)
            {
                this.before = before;
                this.cmd = cmd;
                this.isclose = isclose;
            }
        }
        PrepareCommandResult PrepareCommand(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, DbParameter[] cmdParms, StringBuilder logtxt)
        {
            var dt = DateTime.Now;
            DbCommand cmd = CreateCommand();
            bool isclose = false;
            cmd.CommandType = cmdType;
            cmd.CommandText = cmdText;
            if (cmdTimeout > 0) cmd.CommandTimeout = cmdTimeout;

            if (cmdParms != null)
            {
                foreach (var parm in cmdParms)
                {
                    if (parm == null) continue;
                    var isnew = false;
                    if (parm.Value == null) parm.Value = DBNull.Value;
                    else
                    {
                        if (parm.Value is Array || parm.Value is IList)
                        {
                            cmd.CommandText = Regex.Replace(cmd.CommandText, @"\s+(in|In|IN|iN)\s+[\:\?\@]" + parm.ParameterName.TrimStart('@', '?', ':'), m =>
                            {
                                isnew = true;
                                var arr = parm.Value as IEnumerable;
                                if (arr == null) return " IS NULL";
                                var vals = new List<object>();
                                foreach (var val in arr) vals.Add(val);
                                return $" in {_util.FormatSql("{0}", new object[] { vals })}";
                            });
                        }
                    }
                    if (isnew == false) cmd.Parameters.Add(parm);
                }
            }

            if (connection == null)
            {
                var tran = transaction ?? ResolveTransaction?.Invoke() ?? TransactionCurrentThread;
                if (tran != null && connection == null)
                {
                    cmd.Connection = tran.Connection;
                    cmd.Transaction = tran;
                }
            }
            else
            {
                if (connection.State != ConnectionState.Open)
                {
                    if (IsTracePerformance) dt = DateTime.Now;
                    connection.Open();
                    if (IsTracePerformance) logtxt.Append("	PrepareCommand_ConnectionOpen: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms\r\n");
                    isclose = true;
                }
                cmd.Connection = connection;
                if (transaction?.Connection == connection)
                    cmd.Transaction = transaction;
            }

            if (IsTracePerformance) dt = DateTime.Now;
            CommitTimeoutTransaction();
            if (IsTracePerformance) logtxt.Append("   CommitTimeoutTransaction: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms\r\n");

            var before = new Aop.CommandBeforeEventArgs(cmd);
            _util?._orm?.Aop.CommandBeforeHandler?.Invoke(_util._orm, before);
            return new PrepareCommandResult(before, cmd, isclose);
        }
    }
}
