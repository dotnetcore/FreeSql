using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FreeSql {
	public interface ISelect<T1, T2, T3, T4, T5, T6, T7> : ISelect0<ISelect<T1, T2, T3, T4, T5, T6, T7>, T1> where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class {

		List<TReturn> ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>> select);

		TMember Sum<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> column);
		TMember Min<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> column);
		TMember Max<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> column);
		TMember Avg<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> column);

		ISelect<T1, T2, T3, T4, T5, T6, T7> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> exp);
		ISelect<T1, T2, T3, T4, T5, T6, T7> WhereIf(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> exp);

		ISelectGrouping<TKey> GroupBy<TKey>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TKey>> exp);

		ISelect<T1, T2, T3, T4, T5, T6, T7> OrderBy<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> column);
		ISelect<T1, T2, T3, T4, T5, T6, T7> OrderByDescending<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> column);
	}
}