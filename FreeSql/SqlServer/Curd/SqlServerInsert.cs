using FreeSql.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace FreeSql.SqlServer.Curd {

	class SqlServerInsert<T1> : Internal.CommonProvider.InsertProvider<T1> where T1 : class {
		public SqlServerInsert(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
			: base(orm, commonUtils, commonExpression) {
		}

		public override long ExecuteIdentity() => int.TryParse(string.Concat(_orm.Ado.ExecuteScalar(CommandType.Text, string.Concat(this.ToSql(), "; SELECT SCOPE_IDENTITY();"), _params)), out var trylng) ? trylng : 0;

		public override List<T1> ExecuteInserted() {
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

			var validx = sql.IndexOf(") VALUES");
			if (validx == -1) throw new ArgumentException("找不到 VALUES");
			sb.Insert(0, sql.Substring(0, validx)).Insert(0, ")");
			sb.Append(sql.Substring(validx + 1));

			return _orm.Ado.Query<T1>(sb.ToString());
		}
	}
}
