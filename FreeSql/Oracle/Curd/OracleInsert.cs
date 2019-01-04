using FreeSql.Internal;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.Oracle.Curd {

	class OracleInsert<T1> : Internal.CommonProvider.InsertProvider<T1> where T1 : class {
		public OracleInsert(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
			: base(orm, commonUtils, commonExpression) {
		}

		public override string ToSql() {
			if (_source == null || _source.Any() == false) return null;
			var sb = new StringBuilder();
			sb.Append("INSERT INTO ").Append(_commonUtils.QuoteSqlName(_table.DbName)).Append("(");
			var colidx = 0;
			foreach (var col in _table.Columns.Values)
				if (_ignore.ContainsKey(col.Attribute.Name) == false) {
					if (colidx > 0) sb.Append(", ");
					sb.Append(_commonUtils.QuoteSqlName(col.Attribute.Name));
					++colidx;
				}
			sb.Append(") VALUES");
			_params = new DbParameter[colidx * _source.Count];
			var didx = 0;
			foreach (var d in _source) {
				if (didx > 0) sb.Append(", ");
				sb.Append("(");
				var colidx2 = 0;
				foreach (var col in _table.Columns.Values)
					if (_ignore.ContainsKey(col.Attribute.Name) == false) {
						if (colidx2 > 0) sb.Append(", ");
						if (col.Attribute.IsIdentity) {
							sb.Append(_commonUtils.QuoteSqlName($"{Utils.GetCsName(_table.DbName)}_seq_{col.Attribute.Name}")).Append(".nextval");
						} else {
							sb.Append(_commonUtils.QuoteWriteParamter(col.CsType, $"{_commonUtils.QuoteParamterName(col.CsName)}{didx}"));
							_params[didx * colidx + colidx2] = _commonUtils.AppendParamter(null, $"{col.CsName}{didx}", col.CsType, _table.Properties.TryGetValue(col.CsName, out var tryp) ? tryp.GetValue(d) : null);
						}
						++colidx2;
					}
				sb.Append(")");
				++didx;
			}
			return sb.ToString();
		}

		public override long ExecuteIdentity() {
			var sql = this.ToSql();
			if (string.IsNullOrEmpty(sql)) return 0;

			var identCols = _table.Columns.Where(a => a.Value.Attribute.IsIdentity);
			if (identCols.Any() == false) {
				_orm.Ado.ExecuteNonQuery(CommandType.Text, sql, _params);
				return 0;
			}
			return long.TryParse(string.Concat(_orm.Ado.ExecuteScalar(CommandType.Text, string.Concat(sql, " RETURNING ", _commonUtils.QuoteSqlName(identCols.First().Value.Attribute.Name)), _params)), out var trylng) ? trylng : 0;
		}
		async public override Task<long> ExecuteIdentityAsync() {
			var sql = this.ToSql();
			if (string.IsNullOrEmpty(sql)) return 0;

			var identCols = _table.Columns.Where(a => a.Value.Attribute.IsIdentity);
			if (identCols.Any() == false) {
				await _orm.Ado.ExecuteNonQueryAsync(CommandType.Text, sql, _params);
				return 0;
			}
			return long.TryParse(string.Concat(await _orm.Ado.ExecuteScalarAsync(CommandType.Text, string.Concat(sql, " RETURNING ", _commonUtils.QuoteSqlName(identCols.First().Value.Attribute.Name)), _params)), out var trylng) ? trylng : 0;
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
