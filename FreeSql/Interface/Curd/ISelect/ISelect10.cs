using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FreeSql {
	public interface ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : ISelect0<ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, T1> where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class {

		List<TReturn> ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>> select);

		TMember Sum<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>> column);
		TMember Min<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>> column);
		TMember Max<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>> column);
		TMember Avg<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>> column);

		ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> exp);
		ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> WhereIf(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> exp);

		ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> WhereLike(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, string[]>> columns, string pattern, bool notLike = false);
		ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> WhereLike(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, string>> column, string pattern, bool notLike = false);

		ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> GroupBy(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, object>> columns);

		ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderBy<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>> column);
		ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderByDescending<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>> column);
	}
}