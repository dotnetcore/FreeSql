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

		public override int ExecuteAffrows() => base.SplitExecuteAffrows(5000, 3000);
		public override Task<int> ExecuteAffrowsAsync() => base.SplitExecuteAffrowsAsync(5000, 3000);
		public override long ExecuteIdentity() => base.SplitExecuteIdentity(5000, 3000);
		public override Task<long> ExecuteIdentityAsync() => base.SplitExecuteIdentityAsync(5000, 3000);
		public override List<T1> ExecuteInserted() => base.SplitExecuteInserted(5000, 3000);
		public override Task<List<T1>> ExecuteInsertedAsync() => base.SplitExecuteInsertedAsync(5000, 3000);


		internal override long RawExecuteIdentity() {
			var sql = this.ToSql();
			if (string.IsNullOrEmpty(sql)) return 0;

			return long.TryParse(string.Concat(_orm.Ado.ExecuteScalar(_connection, _transaction, CommandType.Text, string.Concat(sql, "; SELECT LAST_INSERT_ID();"), _params)), out var trylng) ? trylng : 0;
		}
		async internal override Task<long> RawExecuteIdentityAsync() {
			var sql = this.ToSql();
			if (string.IsNullOrEmpty(sql)) return 0;

			return long.TryParse(string.Concat(await _orm.Ado.ExecuteScalarAsync(_connection, _transaction, CommandType.Text, string.Concat(sql, "; SELECT LAST_INSERT_ID();"), _params)), out var trylng) ? trylng : 0;
		}
		internal override List<T1> RawExecuteInserted() {
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
			return _orm.Ado.Query<T1>(_connection, _transaction, CommandType.Text, sb.ToString(), _params);
		}
		async internal override Task<List<T1>> RawExecuteInsertedAsync() {
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
			return await _orm.Ado.QueryAsync<T1>(_connection, _transaction, CommandType.Text, sb.ToString(), _params);
		}
	}
}
