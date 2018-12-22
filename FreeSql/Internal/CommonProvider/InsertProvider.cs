using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FreeSql.Internal.CommonProvider {

	abstract partial class InsertProvider<T1> : IInsert<T1> where T1 : class {
		protected IFreeSql _orm;
		protected CommonUtils _commonUtils;
		protected CommonExpression _commonExpression;
		protected List<T1> _source = new List<T1>();
		protected Dictionary<string, bool> _ignore = new Dictionary<string, bool>(StringComparer.CurrentCultureIgnoreCase);
		protected TableInfo _table;
		protected DbParameter[] _params;

		public InsertProvider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression) {
			_orm = orm;
			_commonUtils = commonUtils;
			_commonExpression = commonExpression;
			_table = _commonUtils.GetTableByEntity(typeof(T1));
			if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure<T1>();
		}

		public IInsert<T1> AppendData(T1 source) {
			if (source != null) _source.Add(source);
			return this;
		}
		public IInsert<T1> AppendData(IEnumerable<T1> source) {
			if (source != null) _source.AddRange(source.Where(a => a != null));
			return this;
		}

		public long ExecuteAffrows() => _orm.Ado.ExecuteNonQuery(CommandType.Text, this.ToSql(), _params);
		public abstract long ExecuteIdentity();
		public abstract List<T1> ExecuteInserted();

		public IInsert<T1> IgnoreColumns(Expression<Func<T1, object>> columns) {
			var cols = _commonExpression.ExpressionSelectColumns_MemberAccess_New_NewArrayInit(null, columns?.Body, false).Distinct();
			_ignore.Clear();
			foreach (var col in cols) _ignore.Add(col, true);
			return this;
		}
		public IInsert<T1> InsertColumns(Expression<Func<T1, object>> columns) {
			var cols = _commonExpression.ExpressionSelectColumns_MemberAccess_New_NewArrayInit(null, columns?.Body, false).ToDictionary(a => a, a => true);
			_ignore.Clear();
			foreach (var col in _table.Columns.Values)
				if (cols.ContainsKey(col.Attribute.Name) == false)
					_ignore.Add(col.Attribute.Name, true);
			return this;
		}

		public string ToSql() {
			if (_source == null || _source.Any() == false) return null;
			var sb = new StringBuilder();
			sb.Append("INSERT INTO ").Append(_commonUtils.QuoteSqlName(_table.DbName)).Append("(");
			var colidx = 0;
			foreach (var col in _table.Columns.Values)
				if (col.Attribute.IsIdentity == false && _ignore.ContainsKey(col.Attribute.Name) == false) {
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
					if (col.Attribute.IsIdentity == false && _ignore.ContainsKey(col.Attribute.Name) == false) {
						if (colidx2 > 0) sb.Append(", ");
						sb.Append(_commonUtils.QuoteParamterName(col.CsName)).Append(didx);
						_params[didx * colidx + colidx2] = _commonUtils.AppendParamter(null, $"{col.CsName}{didx}", _table.Properties.TryGetValue(col.CsName, out var tryp) ? tryp.GetValue(d) : DBNull.Value);
						++colidx2;
					}
				sb.Append(")");
				++didx;
			}
			return sb.ToString();
		}
	}
}

