using FreeSql.Internal;
using SafeObjectPool;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.SqlServer.Curd {

	class SqlServerInsert<T1> : Internal.CommonProvider.InsertProvider<T1> where T1 : class {
		public SqlServerInsert(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
			: base(orm, commonUtils, commonExpression) {
		}

		public override int ExecuteAffrows() => base.SplitExecuteAffrows(1000, 2100);
		public override Task<int> ExecuteAffrowsAsync() => base.SplitExecuteAffrowsAsync(1000, 2100);
		public override long ExecuteIdentity() => base.SplitExecuteIdentity(1000, 2100);
		public override Task<long> ExecuteIdentityAsync() => base.SplitExecuteIdentityAsync(1000, 2100);
		public override List<T1> ExecuteInserted() => base.SplitExecuteInserted(1000, 2100);
		public override Task<List<T1>> ExecuteInsertedAsync() => base.SplitExecuteInsertedAsync(1000, 2100);


		internal override long RawExecuteIdentity() {
			var sql = this.ToSql();
			if (string.IsNullOrEmpty(sql)) return 0;

			return long.TryParse(string.Concat(_orm.Ado.ExecuteScalar(_connection, _transaction, CommandType.Text, string.Concat(sql, "; SELECT SCOPE_IDENTITY();"), _params)), out var trylng) ? trylng : 0;
		}
		async internal override Task<long> RawExecuteIdentityAsync() {
			var sql = this.ToSql();
			if (string.IsNullOrEmpty(sql)) return 0;

			return long.TryParse(string.Concat(await _orm.Ado.ExecuteScalarAsync(_connection, _transaction, CommandType.Text, string.Concat(sql, "; SELECT SCOPE_IDENTITY();"), _params)), out var trylng) ? trylng : 0;
		}

		internal override List<T1> RawExecuteInserted() {
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

			return _orm.Ado.Query<T1>(_connection, _transaction, CommandType.Text, sb.ToString(), _params);
		}
		async internal override Task<List<T1>> RawExecuteInsertedAsync() {
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

			return await _orm.Ado.QueryAsync<T1>(_connection, _transaction, CommandType.Text, sb.ToString(), _params);
		}
	}
}