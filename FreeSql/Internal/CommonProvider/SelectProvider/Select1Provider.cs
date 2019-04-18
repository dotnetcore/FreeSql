using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.Internal.CommonProvider {

	abstract class Select1Provider<T1> : Select0Provider<ISelect<T1>, T1>, ISelect<T1>
			where T1 : class {
		public Select1Provider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) {

		}

		protected ISelect<T1> InternalFrom(Expression exp) {
			if (exp.NodeType == ExpressionType.Call) {
				var expCall = exp as MethodCallExpression;
				var stockCall = new Stack<MethodCallExpression>();
				while (expCall != null) {
					stockCall.Push(expCall);
					expCall = expCall.Object as MethodCallExpression;
				}
				while (stockCall.Any()) {
					expCall = stockCall.Pop();

					switch (expCall.Method.Name) {
						case "Where": this.InternalWhere(expCall.Arguments[0]); break;
						case "WhereIf":
							var whereIfCond = _commonExpression.ExpressionSelectColumn_MemberAccess(null, null, SelectTableInfoType.From, expCall.Arguments[0], false, null);
							if (whereIfCond == "1" || whereIfCond == "'t'")
								this.InternalWhere(expCall.Arguments[1]);
							break;
						case "OrderBy": this.InternalOrderBy(expCall.Arguments[0]); break;
						case "OrderByDescending": this.InternalOrderByDescending(expCall.Arguments[0]); break;

						case "LeftJoin": this.InternalJoin(expCall.Arguments[0], SelectTableInfoType.LeftJoin); break;
						case "InnerJoin": this.InternalJoin(expCall.Arguments[0], SelectTableInfoType.InnerJoin); break;
						case "RightJoin": this.InternalJoin(expCall.Arguments[0], SelectTableInfoType.RightJoin); break;

						default: throw new NotImplementedException($"未实现 {expCall.Method.Name}");
					}
				}
			}
			return this;
		}

		public ISelect<T1> As(string alias) {
			var oldAs = _tables.First().Alias;
			var newAs = string.IsNullOrEmpty(alias) ? "a" : alias;
			if (oldAs != newAs) {
				_tables.First().Alias = newAs;
				var wh = _where.ToString();
				_where.Replace($" {oldAs}.", $" {newAs}.");
			}
			return this;
		}

		public TMember Avg<TMember>(Expression<Func<T1, TMember>> column) {
			if (column == null) return default(TMember);
			_tables[0].Parameter = column.Parameters[0];
			return this.InternalAvg<TMember>(column?.Body);
		}

		public Task<TMember> AvgAsync<TMember>(Expression<Func<T1, TMember>> column) {
			if (column == null) return Task.FromResult(default(TMember));
			_tables[0].Parameter = column.Parameters[0];
			return this.InternalAvgAsync<TMember>(column?.Body);
		}

		public abstract ISelect<T1, T2> From<T2>(Expression<Func<ISelectFromExpression<T1>, T2, ISelectFromExpression<T1>>> exp) where T2 : class;// { this.InternalFrom(exp?.Body); var ret = new Select3Provider<T1, T2, T3>(_orm, _commonUtils, _commonExpression, null); Select0Provider<ISelect<T1>, T1>.CopyData(this, ret, exp?.Parameters); return ret; }
		public abstract ISelect<T1, T2, T3> From<T2, T3>(Expression<Func<ISelectFromExpression<T1>, T2, T3, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class;// { this.InternalFrom(exp?.Body); var ret = new Select3Provider<T1, T2, T3>(_orm, _commonUtils, _commonExpression, null); Select0Provider<ISelect<T1>, T1>.CopyData(this, ret, exp?.Parameters); return ret; }

		public abstract ISelect<T1, T2, T3, T4> From<T2, T3, T4>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class where T4 : class;// { this.InternalFrom(exp?.Body); var ret = new Select4Provider<T1, T2, T3, T4>(_orm, _commonUtils, _commonExpression, null); Select0Provider<ISelect<T1>, T1>.CopyData(this, ret, exp?.Parameters); return ret; }

		public abstract ISelect<T1, T2, T3, T4, T5> From<T2, T3, T4, T5>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class where T4 : class where T5 : class;// { this.InternalFrom(exp?.Body); var ret = new Select5Provider<T1, T2, T3, T4, T5>(_orm, _commonUtils, _commonExpression, null); Select0Provider<ISelect<T1>, T1>.CopyData(this, ret, exp?.Parameters); return ret; }

		public abstract ISelect<T1, T2, T3, T4, T5, T6> From<T2, T3, T4, T5, T6>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class;// { this.InternalFrom(exp?.Body); var ret = new Select6Provider<T1, T2, T3, T4, T5, T6>(_orm, _commonUtils, _commonExpression, null); Select0Provider<ISelect<T1>, T1>.CopyData(this, ret, exp?.Parameters); return ret; }

		public abstract ISelect<T1, T2, T3, T4, T5, T6, T7> From<T2, T3, T4, T5, T6, T7>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class;// { this.InternalFrom(exp?.Body); var ret = new Select7Provider<T1, T2, T3, T4, T5, T6, T7>(_orm, _commonUtils, _commonExpression, null); Select0Provider<ISelect<T1>, T1>.CopyData(this, ret, exp?.Parameters); return ret; }

		public abstract ISelect<T1, T2, T3, T4, T5, T6, T7, T8> From<T2, T3, T4, T5, T6, T7, T8>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class;// { this.InternalFrom(exp?.Body); var ret = new Select8Provider<T1, T2, T3, T4, T5, T6, T7, T8>(_orm, _commonUtils, _commonExpression, null); Select0Provider<ISelect<T1>, T1>.CopyData(this, ret, exp?.Parameters); return ret; }

		public abstract ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T2, T3, T4, T5, T6, T7, T8, T9>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class;// { this.InternalFrom(exp?.Body); var ret = new Select9Provider<T1, T2, T3, T4, T5, T6, T7, T8, T9>(_orm, _commonUtils, _commonExpression, null); Select0Provider<ISelect<T1>, T1>.CopyData(this, ret, exp?.Parameters); return ret; }

		public abstract ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T2, T3, T4, T5, T6, T7, T8, T9, T10>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class;// { this.InternalFrom(exp?.Body); var ret = new Select10Provider<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(_orm, _commonUtils, _commonExpression, null); Select0Provider<ISelect<T1>, T1>.CopyData(this, ret, exp?.Parameters); return ret; }

		public ISelectGrouping<TKey> GroupBy<TKey>(Expression<Func<T1, TKey>> columns) {
			if (columns == null) return this.InternalGroupBy<TKey>(columns);
			_tables[0].Parameter = columns.Parameters[0];
			return this.InternalGroupBy<TKey>(columns);
		}

		public TMember Max<TMember>(Expression<Func<T1, TMember>> column) {
			if (column == null) return default(TMember);
			_tables[0].Parameter = column.Parameters[0];
			return this.InternalMax<TMember>(column?.Body);
		}

		public Task<TMember> MaxAsync<TMember>(Expression<Func<T1, TMember>> column) {
			if (column == null) return Task.FromResult(default(TMember));
			_tables[0].Parameter = column.Parameters[0];
			return this.InternalMaxAsync<TMember>(column?.Body);
		}

		public TMember Min<TMember>(Expression<Func<T1, TMember>> column) {
			if (column == null) return default(TMember);
			_tables[0].Parameter = column.Parameters[0];
			return this.InternalMin<TMember>(column?.Body);
		}

		public Task<TMember> MinAsync<TMember>(Expression<Func<T1, TMember>> column) {
			if (column == null) return Task.FromResult(default(TMember));
			_tables[0].Parameter = column.Parameters[0];
			return this.InternalMinAsync<TMember>(column?.Body);
		}

		public ISelect<T1> OrderBy<TMember>(Expression<Func<T1, TMember>> column) {
			if (column == null) return this;
			_tables[0].Parameter = column.Parameters[0];
			return this.InternalOrderBy(column?.Body);
		}

		public ISelect<T1> OrderByDescending<TMember>(Expression<Func<T1, TMember>> column) {
			if (column == null) return this;
			_tables[0].Parameter = column.Parameters[0];
			return this.InternalOrderByDescending(column?.Body);
		}

		public TMember Sum<TMember>(Expression<Func<T1, TMember>> column) {
			if (column == null) return default(TMember);
			_tables[0].Parameter = column.Parameters[0];
			return this.InternalSum<TMember>(column?.Body);
		}

		public Task<TMember> SumAsync<TMember>(Expression<Func<T1, TMember>> column) {
			if (column == null) return Task.FromResult(default(TMember));
			_tables[0].Parameter = column.Parameters[0];
			return this.InternalSumAsync<TMember>(column?.Body);
		}

		public List<TReturn> ToList<TReturn>(Expression<Func<T1, TReturn>> select) {
			if (select == null) return this.InternalToList<TReturn>(select?.Body);
			_tables[0].Parameter = select.Parameters[0];
			return this.InternalToList<TReturn>(select?.Body);
		}

		public Task<List<TReturn>> ToListAsync<TReturn>(Expression<Func<T1, TReturn>> select) {
			if (select == null) return this.InternalToListAsync<TReturn>(select?.Body);
			_tables[0].Parameter = select.Parameters[0];
			return this.InternalToListAsync<TReturn>(select?.Body);
		}

		public DataTable ToDataTable<TReturn>(Expression<Func<T1, TReturn>> select) {
			if (select == null) return this.InternalToDataTable(select?.Body);
			_tables[0].Parameter = select.Parameters[0];
			return this.InternalToDataTable(select?.Body);
		}

		public Task<DataTable> ToDataTableAsync<TReturn>(Expression<Func<T1, TReturn>> select) {
			if (select == null) return this.InternalToDataTableAsync(select?.Body);
			_tables[0].Parameter = select.Parameters[0];
			return this.InternalToDataTableAsync(select?.Body);
		}

		public string ToSql<TReturn>(Expression<Func<T1, TReturn>> select) {
			if (select == null) return this.InternalToSql<TReturn>(select?.Body);
			_tables[0].Parameter = select.Parameters[0];
			return this.InternalToSql<TReturn>(select?.Body);
		}

		public TReturn ToAggregate<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, TReturn>> select) {
			if (select == null) return default(TReturn);
			_tables[0].Parameter = select.Parameters[0];
			return this.InternalToAggregate<TReturn>(select?.Body);
		}

		public Task<TReturn> ToAggregateAsync<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, TReturn>> select) {
			if (select == null) return Task.FromResult(default(TReturn));
			_tables[0].Parameter = select.Parameters[0];
			return this.InternalToAggregateAsync<TReturn>(select?.Body);
		}

		public ISelect<T1> Where(Expression<Func<T1, bool>> exp) {
			if (exp == null) return this;
			_tables[0].Parameter = exp.Parameters[0];
			return this.InternalWhere(exp?.Body);
		}

		public ISelect<T1> Where<T2>(Expression<Func<T1, T2, bool>> exp) where T2 : class {
			if (exp == null) return this;
			_tables[0].Parameter = exp.Parameters[0];
			return this.InternalWhere(exp?.Body);
		}

		public ISelect<T1> Where<T2>(Expression<Func<T2, bool>> exp) where T2 : class {
			if (exp == null) return this;
			//_tables[0].Parameter = exp.Parameters[0];
			return this.InternalWhere(exp?.Body);
		}

		public ISelect<T1> Where<T2, T3>(Expression<Func<T1, T2, T3, bool>> exp) where T2 : class where T3 : class {
			if (exp == null) return this;
			_tables[0].Parameter = exp.Parameters[0];
			return this.InternalWhere(exp?.Body);
		}

		public ISelect<T1> Where<T2, T3, T4>(Expression<Func<T1, T2, T3, T4, bool>> exp) where T2 : class where T3 : class where T4 : class {
			if (exp == null) return this;
			_tables[0].Parameter = exp.Parameters[0];
			return this.InternalWhere(exp?.Body);
		}

		public ISelect<T1> Where<T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, bool>> exp) where T2 : class where T3 : class where T4 : class where T5 : class {
			if (exp == null) return this;
			_tables[0].Parameter = exp.Parameters[0];
			return this.InternalWhere(exp?.Body);
		}
		public ISelect<T1> WhereDynamic(object dywhere) => this.Where(_commonUtils.WhereObject(_tables.First().Table, $"{_tables.First().Alias}.", dywhere));

		public ISelect<T1> WhereIf(bool condition, Expression<Func<T1, bool>> exp) {
			if (condition == false || exp == null) return this;
			_tables[0].Parameter = exp.Parameters[0];
			return this.InternalWhere(exp?.Body);
		}

		public bool Any(Expression<Func<T1, bool>> exp) => this.Where(exp).Any();

		public Task<bool> AnyAsync(Expression<Func<T1, bool>> exp) => this.Where(exp).AnyAsync();

		public TReturn ToOne<TReturn>(Expression<Func<T1, TReturn>> select) => this.Limit(1).ToList(select).FirstOrDefault();

		async public Task<TReturn> ToOneAsync<TReturn>(Expression<Func<T1, TReturn>> select) => (await this.Limit(1).ToListAsync(select)).FirstOrDefault();

		public TReturn First<TReturn>(Expression<Func<T1, TReturn>> select) => this.ToOne(select);

		public Task<TReturn> FirstAsync<TReturn>(Expression<Func<T1, TReturn>> select) => this.ToOneAsync(select);
	}
}