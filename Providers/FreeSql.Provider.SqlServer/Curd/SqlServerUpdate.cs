using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.SqlServer.Curd
{

    class SqlServerUpdate<T1> : Internal.CommonProvider.UpdateProvider<T1>
    {

        public SqlServerUpdate(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere)
            : base(orm, commonUtils, commonExpression, dywhere)
        {
        }

        public override int ExecuteAffrows() => base.SplitExecuteAffrows(_batchRowsLimit > 0 ? _batchRowsLimit : 500, _batchParameterLimit > 0 ? _batchParameterLimit : 2100);
		protected override List<TReturn> ExecuteUpdated<TReturn>(IEnumerable<ColumnInfo> columns) => base.SplitExecuteUpdated<TReturn>(_batchRowsLimit > 0 ? _batchRowsLimit : 500, _batchParameterLimit > 0 ? _batchParameterLimit : 2100, columns);

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
					sbret.Append(" OUTPUT ");

					var colidx = 0;
					foreach (var col in columns)
					{
						if (colidx > 0) sbret.Append(", ");
						sbret.Append(_commonUtils.RereadColumn(col, $"INSERTED.{_commonUtils.QuoteSqlName(col.Attribute.Name)}")).Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
						++colidx;
					}
				}
				var sql = sb.ToString();
				var validx = sql.IndexOf(" \r\nWHERE ");
				if (validx == -1) throw new ArgumentException(CoreErrorStrings.S_NotFound_Name("WHERE"));
				sql = sb.Clear().Append(sql.Substring(0, validx))
					.Append(sbret)
					.Append(sql.Substring(validx)).ToString();

				var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Update, sql, dbParms);
				_orm.Aop.CurdBeforeHandler?.Invoke(this, before);

				Exception exception = null;
				try
				{
                    var queryType = typeof(TReturn) == typeof(T1) ? (_table.TypeLazy ?? _table.Type) : null;
					var rettmp = _orm.Ado.Query<TReturn>(queryType, _connection, _transaction, CommandType.Text, sql, _commandTimeout, dbParms);
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
            caseWhen.Append("(");
            var pkidx = 0;
            foreach (var pk in primarys)
            {
                if (pkidx > 0) caseWhen.Append(" + '+' + ");
                caseWhen.Append("cast(").Append(_commonUtils.RereadColumn(pk, _commonUtils.QuoteSqlName(pk.Attribute.Name))).Append(" as varchar(2000))");
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
            var pkidx = 0;
            foreach (var pk in primarys)
            {
                if (pkidx > 0) sb.Append(" + '+' + ");
                sb.Append("cast(").Append(_commonUtils.FormatSql("{0}", pk.GetDbValue(d))).Append(" as varchar(2000))");
                ++pkidx;
            }
        }

#if net40
#else
        public override Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default) => base.SplitExecuteAffrowsAsync(_batchRowsLimit > 0 ? _batchRowsLimit : 500, _batchParameterLimit > 0 ? _batchParameterLimit : 2100, cancellationToken);
		protected override Task<List<TReturn>> ExecuteUpdatedAsync<TReturn>(IEnumerable<ColumnInfo> columns, CancellationToken cancellationToken = default) => base.SplitExecuteUpdatedAsync<TReturn>(_batchRowsLimit > 0 ? _batchRowsLimit : 500, _batchParameterLimit > 0 ? _batchParameterLimit : 2100, columns, cancellationToken);

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
                    sbret.Append(" OUTPUT ");

                    var colidx = 0;
                    foreach (var col in columns)
                    {
                        if (colidx > 0) sbret.Append(", ");
                        sbret.Append(_commonUtils.RereadColumn(col, $"INSERTED.{_commonUtils.QuoteSqlName(col.Attribute.Name)}")).Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
                        ++colidx;
                    }
                }
                var sql = sb.ToString();
                var validx = sql.IndexOf(" \r\nWHERE ");
                if (validx == -1) throw new ArgumentException(CoreErrorStrings.S_NotFound_Name("WHERE"));
                sql = sb.Clear().Append(sql.Substring(0, validx))
                    .Append(sbret)
                    .Append(sql.Substring(validx)).ToString();

                var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Update, sql, dbParms);
                _orm.Aop.CurdBeforeHandler?.Invoke(this, before);

                Exception exception = null;
                try
				{
					var queryType = typeof(TReturn) == typeof(T1) ? (_table.TypeLazy ?? _table.Type) : null;
					var rettmp = await _orm.Ado.QueryAsync<TReturn>(queryType, _connection, _transaction, CommandType.Text, sql, _commandTimeout, dbParms, cancellationToken);
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
