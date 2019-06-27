using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace FreeSql
{

    internal class DataFilterUtil
    {

        internal static Action<FluentDataFilter> _globalDataFilter;

        static ConcurrentDictionary<Type, Delegate> _dicSetRepositoryDataFilterApplyDataFilterFunc = new ConcurrentDictionary<Type, Delegate>();
        static ConcurrentDictionary<Type, ConcurrentDictionary<string, bool>> _dicSetRepositoryDataFilterConvertFilterNotExists = new ConcurrentDictionary<Type, ConcurrentDictionary<string, bool>>();
        internal static void SetRepositoryDataFilter(object repos, Action<FluentDataFilter> scopedDataFilter)
        {
            if (scopedDataFilter != null)
            {
                SetRepositoryDataFilter(repos, null);
            }
            if (scopedDataFilter == null)
            {
                scopedDataFilter = _globalDataFilter;
            }
            if (scopedDataFilter == null) return;
            using (var globalFilter = new FluentDataFilter())
            {
                scopedDataFilter(globalFilter);

                var type = repos.GetType();
                Type entityType = (repos as IBaseRepository).EntityType;
                if (entityType == null) throw new Exception("FreeSql.Repository 设置过滤器失败，原因是对象不属于 IRepository");

                var notExists = _dicSetRepositoryDataFilterConvertFilterNotExists.GetOrAdd(type, t => new ConcurrentDictionary<string, bool>());
                var newFilter = new Dictionary<string, LambdaExpression>();
                foreach (var gf in globalFilter._filters)
                {
                    if (notExists.ContainsKey(gf.name)) continue;

                    LambdaExpression newExp = null;
                    var filterParameter1 = Expression.Parameter(entityType, gf.exp.Parameters[0].Name);
                    try
                    {
                        newExp = Expression.Lambda(
                            typeof(Func<,>).MakeGenericType(entityType, typeof(bool)),
                            new ReplaceVisitor().Modify(gf.exp.Body, filterParameter1),
                            filterParameter1
                        );
                    }
                    catch
                    {
                        notExists.TryAdd(gf.name, true); //防止第二次错误
                        continue;
                    }
                    newFilter.Add(gf.name, newExp);
                }
                if (newFilter.Any() == false) return;

                var del = _dicSetRepositoryDataFilterApplyDataFilterFunc.GetOrAdd(type, t =>
                {
                    var reposParameter = Expression.Parameter(type);
                    var nameParameter = Expression.Parameter(typeof(string));
                    var expressionParameter = Expression.Parameter(
                        typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(entityType, typeof(bool)))
                    );
                    return Expression.Lambda(
                        Expression.Block(
                            Expression.Call(reposParameter, type.GetMethod("ApplyDataFilter", BindingFlags.Instance | BindingFlags.NonPublic), nameParameter, expressionParameter)
                        ),
                        new[] {
                        reposParameter, nameParameter, expressionParameter
                        }
                    ).Compile();
                });
                foreach (var nf in newFilter)
                {
                    del.DynamicInvoke(repos, nf.Key, nf.Value);
                }
                newFilter.Clear();
            }
        }
    }

    class ReplaceVisitor : ExpressionVisitor
    {
        private ParameterExpression parameter;

        public Expression Modify(Expression expression, ParameterExpression parameter)
        {
            this.parameter = parameter;
            return Visit(expression);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression?.NodeType == ExpressionType.Parameter)
                return Expression.Property(parameter, node.Member.Name);
            return base.VisitMember(node);
        }
    }
}
