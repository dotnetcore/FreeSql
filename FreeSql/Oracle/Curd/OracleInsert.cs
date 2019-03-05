using FreeSql.Internal;
using FreeSql.Internal.Model;
using Oracle.ManagedDataAccess.Client;
using System;
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
			sb.Append("INSERT ");
			if (_source.Count > 1) sb.Append("ALL");

			_identCol = null;
			var sbtb = new StringBuilder();
			sbtb.Append("INTO ");
			sbtb.Append(_commonUtils.QuoteSqlName(_tableRule?.Invoke(_table.DbName) ?? _table.DbName)).Append("(");
			var colidx = 0;
			foreach (var col in _table.Columns.Values) {
				if (col.Attribute.IsIdentity == true) {
					_identCol = col;
					continue;
				}
				if (col.Attribute.IsIdentity == false && _ignore.ContainsKey(col.Attribute.Name) == false) {
					if (colidx > 0) sbtb.Append(", ");
					sbtb.Append(_commonUtils.QuoteSqlName(col.Attribute.Name));
					++colidx;
				}
			}
			sbtb.Append(") ");

			_params = new DbParameter[colidx * _source.Count];
			var didx = 0;
			foreach (var d in _source) {
				if (_source.Count > 1) sb.Append("\r\n");
				sb.Append(sbtb);
				sb.Append("VALUES");
				sb.Append("(");
				var colidx2 = 0;
				foreach (var col in _table.Columns.Values) {
					if (col.Attribute.IsIdentity == false && _ignore.ContainsKey(col.Attribute.Name) == false) {
						if (colidx2 > 0) sb.Append(", ");
						sb.Append(_commonUtils.QuoteWriteParamter(col.CsType, $"{_commonUtils.QuoteParamterName(col.CsName)}{didx}"));
						object val = null;
						if (_table.Properties.TryGetValue(col.CsName, out var tryp)) {
							val = tryp.GetValue(d);
							if (col.Attribute.IsPrimary && (col.CsType == typeof(Guid) || col.CsType == typeof(Guid?))
								&& (val == null || (Guid)val == Guid.Empty)) tryp.SetValue(d, val = FreeUtil.NewMongodbId());
						}
						_params[didx * colidx + colidx2] = _commonUtils.AppendParamter(null, $"{col.CsName}{didx}", col.CsType, val);
						++colidx2;
					}
				}
				sb.Append(")");
				++didx;
			}
			if (_source.Count > 1) sb.Append("\r\n SELECT 1 FROM DUAL");
			return sb.ToString();
		}

		ColumnInfo _identCol;
		public override long ExecuteIdentity() {
			var sql = this.ToSql();
			if (string.IsNullOrEmpty(sql)) return 0;

			if (_identCol == null || _source.Count > 1) {
				_orm.Ado.ExecuteNonQuery(CommandType.Text, sql, _params);
				return 0;
			}
			var identColName = _commonUtils.QuoteSqlName(_identCol.Attribute.Name);
			var identParam = _commonUtils.AppendParamter(null, $"{_identCol.CsName}99", _identCol.CsType, 0) as OracleParameter;
			identParam.Direction = ParameterDirection.Output;
			_orm.Ado.ExecuteNonQuery(CommandType.Text, $"{sql} RETURNING {identColName} INTO {identParam.ParameterName}", _params.Concat(new[] { identParam }).ToArray());
			return long.TryParse(string.Concat(identParam.Value), out var trylng) ? trylng : 0;
		}
		async public override Task<long> ExecuteIdentityAsync() {
			var sql = this.ToSql();
			if (string.IsNullOrEmpty(sql)) return 0;

			if (_identCol == null || _source.Count > 1) {
				await _orm.Ado.ExecuteNonQueryAsync(CommandType.Text, sql, _params);
				return 0;
			}
			var identColName = _commonUtils.QuoteSqlName(_identCol.Attribute.Name);
			var identParam = _commonUtils.AppendParamter(null, $"{_identCol.CsName}99", _identCol.CsType, 0) as OracleParameter;
			identParam.Direction = ParameterDirection.Output;
			await _orm.Ado.ExecuteNonQueryAsync(CommandType.Text, $"{sql} RETURNING {identColName} INTO {identParam.ParameterName}", _params.Concat(new[] { identParam }).ToArray());
			return long.TryParse(string.Concat(identParam.Value), out var trylng) ? trylng : 0;
		}

		public override List<T1> ExecuteInserted() {
			throw new NotImplementedException();

//			var sql = this.ToSql();
//			if (string.IsNullOrEmpty(sql)) return new List<T1>();

//			var sb = new StringBuilder();
//			sb.Append(@"declare
//type v_tp_rec is record(");

//			var colidx = 0;
//			foreach (var col in _table.Columns.Values) {
//				if (colidx > 0) sb.Append(", ");
//				sb.Append(_commonUtils.QuoteSqlName(col.CsName)).Append(" ").Append(_commonUtils.QuoteSqlName(_tableRule?.Invoke(_table.DbName) ?? _table.DbName))).Append(".").Append(_commonUtils.QuoteSqlName(col.Attribute.Name)).Append("%type");
//				++colidx;
//			}
//			sb.Append(@");
//type v_tp_tab is table of v_tp_rec;
//v_tab v_tp_tab;
//begin
//");

//			sb.Append(sql).Append(" RETURNING ");
//			colidx = 0;
//			foreach (var col in _table.Columns.Values) {
//				if (colidx > 0) sb.Append(", ");
//				sb.Append(_commonUtils.QuoteReadColumn(col.CsType, _commonUtils.QuoteSqlName(col.Attribute.Name)));
//				++colidx;
//			}
//			sb.Append(@"bulk collect into v_tab;
//for i in 1..v_tab.count loop
//	dbms_output.put_line(");
//			//v_tab(i).empno||'-'||v_tab(i).ename
//			colidx = 0;
//			foreach (var col in _table.Columns.Values) {
//				if (colidx > 0) sb.Append("||'-'||");
//				sb.Append("v_tab(i).").Append(_commonUtils.QuoteSqlName(col.CsName));
//				++colidx;
//			}
//			sb.Append(@");
//end loop;
//end;
//");
//			return _orm.Ado.Query<T1>(CommandType.Text, sb.ToString(), _params);
		}
		public override Task<List<T1>> ExecuteInsertedAsync() {
			throw new NotImplementedException();
		}
	}
}
