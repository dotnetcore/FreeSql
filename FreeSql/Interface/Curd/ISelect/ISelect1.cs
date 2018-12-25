using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FreeSql {
	public interface ISelect<T1> : ISelect0<ISelect<T1>, T1> where T1 : class {

		/// <summary>
		/// 执行SQL查询，返回指定字段的记录，记录不存在时返回 Count 为 0 的列表
		/// </summary>
		/// <typeparam name="TReturn">返回类型</typeparam>
		/// <param name="select">选择列</param>
		/// <returns></returns>
		List<TReturn> ToList<TReturn>(Expression<Func<T1, TReturn>> select);

		/// <summary>
		/// 求和
		/// </summary>
		/// <typeparam name="TMember">返回类型</typeparam>
		/// <param name="column">列</param>
		/// <returns></returns>
		TMember Sum<TMember>(Expression<Func<T1, TMember>> column);
		/// <summary>
		/// 最小值
		/// </summary>
		/// <typeparam name="TMember">返回类型</typeparam>
		/// <param name="column">列</param>
		/// <returns></returns>
		TMember Min<TMember>(Expression<Func<T1, TMember>> column);
		/// <summary>
		/// 最大值
		/// </summary>
		/// <typeparam name="TMember">返回类型</typeparam>
		/// <param name="column">列</param>
		/// <returns></returns>
		TMember Max<TMember>(Expression<Func<T1, TMember>> column);
		/// <summary>
		/// 平均值
		/// </summary>
		/// <typeparam name="TMember">返回类型</typeparam>
		/// <param name="column">列</param>
		/// <returns></returns>
		TMember Avg<TMember>(Expression<Func<T1, TMember>> column);

		/// <summary>
		/// 指定别名
		/// </summary>
		/// <param name="alias">别名</param>
		/// <returns></returns>
		ISelect<T1> As(string alias = "a");

		/// <summary>
		/// 多表查询
		/// </summary>
		/// <typeparam name="T2"></typeparam>
		/// <typeparam name="T3"></typeparam>
		/// <param name="exp"></param>
		/// <returns></returns>
		ISelect<T1, T2, T3> From<T2, T3>(Expression<Func<ISelectFromExpression<T1>, T2, T3, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class;
		/// <summary>
		/// 多表查询
		/// </summary>
		/// <typeparam name="T2"></typeparam>
		/// <typeparam name="T3"></typeparam>
		/// <typeparam name="T4"></typeparam>
		/// <param name="exp"></param>
		/// <returns></returns>
		ISelect<T1, T2, T3, T4> From<T2, T3, T4>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class where T4 : class;
		/// <summary>
		/// 多表查询
		/// </summary>
		/// <typeparam name="T2"></typeparam>
		/// <typeparam name="T3"></typeparam>
		/// <typeparam name="T4"></typeparam>
		/// <typeparam name="T5"></typeparam>
		/// <param name="exp"></param>
		/// <returns></returns>
		ISelect<T1, T2, T3, T4, T5> From<T2, T3, T4, T5>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class where T4 : class where T5 : class;
		/// <summary>
		/// 多表查询
		/// </summary>
		/// <typeparam name="T2"></typeparam>
		/// <typeparam name="T3"></typeparam>
		/// <typeparam name="T4"></typeparam>
		/// <typeparam name="T5"></typeparam>
		/// <typeparam name="T6"></typeparam>
		/// <param name="exp"></param>
		/// <returns></returns>
		ISelect<T1, T2, T3, T4, T5, T6> From<T2, T3, T4, T5, T6>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class;
		/// <summary>
		/// 多表查询
		/// </summary>
		/// <typeparam name="T2"></typeparam>
		/// <typeparam name="T3"></typeparam>
		/// <typeparam name="T4"></typeparam>
		/// <typeparam name="T5"></typeparam>
		/// <typeparam name="T6"></typeparam>
		/// <typeparam name="T7"></typeparam>
		/// <param name="exp"></param>
		/// <returns></returns>
		ISelect<T1, T2, T3, T4, T5, T6, T7> From<T2, T3, T4, T5, T6, T7>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class;
		/// <summary>
		/// 多表查询
		/// </summary>
		/// <typeparam name="T2"></typeparam>
		/// <typeparam name="T3"></typeparam>
		/// <typeparam name="T4"></typeparam>
		/// <typeparam name="T5"></typeparam>
		/// <typeparam name="T6"></typeparam>
		/// <typeparam name="T7"></typeparam>
		/// <typeparam name="T8"></typeparam>
		/// <param name="exp"></param>
		/// <returns></returns>
		ISelect<T1, T2, T3, T4, T5, T6, T7, T8> From<T2, T3, T4, T5, T6, T7, T8>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class;
		/// <summary>
		/// 多表查询
		/// </summary>
		/// <typeparam name="T2"></typeparam>
		/// <typeparam name="T3"></typeparam>
		/// <typeparam name="T4"></typeparam>
		/// <typeparam name="T5"></typeparam>
		/// <typeparam name="T6"></typeparam>
		/// <typeparam name="T7"></typeparam>
		/// <typeparam name="T8"></typeparam>
		/// <typeparam name="T9"></typeparam>
		/// <param name="exp"></param>
		/// <returns></returns>
		ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T2, T3, T4, T5, T6, T7, T8, T9>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class;
		/// <summary>
		/// 多表查询
		/// </summary>
		/// <typeparam name="T2"></typeparam>
		/// <typeparam name="T3"></typeparam>
		/// <typeparam name="T4"></typeparam>
		/// <typeparam name="T5"></typeparam>
		/// <typeparam name="T6"></typeparam>
		/// <typeparam name="T7"></typeparam>
		/// <typeparam name="T8"></typeparam>
		/// <typeparam name="T9"></typeparam>
		/// <typeparam name="T10"></typeparam>
		/// <param name="exp"></param>
		/// <returns></returns>
		ISelect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T2, T3, T4, T5, T6, T7, T8, T9, T10>(Expression<Func<ISelectFromExpression<T1>, T2, T3, T4, T5, T6, T7, T8, T9, T10, ISelectFromExpression<T1>>> exp) where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class where T7 : class where T8 : class where T9 : class where T10 : class;

		/// <summary>
		/// 查询条件，Where(a => a.Id > 10)，支持导航对象查询，Where(a => a.Author.Email == "2881099@qq.com")
		/// </summary>
		/// <param name="exp">lambda表达式</param>
		/// <returns></returns>
		ISelect<T1> Where(Expression<Func<T1, bool>> exp);
		/// <summary>
		/// 查询条件，Where(true, a => a.Id > 10)，支导航对象查询，Where(true, a => a.Author.Email == "2881099@qq.com")
		/// </summary>
		/// <param name="condition">true 时生效</param>
		/// <param name="exp">lambda表达式</param>
		/// <returns></returns>
		ISelect<T1> WhereIf(bool condition, Expression<Func<T1, bool>> exp);
		/// <summary>
		/// 多表条件查询
		/// </summary>
		/// <typeparam name="T2"></typeparam>
		/// <param name="exp">lambda表达式</param>
		/// <returns></returns>
		ISelect<T1> Where<T2>(Expression<Func<T1, T2, bool>> exp) where T2 : class;
		/// <summary>
		/// 多表条件查询
		/// </summary>
		/// <typeparam name="T2"></typeparam>
		/// <typeparam name="T3"></typeparam>
		/// <param name="exp">lambda表达式</param>
		/// <returns></returns>
		ISelect<T1> Where<T2, T3>(Expression<Func<T1, T2, T3, bool>> exp) where T2 : class where T3 : class;
		/// <summary>
		/// 多表条件查询
		/// </summary>
		/// <typeparam name="T2"></typeparam>
		/// <typeparam name="T3"></typeparam>
		/// <typeparam name="T4"></typeparam>
		/// <param name="exp">lambda表达式</param>
		/// <returns></returns>
		ISelect<T1> Where<T2, T3, T4>(Expression<Func<T1, T2, T3, T4, bool>> exp) where T2 : class where T3 : class where T4 : class;
		/// <summary>
		/// 多表条件查询
		/// </summary>
		/// <typeparam name="T2"></typeparam>
		/// <typeparam name="T3"></typeparam>
		/// <typeparam name="T4"></typeparam>
		/// <param name="exp">lambda表达式</param>
		/// <returns></returns>
		ISelect<T1> Where<T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, bool>> exp) where T2 : class where T3 : class where T4 : class where T5 : class;

		/// <summary>
		/// 按选择的列分组，GroupBy(a => a.Name) | GroupBy(a => new{a.Name,a.Time}) | GroupBy(a => new[]{"name","time"})
		/// </summary>
		/// <param name="columns"></param>
		/// <returns></returns>
		ISelect<T1> GroupBy<TKey>(Expression<Func<T1, TKey>> columns);

		/// <summary>
		/// 按列排序，OrderBy(a => a.Time)
		/// </summary>
		/// <typeparam name="TMember"></typeparam>
		/// <param name="column"></param>
		/// <returns></returns>
		ISelect<T1> OrderBy<TMember>(Expression<Func<T1, TMember>> column);
		/// <summary>
		/// 按列倒向排序，OrderByDescending(a => a.Time)
		/// </summary>
		/// <param name="column">列</param>
		/// <returns></returns>
		ISelect<T1> OrderByDescending<TMember>(Expression<Func<T1, TMember>> column);
	}
}