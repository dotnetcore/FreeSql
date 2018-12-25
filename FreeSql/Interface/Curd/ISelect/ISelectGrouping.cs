using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace FreeSql {
	public interface ISelectGrouping<T1> {
		/// <summary>
		/// 按聚合条件过滤，Where(a => a.Count() > 10)
		/// </summary>
		/// <param name="exp">lambda表达式</param>
		/// <returns></returns>
		ISelectGrouping<T1> Having(Expression<Func<ISelectGroupingAggregate<T1>, bool>> exp);

		/// <summary>
		/// 按列排序，OrderBy(a => a.Time)
		/// </summary>
		/// <typeparam name="TMember"></typeparam>
		/// <param name="column"></param>
		/// <returns></returns>
		ISelectGrouping<T1> OrderBy<TMember>(Expression<Func<ISelectGroupingAggregate<T1>, TMember>> column);
		/// <summary>
		/// 按列倒向排序，OrderByDescending(a => a.Time)
		/// </summary>
		/// <param name="column">列</param>
		/// <returns></returns>
		ISelectGrouping<T1> OrderByDescending<TMember>(Expression<Func<ISelectGroupingAggregate<T1>, TMember>> column);

		/// <summary>
		/// 执行SQL查询，返回指定字段的记录，记录不存在时返回 Count 为 0 的列表
		/// </summary>
		/// <typeparam name="TReturn">返回类型</typeparam>
		/// <param name="select">选择列</param>
		/// <returns></returns>
		List<TReturn> ToList<TReturn>(Expression<Func<ISelectGroupingAggregate<T1>, TReturn>> select);
	}

	public interface ISelectGroupingAggregate<T1> {
		T1 Key { get; set; }
		int Count();
		T3 Sum<T3>(T3 column);
		T3 Avg<T3>(T3 column);
		T3 Max<T3>(T3 column);
		T3 Min<T3>(T3 column);
	}
}
