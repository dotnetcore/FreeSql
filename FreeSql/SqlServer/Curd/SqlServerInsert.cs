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

		public override long ExecuteIdentity() {
			//if (_source?.Count > 999) {
			//	List<IInsert<T1>> inserts = new List<IInsert<T1>>();
			//	var idx = 0;
			//	while (idx < _source.Count) {
			//		var count = Math.Min(_source.Count, idx + 999) - idx;
			//		var insert = _orm.Insert<T1>().AppendData(_source.GetRange(idx, count));
			//		_
			//		inserts.Add(insert);
			//		idx += 999;
			//	}
			//	Object<DbConnection> conn = null;
			//	var trans = _transaction;
			//	if (_transaction == null) {
			//		conn = _orm.Ado.MasterPool.Get();
			//		trans = conn.Value.BeginTransaction();
			//	}
			//	try {
			//		for (var a = 0; a < inserts.Count; a++) {
			//			inserts[a].WithTransaction(trans)
			//		}
			//		if (_transaction == null) trans.Commit();
			//	} catch {
			//		if (_transaction == null) trans.Rollback();
			//		throw;
			//	}
			//}
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