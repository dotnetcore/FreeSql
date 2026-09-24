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

namespace FreeSql.Custom.MySql
{

    class CustomMySqlDelete<T1> : Internal.CommonProvider.DeleteProvider<T1>
    {
        public CustomMySqlDelete(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere)
            : base(orm, commonUtils, commonExpression, dywhere)
        {
        }

        public override List<T1> ExecuteDeleted()
        {
            var ret = new List<T1>();
            DbParameter[] dbParms = null;
            StringBuilder sbSelectCols = null;
            ToSqlFetch(sb =>
            {
                if (dbParms == null)
                {
                    dbParms = _params.ToArray();
                    sbSelectCols = new StringBuilder();
                    var colidx = 0;
                    foreach (var col in _table.Columns.Values)
                    {
                        if (colidx > 0) sbSelectCols.Append(", ");
                        sbSelectCols.Append(_commonUtils.RereadColumn(col, _commonUtils.QuoteSqlName(col.Attribute.Name))).Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
                        ++colidx;
                    }
                }
                var deleteSql = sb.ToString();
                var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Delete, deleteSql, dbParms);
                _orm.Aop.CurdBeforeHandler?.Invoke(this, before);

                Exception exception = null;
                try
                {
                    // 必须在 DELETE 之前 SELECT，否则数据已被删除无法查询
                    var whereIdx = deleteSql.LastIndexOf(" WHERE ");
                    if (whereIdx >= 0)
                    {
                        var tableName = _commonUtils.QuoteSqlName(TableRuleInvoke());
                        var selectSql = new StringBuilder()
                            .Append("SELECT ").Append(sbSelectCols)
                            .Append(" FROM ").Append(tableName).Append(deleteSql.Substring(whereIdx))
                            .ToString();

                        var before2 = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Delete, selectSql, dbParms);
                        _orm.Aop.CurdBeforeHandler?.Invoke(this, before2);

                        ret.AddRange(_orm.Ado.Query<T1>(_table.TypeLazy ?? _table.Type, _connection, _transaction, CommandType.Text, selectSql, _commandTimeout, dbParms));
                    }
                    _orm.Ado.ExecuteNonQuery(_connection, _transaction, CommandType.Text, deleteSql, _commandTimeout, dbParms);
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
            if (dbParms != null)
            {
                this.ClearData();
                sbSelectCols?.Clear();
            }
            return ret;
        }

#if net40
#else
        async public override Task<List<T1>> ExecuteDeletedAsync(CancellationToken cancellationToken = default)
        {
            var ret = new List<T1>();
            DbParameter[] dbParms = null;
            StringBuilder sbSelectCols = null;
            await ToSqlFetchAsync(async sb =>
            {
                if (dbParms == null)
                {
                    dbParms = _params.ToArray();
                    sbSelectCols = new StringBuilder();
                    var colidx = 0;
                    foreach (var col in _table.Columns.Values)
                    {
                        if (colidx > 0) sbSelectCols.Append(", ");
                        sbSelectCols.Append(_commonUtils.RereadColumn(col, _commonUtils.QuoteSqlName(col.Attribute.Name))).Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
                        ++colidx;
                    }
                }
                var deleteSql = sb.ToString();
                var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Delete, deleteSql, dbParms);
                _orm.Aop.CurdBeforeHandler?.Invoke(this, before);

                Exception exception = null;
                try
                {
                    // 必须在 DELETE 之前 SELECT，否则数据已被删除无法查询
                    var whereIdx = deleteSql.LastIndexOf(" WHERE ");
                    if (whereIdx >= 0)
                    {
                        var tableName = _commonUtils.QuoteSqlName(TableRuleInvoke());
                        var selectSql = new StringBuilder()
                            .Append("SELECT ").Append(sbSelectCols)
                            .Append(" FROM ").Append(tableName).Append(deleteSql.Substring(whereIdx))
                            .ToString();

                        var before2 = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Delete, selectSql, dbParms);
                        _orm.Aop.CurdBeforeHandler?.Invoke(this, before2);

                        ret.AddRange(await _orm.Ado.QueryAsync<T1>(_table.TypeLazy ?? _table.Type, _connection, _transaction, CommandType.Text, selectSql, _commandTimeout, dbParms, cancellationToken));
                    }
                    await _orm.Ado.ExecuteNonQueryAsync(_connection, _transaction, CommandType.Text, deleteSql, _commandTimeout, dbParms, cancellationToken);
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
            if (dbParms != null)
            {
                this.ClearData();
                sbSelectCols?.Clear();
            }
            return ret;
        }
#endif
    }
}
