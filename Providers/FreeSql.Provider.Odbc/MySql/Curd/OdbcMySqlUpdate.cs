using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.Internal.Model;
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

    class OdbcMySqlUpdate<T1> : Internal.CommonProvider.UpdateProvider<T1>
    {

        public OdbcMySqlUpdate(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere)
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
		protected override List<TReturn> ExecuteUpdated<TReturn>(IEnumerable<ColumnInfo> columns) => base.SplitExecuteUpdated<TReturn>(_batchRowsLimit > 0 ? _batchRowsLimit : 500, _batchParameterLimit > 0 ? _batchParameterLimit : 3000, columns);

		protected override List<TReturn> RawExecuteUpdated<TReturn>(IEnumerable<ColumnInfo> columns)
		{
			var ret = new List<TReturn>();
			DbParameter[] dbParms = null;
			StringBuilder sbret = null;
            var queryType = typeof(TReturn) == typeof(T1) ? (_table.TypeLazy ?? _table.Type) : null;
            int[] queryIndexs = null;
            var queryFlag = "";
            ToSqlFetch(sb =>
            {
                if (dbParms == null)
                {
                    dbParms = _params.Concat(_paramsSource).ToArray();
                    sbret = new StringBuilder();
                    sbret.Append(" RETURNING ");

                    var colidx = 0;
                    var sbflag = new StringBuilder().Append("adoQuery(crud)");
                    var dic = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
                    foreach (var col in columns)
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
                    var rettmp = new List<TReturn>();
                    _orm.Ado.ExecuteReader(_connection, _transaction, fetch =>
                    {
                        rettmp.Add((TReturn)Utils.ExecuteReaderToClass(queryFlag, queryType, queryIndexs, fetch.Object, 0, _commonUtils));
                    }, CommandType.Text, sql, _commandTimeout, dbParms);
                    ValidateVersionAndThrow(rettmp.Count, sql, dbParms);
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
		protected override Task<List<TReturn>> ExecuteUpdatedAsync<TReturn>(IEnumerable<ColumnInfo> columns, CancellationToken cancellationToken = default) => base.SplitExecuteUpdatedAsync<TReturn>(_batchRowsLimit > 0 ? _batchRowsLimit : 500, _batchParameterLimit > 0 ? _batchParameterLimit : 3000, columns, cancellationToken);

		async protected override Task<List<TReturn>> RawExecuteUpdatedAsync<TReturn>(IEnumerable<ColumnInfo> columns, CancellationToken cancellationToken = default)
		{
			var ret = new List<TReturn>();
			DbParameter[] dbParms = null;
			StringBuilder sbret = null;
            var queryType = typeof(TReturn) == typeof(T1) ? (_table.TypeLazy ?? _table.Type) : null;
            int[] queryIndexs = null;
            var queryFlag = "";
            await ToSqlFetchAsync(async sb =>
            {
                if (dbParms == null)
                {
                    dbParms = _params.Concat(_paramsSource).ToArray();
                    sbret = new StringBuilder();
                    sbret.Append(" RETURNING ");

                    var colidx = 0;
                    var sbflag = new StringBuilder().Append("adoQuery(crud)");
                    var dic = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
                    foreach (var col in columns)
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
                    var rettmp = new List<TReturn>();
                    await _orm.Ado.ExecuteReaderAsync(_connection, _transaction, fetch =>
                    {
                        rettmp.Add((TReturn)Utils.ExecuteReaderToClass(queryFlag, queryType, queryIndexs, fetch.Object, 0, _commonUtils));
                        return Task.FromResult(false);
                    }, CommandType.Text, sql, _commandTimeout, dbParms, cancellationToken);
                    ValidateVersionAndThrow(rettmp.Count, sql, dbParms);
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
#endif
	}
}
