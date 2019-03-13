using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.Internal.CommonProvider {

	abstract partial class UpdateProvider<T1> : IUpdate<T1> where T1 : class {
		protected IFreeSql _orm;
		protected CommonUtils _commonUtils;
		protected CommonExpression _commonExpression;
		protected List<T1> _source = new List<T1>();
		protected Dictionary<string, bool> _ignore = new Dictionary<string, bool>(StringComparer.CurrentCultureIgnoreCase);
		protected TableInfo _table;
		protected Func<string, string> _tableRule;
		protected StringBuilder _where = new StringBuilder();
		protected StringBuilder _set = new StringBuilder();
		protected List<DbParameter> _params = new List<DbParameter>();
		protected List<DbParameter> _paramsSource = new List<DbParameter>();
		protected bool _noneParameter;
		protected DbTransaction _transaction;

		public UpdateProvider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) {
			_orm = orm;
			_commonUtils = commonUtils;
			_commonExpression = commonExpression;
			_table = _commonUtils.GetTableByEntity(typeof(T1));
			_noneParameter = _orm.CodeFirst.IsNoneCommandParameter;
			this.Where(_commonUtils.WhereObject(_table, "", dywhere));
			if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure<T1>();
		}

		public IUpdate<T1> WithTransaction(DbTransaction transaction) {
			_transaction = transaction;
			return this;
		}
		public IUpdate<T1> NoneParameter() {
			_noneParameter = false;
			return this;
		}

		public int ExecuteAffrows() {
			var sql = this.ToSql();
			if (string.IsNullOrEmpty(sql)) return 0;
			return _orm.Ado.ExecuteNonQuery(_transaction, CommandType.Text, sql, _params.Concat(_paramsSource).ToArray());
		}
		async public Task<int> ExecuteAffrowsAsync() {
			var sql = this.ToSql();
			if (string.IsNullOrEmpty(sql)) return 0;
			return await _orm.Ado.ExecuteNonQueryAsync(_transaction, CommandType.Text, sql, _params.Concat(_paramsSource).ToArray());
		}
		public abstract List<T1> ExecuteUpdated();
		public abstract Task<List<T1>> ExecuteUpdatedAsync();

		public IUpdate<T1> IgnoreColumns(Expression<Func<T1, object>> columns) {
			var cols = _commonExpression.ExpressionSelectColumns_MemberAccess_New_NewArrayInit(null, columns?.Body, false, null).Distinct();
			_ignore.Clear();
			foreach (var col in cols) _ignore.Add(col, true);
			return this;
		}

		public IUpdate<T1> SetSource(T1 source) => this.SetSource(new[] { source });
		public IUpdate<T1> SetSource(IEnumerable<T1> source) {
			if (source == null || source.Any() == false) return this;
			_source.AddRange(source.Where(a => a != null));
			return this.Where(_source);
		}

		public IUpdate<T1> Set<TMember>(Expression<Func<T1, TMember>> column, TMember value) {
			var cols = new List<SelectColumnInfo>();
			_commonExpression.ExpressionSelectColumn_MemberAccess(null, cols, SelectTableInfoType.From, column?.Body, true, null);
			if (cols.Count != 1) return this;
			var col = cols.First();
			_set.Append(", ").Append(_commonUtils.QuoteSqlName(col.Column.Attribute.Name)).Append(" = ");
			if (_noneParameter) {
				_set.Append(_commonUtils.GetNoneParamaterSqlValue(_params, col.Column.CsType, value));
			} else {
				_set.Append(_commonUtils.QuoteWriteParamter(col.Column.CsType, $"{_commonUtils.QuoteParamterName("p_")}{_params.Count}"));
				_commonUtils.AppendParamter(_params, null, col.Column.CsType, value);
			}
			//foreach (var t in _source) Utils.FillPropertyValue(t, tryf.CsName, value);
			return this;
		}
		public IUpdate<T1> Set<TMember>(Expression<Func<T1, TMember>> binaryExpression) {
			if (binaryExpression?.Body is BinaryExpression == false) return this;
			var cols = new List<SelectColumnInfo>();
			var expt = _commonExpression.ExpressionWhereLambdaNoneForeignObject(null, cols, binaryExpression, null);
			if (cols.Any() == false) return this;
			foreach (var col in cols) {
				if (col.Column.Attribute.IsNullable == true) {
					var replval = _orm.CodeFirst.GetDbInfo(col.Column.CsType.GenericTypeArguments.FirstOrDefault())?.defaultValue;
					if (replval == null) continue;
					var replname = _commonUtils.QuoteSqlName(col.Column.Attribute.Name);
					expt = expt.Replace(replname, _commonUtils.IsNull(replname, _commonUtils.FormatSql("{0}", replval)));
				}
			}
			_set.Append(", ").Append(_commonUtils.QuoteSqlName(cols.First().Column.Attribute.Name)).Append(" = ").Append(expt);
			return this;
		}
		public IUpdate<T1> SetRaw(string sql, object parms = null) {
			if (string.IsNullOrEmpty(sql)) return this;
			_set.Append(", ").Append(sql);
			if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject(sql, parms));
			return this;
		}

		public IUpdate<T1> Where(Expression<Func<T1, bool>> expression) => this.Where(_commonExpression.ExpressionWhereLambdaNoneForeignObject(null, null, expression?.Body, null));
		public IUpdate<T1> Where(string sql, object parms = null) {
			if (string.IsNullOrEmpty(sql)) return this;
			_where.Append(" AND (").Append(sql).Append(")");
			if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject(sql, parms));
			return this;
		}
		public IUpdate<T1> Where(T1 item) => this.Where(new[] { item });
		public IUpdate<T1> Where(IEnumerable<T1> items) => this.Where(_commonUtils.WhereItems(_table, "", items));
		public IUpdate<T1> WhereExists<TEntity2>(ISelect<TEntity2> select, bool notExists = false) where TEntity2 : class => this.Where($"{(notExists ? "NOT " : "")}EXISTS({select.ToSql("1")})");

		protected abstract void ToSqlCase(StringBuilder caseWhen, ColumnInfo[] primarys);
		protected abstract void ToSqlWhen(StringBuilder sb, ColumnInfo[] primarys, object d);

		public IUpdate<T1> AsTable(Func<string, string> tableRule) {
			_tableRule = tableRule;
			return this;
		}
		public string ToSql() {
			if (_where.Length == 0) return null;

			var sb = new StringBuilder();
			sb.Append("UPDATE ").Append(_commonUtils.QuoteSqlName(_tableRule?.Invoke(_table.DbName) ?? _table.DbName)).Append(" SET ");

			if (_set.Length > 0) { //指定 set 更新
				sb.Append(_set.ToString().Substring(2));

			} else if (_source.Count == 1) { //保存 Source
				_paramsSource.Clear();
				var colidx = 0;
				foreach (var col in _table.Columns.Values) {
					if (col.Attribute.IsIdentity == false && _ignore.ContainsKey(col.CsName) == false) {
						if (colidx > 0) sb.Append(", ");
						sb.Append(_commonUtils.QuoteSqlName(col.Attribute.Name)).Append(" = ");
						var value = _table.Properties.TryGetValue(col.CsName, out var tryp) ? tryp.GetValue(_source.First()) : null;
						if (_noneParameter) {
							sb.Append(_commonUtils.GetNoneParamaterSqlValue(_paramsSource, col.CsType, value));
						} else {
							sb.Append(_commonUtils.QuoteWriteParamter(col.CsType, _commonUtils.QuoteParamterName($"p_{_paramsSource.Count}")));
							_commonUtils.AppendParamter(_paramsSource, null, col.CsType, value);
						}
						++colidx;
					}
				}
				if (colidx == 0) return null;

			} else if (_source.Count > 1) { //批量保存 Source
				if (_table.Primarys.Any() == false) return null;

				var caseWhen = new StringBuilder();
				caseWhen.Append("CASE ");
				ToSqlCase(caseWhen, _table.Primarys);
				//if (_table.Primarys.Length > 1) caseWhen.Append("CONCAT(");
				//var pkidx = 0;
				//foreach (var pk in _table.Primarys) {
				//	if (pkidx > 0) caseWhen.Append(", ");
				//	caseWhen.Append(_commonUtils.QuoteSqlName(pk.Attribute.Name));
				//	++pkidx;
				//}
				//if (_table.Primarys.Length > 1) caseWhen.Append(")");
				var cw = caseWhen.ToString();

				_paramsSource.Clear();
				var colidx = 0;
				foreach (var col in _table.Columns.Values) {
					if (col.Attribute.IsIdentity == false && _ignore.ContainsKey(col.CsName) == false) {
						if (colidx > 0) sb.Append(", ");
						sb.Append(_commonUtils.QuoteSqlName(col.Attribute.Name)).Append(" = ").Append(cw);
						foreach (var d in _source) {
							sb.Append(" \r\nWHEN ");
							ToSqlWhen(sb, _table.Primarys, d);
							//if (_table.Primarys.Length > 1) sb.Append("CONCAT(");
							//pkidx = 0;
							//foreach (var pk in _table.Primarys) {
							//	if (pkidx > 0) sb.Append(", ");
							//	sb.Append(_commonUtils.FormatSql("{0}", _table.Properties.TryGetValue(pk.CsName, out var tryp2) ? tryp2.GetValue(d) : null));
							//	++pkidx;
							//}
							//if (_table.Primarys.Length > 1) sb.Append(")");
							sb.Append(" THEN ");
							var value = _table.Properties.TryGetValue(col.CsName, out var tryp) ? tryp.GetValue(d) : DBNull.Value;
							if (_noneParameter) {
								sb.Append(_commonUtils.GetNoneParamaterSqlValue(_paramsSource, col.CsType, value));
							} else {
								sb.Append(_commonUtils.QuoteWriteParamter(col.CsType, _commonUtils.QuoteParamterName($"p_{_paramsSource.Count}")));
								_commonUtils.AppendParamter(_paramsSource, null, col.CsType, value);
							}
						}
						sb.Append(" END");
						++colidx;
					}
				}
				if (colidx == 0) return null;
			} else
				return null;

			sb.Append(" \r\nWHERE ").Append(_where.ToString().Substring(5));
			return sb.ToString();
		}
	}
}