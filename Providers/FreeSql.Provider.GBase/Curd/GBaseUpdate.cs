using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.GBase.Curd
{

    class GBaseUpdate<T1> : Internal.CommonProvider.UpdateProvider<T1>
    {

        public GBaseUpdate(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere)
            : base(orm, commonUtils, commonExpression, dywhere)
        {
        }

        public override int ExecuteAffrows() => base.SplitExecuteAffrows(_batchRowsLimit > 0 ? _batchRowsLimit : 200, _batchParameterLimit > 0 ? _batchParameterLimit : 999);
        protected override List<TReturn> ExecuteUpdated<TReturn>(IEnumerable<ColumnInfo> columns) => base.SplitExecuteUpdated<TReturn>(_batchRowsLimit > 0 ? _batchRowsLimit : 200, _batchParameterLimit > 0 ? _batchParameterLimit : 999, columns);

		protected override List<TReturn> RawExecuteUpdated<TReturn>(IEnumerable<ColumnInfo> columns)
		{
			var ret = new List<TReturn>();
			DbParameter[] dbParms = null;
			StringBuilder sbret = null;
			ToSqlFetch(sb =>
			{
				if (dbParms == null)
				{
					dbParms = _params.Concat(_paramsSource).ToArray();
					sbret = new StringBuilder();
					var colidx = 0;
					foreach (var col in columns)
					{
						if (colidx > 0) sbret.Append(", ");
						sbret.Append(_commonUtils.RereadColumn(col, _commonUtils.QuoteSqlName(col.Attribute.Name))).Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
						++colidx;
					}
				}
				var sql = sb.ToString();
				var validx = sql.IndexOf(" \r\nWHERE ");
				if (validx == -1) throw new ArgumentException(CoreErrorStrings.S_NotFound_Name("WHERE"));
				var wherePart = sql.Substring(validx);
				var selectSql = new StringBuilder()
					.Append("SELECT ").Append(sbret)
					.Append(" FROM ").Append(_commonUtils.QuoteSqlName(TableRuleInvoke()));
				if (string.IsNullOrWhiteSpace(_tableAlias) == false) selectSql.Append(" ").Append(_tableAlias);
				selectSql.Append(wherePart);

				var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Update, string.Concat(sql, "; ", selectSql.ToString(), ";"), dbParms);
				_orm.Aop.CurdBeforeHandler?.Invoke(this, before);

				Exception exception = null;
				try
				{
					var affrowstmp = _orm.Ado.ExecuteNonQuery(_connection, _transaction, CommandType.Text, sql, _commandTimeout, dbParms);
					ValidateVersionAndThrow(affrowstmp, sql, dbParms);
					var queryType = typeof(TReturn) == typeof(T1) ? (_table.TypeLazy ?? _table.Type) : null;
					var rettmp = _orm.Ado.Query<TReturn>(queryType, _connection, _transaction, CommandType.Text, selectSql.ToString(), _commandTimeout, dbParms);
					ret.AddRange(rettmp);
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

        protected override void ToSqlCase(StringBuilder caseWhen, ColumnInfo[] primarys)
        {
            if (primarys.Length == 1)
            {
                var pk = primarys.First();
                caseWhen.Append(_commonUtils.RereadColumn(pk, _commonUtils.QuoteSqlName(pk.Attribute.Name)));
                return;
            }
            caseWhen.Append("(");
            var pkidx = 0;
            foreach (var pk in primarys)
            {
                if (pkidx > 0) caseWhen.Append(" || '+' || ");
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
            sb.Append("(");
            var pkidx = 0;
            foreach (var pk in primarys)
            {
                if (pkidx > 0) sb.Append(" || '+' || ");
                sb.Append(_commonUtils.FormatSql("{0}", pk.GetDbValue(d)));
                ++pkidx;
            }
            sb.Append(")");
        }

#if net40
#else
        public override Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default) => base.SplitExecuteAffrowsAsync(_batchRowsLimit > 0 ? _batchRowsLimit : 200, _batchParameterLimit > 0 ? _batchParameterLimit : 999, cancellationToken);
		protected override Task<List<TReturn>> ExecuteUpdatedAsync<TReturn>(IEnumerable<ColumnInfo> columns, CancellationToken cancellationToken = default) => base.SplitExecuteUpdatedAsync<TReturn>(_batchRowsLimit > 0 ? _batchRowsLimit : 200, _batchParameterLimit > 0 ? _batchParameterLimit : 999, columns, cancellationToken);

		async protected override Task<List<TReturn>> RawExecuteUpdatedAsync<TReturn>(IEnumerable<ColumnInfo> columns, CancellationToken cancellationToken = default)
		{
			var ret = new List<TReturn>();
			DbParameter[] dbParms = null;
			StringBuilder sbret = null;
			await ToSqlFetchAsync(async sb =>
			{
				if (dbParms == null)
				{
					dbParms = _params.Concat(_paramsSource).ToArray();
					sbret = new StringBuilder();
					var colidx = 0;
					foreach (var col in columns)
					{
						if (colidx > 0) sbret.Append(", ");
						sbret.Append(_commonUtils.RereadColumn(col, _commonUtils.QuoteSqlName(col.Attribute.Name))).Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
						++colidx;
					}
				}
				var sql = sb.ToString();
				var validx = sql.IndexOf(" \r\nWHERE ");
				if (validx == -1) throw new ArgumentException(CoreErrorStrings.S_NotFound_Name("WHERE"));
				var wherePart = sql.Substring(validx);
				var selectSql = new StringBuilder()
					.Append("SELECT ").Append(sbret)
					.Append(" FROM ").Append(_commonUtils.QuoteSqlName(TableRuleInvoke()));
				if (string.IsNullOrWhiteSpace(_tableAlias) == false) selectSql.Append(" ").Append(_tableAlias);
				selectSql.Append(wherePart);

				var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Update, string.Concat(sql, "; ", selectSql.ToString(), ";"), dbParms);
				_orm.Aop.CurdBeforeHandler?.Invoke(this, before);

				Exception exception = null;
				try
				{
					var affrowstmp = await _orm.Ado.ExecuteNonQueryAsync(_connection, _transaction, CommandType.Text, sql, _commandTimeout, dbParms, cancellationToken);
					ValidateVersionAndThrow(affrowstmp, sql, dbParms);
					var queryType = typeof(TReturn) == typeof(T1) ? (_table.TypeLazy ?? _table.Type) : null;
					var rettmp = await _orm.Ado.QueryAsync<TReturn>(queryType, _connection, _transaction, CommandType.Text, selectSql.ToString(), _commandTimeout, dbParms, cancellationToken);
					ret.AddRange(rettmp);
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
