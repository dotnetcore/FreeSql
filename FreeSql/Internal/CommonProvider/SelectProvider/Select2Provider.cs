using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FreeSql.Internal.CommonProvider {

	abstract class Select2Provider<T1, T2> : Select0Provider<ISelect<T1, T2>, T1>, ISelect<T1, T2>
			where T1 : class
			where T2 : class {

		public Select2Provider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) {
			if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure<T2>();
		}

		TMember ISelect<T1, T2>.Avg<TMember>(Expression<Func<T1, T2, TMember>> column) => this.InternalAvg<TMember>(column?.Body);

		ISelect<T1, T2> ISelect<T1, T2>.GroupBy(Expression<Func<T1, T2, object>> columns) => this.InternalGroupBy(columns?.Body);

		TMember ISelect<T1, T2>.Max<TMember>(Expression<Func<T1, T2, TMember>> column) => this.InternalMax<TMember>(column?.Body);

		TMember ISelect<T1, T2>.Min<TMember>(Expression<Func<T1, T2, TMember>> column) => this.InternalMin<TMember>(column?.Body);

		ISelect<T1, T2> ISelect<T1, T2>.OrderBy<TMember>(Expression<Func<T1, T2, TMember>> column) => this.InternalOrderBy(column?.Body);

		ISelect<T1, T2> ISelect<T1, T2>.OrderByDescending<TMember>(Expression<Func<T1, T2, TMember>> column) => this.InternalOrderByDescending(column?.Body);

		TMember ISelect<T1, T2>.Sum<TMember>(Expression<Func<T1, T2, TMember>> column) => this.InternalSum<TMember>(column?.Body);

		List<TReturn> ISelect<T1, T2>.ToList<TReturn>(Expression<Func<T1, T2, TReturn>> select) => this.InternalToList<TReturn>(select?.Body);

		ISelect<T1, T2> ISelect<T1, T2>.Where(Expression<Func<T1, T2, bool>> exp) => this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body));

		ISelect<T1, T2> ISelect<T1, T2>.WhereIf(bool condition, Expression<Func<T1, T2, bool>> exp) => condition ? this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body)) : this;

		ISelect<T1, T2> ISelect<T1, T2>.WhereLike(Expression<Func<T1, T2, string[]>> columns, string pattern, bool notLike) => this.InternalWhereLikeOr(columns?.Body, pattern, notLike);

		ISelect<T1, T2> ISelect<T1, T2>.WhereLike(Expression<Func<T1, T2, string>> column, string pattern, bool notLike) => this.InternalWhereLike(column?.Body, pattern, notLike);
	}
}