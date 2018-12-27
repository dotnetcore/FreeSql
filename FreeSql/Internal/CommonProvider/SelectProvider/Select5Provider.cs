using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FreeSql.Internal.CommonProvider {

	abstract class Select5Provider<T1, T2, T3, T4, T5> : Select0Provider<ISelect<T1, T2, T3, T4, T5>, T1>, ISelect<T1, T2, T3, T4, T5>
			where T1 : class
			where T2 : class
			where T3 : class
			where T4 : class
			where T5 : class {

		public Select5Provider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) {
			if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure(typeof(T2), typeof(T3), typeof(T4), typeof(T5));
		}

		TMember ISelect<T1, T2, T3, T4, T5>.Avg<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> column) => this.InternalAvg<TMember>(column?.Body);

		ISelectGrouping<TKey> ISelect<T1, T2, T3, T4, T5>.GroupBy<TKey>(Expression<Func<T1, T2, T3, T4, T5, TKey>> exp) => this.InternalGroupBy<TKey>(exp?.Body);

		TMember ISelect<T1, T2, T3, T4, T5>.Max<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> column) => this.InternalMax<TMember>(column?.Body);

		TMember ISelect<T1, T2, T3, T4, T5>.Min<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> column) => this.InternalMin<TMember>(column?.Body);

		ISelect<T1, T2, T3, T4, T5> ISelect<T1, T2, T3, T4, T5>.OrderBy<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> column) => this.InternalOrderBy(column?.Body);

		ISelect<T1, T2, T3, T4, T5> ISelect<T1, T2, T3, T4, T5>.OrderByDescending<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> column) => this.InternalOrderByDescending(column?.Body);

		TMember ISelect<T1, T2, T3, T4, T5>.Sum<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> column) => this.InternalSum<TMember>(column?.Body);

		List<TReturn> ISelect<T1, T2, T3, T4, T5>.ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, TReturn>> select) => this.InternalToList<TReturn>(select?.Body);

		ISelect<T1, T2, T3, T4, T5> ISelect<T1, T2, T3, T4, T5>.Where(Expression<Func<T1, T2, T3, T4, T5, bool>> exp) => this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null));

		ISelect<T1, T2, T3, T4, T5> ISelect<T1, T2, T3, T4, T5>.WhereIf(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> exp) => condition ? this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body, null)) : this;
	}
}
