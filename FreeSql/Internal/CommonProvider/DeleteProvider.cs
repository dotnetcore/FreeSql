using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;

namespace FreeSql.Internal.CommonProvider {

	abstract partial class DeleteProvider<T1> : IDelete<T1> where T1 : class {
		protected IFreeSql _orm;
		protected CommonUtils _commonUtils;
		protected CommonExpression _commonExpression;
		protected List<T1> _source = new List<T1>();
		protected TableInfo _table;
		protected StringBuilder _where = new StringBuilder();
		protected int _whereTimes = 0;
		protected List<DbParameter> _params = new List<DbParameter>();

		public DeleteProvider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) {
			_orm = orm;
			_commonUtils = commonUtils;
			_commonExpression = commonExpression;
			_table = _commonUtils.GetTableByEntity(typeof(T1));
			_where.Append("DELETE FROM ").Append(_commonUtils.QuoteSqlName(_table.DbName)).Append(" WHERE ");
			this.Where(_commonUtils.WhereObject(_table, "", dywhere));
			if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure<T1>();
		}

		public long ExecuteAffrows() {
			var sql = this.ToSql();
			if (string.IsNullOrEmpty(sql)) return 0;
			return _orm.Ado.ExecuteNonQuery(CommandType.Text, sql, _params.ToArray());
		}
		public abstract List<T1> ExecuteDeleted();

		public IDelete<T1> Where(Expression<Func<T1, bool>> exp) => this.Where(_commonExpression.ExpressionWhereLambdaNoneForeignObject(null, null, exp?.Body, null));
		public IDelete<T1> Where(string sql, object parms = null) {
			if (string.IsNullOrEmpty(sql)) return this;
			if (++_whereTimes > 1) _where.Append(" AND ");
			_where.Append("(").Append(sql).Append(")");
			if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject(sql, parms));
			return this;
		}
		public IDelete<T1> Where(T1 item) => this.Where(new[] { item });
		public IDelete<T1> Where(IEnumerable<T1> items) => this.Where(_commonUtils.WhereItems(_table, "", items));
		public IDelete<T1> WhereExists<TEntity2>(ISelect<TEntity2> select, bool notExists = false) where TEntity2 : class => this.Where($"{(notExists ? "NOT " : "")}EXISTS({select.ToSql("1")})");

		public string ToSql() => _whereTimes <= 0 ? null : _where.ToString();
	}
}
