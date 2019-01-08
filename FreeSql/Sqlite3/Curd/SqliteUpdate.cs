using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.Sqlite3.Curd {

	class SqliteUpdate<T1> : Internal.CommonProvider.UpdateProvider<T1> where T1 : class {

		public SqliteUpdate(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere)
			: base(orm, commonUtils, commonExpression, dywhere) {
		}

		public override List<T1> ExecuteUpdated() {
			throw new NotImplementedException();
		}
		public override Task<List<T1>> ExecuteUpdatedAsync() {
			throw new NotImplementedException();
		}

		protected override void ToSqlCase(StringBuilder caseWhen, ColumnInfo[] primarys) {
			if (_table.Primarys.Length == 1) {
				caseWhen.Append(_commonUtils.QuoteReadColumn(_table.Primarys.First().CsType, _commonUtils.QuoteSqlName(_table.Primarys.First().Attribute.Name)));
				return;
			}
			caseWhen.Append("CONCAT(");
			var pkidx = 0;
			foreach (var pk in _table.Primarys) {
				if (pkidx > 0) caseWhen.Append(", ");
				caseWhen.Append(_commonUtils.QuoteReadColumn(pk.CsType, _commonUtils.QuoteSqlName(pk.Attribute.Name)));
				++pkidx;
			}
			caseWhen.Append(")");
		}

		protected override void ToSqlWhen(StringBuilder sb, ColumnInfo[] primarys, object d) {
			if (_table.Primarys.Length == 1) {
				sb.Append(_commonUtils.FormatSql("{0}", _table.Properties.TryGetValue(_table.Primarys.First().CsName, out var tryp2) ? tryp2.GetValue(d) : null));
				return;
			}
			sb.Append("CONCAT(");
			var pkidx = 0;
			foreach (var pk in _table.Primarys) {
				if (pkidx > 0) sb.Append(", ");
				sb.Append(_commonUtils.FormatSql("{0}", _table.Properties.TryGetValue(pk.CsName, out var tryp2) ? tryp2.GetValue(d) : null));
				++pkidx;
			}
			sb.Append(")");
		}
	}
}
