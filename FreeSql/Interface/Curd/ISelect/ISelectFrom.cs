using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FreeSql
{
    public interface ISelectFromExpression<T1>
    {

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