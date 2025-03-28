using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.Odbc.MySql
{

    class OdbcMySqlDelete<T1> : Internal.CommonProvider.DeleteProvider<T1>
    {
        public OdbcMySqlDelete(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere)
            : base(orm, commonUtils, commonExpression, dywhere)
        {
        }

        public override List<T1> ExecuteDeleted()
        {
            var ret = new List<T1>();
            DbParameter[] dbParms = null;
            StringBuilder sbret = null;
            var queryType = _table.TypeLazy ?? _table.Type;
            int[] queryIndexs = null;
            var queryFlag = "";
            ToSqlFetch(sb =>
            {
                if (dbParms == null)
                {
                    dbParms = _params.ToArray();
                    sbret = new StringBuilder();
                    sbret.Append(" RETURNING ");

                    var colidx = 0;
                    var sbflag = new StringBuilder().Append("adoQuery(crud)");
                    var dic = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
                    foreach (var col in _table.Columns.Values)
                    {
                        if (colidx > 0) sbret.Append(", ");
                        sbret.Append(_commonUtils.RereadColumn(col, _commonUtils.QuoteSqlName(col.Attribute.Name)));
                        if (dic.ContainsKey(col.CsName)) continue;
                        sbflag.Append(col.Attribute.Name).Append(":").Append(colidx).Append(",");
                        dic.Add(col.CsName, colidx);
                        ++colidx;
                    }
                    queryIndexs = AdoProvider.GetQueryTypeProperties(queryType).Select(a => dic.TryGetValue(a.Key, out var tryint) ? tryint : -1).ToArray();
                    queryFlag = sbflag.ToString();
                }
                var sql = sb.Append(sbret).ToString();
                var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Update, sql, dbParms);
                _orm.Aop.CurdBeforeHandler?.Invoke(this, before);

                Exception exception = null;
                try
                {
                    _orm.Ado.ExecuteReader(_connection, _transaction, fetch =>
                    {
                        ret.Add((T1)Utils.ExecuteReaderToClass(queryFlag, queryType, queryIndexs, fetch.Object, 0, _commonUtils));
                    }, CommandType.Text, sql, _commandTimeout, dbParms);
                }
                catch (Exception ex)
                {
                    exception = ex;
                    throw;
                }
                finally
                {
                    var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                    _orm.Aop.CurdAfterHandler?.Invoke(this, after);
                }
            });
            sbret?.Clear();
            return ret;
        }

#if net40
#else
        async public override Task<List<T1>> ExecuteDeletedAsync(CancellationToken cancellationToken = default)
        {
            var ret = new List<T1>();
            DbParameter[] dbParms = null;
            StringBuilder sbret = null;
            var queryType = _table.TypeLazy ?? _table.Type;
            int[] queryIndexs = null;
            var queryFlag = "";
            await ToSqlFetchAsync(async sb =>
            {
                if (dbParms == null)
                {
                    dbParms = _params.ToArray();
                    sbret = new StringBuilder();
                    sbret.Append(" RETURNING ");

                    var colidx = 0;
                    var sbflag = new StringBuilder().Append("adoQuery(crud)");
                    var dic = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
                    foreach (var col in _table.Columns.Values)
                    {
                        if (colidx > 0) sbret.Append(", ");
                        sbret.Append(_commonUtils.RereadColumn(col, _commonUtils.QuoteSqlName(col.Attribute.Name)));
                        if (dic.ContainsKey(col.CsName)) continue;
                        sbflag.Append(col.Attribute.Name).Append(":").Append(colidx).Append(",");
                        dic.Add(col.CsName, colidx);
                        ++colidx;
                    }
                    queryIndexs = AdoProvider.GetQueryTypeProperties(queryType).Select(a => dic.TryGetValue(a.Key, out var tryint) ? tryint : -1).ToArray();
                    queryFlag = sbflag.ToString();
                }
                var sql = sb.Append(sbret).ToString();
                var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Update, sql, dbParms);
                _orm.Aop.CurdBeforeHandler?.Invoke(this, before);

                Exception exception = null;
                try
                {
                    await _orm.Ado.ExecuteReaderAsync(_connection, _transaction, fetch =>
                    {
                        ret.Add((T1)Utils.ExecuteReaderToClass(queryFlag, queryType, queryIndexs, fetch.Object, 0, _commonUtils));
                        return Task.FromResult(false);
                    }, CommandType.Text, sql, _commandTimeout, dbParms, cancellationToken);
                }
                catch (Exception ex)
                {
                    exception = ex;
                    throw;
                }
                finally
                {
                    var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                    _orm.Aop.CurdAfterHandler?.Invoke(this, after);
                }
            });
            sbret?.Clear();
            return ret;
        }
#endif
    }
}
