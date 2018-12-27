using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.Internal.CommonProvider {

	abstract class Select0Provider<TSelect, T1> : ISelect0<TSelect, T1> where TSelect : class where T1 : class {

		protected int _limit, _skip;
		protected string _select = "SELECT ", _orderby, _groupby, _having;
		protected StringBuilder _where = new StringBuilder();
		protected List<DbParameter> _params = new List<DbParameter>();
		internal List<SelectTableInfo> _tables = new List<SelectTableInfo>();
		protected StringBuilder _join = new StringBuilder();
		protected (int seconds, string key) _cache = (0, null);
		protected IFreeSql _orm;
		protected CommonUtils _commonUtils;
		protected CommonExpression _commonExpression;

		internal static void CopyData(Select0Provider<TSelect, T1> from, object to) {
			var toType = to?.GetType();
			if (toType == null) return;
			toType.GetField("_limit", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(to, from._limit);
			toType.GetField("_skip", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(to, from._skip);
			toType.GetField("_select", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(to, from._select);
			toType.GetField("_where", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(to, new StringBuilder().Append(from._where.ToString()));
			toType.GetField("_params", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(to, new List<DbParameter>(from._params.ToArray()));
			toType.GetField("_tables", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(to, new List<SelectTableInfo>(from._tables.ToArray()));
			toType.GetField("_join", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(to, new StringBuilder().Append(from._join.ToString()));
			toType.GetField("_cache", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(to, from._cache);
			//toType.GetField("_orm", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(to, from._orm);
			//toType.GetField("_commonUtils", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(to, from._commonUtils);
			//toType.GetField("_commonExpression", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(to, from._commonExpression);
		}

		public Select0Provider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) {
			_orm = orm;
			_commonUtils = commonUtils;
			_commonExpression = commonExpression;
			_tables.Add(new SelectTableInfo { Table = _commonUtils.GetTableByEntity(typeof(T1)), Alias = "a", On = null, Type = SelectTableInfoType.From });
			this.Where(_commonUtils.WhereObject(_tables.First().Table, "a.", dywhere));
			if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure<T1>();
		}

		public bool Any() {
			this.Limit(1);
			return this.ToList<int>("1").FirstOrDefault() == 1;
		}
		async public Task<bool> AnyAsync() {
			this.Limit(1);
			return (await this.ToListAsync<int>("1")).FirstOrDefault() == 1;
		}

		public TSelect Caching(int seconds, string key = null) {
			_cache = (seconds, key);
			return this as TSelect;
		}
		public long Count() => this.ToList<int>("count(1)").FirstOrDefault();
		async public Task<long> CountAsync() => (await this.ToListAsync<int>("count(1)")).FirstOrDefault();
		
		public TSelect Count(out long count) {
			count = this.Count();
			return this as TSelect;
		}

		public TSelect GroupBy(string sql, object parms = null) {
			_groupby = sql;
			if (string.IsNullOrEmpty(_groupby)) return this as TSelect;
			_groupby = string.Concat(" \r\nGROUP BY ", _groupby);
			if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject(_groupby, parms));
			return this as TSelect;
		}
		public TSelect Having(string sql, object parms = null) {
			if (string.IsNullOrEmpty(_groupby) || string.IsNullOrEmpty(sql)) return this as TSelect;
			_having = string.Concat(_having, " AND (", sql, ")");
			if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject(sql, parms));
			return this as TSelect;
		}

		public TSelect LeftJoin(Expression<Func<T1, bool>> exp) => this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
		public TSelect InnerJoin(Expression<Func<T1, bool>> exp) => this.InternalJoin(exp?.Body, SelectTableInfoType.InnerJoin);
		public TSelect RightJoin(Expression<Func<T1, bool>> exp) => this.InternalJoin(exp?.Body, SelectTableInfoType.RightJoin);
		public TSelect LeftJoin<T2>(Expression<Func<T1, T2, bool>> exp) => this.InternalJoin(exp?.Body, SelectTableInfoType.LeftJoin);
		public TSelect InnerJoin<T2>(Expression<Func<T1, T2, bool>> exp) => this.InternalJoin(exp?.Body, SelectTableInfoType.InnerJoin);
		public TSelect RightJoin<T2>(Expression<Func<T1, T2, bool>> exp) => this.InternalJoin(exp?.Body, SelectTableInfoType.RightJoin);

		public TSelect InnerJoin(string sql, object parms = null) {
			if (string.IsNullOrEmpty(sql)) return this as TSelect;
			_join.Append(" \r\nINNER JOIN ").Append(sql);
			if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject(sql, parms));
			return this as TSelect;
		}
		public TSelect LeftJoin(string sql, object parms = null) {
			if (string.IsNullOrEmpty(sql)) return this as TSelect;
			_join.Append(" \r\nLEFT JOIN ").Append(sql);
			if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject(sql, parms));
			return this as TSelect;
		}

		public TSelect Limit(int limit) {
			_limit = limit;
			return this as TSelect;
		}
		public TSelect Master() {
			_select = " SELECT ";
			return this as TSelect;
		}
		public TSelect Offset(int offset) => this.Skip(offset) as TSelect;

		public TSelect OrderBy(string sql, object parms = null) {
			if (string.IsNullOrEmpty(sql)) _orderby = null;
			var isnull = string.IsNullOrEmpty(_orderby);
			_orderby = string.Concat(isnull ? " \r\nORDER BY " : "", _orderby, isnull ? "" : ", ", sql);
			if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject(sql, parms));
			return this as TSelect;
		}
		public TSelect Page(int pageIndex, int pageSize) {
			this.Skip(Math.Max(0, pageIndex - 1) * pageSize);
			return this.Limit(pageSize) as TSelect;
		}

		public TSelect RightJoin(string sql, object parms = null) {
			if (string.IsNullOrEmpty(sql)) return this as TSelect;
			_join.Append(" \r\nRIGHT JOIN ").Append(sql);
			if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject(sql, parms));
			return this as TSelect;
		}

		public TSelect Skip(int offset) {
			_skip = offset;
			return this as TSelect;
		}
		public TSelect Take(int limit) => this.Limit(limit) as TSelect;

		public List<TTuple> ToList<TTuple>(string field) {
			var sql = this.ToSql(field);
			if (_cache.seconds > 0 && string.IsNullOrEmpty(_cache.key)) _cache.key = sql;

			return _orm.Cache.Shell(_cache.key, _cache.seconds, () => {
				List<TTuple> ret = new List<TTuple>();
				Type type = typeof(TTuple);
				var ds = _orm.Ado.ExecuteArray(CommandType.Text, sql, _params.ToArray());
				foreach (var dr in ds) {
					var read = Utils.ExecuteArrayRowReadClassOrTuple(type, null, dr);
					ret.Add(read.value == null ? default(TTuple) : (TTuple)read.value);
				}
				return ret;
			});
		}
		public Task<List<TTuple>> ToListAsync<TTuple>(string field) {
			var sql = this.ToSql(field);
			if (_cache.seconds > 0 && string.IsNullOrEmpty(_cache.key)) _cache.key = sql;

			return _orm.Cache.ShellAsync(_cache.key, _cache.seconds, async () => {
				List<TTuple> ret = new List<TTuple>();
				Type type = typeof(TTuple);
				var ds = await _orm.Ado.ExecuteArrayAsync(CommandType.Text, sql, _params.ToArray());
				foreach (var dr in ds) {
					var read = Utils.ExecuteArrayRowReadClassOrTuple(type, null, dr);
					ret.Add(read.value == null ? default(TTuple) : (TTuple)read.value);
				}
				return ret;
			});
		}
		public List<T1> ToList() {
			return this.ToListMapReader<T1>(this.GetAllField());
		}
		public Task<List<T1>> ToListAsync() {
			return this.ToListMapReaderAsync<T1>(this.GetAllField());
		}
		public T1 ToOne() {
			this.Limit(1);
			return this.ToList().FirstOrDefault();
		}
		async public Task<T1> ToOneAsync() {
			this.Limit(1);
			return (await this.ToListAsync()).FirstOrDefault();
		}

		protected List<TReturn> ToListMapReader<TReturn>((ReadAnonymousTypeInfo map, string field) af) {
			var sql = this.ToSql(af.field);
			if (_cache.seconds > 0 && string.IsNullOrEmpty(_cache.key)) _cache.key = $"{sql}{string.Join("|", _params.Select(a => a.Value))}";

			var drarr = _orm.Cache.Shell(_cache.key, _cache.seconds, () => _orm.Ado.ExecuteArray(CommandType.Text, sql, _params.ToArray()));
			var ret = new List<TReturn>();
			for (var a = 0; a < drarr.Length; a++) {
				var dr = drarr[a];
				var index = -1;
				ret.Add((TReturn)_commonExpression.ReadAnonymous(af.map, dr, ref index));
			}
			return ret;
		}
		async protected Task<List<TReturn>> ToListMapReaderAsync<TReturn>((ReadAnonymousTypeInfo map, string field) af) {
			var sql = this.ToSql(af.field);
			if (_cache.seconds > 0 && string.IsNullOrEmpty(_cache.key)) _cache.key = $"{sql}{string.Join("|", _params.Select(a => a.Value))}";

			var drarr = await _orm.Cache.ShellAsync(_cache.key, _cache.seconds, () => _orm.Ado.ExecuteArrayAsync(CommandType.Text, sql, _params.ToArray()));
			var ret = new List<TReturn>();
			for (var a = 0; a < drarr.Length; a++) {
				var dr = drarr[a];
				var index = -1;
				ret.Add((TReturn)_commonExpression.ReadAnonymous(af.map, dr, ref index));
			}
			return ret;
		}
		protected (ReadAnonymousTypeInfo map, string field) GetNewExpressionField(NewExpression newexp) {
			var map = new ReadAnonymousTypeInfo();
			var field = new StringBuilder();
			var index = 0;

			_commonExpression.ReadAnonymousField(_tables, field, map, ref index, newexp, null);
			return (map, map.Childs.Count > 0 ? field.Remove(0, 2).ToString() : null);
		}
		protected (ReadAnonymousTypeInfo map, string field) GetAllField() {
			var type = typeof(T1);
			var map = new ReadAnonymousTypeInfo { Consturctor = type.GetConstructor(new Type[0]), ConsturctorType = ReadAnonymousTypeInfoConsturctorType.Properties };
			var field = new StringBuilder();
			var dicfield = new Dictionary<string, bool>();
			var tb = _tables.First();
			var index = 0;
			var ps = typeof(T1).GetProperties();
			foreach (var p in ps) {
				var child = new ReadAnonymousTypeInfo { CsName = p.Name };
				if (tb.Table.ColumnsByCs.TryGetValue(p.Name, out var col)) { //普通字段
					if (index > 0) field.Append(", ");
					var quoteName = _commonUtils.QuoteSqlName(col.Attribute.Name);
					field.Append(_commonUtils.QuoteReadColumn(col.CsType, $"{tb.Alias}.{quoteName}"));
					++index;
					if (dicfield.ContainsKey(quoteName)) field.Append(" as").Append(index);
					else dicfield.Add(quoteName, true);
				} else {
					var tb2 = _tables.Where(a => a.Table.Type == p.PropertyType && a.Alias.Contains(p.Name)).FirstOrDefault();
					if (tb2 == null && ps.Where(pw => pw.PropertyType == p.PropertyType).Count() == 1) tb2 = _tables.Where(a => a.Table.Type == p.PropertyType).FirstOrDefault();
					if (tb2 == null) continue;
					child.Consturctor = tb2.Table.Type.GetConstructor(new Type[0]);
					child.ConsturctorType = ReadAnonymousTypeInfoConsturctorType.Properties;
					foreach (var col2 in tb2.Table.Columns.Values) {
						if (index > 0) field.Append(", ");
						var quoteName = _commonUtils.QuoteSqlName(col2.Attribute.Name);
						field.Append(_commonUtils.QuoteReadColumn(col2.CsType, $"{tb2.Alias}.{quoteName}"));
						++index;
						if (dicfield.ContainsKey(quoteName)) field.Append(" as").Append(index);
						else dicfield.Add(quoteName, true);
						child.Childs.Add(new ReadAnonymousTypeInfo { CsName = col2.CsName });
					}
				}
				map.Childs.Add(child);
			}
			return (map, field.ToString());
		}
		public abstract string ToSql(string field = null);

		public TSelect Where(string sql, object parms = null) => this.WhereIf(true, sql, parms);
		public TSelect WhereIf(bool condition, string sql, object parms = null) {
			if (condition == false || string.IsNullOrEmpty(sql)) return this as TSelect;
			_where.Append(" AND (").Append(sql).Append(")");
			if (parms != null) _params.AddRange(_commonUtils.GetDbParamtersByObject(sql, parms));
			return this as TSelect;
		}
		#region common

		protected TMember InternalAvg<TMember>(Expression exp) => this.ToList<TMember>($"avg({_commonExpression.ExpressionSelectColumn_MemberAccess(_tables, null, SelectTableInfoType.From, exp, true, null)})").FirstOrDefault();
		async protected Task<TMember> InternalAvgAsync<TMember>(Expression exp) => (await this.ToListAsync<TMember>($"avg({_commonExpression.ExpressionSelectColumn_MemberAccess(_tables, null, SelectTableInfoType.From, exp, true, null)})")).FirstOrDefault();
		protected TMember InternalMax<TMember>(Expression exp) => this.ToList<TMember>($"max({_commonExpression.ExpressionSelectColumn_MemberAccess(_tables, null, SelectTableInfoType.From, exp, true, null)})").FirstOrDefault();
		async protected Task<TMember> InternalMaxAsync<TMember>(Expression exp) => (await this.ToListAsync<TMember>($"max({_commonExpression.ExpressionSelectColumn_MemberAccess(_tables, null, SelectTableInfoType.From, exp, true, null)})")).FirstOrDefault();
		protected TMember InternalMin<TMember>(Expression exp) => this.ToList<TMember>($"min({_commonExpression.ExpressionSelectColumn_MemberAccess(_tables, null, SelectTableInfoType.From, exp, true, null)})").FirstOrDefault();
		async protected Task<TMember> InternalMinAsync<TMember>(Expression exp) => (await this.ToListAsync<TMember>($"min({_commonExpression.ExpressionSelectColumn_MemberAccess(_tables, null, SelectTableInfoType.From, exp, true, null)})")).FirstOrDefault();
		protected TMember InternalSum<TMember>(Expression exp) => this.ToList<TMember>($"sum({_commonExpression.ExpressionSelectColumn_MemberAccess(_tables, null, SelectTableInfoType.From, exp, true, null)})").FirstOrDefault();
		async protected Task<TMember> InternalSumAsync<TMember>(Expression exp) => (await this.ToListAsync<TMember>($"sum({_commonExpression.ExpressionSelectColumn_MemberAccess(_tables, null, SelectTableInfoType.From, exp, true, null)})")).FirstOrDefault();

		protected ISelectGrouping<TKey> InternalGroupBy<TKey>(Expression columns) {
			var map = new ReadAnonymousTypeInfo();
			var field = new StringBuilder();
			var index = -10000; //临时规则，不返回 as1

			_commonExpression.ReadAnonymousField(_tables, field, map, ref index, columns, null);
			this.GroupBy(map.Childs.Count > 0 ? field.Remove(0, 2).ToString() : null);
			return new SelectGroupingProvider<TKey>(this, map, _commonExpression);
		}
		protected TSelect InternalJoin(Expression exp, SelectTableInfoType joinType) {
			_commonExpression.ExpressionJoinLambda(_tables, joinType, exp, null);
			return this as TSelect;
		}
		protected TSelect InternalJoin<T2>(Expression exp, SelectTableInfoType joinType) {
			var tb = _commonUtils.GetTableByEntity(typeof(T2));
			if (tb == null) throw new ArgumentException("T2 类型错误");
			_tables.Add(new SelectTableInfo { Table = tb, Alias = $"IJ{_tables.Count}", On = null, Type = joinType });
			_commonExpression.ExpressionJoinLambda(_tables, joinType, exp, null);
			return this as TSelect;
		}
		protected TSelect InternalOrderBy(Expression column) => this.OrderBy(_commonExpression.ExpressionSelectColumn_MemberAccess(_tables, null, SelectTableInfoType.From, column, true, null));
		protected TSelect InternalOrderByDescending(Expression column) => this.OrderBy($"{_commonExpression.ExpressionSelectColumn_MemberAccess(_tables, null, SelectTableInfoType.From, column, true, null)} DESC");

		protected List<TReturn> InternalToList<TReturn>(Expression select) => this.ToListMapReader<TReturn>(this.GetNewExpressionField(select as NewExpression));
		protected Task<List<TReturn>> InternalToListAsync<TReturn>(Expression select) => this.ToListMapReaderAsync<TReturn>(this.GetNewExpressionField(select as NewExpression));
		protected string InternalToSql<TReturn>(Expression select) => this.ToSql(this.GetNewExpressionField(select as NewExpression).field);

		protected TReturn InternalToAggregate<TReturn>(Expression select) {
			var map = new ReadAnonymousTypeInfo();
			var field = new StringBuilder();
			var index = 0;

			_commonExpression.ReadAnonymousField(_tables, field, map, ref index, select, null);
			return this.ToListMapReader<TReturn>((map, map.Childs.Count > 0 ? field.Remove(0, 2).ToString() : null)).FirstOrDefault();
		}
		async protected Task<TReturn> InternalToAggregateAsync<TReturn>(Expression select) {
			var map = new ReadAnonymousTypeInfo();
			var field = new StringBuilder();
			var index = 0;

			_commonExpression.ReadAnonymousField(_tables, field, map, ref index, select, null);
			return (await this.ToListMapReaderAsync<TReturn>((map, map.Childs.Count > 0 ? field.Remove(0, 2).ToString() : null))).FirstOrDefault();
		}

		protected TSelect InternalWhere(Expression exp) => this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp, null));

		protected TSelect InternalJoin(Expression exp) {
			return this as TSelect;
		}
		#endregion
	}
}