using FreeSql.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.Sqlite.Curd {

	class SqliteInsert<T1> : Internal.CommonProvider.InsertProvider<T1> where T1 : class {
		public SqliteInsert(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
			: base(orm, commonUtils, commonExpression) {
		}

		public override long ExecuteIdentity() {
			var sql = this.ToSql();
			if (string.IsNullOrEmpty(sql)) return 0;

			return long.TryParse(string.Concat(_orm.Ado.ExecuteScalar(_transaction, CommandType.Text, string.Concat(sql, "; SELECT last_insert_rowid();"), _params)), out var trylng) ? trylng : 0;
		}
		async public override Task<long> ExecuteIdentityAsync() {
			var sql = this.ToSql();
			if (string.IsNullOrEmpty(sql)) return 0;

			return long.TryParse(string.Concat(await _orm.Ado.ExecuteScalarAsync(_transaction, CommandType.Text, string.Concat(sql, "; SELECT last_insert_rowid();"), _params)), out var trylng) ? trylng : 0;
		}

		public override List<T1> ExecuteInserted() {
			throw new NotImplementedException();
		}
		public override Task<List<T1>> ExecuteInsertedAsync() {
			throw new NotImplementedException();
		}
	}
}
