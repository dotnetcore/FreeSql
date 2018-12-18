using FreeSql.Internal;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace FreeSql.MySql.Curd {

	class MySqlInsert<T1> : Internal.CommonProvider.InsertProvider<T1> where T1 : class {
		public MySqlInsert(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
			: base(orm, commonUtils, commonExpression) {
		}

		public override long ExecuteIdentity() => int.TryParse(string.Concat(_orm.Ado.ExecuteScalar(CommandType.Text, string.Concat(this.ToSql(), "; SELECT LAST_INSERT_ID();"), _params)), out var trylng) ? trylng : 0;

		public override List<T1> ExecuteInserted() {
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
	}
}
