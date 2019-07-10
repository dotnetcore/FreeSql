using FreeSql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Linq.Expressions
{
    public static partial class LambadaExpressionExtensions
    {

        /// <summary>
        /// 使用 and 拼接两个 lambda 表达式
        /// </summary>
        /// <returns></returns>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> exp1, Expression<Func<T, bool>> exp2) => And(exp1, true, exp2);
        /// <summary>
        /// 使用 and 拼接两个 lambda 表达式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="exp1"></param>
        /// <param name="condition">true 时生效</param>
        /// <param name="exp2"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> exp1, bool condition, Expression<Func<T, bool>> exp2)
        {
            if (condition == false) return exp1;
            if (exp1 == null) return exp2;
            if (exp2 == null) return exp1;

            ParameterExpression newParameter = Expression.Parameter(typeof(T), "c");
            NewExpressionVisitor visitor = new NewExpressionVisitor(newParameter, exp2.Parameters.FirstOrDefault());

            var left = visitor.Replace(exp1.Body);
            var right = visitor.Replace(exp2.Body);
            var body = Expression.AndAlso(left, right);
            return Expression.Lambda<Func<T, bool>>(body, newParameter);
        }

        /// <summary>
        /// 使用 or 拼接两个 lambda 表达式
        /// </summary>
        /// <returns></returns>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> exp1, Expression<Func<T, bool>> exp2) => Or(exp1, true, exp2);
        /// <summary>
        /// 使用 or 拼接两个 lambda 表达式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="exp1"></param>
        /// <param name="condition">true 时生效</param>
        /// <param name="exp2"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> exp1, bool condition, Expression<Func<T, bool>> exp2)
        {
            if (condition == false) return exp1;
            if (exp1 == null) return exp2;
            if (exp2 == null) return exp1;

            ParameterExpression newParameter = Expression.Parameter(typeof(T), "c");
            NewExpressionVisitor visitor = new NewExpressionVisitor(newParameter, exp2.Parameters.FirstOrDefault());

            var left = visitor.Replace(exp1.Body);
            var right = visitor.Replace(exp2.Body);
            var body = Expression.OrElse(left, right);
            return Expression.Lambda<Func<T, bool>>(body, newParameter);
        }

        /// <summary>
        /// 将 lambda 表达式取反
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="exp"></param>
        /// <param name="condition">true 时生效</param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> exp, bool condition = true)
        {
            if (condition == false) return exp;
            if (exp == null) return null;

            var candidateExpr = exp.Parameters[0];
            var body = Expression.Not(exp.Body);
            return Expression.Lambda<Func<T, bool>>(body, candidateExpr);
        }
    }

    internal class NewExpressionVisitor : ExpressionVisitor
    {
        ParameterExpression _newParameter;
        ParameterExpression _oldParameter;
        public NewExpressionVisitor(ParameterExpression newParam, ParameterExpression oldParam)
        {
            this._newParameter = newParam;
            this._oldParameter = oldParam;
        }
        public Expression Replace(Expression exp) => this.Visit(exp);

        protected override Expression VisitParameter(ParameterExpression node) =>
            node == _oldParameter ? this._newParameter : node;
    }
}