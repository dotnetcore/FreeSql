using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FreeSql.Internal.CommonProvider {

	abstract class Select3Provider<T1, T2, T3> : Select0Provider<ISelect<T1, T2, T3>, T1>, ISelect<T1, T2, T3>
			where T1 : class
			where T2 : class
			where T3 : class {

		public Select3Provider(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere) : base(orm, commonUtils, commonExpression, dywhere) {
			if (_orm.CodeFirst.IsAutoSyncStructure) _orm.CodeFirst.SyncStructure(typeof(T2), typeof(T3));
		}

		TMember ISelect<T1, T2, T3>.Avg<TMember>(Expression<Func<T1, T2, T3, TMember>> column) => this.InternalAvg<TMember>(column?.Body);

		ISelect<T1, T2, T3> ISelect<T1, T2, T3>.GroupBy(Expression<Func<T1, T2, T3, object>> columns) => this.InternalGroupBy(columns?.Body);

		TMember ISelect<T1, T2, T3>.Max<TMember>(Expression<Func<T1, T2, T3, TMember>> column) => this.InternalMax<TMember>(column?.Body);

		TMember ISelect<T1, T2, T3>.Min<TMember>(Expression<Func<T1, T2, T3, TMember>> column) => this.InternalMin<TMember>(column?.Body);

		ISelect<T1, T2, T3> ISelect<T1, T2, T3>.OrderBy<TMember>(Expression<Func<T1, T2, T3, TMember>> column) => this.InternalOrderBy(column?.Body);

		ISelect<T1, T2, T3> ISelect<T1, T2, T3>.OrderByDescending<TMember>(Expression<Func<T1, T2, T3, TMember>> column) => this.InternalOrderByDescending(column?.Body);

		TMember ISelect<T1, T2, T3>.Sum<TMember>(Expression<Func<T1, T2, T3, TMember>> column) => this.InternalSum<TMember>(column?.Body);

		List<TReturn> ISelect<T1, T2, T3>.ToList<TReturn>(Expression<Func<T1, T2, T3, TReturn>> select) => this.InternalToList<TReturn>(select?.Body);

		ISelect<T1, T2, T3> ISelect<T1, T2, T3>.Where(Expression<Func<T1, T2, T3, bool>> exp) => this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body));

		ISelect<T1, T2, T3> ISelect<T1, T2, T3>.WhereIf(bool condition, Expression<Func<T1, T2, T3, bool>> exp) => condition ? this.Where(_commonExpression.ExpressionWhereLambda(_tables, exp?.Body)) : this;
	}
}