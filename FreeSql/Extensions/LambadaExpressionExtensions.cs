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

        static LambdaExpression InternalAndOrExpression(bool condition, LambdaExpression exp1, LambdaExpression exp2, bool isAndAlso)
        {
            if (condition == false) return exp1;
            if (exp1 == null) return exp2;
            if (exp2 == null) return exp1;

            var newParameters = exp1.Parameters.Select((a, b) => Expression.Parameter(a.Type, a.Name /*$"new{b}"*/)).ToArray();

            var left = new NewExpressionVisitor(newParameters, exp1.Parameters.ToArray()).Replace(exp1.Body);
            var right = new NewExpressionVisitor(newParameters, exp2.Parameters.ToArray()).Replace(exp2.Body);
            var body = isAndAlso ? Expression.AndAlso(left, right) : Expression.OrElse(left, right);
            return Expression.Lambda(exp1.Type, body, newParameters);
        }
        static LambdaExpression InternalNotExpression(bool condition, LambdaExpression exp)
        {
            if (condition == false) return exp;
            if (exp == null) return null;

            var newParameters = exp.Parameters.Select((a, b) => Expression.Parameter(a.Type, a.Name /*$"new{b}"*/)).ToArray();
            var body = Expression.Not(exp.Body);
            return Expression.Lambda(exp.Type, body, newParameters);
        }

        #region T1
        /// <summary>
        /// 使用 and 拼接两个 lambda 表达式
        /// </summary>
        /// <returns></returns>
        public static Expression<Func<T1, bool>> And<T1>(this Expression<Func<T1, bool>> exp1, Expression<Func<T1, bool>> exp2) => And(exp1, true, exp2);
        /// <summary>
        /// 使用 and 拼接两个 lambda 表达式
        /// </summary>
        /// <param name="exp1"></param>
        /// <param name="condition">true 时生效</param>
        /// <param name="exp2"></param>
        /// <returns></returns>
        public static Expression<Func<T1, bool>> And<T1>(this Expression<Func<T1, bool>> exp1, bool condition, Expression<Func<T1, bool>> exp2) => (Expression<Func<T1, bool>>)InternalAndOrExpression(condition, exp1, exp2, true);

        /// <summary>
        /// 使用 or 拼接两个 lambda 表达式
        /// </summary>
        /// <returns></returns>
        public static Expression<Func<T1, bool>> Or<T1>(this Expression<Func<T1, bool>> exp1, Expression<Func<T1, bool>> exp2) => Or(exp1, true, exp2);
        /// <summary>
        /// 使用 or 拼接两个 lambda 表达式
        /// </summary>
        /// <param name="exp1"></param>
        /// <param name="condition">true 时生效</param>
        /// <param name="exp2"></param>
        /// <returns></returns>
        public static Expression<Func<T1, bool>> Or<T1>(this Expression<Func<T1, bool>> exp1, bool condition, Expression<Func<T1, bool>> exp2) => (Expression<Func<T1, bool>>)InternalAndOrExpression(condition, exp1, exp2, false);

        /// <summary>
        /// 将 lambda 表达式取反
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="condition">true 时生效</param>
        /// <returns></returns>
        public static Expression<Func<T1, bool>> Not<T1>(this Expression<Func<T1, bool>> exp, bool condition = true) => (Expression<Func<T1, bool>>)InternalNotExpression(condition, exp);
        #endregion

        #region T1, T2
        /// <summary>
        /// 使用 and 拼接两个 lambda 表达式
        /// </summary>
        /// <returns></returns>
        public static Expression<Func<T1, T2, bool>> And<T1, T2>(this Expression<Func<T1, T2, bool>> exp1, Expression<Func<T1, T2, bool>> exp2) => And(exp1, true, exp2);
        /// <summary>
        /// 使用 and 拼接两个 lambda 表达式
        /// </summary>
        /// <param name="exp1"></param>
        /// <param name="condition">true 时生效</param>
        /// <param name="exp2"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, bool>> And<T1, T2>(this Expression<Func<T1, T2, bool>> exp1, bool condition, Expression<Func<T1, T2, bool>> exp2) => (Expression<Func<T1, T2, bool>>)InternalAndOrExpression(condition, exp1, exp2, true);

        /// <summary>
        /// 使用 or 拼接两个 lambda 表达式
        /// </summary>
        /// <returns></returns>
        public static Expression<Func<T1, T2, bool>> Or<T1, T2>(this Expression<Func<T1, T2, bool>> exp1, Expression<Func<T1, T2, bool>> exp2) => Or(exp1, true, exp2);
        /// <summary>
        /// 使用 or 拼接两个 lambda 表达式
        /// </summary>
        /// <param name="exp1"></param>
        /// <param name="condition">true 时生效</param>
        /// <param name="exp2"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, bool>> Or<T1, T2>(this Expression<Func<T1, T2, bool>> exp1, bool condition, Expression<Func<T1, T2, bool>> exp2) => (Expression<Func<T1, T2, bool>>)InternalAndOrExpression(condition, exp1, exp2, false);

        /// <summary>
        /// 将 lambda 表达式取反
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="condition">true 时生效</param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, bool>> Not<T1, T2>(this Expression<Func<T1, T2, bool>> exp, bool condition = true) => (Expression<Func<T1, T2, bool>>)InternalNotExpression(condition, exp);
        #endregion

        #region T1, T2, T3
        /// <summary>
        /// 使用 and 拼接两个 lambda 表达式
        /// </summary>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, bool>> And<T1, T2, T3>(this Expression<Func<T1, T2, T3, bool>> exp1, Expression<Func<T1, T2, T3, bool>> exp2) => And(exp1, true, exp2);
        /// <summary>
        /// 使用 and 拼接两个 lambda 表达式
        /// </summary>
        /// <param name="exp1"></param>
        /// <param name="condition">true 时生效</param>
        /// <param name="exp2"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, bool>> And<T1, T2, T3>(this Expression<Func<T1, T2, T3, bool>> exp1, bool condition, Expression<Func<T1, T2, T3, bool>> exp2) => (Expression<Func<T1, T2, T3, bool>>)InternalAndOrExpression(condition, exp1, exp2, true);

        /// <summary>
        /// 使用 or 拼接两个 lambda 表达式
        /// </summary>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, bool>> Or<T1, T2, T3>(this Expression<Func<T1, T2, T3, bool>> exp1, Expression<Func<T1, T2, T3, bool>> exp2) => Or(exp1, true, exp2);
        /// <summary>
        /// 使用 or 拼接两个 lambda 表达式
        /// </summary>
        /// <param name="exp1"></param>
        /// <param name="condition">true 时生效</param>
        /// <param name="exp2"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, bool>> Or<T1, T2, T3>(this Expression<Func<T1, T2, T3, bool>> exp1, bool condition, Expression<Func<T1, T2, T3, bool>> exp2) => (Expression<Func<T1, T2, T3, bool>>)InternalAndOrExpression(condition, exp1, exp2, false);

        /// <summary>
        /// 将 lambda 表达式取反
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="condition">true 时生效</param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, bool>> Not<T1, T2, T3>(this Expression<Func<T1, T2, T3, bool>> exp, bool condition = true) => (Expression<Func<T1, T2, T3, bool>>)InternalNotExpression(condition, exp);
        #endregion

        #region T1, T2, T3, T4
        /// <summary>
        /// 使用 and 拼接两个 lambda 表达式
        /// </summary>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, bool>> And<T1, T2, T3, T4>(this Expression<Func<T1, T2, T3, T4, bool>> exp1, Expression<Func<T1, T2, T3, T4, bool>> exp2) => And(exp1, true, exp2);
        /// <summary>
        /// 使用 and 拼接两个 lambda 表达式
        /// </summary>
        /// <param name="exp1"></param>
        /// <param name="condition">true 时生效</param>
        /// <param name="exp2"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, bool>> And<T1, T2, T3, T4>(this Expression<Func<T1, T2, T3, T4, bool>> exp1, bool condition, Expression<Func<T1, T2, T3, T4, bool>> exp2) => (Expression<Func<T1, T2, T3, T4, bool>>)InternalAndOrExpression(condition, exp1, exp2, true);

        /// <summary>
        /// 使用 or 拼接两个 lambda 表达式
        /// </summary>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, bool>> Or<T1, T2, T3, T4>(this Expression<Func<T1, T2, T3, T4, bool>> exp1, Expression<Func<T1, T2, T3, T4, bool>> exp2) => Or(exp1, true, exp2);
        /// <summary>
        /// 使用 or 拼接两个 lambda 表达式
        /// </summary>
        /// <param name="exp1"></param>
        /// <param name="condition">true 时生效</param>
        /// <param name="exp2"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, bool>> Or<T1, T2, T3, T4>(this Expression<Func<T1, T2, T3, T4, bool>> exp1, bool condition, Expression<Func<T1, T2, T3, T4, bool>> exp2) => (Expression<Func<T1, T2, T3, T4, bool>>)InternalAndOrExpression(condition, exp1, exp2, false);

        /// <summary>
        /// 将 lambda 表达式取反
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="condition">true 时生效</param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, bool>> Not<T1, T2, T3, T4>(this Expression<Func<T1, T2, T3, T4, bool>> exp, bool condition = true) => (Expression<Func<T1, T2, T3, T4, bool>>)InternalNotExpression(condition, exp);
        #endregion

        #region T1, T2, T3, T4, T5
        /// <summary>
        /// 使用 and 拼接两个 lambda 表达式
        /// </summary>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, bool>> And<T1, T2, T3, T4, T5>(this Expression<Func<T1, T2, T3, T4, T5, bool>> exp1, Expression<Func<T1, T2, T3, T4, T5, bool>> exp2) => And(exp1, true, exp2);
        /// <summary>
        /// 使用 and 拼接两个 lambda 表达式
        /// </summary>
        /// <param name="exp1"></param>
        /// <param name="condition">true 时生效</param>
        /// <param name="exp2"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, bool>> And<T1, T2, T3, T4, T5>(this Expression<Func<T1, T2, T3, T4, T5, bool>> exp1, bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> exp2) => (Expression<Func<T1, T2, T3, T4, T5, bool>>)InternalAndOrExpression(condition, exp1, exp2, true);

        /// <summary>
        /// 使用 or 拼接两个 lambda 表达式
        /// </summary>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, bool>> Or<T1, T2, T3, T4, T5>(this Expression<Func<T1, T2, T3, T4, T5, bool>> exp1, Expression<Func<T1, T2, T3, T4, T5, bool>> exp2) => Or(exp1, true, exp2);
        /// <summary>
        /// 使用 or 拼接两个 lambda 表达式
        /// </summary>
        /// <param name="exp1"></param>
        /// <param name="condition">true 时生效</param>
        /// <param name="exp2"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, bool>> Or<T1, T2, T3, T4, T5>(this Expression<Func<T1, T2, T3, T4, T5, bool>> exp1, bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> exp2) => (Expression<Func<T1, T2, T3, T4, T5, bool>>)InternalAndOrExpression(condition, exp1, exp2, false);

        /// <summary>
        /// 将 lambda 表达式取反
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="condition">true 时生效</param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, bool>> Not<T1, T2, T3, T4, T5>(this Expression<Func<T1, T2, T3, T4, T5, bool>> exp, bool condition = true) => (Expression<Func<T1, T2, T3, T4, T5, bool>>)InternalNotExpression(condition, exp);
        #endregion

        internal static bool IsParameter(this Expression exp)
        {
            var test = new TestParameterExpressionVisitor();
            test.Visit(exp);
            return test.Result;
        }
    }

    internal class NewExpressionVisitor : ExpressionVisitor
    {
        ParameterExpression[] _newParameters;
        ParameterExpression[] _oldParameters;
        public NewExpressionVisitor(ParameterExpression newParam, ParameterExpression oldParam) : this(new[] { newParam }, new[] { oldParam }) { }
        public NewExpressionVisitor(ParameterExpression[] newParams, ParameterExpression[] oldParams)
        {
            this._newParameters = newParams;
            this._oldParameters = oldParams;
        }
        public Expression Replace(Expression exp) => this.Visit(exp);

        protected override Expression VisitParameter(ParameterExpression node)
        {
            for (var a = 0; a < _oldParameters.Length; a++)
                if (_oldParameters[a] == node)
                    return _newParameters[a];
            return node;
        }
    }

    internal class TestParameterExpressionVisitor : ExpressionVisitor
    {
        public bool Result { get; private set; }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (!Result) Result = true;
            return node;
        }
    }
}