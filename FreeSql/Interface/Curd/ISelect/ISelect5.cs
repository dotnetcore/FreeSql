using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FreeSql {
	public interface ISelect<T1, T2, T3, T4, T5> : ISelect0<ISelect<T1, T2, T3, T4, T5>, T1> where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class {

		List<TReturn> ToList<TReturn>(Expression<Func<T1, T2, T3, T4, T5, TReturn>> select);

		TMember Sum<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> column);
		TMember Min<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> column);
		TMember Max<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> column);
		TMember Avg<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> column);

		ISelect<T1, T2, T3, T4, T5> Where(Expression<Func<T1, T2, T3, T4, T5, bool>> exp);
		ISelect<T1, T2, T3, T4, T5> WhereIf(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> exp);

		ISelect<T1, T2, T3, T4, T5> WhereLike(Expression<Func<T1, T2, T3, T4, T5, string[]>> columns, string pattern, bool notLike = false);
		ISelect<T1, T2, T3, T4, T5> WhereLike(Expression<Func<T1, T2, T3, T4, T5, string>> column, string pattern, bool notLike = false);

		ISelect<T1, T2, T3, T4, T5> GroupBy(Expression<Func<T1, T2, T3, T4, T5, object>> columns);

		ISelect<T1, T2, T3, T4, T5> OrderBy<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> column);
		ISelect<T1, T2, T3, T4, T5> OrderByDescending<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> column);
	}
}