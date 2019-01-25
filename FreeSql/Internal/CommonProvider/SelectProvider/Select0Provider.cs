using FreeSql.Internal.Model;
using System;
using System.Collections.Concurrent;
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
				_orm.Ado.ExecuteReader(dr => {
					var read = Utils.ExecuteArrayRowReadClassOrTuple(type, null, dr);
					ret.Add((TTuple)read.Value);
				}, CommandType.Text, sql, _params.ToArray());
				return ret;
			});
		}
		public Task<List<TTuple>> ToListAsync<TTuple>(string field) {
			var sql = this.ToSql(field);
			if (_cache.seconds > 0 && string.IsNullOrEmpty(_cache.key)) _cache.key = sql;

			return _orm.Cache.ShellAsync(_cache.key, _cache.seconds, async () => {
				List<TTuple> ret = new List<TTuple>();
				Type type = typeof(TTuple);
				await _orm.Ado.ExecuteReaderAsync(dr => {
					var read = Utils.ExecuteArrayRowReadClassOrTuple(type, null, dr);
					ret.Add((TTuple)read.Value);
					return Task.CompletedTask;
				}, CommandType.Text, sql, _params.ToArray());
				return ret;
			});
		}
		public List<T1> ToList() {
			var af = this.GetAllFieldExpressionTree();
			var sql = this.ToSql(af.Field);
			if (_cache.seconds > 0 && string.IsNullOrEmpty(_cache.key)) _cache.key = $"{sql}{string.Join("|", _params.Select(a => a.Value))}";

			return _orm.Cache.Shell(_cache.key, _cache.seconds, () => {
				List<T1> ret = new List<T1>();
				_orm.Ado.ExecuteReader(dr => {
					ret.Add(af.Read(dr));
				}, CommandType.Text, sql, _params.ToArray());
				return ret;
			});
		}
		async public Task<List<T1>> ToListAsync() {
			var af = this.GetAllFieldExpressionTree();
			var sql = this.ToSql(af.Field);
			if (_cache.seconds > 0 && string.IsNullOrEmpty(_cache.key)) _cache.key = $"{sql}{string.Join("|", _params.Select(a => a.Value))}";

			return await _orm.Cache.ShellAsync(_cache.key, _cache.seconds, async () => {
				List<T1> ret = new List<T1>();
				await _orm.Ado.ExecuteReaderAsync(dr => {
					ret.Add(af.Read(dr));
					return Task.CompletedTask;
				}, CommandType.Text, sql, _params.ToArray());
				return ret;
			});
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

			return _orm.Cache.Shell(_cache.key, _cache.seconds, () => {
				List<TReturn> ret = new List<TReturn>();
				Type type = typeof(TReturn);
				_orm.Ado.ExecuteReader(dr => {
					var index = -1;
					ret.Add((TReturn)_commonExpression.ReadAnonymous(af.map, dr, ref index));
				}, CommandType.Text, sql, _params.ToArray());
				return ret;
			});
		}
		async protected Task<List<TReturn>> ToListMapReaderAsync<TReturn>((ReadAnonymousTypeInfo map, string field) af) {
			var sql = this.ToSql(af.field);
			if (_cache.seconds > 0 && string.IsNullOrEmpty(_cache.key)) _cache.key = $"{sql}{string.Join("|", _params.Select(a => a.Value))}";

			return await _orm.Cache.ShellAsync(_cache.key, _cache.seconds, async () => {
				List<TReturn> ret = new List<TReturn>();
				Type type = typeof(TReturn);
				await _orm.Ado.ExecuteReaderAsync(dr => {
					var index = -1;
					ret.Add((TReturn)_commonExpression.ReadAnonymous(af.map, dr, ref index));
					return Task.CompletedTask;
				}, CommandType.Text, sql, _params.ToArray());
				return ret;
			});
		}
		protected (ReadAnonymousTypeInfo map, string field) GetExpressionField(Expression newexp) {
			var map = new ReadAnonymousTypeInfo();
			var field = new StringBuilder();
			var index = 0;

			_commonExpression.ReadAnonymousField(_tables, field, map, ref index, newexp, null);
			return (map, field.Length > 0 ? field.Remove(0, 2).ToString() : null);
		}
		static ConcurrentDictionary<Type, ConstructorInfo> _dicConstructor = new ConcurrentDictionary<Type, ConstructorInfo>();
		static ConcurrentDictionary<string, GetAllFieldExpressionTreeInfo> _dicGetAllFieldExpressionTree = new ConcurrentDictionary<string, GetAllFieldExpressionTreeInfo>();
		public class GetAllFieldExpressionTreeInfo {
			public string Field { get; set; }
			public Func<DbDataReader, T1> Read { get; set; }
		}
		protected GetAllFieldExpressionTreeInfo GetAllFieldExpressionTree() {
			return _dicGetAllFieldExpressionTree.GetOrAdd(string.Join("+", _tables.Select(a => $"{a.Table.DbName}-{a.Alias}-{a.Type}")), s => {
				var tb1 = _tables.First().Table;
				var type = tb1.TypeLazy ?? tb1.Type;
				var props = tb1.Properties;

				var rowExp = Expression.Parameter(typeof(DbDataReader), "row");
				var returnTarget = Expression.Label(type);
				var retExp = Expression.Variable(type, "ret");
				var dataIndexExp = Expression.Variable(typeof(int), "dataIndex");
				var readExp = Expression.Variable(typeof(Utils.RowInfo), "read");
				var readExpValue = Expression.MakeMemberAccess(readExp, Utils.RowInfo.PropertyValue);
				var readExpDataIndex = Expression.MakeMemberAccess(readExp, Utils.RowInfo.PropertyDataIndex);
				var blockExp = new List<Expression>();
				var ctor = type.GetConstructor(new Type[0]) ?? type.GetConstructors().First();
				blockExp.AddRange(new Expression[] {
					Expression.Assign(retExp, Expression.New(ctor, ctor.GetParameters().Select(a => Expression.Default(a.ParameterType)))),
					Expression.Assign(dataIndexExp, Expression.Constant(0))
				});
				//typeof(Topic).GetMethod("get_Type").IsVirtual

				var field = new StringBuilder();
				var dicfield = new Dictionary<string, bool>();
				var tb = _tables.First();
				var index = 0;
				var otherindex = 0;
				foreach (var prop in props.Values) {
					if (tb.Table.ColumnsByCs.TryGetValue(prop.Name, out var col)) { //普通字段
						if (index > 0) field.Append(", ");
						var quoteName = _commonUtils.QuoteSqlName(col.Attribute.Name);
						field.Append(_commonUtils.QuoteReadColumn(col.CsType, $"{tb.Alias}.{quoteName}"));
						++index;
						if (dicfield.ContainsKey(quoteName)) field.Append(" as").Append(index);
						else dicfield.Add(quoteName, true);
					} else {
						var tb2 = _tables.Where((a, b) => b > 0 && 
							(a.Type == SelectTableInfoType.InnerJoin || a.Type == SelectTableInfoType.LeftJoin || a.Type == SelectTableInfoType.RightJoin) && 
							string.IsNullOrEmpty(a.On) == false &&
							a.Alias.Contains(prop.Name)).FirstOrDefault(); //判断 b > 0 防止 parent 递归关系
						if (tb2 == null && props.Where(pw => pw.Value.PropertyType == prop.PropertyType).Count() == 1)
							tb2 = _tables.Where((a, b) => b > 0 && 
								(a.Type == SelectTableInfoType.InnerJoin || a.Type == SelectTableInfoType.LeftJoin || a.Type == SelectTableInfoType.RightJoin) &&
								string.IsNullOrEmpty(a.On) == false &&
								a.Table.Type == prop.PropertyType).FirstOrDefault();
						if (tb2 == null) continue;
						foreach (var col2 in tb2.Table.Columns.Values) {
							if (index > 0) field.Append(", ");
							var quoteName = _commonUtils.QuoteSqlName(col2.Attribute.Name);
							field.Append(_commonUtils.QuoteReadColumn(col2.CsType, $"{tb2.Alias}.{quoteName}"));
							++index;
							++otherindex;
							if (dicfield.ContainsKey(quoteName)) field.Append(" as").Append(index);
							else dicfield.Add(quoteName, true);
						}
					}
					//只读到二级属性
					var propGetSetMethod = prop.GetSetMethod();
					Expression readExpAssign = null; //加速缓存
					if (prop.PropertyType.IsArray) readExpAssign = Expression.New(Utils.RowInfo.Constructor,
						Utils.GetDataReaderValueBlockExpression(prop.PropertyType, Expression.Call(rowExp, Utils.MethodDataReaderGetValue, dataIndexExp)),
						//Expression.Call(Utils.MethodGetDataReaderValue, new Expression[] { Expression.Constant(prop.PropertyType), Expression.Call(rowExp, Utils.MethodDataReaderGetValue, dataIndexExp) }),
						Expression.Add(dataIndexExp, Expression.Constant(1))
					);
					else {
						var proptypeGeneric = prop.PropertyType;
						if (proptypeGeneric.FullName.StartsWith("System.Nullable`1[")) proptypeGeneric = proptypeGeneric.GenericTypeArguments.First();
						if (proptypeGeneric.IsEnum ||
							Utils.dicExecuteArrayRowReadClassOrTuple.ContainsKey(proptypeGeneric)) readExpAssign = Expression.New(Utils.RowInfo.Constructor,
								Utils.GetDataReaderValueBlockExpression(prop.PropertyType, Expression.Call(rowExp, Utils.MethodDataReaderGetValue, dataIndexExp)),
								//Expression.Call(Utils.MethodGetDataReaderValue, new Expression[] { Expression.Constant(prop.PropertyType), Expression.Call(rowExp, Utils.MethodDataReaderGetValue, dataIndexExp) }),
								Expression.Add(dataIndexExp, Expression.Constant(1))
						);
						else {
							readExpAssign = Expression.Call(Utils.MethodExecuteArrayRowReadClassOrTuple, new Expression[] { Expression.Constant(prop.PropertyType), Expression.Constant(null, typeof(int[])), rowExp, dataIndexExp });
						}
					}
					blockExp.AddRange(new Expression[] {
						Expression.Assign(readExp, readExpAssign),
						Expression.IfThen(Expression.GreaterThan(readExpDataIndex, dataIndexExp),
							Expression.Assign(dataIndexExp, readExpDataIndex)),
						Expression.IfThenElse(Expression.Equal(readExpValue, Expression.Constant(null)),
							Expression.Call(retExp, propGetSetMethod, Expression.Default(prop.PropertyType)),
							Expression.Call(retExp, propGetSetMethod, Expression.Convert(readExpValue, prop.PropertyType)))
					});
				}
				if (otherindex == 0) { //不读导航属性，优化单表读取性能
					blockExp.Clear();
					blockExp.AddRange(new Expression[] {
						Expression.Assign(dataIndexExp, Expression.Constant(0)),
						Expression.Assign(readExp, Expression.Call(Utils.MethodExecuteArrayRowReadClassOrTuple, new Expression[] { Expression.Constant(type), Expression.Constant(null, typeof(int[])), rowExp, dataIndexExp })),
						Expression.Assign(retExp, Expression.Convert(readExpValue, type))
					});
				}
				if (tb1.TypeLazy != null) blockExp.Add(Expression.Call(retExp, tb1.TypeLazySetOrm, Expression.Constant(_orm))); //将 orm 传递给 lazy
				blockExp.AddRange(new Expression[] {
					Expression.Return(returnTarget, retExp),
					Expression.Label(returnTarget, Expression.Default(type))
				});
				return new GetAllFieldExpressionTreeInfo {
					Field = field.ToString(),
					Read = Expression.Lambda<Func<DbDataReader, T1>>(Expression.Block(new[] { retExp, dataIndexExp, readExp }, blockExp), new[] { rowExp }).Compile()
				};
			});
		}
		protected (ReadAnonymousTypeInfo map, string field) GetAllFieldReflection() {
			var tb1 = _tables.First().Table;
			var type = tb1.Type;
			var constructor = _dicConstructor.GetOrAdd(type, s => type.GetConstructor(new Type[0]));
			var map = new ReadAnonymousTypeInfo { Consturctor = constructor, ConsturctorType = ReadAnonymousTypeInfoConsturctorType.Properties };

			var field = new StringBuilder();
			var dicfield = new Dictionary<string, bool>();
			var tb = _tables.First();
			var index = 0;
			var ps = tb1.Properties;
			foreach (var p in ps.Values) {
				var child = new ReadAnonymousTypeInfo { Property = p, CsName = p.Name };
				if (tb.Table.ColumnsByCs.TryGetValue(p.Name, out var col)) { //普通字段
					if (index > 0) field.Append(", ");
					var quoteName = _commonUtils.QuoteSqlName(col.Attribute.Name);
					field.Append(_commonUtils.QuoteReadColumn(col.CsType, $"{tb.Alias}.{quoteName}"));
					++index;
					if (dicfield.ContainsKey(quoteName)) field.Append(" as").Append(index);
					else dicfield.Add(quoteName, true);
				} else {
					var tb2 = _tables.Where(a => a.Table.Type == p.PropertyType && a.Alias.Contains(p.Name)).FirstOrDefault();
					if (tb2 == null && ps.Where(pw => pw.Value.PropertyType == p.PropertyType).Count() == 1) tb2 = _tables.Where(a => a.Table.Type == p.PropertyType).FirstOrDefault();
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
						child.Childs.Add(new ReadAnonymousTypeInfo {
							Property = tb2.Table.Type.GetProperty(col2.CsName, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance),
							CsName = col2.CsName });
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
			this.GroupBy(field.Length > 0 ? field.Remove(0, 2).ToString() : null);
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

		protected List<TReturn> InternalToList<TReturn>(Expression select) => this.ToListMapReader<TReturn>(this.GetExpressionField(select));
		protected Task<List<TReturn>> InternalToListAsync<TReturn>(Expression select) => this.ToListMapReaderAsync<TReturn>(this.GetExpressionField(select));
		protected string InternalToSql<TReturn>(Expression select) {
			var af = this.GetExpressionField(select);
			return this.ToSql(af.field);
		}

		protected TReturn InternalToAggregate<TReturn>(Expression select) {
			var map = new ReadAnonymousTypeInfo();
			var field = new StringBuilder();
			var index = 0;

			_commonExpression.ReadAnonymousField(_tables, field, map, ref index, select, null);
			return this.ToListMapReader<TReturn>((map, field.Length > 0 ? field.Remove(0, 2).ToString() : null)).FirstOrDefault();
		}
		async protected Task<TReturn> InternalToAggregateAsync<TReturn>(Expression select) {
			var map = new ReadAnonymousTypeInfo();
			var field = new StringBuilder();
			var index = 0;

			_commonExpression.ReadAnonymousField(_tables, field, map, ref index, select, null);
			return (await this.ToListMapReaderAsync<TReturn>((map, field.Length > 0 ? field.Remove(0, 2).ToString() : null))).FirstOrDefault();
		}

		protected TSelect InternalWhere(Expression exp) => this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp, null));

		protected TSelect InternalJoin(Expression exp) {
			return this as TSelect;
		}
		#endregion
	}
}