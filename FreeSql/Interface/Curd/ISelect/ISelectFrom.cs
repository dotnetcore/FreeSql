using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FreeSql {
	public interface ISelectFromExpression<T1> where T1 : class {

		ISelectFromExpression<T1> LeftJoin(Expression<Func<T1, bool>> exp);
		ISelectFromExpression<T1> InnerJoin(Expression<Func<T1, bool>> exp);
		ISelectFromExpression<T1> RightJoin(Expression<Func<T1, bool>> exp);

		/// <summary>
		/// 查询条件，Where(a => a.Id > 10)，支持导航对象查询，Where(a => a.Author.Email == "2881099@qq.com")
		/// </summary>
		/// <param name="exp">lambda表达式</param>
		/// <returns></returns>
		ISelectFromExpression<T1> Where(Expression<Func<T1, bool>> exp);
		/// <summary>
		/// 查询条件，Where(true, a => a.Id > 10)，支导航对象查询，Where(true, a => a.Author.Email == "2881099@qq.com")
		/// </summary>
		/// <param name="condition">true 时生效</param>
		/// <param name="exp">lambda表达式</param>
		/// <returns></returns>
		ISelectFromExpression<T1> WhereIf(bool condition, Expression<Func<T1, bool>> exp);

		/// <summary>
		/// 模糊查询，选择多个列 OR，WhereLike(a => new[] { a.Title, a.Content }, "%sql%")
		/// </summary>
		/// <param name="columns">lambda选择列</param>
		/// <param name="pattern">查询内容</param>
		/// <param name="notLike">not like</param>
		/// <returns></returns>
		ISelectFromExpression<T1> WhereLike(Expression<Func<T1, string[]>> columns, string pattern, bool notLike = false);
		/// <summary>
		/// 模糊查询，WhereLike(a => a.Title, "%sql")
		/// </summary>
		/// <param name="column">lambda选择列</param>
		/// <param name="pattern">查询内容</param>
		/// <param name="notLike">not like</param>
		/// <returns></returns>
		ISelectFromExpression<T1> WhereLike(Expression<Func<T1, string>> column, string pattern, bool notLike = false);

		/// <summary>
		/// 按选择的列分组，GroupBy(a => a.Name) | GroupBy(a => new{a.Name,a.Time}) | GroupBy(a => new[]{"name","time"})
		/// </summary>
		/// <param name="columns"></param>
		/// <returns></returns>
		ISelectFromExpression<T1> GroupBy(Expression<Func<T1, object>> columns);

		/// <summary>
		/// 按列排序，OrderBy(a => a.Time)
		/// </summary>
		/// <typeparam name="TMember"></typeparam>
		/// <param name="column"></param>
		/// <returns></returns>
		ISelectFromExpression<T1> OrderBy<TMember>(Expression<Func<T1, TMember>> column);
		/// <summary>
		/// 按列倒向排序，OrderByDescending(a => a.Time)
		/// </summary>
		/// <param name="column">列</param>
		/// <returns></returns>
		ISelectFromExpression<T1> OrderByDescending<TMember>(Expression<Func<T1, TMember>> column);
	}
}