using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FreeSql.Internal.CommonProvider {

	abstract class Select7Provider<T1, T2, T3, T4, T5, T6, T7> : Select0Provider<ISelect<T1, T2, T3, T4, T5, T6, T7>, T1>, ISelect<T1, T2, T3, T4, T5, T6, T7>
			where T1 : class
			where T2 : class
			where T3 : class
			where T4 : class
			where T5 : class
			where T6 : class
			where T7 : class {

		public Select7Provider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) {
			if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure(typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
		}

		TMember ISelect<T1, T2, T3, T4, T5, T6, T7>.Avg<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> column) => this.InternalAvg<TMember>(column?.Body);

		ISelect<T1, T2, T3, T4, T5, T6, T7> ISelect<T1, T2, T3, T4, T5, T6, T7>.GroupBy(Expression<Func<T1, T2, T3, T4, T5, T6, T7, object>> columns) => this.InternalGroupBy(columns?.Body);

		TMember ISelect<T1, T2, T3, T4, T5, T6, T7>.Max<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> column) => this.InternalMax<TMember>(column?.Body);

		TMember ISelect<T1, T2, T3, T4, T5, T6, T7>.Min<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> column) => this.InternalMin<TMember>(column?.Body);

		ISelect<T1, T2, T3, T4, T5, T6, T7> ISelect<T1, T2, T3, T4, T5, T6, T7>.OrderBy<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> column) => this.InternalOrderBy(column?.Body);

		ISelect<T1, T2, T3, T4, T5, T6, T7> ISelect<T1, T2, T3, T4, T5, T6, T7>.OrderByDescending<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> column) => this.InternalOrderByDescending(column?.Body);

		TMember ISelect<T1, T2, T3, T4, T5, T6, T7>.Sum<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> column) => this.InternalSum<TMember>(column?.Body);

		List<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7>.ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>> select) => this.InternalToList<TReturn>(select?.Body);

		ISelect<T1, T2, T3, T4, T5, T6, T7> ISelect<T1, T2, T3, T4, T5, T6, T7>.Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> exp) => this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body));

		ISelect<T1, T2, T3, T4, T5, T6, T7> ISelect<T1, T2, T3, T4, T5, T6, T7>.WhereIf(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> exp) => condition ? this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body)) : this;

		ISelect<T1, T2, T3, T4, T5, T6, T7> ISelect<T1, T2, T3, T4, T5, T6, T7>.WhereLike(Expression<Func<T1, T2, T3, T4, T5, T6, T7, string[]>> columns, string pattern, bool notLike) => this.InternalWhereLikeOr(columns?.Body, pattern, notLike);

		ISelect<T1, T2, T3, T4, T5, T6, T7> ISelect<T1, T2, T3, T4, T5, T6, T7>.WhereLike(Expression<Func<T1, T2, T3, T4, T5, T6, T7, string>> column, string pattern, bool notLike) => this.InternalWhereLike(column?.Body, pattern, notLike);
	}
}