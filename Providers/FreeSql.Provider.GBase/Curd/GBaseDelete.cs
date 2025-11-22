using FreeSql.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.GBase.Curd
{

    class GBaseDelete<T1> : Internal.CommonProvider.DeleteProvider<T1>
    {
        public GBaseDelete(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere)
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

                    var colidx = 0;
                    foreach (var col in _table.Columns.Values)
                    {
                        if (colidx > 0) sbret.Append(", ");
                        sbret.Append(_commonUtils.RereadColumn(col, _commonUtils.QuoteSqlName(col.Attribute.Name))).Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
                        ++colidx;
                    }
                }
                var delSql = sb.ToString();
                var validx = delSql.IndexOf(" WHERE ");
                if (validx == -1) throw new ArgumentException(CoreErrorStrings.S_NotFound_Name("WHERE"));
                var wherePart = delSql.Substring(validx);
                var selectSql = new StringBuilder()
                    .Append("SELECT ").Append(sbret)
                    .Append(" FROM ").Append(_commonUtils.QuoteSqlName(TableRuleInvoke()))
                    .Append(wherePart);

                var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Delete, string.Concat(selectSql.ToString(), "; ", delSql, ";"), dbParms);
                _orm.Aop.CurdBeforeHandler?.Invoke(this, before);

                Exception exception = null;
                try
                {
                    ret.AddRange(_orm.Ado.Query<T1>(_table.TypeLazy ?? _table.Type, _connection, _transaction, CommandType.Text, selectSql.ToString(), _commandTimeout, dbParms));
                    _orm.Ado.ExecuteNonQuery(_connection, _transaction, CommandType.Text, delSql, _commandTimeout, dbParms);
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

                    var colidx = 0;
                    foreach (var col in _table.Columns.Values)
                    {
                        if (colidx > 0) sbret.Append(", ");
                        sbret.Append(_commonUtils.RereadColumn(col, _commonUtils.QuoteSqlName(col.Attribute.Name))).Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
                        ++colidx;
                    }
                }
                var delSql = sb.ToString();
                var validx = delSql.IndexOf(" WHERE ");
                if (validx == -1) throw new ArgumentException(CoreErrorStrings.S_NotFound_Name("WHERE"));
                var wherePart = delSql.Substring(validx);
                var selectSql = new StringBuilder()
                    .Append("SELECT ").Append(sbret)
                    .Append(" FROM ").Append(_commonUtils.QuoteSqlName(TableRuleInvoke()))
                    .Append(wherePart);

                var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Delete, string.Concat(selectSql.ToString(), "; ", delSql, ";"), dbParms);
                _orm.Aop.CurdBeforeHandler?.Invoke(this, before);

                Exception exception = null;
                try
                {
                    ret.AddRange(await _orm.Ado.QueryAsync<T1>(_table.TypeLazy ?? _table.Type, _connection, _transaction, CommandType.Text, selectSql.ToString(), _commandTimeout, dbParms, cancellationToken));
                    await _orm.Ado.ExecuteNonQueryAsync(_connection, _transaction, CommandType.Text, delSql, _commandTimeout, dbParms, cancellationToken);
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
            return ret;
        }
#endif
    }
}
