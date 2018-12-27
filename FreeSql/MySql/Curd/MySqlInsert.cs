using FreeSql.Internal;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.MySql.Curd {

	class MySqlInsert<T1> : Internal.CommonProvider.InsertProvider<T1> where T1 : class {
		public MySqlInsert(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
			: base(orm, commonUtils, commonExpression) {
		}

		public override long ExecuteIdentity() {
			var sql = this.ToSql();
			if (string.IsNullOrEmpty(sql)) return 0;

			return long.TryParse(string.Concat(_orm.Ado.ExecuteScalar(CommandType.Text, string.Concat(sql, "; SELECT LAST_INSERT_ID();"), _params)), out var trylng) ? trylng : 0;
		}
		async public override Task<long> ExecuteIdentityAsync() {
			var sql = this.ToSql();
			if (string.IsNullOrEmpty(sql)) return 0;

			return long.TryParse(string.Concat(await _orm.Ado.ExecuteScalarAsync(CommandType.Text, string.Concat(sql, "; SELECT LAST_INSERT_ID();"), _params)), out var trylng) ? trylng : 0;
		}

		public override List<T1> ExecuteInserted() {
			var sql = this.ToSql();
			if (string.IsNullOrEmpty(sql)) return new List<T1>();

			var sb = new StringBuilder();
			sb.Append(sql).Append(" RETURNING ");

			var colidx = 0;
			foreach (var col in _table.Columns.Values) {
				if (colidx > 0) sb.Append(", ");
				sb.Append(_commonUtils.QuoteReadColumn(col.CsType, _commonUtils.QuoteSqlName(col.Attribute.Name))).Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
				++colidx;
			}
			return _orm.Ado.Query<T1>(CommandType.Text, sb.ToString(), _params);
		}
		async public override Task<List<T1>> ExecuteInsertedAsync() {
			var sql = this.ToSql();
			if (string.IsNullOrEmpty(sql)) return new List<T1>();

			var sb = new StringBuilder();
			sb.Append(sql).Append(" RETURNING ");

			var colidx = 0;
			foreach (var col in _table.Columns.Values) {
				if (colidx > 0) sb.Append(", ");
				sb.Append(_commonUtils.QuoteReadColumn(col.CsType, _commonUtils.QuoteSqlName(col.Attribute.Name))).Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
				++colidx;
			}
			return await _orm.Ado.QueryAsync<T1>(CommandType.Text, sb.ToString(), _params);
		}
	}
}
