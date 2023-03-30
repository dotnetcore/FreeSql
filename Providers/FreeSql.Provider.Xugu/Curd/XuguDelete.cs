using FreeSql.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.Xugu.Curd
{

    class XuguDelete<T1> : Internal.CommonProvider.DeleteProvider<T1>
    {
        public XuguDelete(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere)
            : base(orm, commonUtils, commonExpression, dywhere)
        {
        }

        public override List<T1> ExecuteDeleted()
        {
            var ret = new List<T1>();
            DbParameter[] dbParms = null;
            StringBuilder sbret = null;
            ToSqlFetch(sb =>
            {
                if (dbParms == null)
                {
                    dbParms = _params.ToArray();
                    sbret = new StringBuilder();
                    sbret.Append(" RETURNING ");

                    var colidx = 0;
                    foreach (var col in _table.Columns.Values)
                    {
                        if (colidx > 0) sbret.Append(", ");
                        sbret.Append(_commonUtils.RereadColumn(col, _commonUtils.QuoteSqlName(col.Attribute.Name))).Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
                        ++colidx;
                    }
                }
                var sql = sb.Append(sbret).ToString();
                var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Delete, sql, dbParms);
                _orm.Aop.CurdBeforeHandler?.Invoke(this, before);

                Exception exception = null;
                try
                {
                    ret.AddRange(_orm.Ado.Query<T1>(_table.TypeLazy ?? _table.Type, _connection, _transaction, CommandType.Text, sql, _commandTimeout, dbParms));
                }
                catch (Exception ex)
                {
                    exception = ex;
                    throw ex;
                }
                finally
                {
                    var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                    _orm.Aop.CurdAfterHandler?.Invoke(this, after);
                }
            });
            if (dbParms != null)
            {
                this.ClearData();
                sbret.Clear();
            }
            return ret;
        }

#if net40
#else
        async public override Task<List<T1>> ExecuteDeletedAsync(CancellationToken cancellationToken = default)
        {
            var ret = new List<T1>();
            DbParameter[] dbParms = null;
            StringBuilder sbret = null;
            await ToSqlFetchAsync(async sb =>
            {
                if (dbParms == null)
                {
                    dbParms = _params.ToArray();
                    sbret = new StringBuilder();
                    sbret.Append(" RETURNING ");

                    var colidx = 0;
                    foreach (var col in _table.Columns.Values)
                    {
                        if (colidx > 0) sbret.Append(", ");
                        sbret.Append(_commonUtils.RereadColumn(col, _commonUtils.QuoteSqlName(col.Attribute.Name))).Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
                        ++colidx;
                    }
                }
                var sql = sb.Append(sbret).ToString();
                var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Delete, sql, dbParms);
                _orm.Aop.CurdBeforeHandler?.Invoke(this, before);

                Exception exception = null;
                try
                {
                    ret.AddRange(await _orm.Ado.QueryAsync<T1>(_table.TypeLazy ?? _table.Type, _connection, _transaction, CommandType.Text, sql, _commandTimeout, dbParms, cancellationToken));
                }
                catch (Exception ex)
                {
                    exception = ex;
                    throw ex;
                }
                finally
                {
                    var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                    _orm.Aop.CurdAfterHandler?.Invoke(this, after);
                }
            });
            if (dbParms != null)
            {
                this.ClearData();
                sbret.Clear();
            }
            return ret;
        }
#endif
    }
}
