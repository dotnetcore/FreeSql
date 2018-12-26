using FreeSql.Internal;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace FreeSql.MySql.Curd {

	class MySqlDelete<T1> : Internal.CommonProvider.DeleteProvider<T1> where T1 : class {
		public MySqlDelete(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere)
			: base(orm, commonUtils, commonExpression, dywhere) {
		}

		public override List<T1> ExecuteDeleted() {
			var sb = new StringBuilder();
			sb.Append(this.ToSql()).Append(" RETURNING ");

			var colidx = 0;
			foreach (var col in _table.Columns.Values) {
				if (colidx > 0) sb.Append(", ");
				sb.Append(_commonUtils.QuoteReadColumn(col.CsType, _commonUtils.QuoteSqlName(col.Attribute.Name))).Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
				++colidx;
			}
			return _orm.Ado.Query<T1>(CommandType.Text, sb.ToString(), _params.ToArray());
		}
	}
}
