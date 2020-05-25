using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;

namespace FreeSql.Extensions.Linq
{
    public static class ExprHelper
    {
        public static object GetConstExprValue(this Expression exp)
        {
            if (exp.IsParameter()) return null;

            var expStack = new Stack<Expression>();
            var exp2 = exp;
            while (true)
            {
                switch (exp2?.NodeType)
                {
                    case ExpressionType.Constant:
                        expStack.Push(exp2);
                        break;
                    case ExpressionType.MemberAccess:
                        expStack.Push(exp2);
                        exp2 = (exp2 as MemberExpression).Expression;
                        if (exp2 == null) break;
                        continue;
                    case ExpressionType.Call:
                        return Expression.Lambda(exp).Compile().DynamicInvoke();
                    case ExpressionType.TypeAs:
                    case ExpressionType.Convert:
                        var oper2 = (exp2 as UnaryExpression).Operand;
                        if (oper2.NodeType == ExpressionType.Parameter)
                        {
                            var oper2Parm = oper2 as ParameterExpression;
                            expStack.Push(exp2.Type.IsAbstract || exp2.Type.IsInterface ? oper2Parm : Expression.Parameter(exp2.Type, oper2Parm.Name));
                        }
                        else
                            expStack.Push(oper2);
                        break;
                }
                break;
            }
            object firstValue = null;
            switch (expStack.First().NodeType)
            {
                case ExpressionType.Constant:
                    var expStackFirst = expStack.Pop() as ConstantExpression;
                    firstValue = expStackFirst?.Value;
                    break;
                case ExpressionType.MemberAccess:
                    var expStackFirstMem = expStack.First() as MemberExpression;
                    if (expStackFirstMem.Expression?.NodeType == ExpressionType.Constant)
                        firstValue = (expStackFirstMem.Expression as ConstantExpression)?.Value;
                    else
                        return Expression.Lambda(exp).Compile().DynamicInvoke();
                    break;
            }
            while (expStack.Any())
            {
                var expStackItem = expStack.Pop();
                switch (expStackItem.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        var memExp = expStackItem as MemberExpression;
                        if (memExp.Member.MemberType == MemberTypes.Property)
                            firstValue = ((PropertyInfo)memExp.Member).GetValue(firstValue, null);
                        else if (memExp.Member.MemberType == MemberTypes.Field)
                            firstValue = ((FieldInfo)memExp.Member).GetValue(firstValue);
                        break;
                }
            }
            return firstValue;
        }

        public static bool IsParameter(this Expression exp)
        {
            var test = new TestParameterExpressionVisitor();
            test.Visit(exp);
            return test.Result;
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
}
