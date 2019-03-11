using FreeSql.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.SqlServer.Curd {

	class SqlServerInsert<T1> : Internal.CommonProvider.InsertProvider<T1> where T1 : class {
		public SqlServerInsert(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
			: base(orm, commonUtils, commonExpression) {
		}

		public override long ExecuteIdentity() {
			var sql = this.ToSql();
			if (string.IsNullOrEmpty(sql)) return 0;

			return long.TryParse(string.Concat(_orm.Ado.ExecuteScalar(_transaction, CommandType.Text, string.Concat(sql, "; SELECT SCOPE_IDENTITY();"), _params)), out var trylng) ? trylng : 0;
		}
		async public override Task<long> ExecuteIdentityAsync() {
			var sql = this.ToSql();
			if (string.IsNullOrEmpty(sql)) return 0;

			return long.TryParse(string.Concat(await _orm.Ado.ExecuteScalarAsync(_transaction, CommandType.Text, string.Concat(sql, "; SELECT SCOPE_IDENTITY();"), _params)), out var trylng) ? trylng : 0;
		}

		public override List<T1> ExecuteInserted() {
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

			var validx = sql.IndexOf(") VALUES");
			if (validx == -1) throw new ArgumentException("找不到 VALUES");
			sb.Insert(0, sql.Substring(0, validx + 1));
			sb.Append(sql.Substring(validx + 1));

			return _orm.Ado.Query<T1>(_transaction, CommandType.Text, sb.ToString(), _params);
		}
		async public override Task<List<T1>> ExecuteInsertedAsync() {
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

			var validx = sql.IndexOf(") VALUES");
			if (validx == -1) throw new ArgumentException("找不到 VALUES");
			sb.Insert(0, sql.Substring(0, validx + 1));
			sb.Append(sql.Substring(validx + 1));

			return await _orm.Ado.QueryAsync<T1>(_transaction, CommandType.Text, sb.ToString(), _params);
		}
	}
}
