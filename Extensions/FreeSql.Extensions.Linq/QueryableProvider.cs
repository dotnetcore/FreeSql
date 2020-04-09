using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.Internal.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FreeSql.Extensions.Linq
{
    class QueryableProvider<TCurrent, TSource> : IQueryable<TCurrent>, IOrderedQueryable<TCurrent> where TSource : class
    {
        private Expression _expression;
        private IQueryProvider _provider;
        private Select1Provider<TSource> _select;

        public QueryableProvider(Select1Provider<TSource> select)
        {
            _select = select;
            _expression = Expression.Constant(this);
            _provider = new QueryProvider<TCurrent, TSource>(_select);
        }
        public QueryableProvider(Expression expression, IQueryProvider provider, Select1Provider<TSource> select)
        {
            _select = select;
            _expression = expression;
            _provider = provider;
        }

        public IEnumerator<TCurrent> GetEnumerator()
        {
            var result = _provider.Execute<List<TCurrent>>(_expression);
            if (result == null)
                yield break;
            foreach (var item in result)
            {
                yield return item;
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

        public Type ElementType => typeof(QueryableProvider<TCurrent, TSource>);
        public Expression Expression => _expression;
        public IQueryProvider Provider => _provider;
    }

    class QueryProvider<TCurrent, TSource> : IQueryProvider where TSource : class
    {
        private Select1Provider<TSource> _select;

        public QueryProvider(Select1Provider<TSource> select)
        {
            _select = select;
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            if (typeof(TElement) != typeof(TCurrent))
                return new QueryableProvider<TElement, TSource>(expression, new QueryProvider<TElement, TSource>(_select), _select);

            return new QueryableProvider<TElement, TSource>(expression, this, _select);
        }
        public IQueryable CreateQuery(Expression expression) => throw new NotImplementedException();

        public TResult Execute<TResult>(Expression expression)
        {
            var stackCallExps = new Stack<MethodCallExpression>();
            var callExp = expression as MethodCallExpression;
            while(callExp != null)
            {
                stackCallExps.Push(callExp);
                callExp = callExp?.Arguments.FirstOrDefault() as MethodCallExpression;
            }

            SelectGroupingProvider groupBy = null;
            var isfirst = false;
            while (stackCallExps.Any())
            {
                callExp = stackCallExps.Pop();
                TResult throwCallExp(string message) => throw new Exception($"FreeSql Queryable 解析出错，执行的方法 {callExp.Method.Name} {message}");
                if (callExp.Method.DeclaringType != typeof(Queryable)) return throwCallExp($"必须属于 System.Linq.Enumerable");

                TResult tplMaxMinAvgSum(string method) {
                    if (callExp.Arguments.Count == 2)
                    {
                        var avgParam = (callExp.Arguments[1] as UnaryExpression)?.Operand as LambdaExpression;
                        return (TResult)Utils.GetDataReaderValue(typeof(TResult), 
                            _select.GetType().GetMethod(method).MakeGenericMethod(avgParam.ReturnType).Invoke(_select, new object[] { avgParam }));
                    }
                    return throwCallExp($" 不支持 {callExp.Arguments.Count}个参数的方法");
                }
                TResult tplOrderBy(string method, bool isDescending)
                {
                    if (callExp.Arguments.Count == 2)
                    {
                        var arg1 = (callExp.Arguments[1] as UnaryExpression)?.Operand as LambdaExpression;
                        _select.OrderByReflection(arg1, isDescending);
                        return default(TResult);
                    }
                    return throwCallExp($" 不支持 {callExp.Arguments.Count}个参数的方法");
                }
                switch (callExp.Method.Name)
                {
                    case "Any":
                        if (callExp.Arguments.Count == 2) _select.Where((Expression<Func<TSource, bool>>)(callExp.Arguments[1] as UnaryExpression)?.Operand);
                        return (TResult)(object)_select.Any();
                    case "AsQueryable":
                        break;

                    case "Max": return tplMaxMinAvgSum("Max");
                    case "Min": return tplMaxMinAvgSum("Min");
                    case "Sum": return tplMaxMinAvgSum("Sum");
                    case "Average": return tplMaxMinAvgSum("Avg");

                    case "Concat":
                        return throwCallExp(" 不支持");
                    case "Contains":
                        if (callExp.Arguments.Count == 2)
                        {
                            var dywhere = (callExp.Arguments[1] as ConstantExpression)?.Value as TSource;
                            if (dywhere == null) return throwCallExp($" 参数值不能为 null");
                            _select.WhereDynamic(dywhere);
                            return (TResult)(object)_select.Any();
                        }
                        return throwCallExp($" 不支持 {callExp.Arguments.Count}个参数的方法");
                    case "Count":
                        if (callExp.Arguments.Count == 2) _select.Where((Expression<Func<TSource, bool>>)(callExp.Arguments[1] as UnaryExpression)?.Operand);
                        return (TResult)Utils.GetDataReaderValue(typeof(TResult), _select.Count());
                    
                    case "Distinct":
                        if (callExp.Arguments.Count == 1)
                        {
                            _select.Distinct();
                            break;
                        }
                        return throwCallExp(" 不支持");

                    case "ElementAt":
                    case "ElementAtOrDefault":
                        _select.Offset((int)(callExp.Arguments[1] as ConstantExpression)?.Value);
                        _select.Limit(1);
                        isfirst = true;
                        break;
                    case "First":
                    case "FirstOrDefault":
                    case "Single":
                    case "SingleOrDefault":
                        if (callExp.Arguments.Count == 2) _select.Where((Expression<Func<TSource, bool>>)(callExp.Arguments[1] as UnaryExpression)?.Operand);
                        _select.Limit(1);
                        isfirst = true;
                        break;

                    case "OrderBy": 
                        tplOrderBy("OrderByReflection", false);
                        break;
                    case "OrderByDescending": 
                        tplOrderBy("OrderByReflection", true); 
                        break;
                    case "ThenBy": 
                        tplOrderBy("OrderByReflection", false); 
                        break;
                    case "ThenByDescending": 
                        tplOrderBy("OrderByReflection", true); 
                        break;

                    case "Where":
                        var whereParam = (callExp.Arguments[1] as UnaryExpression)?.Operand as LambdaExpression;
                        if (whereParam.Parameters.Count == 1)
                        {
                            if (groupBy != null) groupBy.InternalHaving(whereParam);
                            else _select.InternalWhere(whereParam);
                            break;
                        }
                        return throwCallExp(" 不支持");

                    case "Skip":
                        _select.Offset((int)(callExp.Arguments[1] as ConstantExpression)?.Value);
                        break;
                    case "Take":
                        _select.Limit((int)(callExp.Arguments[1] as ConstantExpression)?.Value);
                        break;

                    case "ToList":
                        if (callExp.Arguments.Count == 1)
                            return (TResult)(object)_select.ToList();
                        return throwCallExp(" 不支持");


                    case "Select":
                        var selectParam = (callExp.Arguments[1] as UnaryExpression)?.Operand as LambdaExpression;
                        if (selectParam.Parameters.Count == 1)
                        {
                            _select._selectExpression = selectParam;
                            break;
                        }
                        return throwCallExp(" 不支持");

                    case "Join":
                        if (callExp.Arguments.Count == 5)
                        {
                            var arg2 = (callExp.Arguments[2] as UnaryExpression)?.Operand as LambdaExpression;
                            var arg3 = (callExp.Arguments[3] as UnaryExpression)?.Operand as LambdaExpression;
                            var arg4 = (callExp.Arguments[4] as UnaryExpression)?.Operand as LambdaExpression;
                            FreeSqlExtensionsLinqSql.InternalJoin2(_select, arg2, arg3, arg4);
                            _select._selectExpression = arg4.Body;
                            break;
                        }
                        return throwCallExp($" 不支持 {callExp.Arguments.Count}个参数的方法");

                    case "GroupJoin":
                        if (callExp.Arguments.Count == 5)
                        {
                            var arg2 = (callExp.Arguments[2] as UnaryExpression)?.Operand as LambdaExpression;
                            var arg3 = (callExp.Arguments[3] as UnaryExpression)?.Operand as LambdaExpression;
                            var arg4 = (callExp.Arguments[4] as UnaryExpression)?.Operand as LambdaExpression;
                            FreeSqlExtensionsLinqSql.InternalJoin2(_select, arg2, arg3, arg4);
                            _select._selectExpression = arg4.Body;
                            break;
                        }
                        return throwCallExp($" 不支持 {callExp.Arguments.Count}个参数的方法");

                    case "SelectMany":
                        if (callExp.Arguments.Count == 3)
                        {
                            var arg1 = (callExp.Arguments[1] as UnaryExpression)?.Operand as LambdaExpression;
                            var arg2 = (callExp.Arguments[2] as UnaryExpression)?.Operand as LambdaExpression;
                            FreeSqlExtensionsLinqSql.InternalSelectMany2(_select, arg1, arg2);
                            _select._selectExpression = arg2.Body;
                            break;
                        }
                        return throwCallExp($" 不支持 {callExp.Arguments.Count}个参数的方法");

                    case "DefaultIfEmpty":
                        break;

                    case "Last":
                    case "LastOrDefault":
                        return throwCallExp(" 不支持");

                    case "GroupBy":
                        return throwCallExp(" 不支持");

                        if (callExp.Arguments.Count == 2) //TODO: 待实现
                        {
                            var arg1 = (callExp.Arguments[1] as UnaryExpression)?.Operand as LambdaExpression;

                            var map = new ReadAnonymousTypeInfo();
                            var field = new StringBuilder();
                            var index = -10000; //临时规则，不返回 as1

                            _select._commonExpression.ReadAnonymousField(_select._tables, field, map, ref index, arg1, null, _select._whereCascadeExpression, false); //不走 DTO 映射
                            var sql = field.ToString();
                            _select.GroupBy(sql.Length > 0 ? sql.Substring(2) : null);
                            groupBy = new SelectGroupingProvider(_select._orm, _select, map, sql, _select._commonExpression, _select._tables);
                            break;
                        }
                        return throwCallExp($" 不支持 {callExp.Arguments.Count}个参数的方法");

                    default:
                        return throwCallExp(" 不支持");
                }
            }
            if (isfirst)
            {
                _select.Limit(1);
                if (_select._selectExpression != null)
                    return (TResult)(object)_select.InternalToList<TCurrent>(_select._selectExpression).FirstOrDefault();
                return (TResult)(object)_select.ToList().FirstOrDefault();
            }
            if (_select._selectExpression != null)
                return (TResult)(object)_select.InternalToList<TCurrent>(_select._selectExpression);
            return (TResult)(object)_select.ToList();
        }
        public object Execute(Expression expression) => throw new NotImplementedException();
    }
}
