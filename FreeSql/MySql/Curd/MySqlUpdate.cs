using FreeSql.Internal;
using FreeSql.Internal.Model;
using System.Collections.Generic;
using System.Text;

namespace FreeSql.MySql.Curd {

	class MySqlUpdate<T1> : Internal.CommonProvider.UpdateProvider<T1> where T1 : class {

		public MySqlUpdate(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere)
			: base(orm, commonUtils, commonExpression, dywhere) {
		}

		public override List<T1> ExecuteUpdated() {
			var sb = new StringBuilder();
			sb.Append(this.ToSql()).Append(" RETURNING ");

			var colidx = 0;
			foreach (var col in _table.Columns.Values) {
				if (colidx > 0) sb.Append(", ");
				sb.Append(_commonUtils.QuoteSqlName(col.Attribute.Name)).Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
				++colidx;
			}
			return _orm.Ado.Query<T1>(sb.ToString());
		}

		protected override void ToSqlCase(StringBuilder caseWhen, ColumnInfo[] primarys) {
			if (_table.Primarys.Length > 1) caseWhen.Append("CONCAT(");
			var pkidx = 0;
			foreach (var pk in _table.Primarys) {
				if (pkidx > 0) caseWhen.Append(", ");
				caseWhen.Append(_commonUtils.QuoteSqlName(pk.Attribute.Name));
				++pkidx;
			}
			if (_table.Primarys.Length > 1) caseWhen.Append(")");
		}

		protected override void ToSqlWhen(StringBuilder sb, ColumnInfo[] primarys, object d) {
			if (_table.Primarys.Length > 1) sb.Append("CONCAT(");
			var pkidx = 0;
			foreach (var pk in _table.Primarys) {
				if (pkidx > 0) sb.Append(", ");
				sb.Append(_commonUtils.FormatSql("{0}", _table.Properties.TryGetValue(pk.CsName, out var tryp2) ? tryp2.GetValue(d) : null));
				++pkidx;
			}
			if (_table.Primarys.Length > 1) sb.Append(")");
		}
	}
}
