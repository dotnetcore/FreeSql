using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.SqlServer.Curd {

	class SqlServerUpdate<T1> : Internal.CommonProvider.UpdateProvider<T1> where T1 : class {

		public SqlServerUpdate(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere)
			: base(orm, commonUtils, commonExpression, dywhere) {
		}

		public override int ExecuteAffrows() => base.SplitExecuteAffrows(500, 2100);
		public override Task<int> ExecuteAffrowsAsync() => base.SplitExecuteAffrowsAsync(500, 2100);
		public override List<T1> ExecuteUpdated() => base.SplitExecuteUpdated(500, 2100);
		public override Task<List<T1>> ExecuteUpdatedAsync() => base.SplitExecuteUpdatedAsync(500, 2100);


		internal override List<T1> RawExecuteUpdated() {
			var sql = this.ToSql();
			if (string.IsNullOrEmpty(sql)) return new List<T1>();

			var sb = new StringBuilder();
			sb.Append(" OUTPUT ");
			var colidx = 0;
			foreach (var col in _table.Columns.Values) {
				if (colidx > 0) sb.Append(", ");
				sb.Append(_commonUtils.QuoteReadColumn(col.CsType, $"INSERTED.{_commonUtils.QuoteSqlName(col.Attribute.Name)}")).Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
				++colidx;
			}

			var validx = sql.IndexOf(" WHERE ");
			if (validx == -1) throw new ArgumentException("找不到 WHERE ");
			sb.Insert(0, sql.Substring(0, validx));
			sb.Append(sql.Substring(validx));

			var ret = _orm.Ado.Query<T1>(_connection, _transaction, CommandType.Text, sb.ToString(), _params.Concat(_paramsSource).ToArray());
			ValidateVersionAndThrow(ret.Count);
			return ret;
		}
		async internal override Task<List<T1>> RawExecuteUpdatedAsync() {
			var sql = this.ToSql();
			if (string.IsNullOrEmpty(sql)) return new List<T1>();

			var sb = new StringBuilder();
			sb.Append(" OUTPUT ");
			var colidx = 0;
			foreach (var col in _table.Columns.Values) {
				if (colidx > 0) sb.Append(", ");
				sb.Append(_commonUtils.QuoteReadColumn(col.CsType, $"INSERTED.{_commonUtils.QuoteSqlName(col.Attribute.Name)}")).Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
				++colidx;
			}

			var validx = sql.IndexOf(" WHERE ");
			if (validx == -1) throw new ArgumentException("找不到 WHERE ");
			sb.Insert(0, sql.Substring(0, validx));
			sb.Append(sql.Substring(validx));

			var ret = await _orm.Ado.QueryAsync<T1>(_connection, _transaction, CommandType.Text, sb.ToString(), _params.Concat(_paramsSource).ToArray());
			ValidateVersionAndThrow(ret.Count);
			return ret;
		}

		protected override void ToSqlCase(StringBuilder caseWhen, ColumnInfo[] primarys) {
			if (_table.Primarys.Length == 1) {
				caseWhen.Append(_commonUtils.QuoteReadColumn(_table.Primarys.First().CsType, _commonUtils.QuoteSqlName(_table.Primarys.First().Attribute.Name)));
				return;
			}
			caseWhen.Append("(");
			var pkidx = 0;
			foreach (var pk in _table.Primarys) {
				if (pkidx > 0) caseWhen.Append(", ");
				caseWhen.Append("cast(").Append(_commonUtils.QuoteReadColumn(_table.Primarys.First().CsType, _commonUtils.QuoteSqlName(pk.Attribute.Name))).Append(" as varchar)");
				++pkidx;
			}
			caseWhen.Append(")");
		}

		protected override void ToSqlWhen(StringBuilder sb, ColumnInfo[] primarys, object d) {
			if (_table.Primarys.Length == 1) {
				sb.Append(_commonUtils.FormatSql("{0}", _table.Properties.TryGetValue(_table.Primarys.First().CsName, out var tryp2) ? tryp2.GetValue(d) : null));
				return;
			}
			var pkidx = 0;
			foreach (var pk in _table.Primarys) {
				if (pkidx > 0) sb.Append(", ");
				sb.Append("cast(").Append(_commonUtils.FormatSql("{0}", _table.Properties.TryGetValue(pk.CsName, out var tryp2) ? tryp2.GetValue(d) : null)).Append(" as varchar)");
				++pkidx;
			}
		}
	}
}
