using FreeSql.Internal.Model;
using FreeSql.Internal.ObjectPool;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

#if net40
#else
namespace FreeSql.Internal.CommonProvider
{
    partial class AdoProvider
    {
        async public Task<bool> ExecuteConnectTestAsync(int commandTimeout = 0, CancellationToken cancellationToken = default)
        {
            try
            {
                switch (DataType)
                {
                    case DataType.Oracle:
                    case DataType.OdbcOracle:
                        await ExecuteNonQueryAsync(null, null, CommandType.Text, " SELECT 1 FROM dual", commandTimeout, null, cancellationToken);
                        return true;
                    case DataType.Firebird:
                        await ExecuteNonQueryAsync(null, null, CommandType.Text, " SELECT FIRST 1 1 FROM rdb$database", commandTimeout, null, cancellationToken);
                        return true;
                }
                await ExecuteNonQueryAsync(null, null, CommandType.Text, " SELECT 1", commandTimeout, null, cancellationToken);
                return true;
            }
            catch
            {
                return false;
            }
        }

        async public Task<T> QuerySingleAsync<T>(string cmdText, object parms = null, CancellationToken cancellationToken = default) => (await QueryAsync<T>(cmdText, parms, cancellationToken)).FirstOrDefault();
        async public Task<T> QuerySingleAsync<T>(CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default) => (await QueryAsync<T>(cmdType, cmdText, cmdParms, cancellationToken)).FirstOrDefault();
        public Task<List<T>> QueryAsync<T>(string cmdText, object parms = null, CancellationToken cancellationToken = default) => QueryAsync<T>(null, null, null, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms), cancellationToken);
        public Task<List<T>> QueryAsync<T>(DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default) => QueryAsync<T>(null, null, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms), cancellationToken);
        public Task<List<T>> QueryAsync<T>(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default) => QueryAsync<T>(null, connection, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms), cancellationToken);
        public Task<List<T>> QueryAsync<T>(CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default) => QueryAsync<T>(null, null, null, cmdType, cmdText, 0, cmdParms, cancellationToken);
        public Task<List<T>> QueryAsync<T>(DbTransaction transaction, CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default) => QueryAsync<T>(null, null, transaction, cmdType, cmdText, 0, cmdParms, cancellationToken);
        public Task<List<T>> QueryAsync<T>(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, DbParameter[] cmdParms, CancellationToken cancellationToken = default) => QueryAsync<T>(null, connection, transaction, cmdType, cmdText, cmdTimeout, cmdParms, cancellationToken);
        async public Task<List<T>> QueryAsync<T>(Type resultType, DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, DbParameter[] cmdParms, CancellationToken cancellationToken = default)
        {
            var ret = new List<T>();
            if (string.IsNullOrEmpty(cmdText)) return ret;
            var type = typeof(T);
            if (resultType != null && type != resultType) type = resultType;
            string flag = null;
            int[] indexes = null;
            var props = GetQueryTypeProperties(type);
            await ExecuteReaderAsync(connection, transaction, fetch =>
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
                return Task.FromResult(false);
            }, cmdType, cmdText, cmdTimeout, cmdParms, cancellationToken);
            return ret;
        }
        #region QueryAsync multi
        public Task<NativeTuple<List<T1>, List<T2>>> QueryAsync<T1, T2>(string cmdText, object parms = null, CancellationToken cancellationToken = default) => QueryAsync<T1, T2>(null, null, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms), cancellationToken);
        public Task<NativeTuple<List<T1>, List<T2>>> QueryAsync<T1, T2>(DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default) => QueryAsync<T1, T2>(null, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms), cancellationToken);
        public Task<NativeTuple<List<T1>, List<T2>>> QueryAsync<T1, T2>(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default) => QueryAsync<T1, T2>(connection, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms), cancellationToken);
        public Task<NativeTuple<List<T1>, List<T2>>> QueryAsync<T1, T2>(CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default) => QueryAsync<T1, T2>(null, null, cmdType, cmdText, 0, cmdParms, cancellationToken);
        public Task<NativeTuple<List<T1>, List<T2>>> QueryAsync<T1, T2>(DbTransaction transaction, CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default) => QueryAsync<T1, T2>(null, transaction, cmdType, cmdText, 0, cmdParms, cancellationToken);
        async public Task<NativeTuple<List<T1>, List<T2>>> QueryAsync<T1, T2>(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, DbParameter[] cmdParms, CancellationToken cancellationToken = default)
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
            await ExecuteReaderMultipleAsync(2, connection, transaction, (fetch, result) =>
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
                return Task.FromResult(false);
            }, null, cmdType, cmdText, cmdTimeout, cmdParms, cancellationToken);
            return NativeTuple.Create(ret1, ret2);
        }

        public Task<NativeTuple<List<T1>, List<T2>, List<T3>>> QueryAsync<T1, T2, T3>(string cmdText, object parms = null, CancellationToken cancellationToken = default) => QueryAsync<T1, T2, T3>(null, null, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms), cancellationToken);
        public Task<NativeTuple<List<T1>, List<T2>, List<T3>>> QueryAsync<T1, T2, T3>(DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default) => QueryAsync<T1, T2, T3>(null, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms), cancellationToken);
        public Task<NativeTuple<List<T1>, List<T2>, List<T3>>> QueryAsync<T1, T2, T3>(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default) => QueryAsync<T1, T2, T3>(connection, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms), cancellationToken);
        public Task<NativeTuple<List<T1>, List<T2>, List<T3>>> QueryAsync<T1, T2, T3>(CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default) => QueryAsync<T1, T2, T3>(null, null, cmdType, cmdText, 0, cmdParms, cancellationToken);
        public Task<NativeTuple<List<T1>, List<T2>, List<T3>>> QueryAsync<T1, T2, T3>(DbTransaction transaction, CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default) => QueryAsync<T1, T2, T3>(null, transaction, cmdType, cmdText, 0, cmdParms, cancellationToken);
        async public Task<NativeTuple<List<T1>, List<T2>, List<T3>>> QueryAsync<T1, T2, T3>(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, DbParameter[] cmdParms, CancellationToken cancellationToken = default)
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
            await ExecuteReaderMultipleAsync(3, connection, transaction, (fetch, result) =>
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
                return Task.FromResult(false);
            }, null, cmdType, cmdText, cmdTimeout, cmdParms, cancellationToken);
            return NativeTuple.Create(ret1, ret2, ret3);
        }

        public Task<NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>>> QueryAsync<T1, T2, T3, T4>(string cmdText, object parms = null, CancellationToken cancellationToken = default) => QueryAsync<T1, T2, T3, T4>(null, null, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms), cancellationToken);
        public Task<NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>>> QueryAsync<T1, T2, T3, T4>(DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default) => QueryAsync<T1, T2, T3, T4>(null, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms), cancellationToken);
        public Task<NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>>> QueryAsync<T1, T2, T3, T4>(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default) => QueryAsync<T1, T2, T3, T4>(connection, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms), cancellationToken);
        public Task<NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>>> QueryAsync<T1, T2, T3, T4>(CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default) => QueryAsync<T1, T2, T3, T4>(null, null, cmdType, cmdText, 0, cmdParms, cancellationToken);
        public Task<NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>>> QueryAsync<T1, T2, T3, T4>(DbTransaction transaction, CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default) => QueryAsync<T1, T2, T3, T4>(null, transaction, cmdType, cmdText, 0, cmdParms, cancellationToken);
        async public Task<NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>>> QueryAsync<T1, T2, T3, T4>(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, DbParameter[] cmdParms, CancellationToken cancellationToken = default)
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
            await ExecuteReaderMultipleAsync(4, connection, transaction, (fetch, result) =>
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
                return Task.FromResult(false);
            }, null, cmdType, cmdText, cmdTimeout, cmdParms, cancellationToken);
            return NativeTuple.Create(ret1, ret2, ret3, ret4);
        }

        public Task<NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>>> QueryAsync<T1, T2, T3, T4, T5>(string cmdText, object parms = null, CancellationToken cancellationToken = default) => QueryAsync<T1, T2, T3, T4, T5>(null, null, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms), cancellationToken);
        public Task<NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>>> QueryAsync<T1, T2, T3, T4, T5>(DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default) => QueryAsync<T1, T2, T3, T4, T5>(null, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms), cancellationToken);
        public Task<NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>>> QueryAsync<T1, T2, T3, T4, T5>(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default) => QueryAsync<T1, T2, T3, T4, T5>(connection, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms), cancellationToken);
        public Task<NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>>> QueryAsync<T1, T2, T3, T4, T5>(CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default) => QueryAsync<T1, T2, T3, T4, T5>(null, null, cmdType, cmdText, 0, cmdParms, cancellationToken);
        public Task<NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>>> QueryAsync<T1, T2, T3, T4, T5>(DbTransaction transaction, CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default) => QueryAsync<T1, T2, T3, T4, T5>(null, transaction, cmdType, cmdText, 0, cmdParms, cancellationToken);
        async public Task<NativeTuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>>> QueryAsync<T1, T2, T3, T4, T5>(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, DbParameter[] cmdParms, CancellationToken cancellationToken = default)
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
            await ExecuteReaderMultipleAsync(5, connection, transaction, (fetch, result) =>
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
                return Task.FromResult(false);
            }, null, cmdType, cmdText, cmdTimeout, cmdParms, cancellationToken);
            return NativeTuple.Create(ret1, ret2, ret3, ret4, ret5);
        }
        #endregion

        public Task ExecuteReaderAsync(Func<FetchCallbackArgs<DbDataReader>, Task> fetchHandler, string cmdText, object parms = null, CancellationToken cancellationToken = default) => ExecuteReaderAsync(null, null, fetchHandler, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms), cancellationToken);
        public Task ExecuteReaderAsync(DbTransaction transaction, Func<FetchCallbackArgs<DbDataReader>, Task> fetchHandler, string cmdText, object parms = null, CancellationToken cancellationToken = default) => ExecuteReaderAsync(null, transaction, fetchHandler, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms), cancellationToken);
        public Task ExecuteReaderAsync(DbConnection connection, DbTransaction transaction, Func<FetchCallbackArgs<DbDataReader>, Task> fetchHandler, string cmdText, object parms = null, CancellationToken cancellationToken = default) => ExecuteReaderAsync(connection, transaction, fetchHandler, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms), cancellationToken);
        public Task ExecuteReaderAsync(Func<FetchCallbackArgs<DbDataReader>, Task> fetchHandler, CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default) => ExecuteReaderAsync(null, null, fetchHandler, cmdType, cmdText, 0, cmdParms, cancellationToken);
        public Task ExecuteReaderAsync(DbTransaction transaction, Func<FetchCallbackArgs<DbDataReader>, Task> fetchHandler, CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default) => ExecuteReaderAsync(null, transaction, fetchHandler, cmdType, cmdText, 0, cmdParms, cancellationToken);
        public Task ExecuteReaderAsync(DbConnection connection, DbTransaction transaction, Func<FetchCallbackArgs<DbDataReader>, Task> fetchHandler, CommandType cmdType, string cmdText, int cmdTimeout, DbParameter[] cmdParms, CancellationToken cancellationToken = default) => ExecuteReaderMultipleAsync(1, connection, transaction, (fetch, result) => fetchHandler(fetch), null, cmdType, cmdText, cmdTimeout, cmdParms, cancellationToken);
        async Task ExecuteReaderMultipleAsync(int multipleResult, DbConnection connection, DbTransaction transaction, Func<FetchCallbackArgs<DbDataReader>, int, Task> fetchHandler, Action<DbDataReader, int> schemaHandler, CommandType cmdType, string cmdText, int cmdTimeout, DbParameter[] cmdParms, CancellationToken cancellationToken = default)
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
                        pool = availables.Count == 1 ? this.SlavePools[0] : availables[slaveRandom.Next(availables.Count)];
                    }
                }
            }

            Object<DbConnection> conn = null;
            var pc = await PrepareCommandAsync(connection, transaction, cmdType, cmdText, cmdTimeout, cmdParms, logtxt, cancellationToken);
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
                        if (pc.cmd.Connection == null) pc.cmd.Connection = (conn = await pool.GetAsync()).Value;
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
                        await ExecuteReaderMultipleAsync(multipleResult, connection, transaction, fetchHandler, schemaHandler, cmdType, cmdText, cmdTimeout, cmdParms, cancellationToken);
                        return;
                    }
                }
                else
                {
                    //主库查询
                    if (pc.cmd.Connection == null) pc.cmd.Connection = (conn = await pool.GetAsync()).Value;
                }
                if (IsTracePerformance)
                {
                    logtxt.Append("Pool.Get: ").Append(DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds).Append("ms Total: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms\r\n");
                    logtxt_dt = DateTime.Now;
                }
                using (var dr = await pc.cmd.ExecuteReaderAsync(cancellationToken))
                {
                    int resultIndex = 0;
                    var fetch = new FetchCallbackArgs<DbDataReader> { Object = dr };
                    while (true)
                    {
                        bool isfirst = true;
                        while (true)
                        {
                            bool isread = await dr.ReadAsync(cancellationToken);
                            if (schemaHandler != null && isfirst)
                            {
                                isfirst = false;
                                schemaHandler(dr, resultIndex);
                            }
                            if (isread == false) break;

                            if (fetchHandler != null)
                            {
                                await fetchHandler(fetch, resultIndex);
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
                if (IsTracePerformance) logtxt_dt = DateTime.Now;
                ReturnConnection(pool, conn, ex); //pool.Return(conn, ex);
                if (IsTracePerformance) logtxt.Append("Pool.Return: ").Append(DateTime.Now.Subtract(logtxt_dt).TotalMilliseconds).Append("ms Total: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms");
            }
            LoggerException(pool, pc, ex, dt, logtxt);
            pc.cmd.Parameters.Clear();
            if (DataType == DataType.Sqlite) pc.cmd.Dispose();
        }
        public Task<object[][]> ExecuteArrayAsync(string cmdText, object parms = null, CancellationToken cancellationToken = default) => ExecuteArrayAsync(null, null, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms), cancellationToken);
        public Task<object[][]> ExecuteArrayAsync(DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default) => ExecuteArrayAsync(null, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms), cancellationToken);
        public Task<object[][]> ExecuteArrayAsync(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default) => ExecuteArrayAsync(connection, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms), cancellationToken);
        public Task<object[][]> ExecuteArrayAsync(CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default) => ExecuteArrayAsync(null, null, cmdType, cmdText, 0, cmdParms, cancellationToken);
        public Task<object[][]> ExecuteArrayAsync(DbTransaction transaction, CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default) => ExecuteArrayAsync(null, transaction, cmdType, cmdText, 0, cmdParms, cancellationToken);
        async public Task<object[][]> ExecuteArrayAsync(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, DbParameter[] cmdParms, CancellationToken cancellationToken = default)
        {
            List<object[]> ret = new List<object[]>();
            await ExecuteReaderAsync(connection, transaction, async fetch =>
            {
                object[] values = new object[fetch.Object.FieldCount];
                for (int a = 0; a < values.Length; a++) if (!await fetch.Object.IsDBNullAsync(a, cancellationToken)) values[a] = await fetch.Object.GetFieldValueAsync<object>(a, cancellationToken);
                ret.Add(values);
            }, cmdType, cmdText, cmdTimeout, cmdParms, cancellationToken);
            return ret.ToArray();
        }

        public Task<DataSet> ExecuteDataSetAsync(string cmdText, object parms = null, CancellationToken cancellationToken = default) => ExecuteDataSetAsync(null, null, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms), cancellationToken);
        public Task<DataSet> ExecuteDataSetAsync(DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default) => ExecuteDataSetAsync(null, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms), cancellationToken);
        public Task<DataSet> ExecuteDataSetAsync(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default) => ExecuteDataSetAsync(connection, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms), cancellationToken);
        public Task<DataSet> ExecuteDataSetAsync(CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default) => ExecuteDataSetAsync(null, null, cmdType, cmdText, 0, cmdParms, cancellationToken);
        public Task<DataSet> ExecuteDataSetAsync(DbTransaction transaction, CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default) => ExecuteDataSetAsync(null, transaction, cmdType, cmdText, 0, cmdParms, cancellationToken);
        async public Task<DataSet> ExecuteDataSetAsync(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, DbParameter[] cmdParms, CancellationToken cancellationToken = default)
        {
            var ret = new DataSet();
            DataTable dt = null;
            await ExecuteReaderMultipleAsync(16, connection, transaction, async (fetch, result) =>
            {
                object[] values = new object[dt.Columns.Count];
                for (int a = 0; a < values.Length; a++) if (!await fetch.Object.IsDBNullAsync(a, cancellationToken)) values[a] = await fetch.Object.GetFieldValueAsync<object>(a, cancellationToken);
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
            }, cmdType, cmdText, cmdTimeout, cmdParms, cancellationToken);
            return ret;
        }
        public Task<DataTable> ExecuteDataTableAsync(string cmdText, object parms = null, CancellationToken cancellationToken = default) => ExecuteDataTableAsync(null, null, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms), cancellationToken);
        public Task<DataTable> ExecuteDataTableAsync(DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default) => ExecuteDataTableAsync(null, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms), cancellationToken);
        public Task<DataTable> ExecuteDataTableAsync(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default) => ExecuteDataTableAsync(connection, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms), cancellationToken);
        public Task<DataTable> ExecuteDataTableAsync(CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default) => ExecuteDataTableAsync(null, null, cmdType, cmdText, 0, cmdParms, cancellationToken);
        public Task<DataTable> ExecuteDataTableAsync(DbTransaction transaction, CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default) => ExecuteDataTableAsync(null, transaction, cmdType, cmdText, 0, cmdParms, cancellationToken);
        async public Task<DataTable> ExecuteDataTableAsync(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, DbParameter[] cmdParms, CancellationToken cancellationToken = default)
        {
            var ret = new DataTable();
            await ExecuteReaderMultipleAsync(1, connection, transaction, async (fetch, result) =>
            {
                object[] values = new object[ret.Columns.Count];
                for (int a = 0; a < values.Length; a++) if (!await fetch.Object.IsDBNullAsync(a, cancellationToken)) values[a] = await fetch.Object.GetFieldValueAsync<object>(a, cancellationToken);
                ret.Rows.Add(values);
            }, (dr, result) =>
            {
                for (var a = 0; a < dr.FieldCount; a++)
                {
                    var name = dr.GetName(a);
                    if (ret.Columns.Contains(name)) name = $"{name}_{Guid.NewGuid().ToString("N").Substring(0, 4)}";
                    ret.Columns.Add(name, dr.GetFieldType(a));
                }
            }, cmdType, cmdText, cmdTimeout, cmdParms, cancellationToken);
            return ret;
        }
        public Task<int> ExecuteNonQueryAsync(string cmdText, object parms = null, CancellationToken cancellationToken = default) => ExecuteNonQueryAsync(null, null, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms), cancellationToken);
        public Task<int> ExecuteNonQueryAsync(DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default) => ExecuteNonQueryAsync(null, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms), cancellationToken);
        public Task<int> ExecuteNonQueryAsync(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default) => ExecuteNonQueryAsync(connection, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms), cancellationToken);
        public Task<int> ExecuteNonQueryAsync(CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default) => ExecuteNonQueryAsync(null, null, cmdType, cmdText, 0, cmdParms, cancellationToken);
        public Task<int> ExecuteNonQueryAsync(DbTransaction transaction, CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default) => ExecuteNonQueryAsync(null, transaction, cmdType, cmdText, 0, cmdParms, cancellationToken);
        async public Task<int> ExecuteNonQueryAsync(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, DbParameter[] cmdParms, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(cmdText)) return 0;
            var dt = DateTime.Now;
            var logtxt = new StringBuilder();
            var logtxt_dt = DateTime.Now;
            Object<DbConnection> conn = null;
            var pc = await PrepareCommandAsync(connection, transaction, cmdType, cmdText, cmdTimeout, cmdParms, logtxt, cancellationToken);
            int val = 0;
            Exception ex = null;
            try
            {
                if (pc.cmd.Connection == null) pc.cmd.Connection = (conn = await this.MasterPool.GetAsync()).Value;
                val = await pc.cmd.ExecuteNonQueryAsync(cancellationToken);
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
        public Task<object> ExecuteScalarAsync(string cmdText, object parms = null, CancellationToken cancellationToken = default) => ExecuteScalarAsync(null, null, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms), cancellationToken);
        public Task<object> ExecuteScalarAsync(DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default) => ExecuteScalarAsync(null, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms), cancellationToken);
        public Task<object> ExecuteScalarAsync(DbConnection connection, DbTransaction transaction, string cmdText, object parms = null, CancellationToken cancellationToken = default) => ExecuteScalarAsync(connection, transaction, CommandType.Text, cmdText, 0, GetDbParamtersByObject(cmdText, parms), cancellationToken);
        public Task<object> ExecuteScalarAsync(CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default) => ExecuteScalarAsync(null, null, cmdType, cmdText, 0, cmdParms, cancellationToken);
        public Task<object> ExecuteScalarAsync(DbTransaction transaction, CommandType cmdType, string cmdText, DbParameter[] cmdParms, CancellationToken cancellationToken = default) => ExecuteScalarAsync(null, transaction, cmdType, cmdText, 0, cmdParms, cancellationToken);
        async public Task<object> ExecuteScalarAsync(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, DbParameter[] cmdParms, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(cmdText)) return null;
            var dt = DateTime.Now;
            var logtxt = new StringBuilder();
            var logtxt_dt = DateTime.Now;
            Object<DbConnection> conn = null;
            var pc = await PrepareCommandAsync(connection, transaction, cmdType, cmdText, cmdTimeout, cmdParms, logtxt, cancellationToken);
            object val = null;
            Exception ex = null;
            try
            {
                if (pc.cmd.Connection == null) pc.cmd.Connection = (conn = await this.MasterPool.GetAsync()).Value;
                val = await pc.cmd.ExecuteScalarAsync(cancellationToken);
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

        async Task<PrepareCommandResult> PrepareCommandAsync(DbConnection connection, DbTransaction transaction, CommandType cmdType, string cmdText, int cmdTimeout, DbParameter[] cmdParms, StringBuilder logtxt, CancellationToken cancellationToken = default)
        {
            DateTime dt = DateTime.Now;
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
                var tran = transaction ?? ResolveTransaction?.Invoke();

                if (tran != null)
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
                    await connection.OpenAsync(cancellationToken);
                    if (IsTracePerformance) logtxt.Append("	PrepareCommand_ConnectionOpen: ").Append(DateTime.Now.Subtract(dt).TotalMilliseconds).Append("ms\r\n");
                    isclose = true;
                }
                cmd.Connection = connection;
                if (transaction?.Connection == connection)
                    cmd.Transaction = transaction;
            }

            var before = new Aop.CommandBeforeEventArgs(cmd);
            _util?._orm?.Aop.CommandBeforeHandler?.Invoke(_util._orm, before);
            return new PrepareCommandResult(before, cmd, isclose);
        }
    }
}
#endif