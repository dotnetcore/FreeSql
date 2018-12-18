using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FreeSql {
	public interface ISelect<T1, T2> : ISelect0<ISelect<T1, T2>, T1> where T1 : class where T2 : class {

		List<TReturn> ToList<TReturn>(Expression<Func<T1, T2, TReturn>> select);

		TMember Sum<TMember>(Expression<Func<T1, T2, TMember>> column);
		TMember Min<TMember>(Expression<Func<T1, T2, TMember>> column);
		TMember Max<TMember>(Expression<Func<T1, T2, TMember>> column);
		TMember Avg<TMember>(Expression<Func<T1, T2, TMember>> column);

		ISelect<T1, T2> Where(Expression<Func<T1, T2, bool>> exp);
		ISelect<T1, T2> WhereIf(bool condition, Expression<Func<T1, T2, bool>> exp);

		ISelect<T1, T2> WhereLike(Expression<Func<T1, T2, string[]>> columns, string pattern, bool notLike = false);
		ISelect<T1, T2> WhereLike(Expression<Func<T1, T2, string>> column, string pattern, bool notLike = false);

		ISelect<T1, T2> GroupBy(Expression<Func<T1, T2, object>> columns);

		ISelect<T1, T2> OrderBy<TMember>(Expression<Func<T1, T2, TMember>> column);
		ISelect<T1, T2> OrderByDescending<TMember>(Expression<Func<T1, T2, TMember>> column);
	}
}