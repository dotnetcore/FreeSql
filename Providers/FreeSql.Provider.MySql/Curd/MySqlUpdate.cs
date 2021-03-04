using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.MySql.Curd
{

    class MySqlUpdate<T1> : Internal.CommonProvider.UpdateProvider<T1>
    {

        public MySqlUpdate(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere)
            : base(orm, commonUtils, commonExpression, dywhere)
        {
        }

        internal StringBuilder InternalSbSet => _set;
        internal StringBuilder InternalSbSetIncr => _setIncr;
        internal Dictionary<string, bool> InternalIgnore => _ignore;
        internal void InternalResetSource(List<T1> source) => _source = source;
        internal string InternalWhereCaseSource(string CsName, Func<string, string> thenValue) => WhereCaseSource(CsName, thenValue);
        internal void InternalToSqlCaseWhenEnd(StringBuilder sb, ColumnInfo col) => ToSqlCaseWhenEnd(sb, col);

        public override int ExecuteAffrows() => base.SplitExecuteAffrows(_batchRowsLimit > 0 ? _batchRowsLimit : 500, _batchParameterLimit > 0 ? _batchParameterLimit : 3000);
        public override List<T1> ExecuteUpdated() => base.SplitExecuteUpdated(_batchRowsLimit > 0 ? _batchRowsLimit : 500, _batchParameterLimit > 0 ? _batchParameterLimit : 3000);

        protected override List<T1> RawExecuteUpdated()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return new List<T1>();

            var sb = new StringBuilder();
            sb.Append(sql).Append(" RETURNING ");

            var colidx = 0;
            foreach (var col in _table.Columns.Values)
            {
                if (colidx > 0) sb.Append(", ");
                sb.Append(_commonUtils.RereadColumn(col, _commonUtils.QuoteSqlName(col.Attribute.Name))).Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
                ++colidx;
            }
            sql = sb.ToString();
            var dbParms = _params.Concat(_paramsSource).ToArray();
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Update, sql, dbParms);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            var ret = new List<T1>();
            Exception exception = null;
            try
            {
                ret = _orm.Ado.Query<T1>(_table.TypeLazy ?? _table.Type, _connection, _transaction, CommandType.Text, sql, _commandTimeout, dbParms);
                ValidateVersionAndThrow(ret.Count, sql, dbParms);
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
            return ret;
        }

        protected override void ToSqlCase(StringBuilder caseWhen, ColumnInfo[] primarys)
        {
            if (primarys.Length == 1)
            {
                var pk = primarys.First();
                caseWhen.Append(_commonUtils.RereadColumn(pk, _commonUtils.QuoteSqlName(pk.Attribute.Name)));
                return;
            }
            caseWhen.Append("CONCAT(");
            var pkidx = 0;
            foreach (var pk in primarys)
            {
                if (pkidx > 0) caseWhen.Append(", '+', ");
                caseWhen.Append(_commonUtils.RereadColumn(pk, _commonUtils.QuoteSqlName(pk.Attribute.Name)));
                ++pkidx;
            }
            caseWhen.Append(")");
        }

        protected override void ToSqlWhen(StringBuilder sb, ColumnInfo[] primarys, object d)
        {
            if (primarys.Length == 1)
            {
                sb.Append(_commonUtils.FormatSql("{0}", primarys[0].GetDbValue(d)));
                return;
            }
            sb.Append("CONCAT(");
            var pkidx = 0;
            foreach (var pk in primarys)
            {
                if (pkidx > 0) sb.Append(", '+', ");
                sb.Append(_commonUtils.FormatSql("{0}", pk.GetDbValue(d)));
                ++pkidx;
            }
            sb.Append(")");
        }

#if net40
#else
        public override Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default) => base.SplitExecuteAffrowsAsync(_batchRowsLimit > 0 ? _batchRowsLimit : 500, _batchParameterLimit > 0 ? _batchParameterLimit : 3000, cancellationToken);
        public override Task<List<T1>> ExecuteUpdatedAsync(CancellationToken cancellationToken = default) => base.SplitExecuteUpdatedAsync(_batchRowsLimit > 0 ? _batchRowsLimit : 500, _batchParameterLimit > 0 ? _batchParameterLimit : 3000, cancellationToken);

        async protected override Task<List<T1>> RawExecuteUpdatedAsync(CancellationToken cancellationToken = default)
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return new List<T1>();

            var sb = new StringBuilder();
            sb.Append(sql).Append(" RETURNING ");

            var colidx = 0;
            foreach (var col in _table.Columns.Values)
            {
                if (colidx > 0) sb.Append(", ");
                sb.Append(_commonUtils.RereadColumn(col, _commonUtils.QuoteSqlName(col.Attribute.Name))).Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
                ++colidx;
            }
            sql = sb.ToString();
            var dbParms = _params.Concat(_paramsSource).ToArray();
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Update, sql, dbParms);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            var ret = new List<T1>();
            Exception exception = null;
            try
            {
                ret = await _orm.Ado.QueryAsync<T1>(_table.TypeLazy ?? _table.Type, _connection, _transaction, CommandType.Text, sql, _commandTimeout, dbParms, cancellationToken);
                ValidateVersionAndThrow(ret.Count, sql, dbParms);
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
            return ret;
        }
#endif
    }
}
