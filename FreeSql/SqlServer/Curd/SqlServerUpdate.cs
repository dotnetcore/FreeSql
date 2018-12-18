using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeSql.SqlServer.Curd {

	class SqlServerUpdate<T1> : Internal.CommonProvider.UpdateProvider<T1> where T1 : class {

		public SqlServerUpdate(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere)
			: base(orm, commonUtils, commonExpression, dywhere) {
		}

		public override List<T1> ExecuteUpdated() {
			var sql = this.ToSql();
			if (string.IsNullOrEmpty(sql)) return new List<T1>();

			var sb = new StringBuilder();
			sb.Append(" OUTPUT ");
			var colidx = 0;
			foreach (var col in _table.Columns.Values) {
				if (colidx > 0) sb.Append(", ");
				sb.Append("INSERTED.").Append(_commonUtils.QuoteSqlName(col.Attribute.Name)).Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
				++colidx;
			}

			var validx = sql.IndexOf(" WHERE ");
			if (validx == -1) throw new ArgumentException("找不到 WHERE ");
			sb.Insert(0, sql.Substring(0, validx));
			sb.Append(sql.Substring(validx));

			return _orm.Ado.Query<T1>(sb.ToString());
		}

		protected override void ToSqlCase(StringBuilder caseWhen, ColumnInfo[] primarys) {
			if (_table.Primarys.Length > 1) caseWhen.Append("(");
			var pkidx = 0;
			foreach (var pk in _table.Primarys) {
				if (pkidx > 0) caseWhen.Append(", ");
				caseWhen.Append("cast(").Append(_commonUtils.QuoteSqlName(pk.Attribute.Name)).Append(" as varchar)");
				++pkidx;
			}
			if (_table.Primarys.Length > 1) caseWhen.Append(")");
		}

		protected override void ToSqlWhen(StringBuilder sb, ColumnInfo[] primarys, object d) {
			if (_table.Primarys.Length > 1) sb.Append("(");
			var pkidx = 0;
			foreach (var pk in _table.Primarys) {
				if (pkidx > 0) sb.Append(", ");
				sb.Append("cast(").Append(_commonUtils.FormatSql("{0}", _table.Properties.TryGetValue(pk.CsName, out var tryp2) ? tryp2.GetValue(d) : null)).Append(" as varchar)");
				++pkidx;
			}
			if (_table.Primarys.Length > 1) sb.Append(")");
		}
	}
}
