using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FreeSql.Internal.CommonProvider {

	abstract class Select9Provider<T1, T2, T3, T4, T5, T6, T7, T8, T9> : Select0Provider<ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>, T1>, ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>
		where T1 : class
		where T2 : class
		where T3 : class
		where T4 : class
		where T5 : class
		where T6 : class
		where T7 : class
		where T8 : class
		where T9 : class {

		public Select9Provider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) {
			if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure(typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
		}

		TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.Avg<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>> column) => this.InternalAvg<TMember>(column?.Body);

		ISelectGrouping<TKey> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.GroupBy<TKey>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TKey>> exp) => this.InternalGroupBy<TKey>(exp?.Body);

		TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.Max<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>> column) => this.InternalMax<TMember>(column?.Body);

		TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.Min<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>> column) => this.InternalMin<TMember>(column?.Body);

		ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.OrderBy<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>> column) => this.InternalOrderBy(column?.Body);

		ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.OrderByDescending<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>> column) => this.InternalOrderByDescending(column?.Body);

		TMember ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.Sum<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>> column) => this.InternalSum<TMember>(column?.Body);

		List<TReturn> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>> select) => this.InternalToList<TReturn>(select?.Body);

		ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> exp) => this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null));

		ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9>.WhereIf(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> exp) => condition ? this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null)) : this;
	}
}
