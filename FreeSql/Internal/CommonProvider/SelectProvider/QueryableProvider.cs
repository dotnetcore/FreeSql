using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FreeSql.Internal.CommonProvider
{
    class QueryableProvider<T> : IQueryable<T>
    {
        private Expression _expression;
        private IQueryProvider _provider;
        private object _select;
        private CommonExpression _commonExpression;

        public QueryableProvider(object select)
        {
            _select = select;
            _commonExpression = _select.GetType().GetField("_commonExpression", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_select) as CommonExpression;
            _expression = Expression.Constant(this);
            _provider = new QueryProvider<T>(_select, _commonExpression);
        }
        public QueryableProvider(Expression expression, IQueryProvider provider, object select, CommonExpression commonExpression)
        {
            _select = select;
            _commonExpression = commonExpression;
            _expression = expression;
            _provider = provider;
        }

        public IEnumerator<T> GetEnumerator()
        {
            var result = _provider.Execute<List<T>>(_expression);
            if (result == null)
                yield break;
            foreach (var item in result)
            {
                yield return item;
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

        public Type ElementType => typeof(QueryableProvider<T>);
        public Expression Expression => _expression;
        public IQueryProvider Provider => _provider;
    }

    class QueryProvider<T> : IQueryProvider
    {
        private object _select;
        private CommonExpression _commonExpression;

        public QueryProvider(object select, CommonExpression commonExpression)
        {
            _select = select;
            _commonExpression = commonExpression;
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            IQueryable<TElement> query = new QueryableProvider<TElement>(expression, this, _select, _commonExpression);
            return query;
        }
        public IQueryable CreateQuery(Expression expression) => throw new NotImplementedException();


        public TResult Execute<TResult>(Expression expression)
        {
            var methodExp = expression as MethodCallExpression;
            while (methodExp != null)
            {
                switch (methodExp.Method.Name)
                {
                    case "First":
                    case "FirstOrDefault":
                        _select.GetType().GetMethod("Limit", new[] { typeof(int) }).Invoke(_select, new object[] { 1 });
                        break;
                    default:
                        var selectMethod = _select.GetType().GetMethod(methodExp.Method.Name, methodExp.Arguments.Where((a, b) => b > 0).Select(a => a.Type).ToArray());
                        if (selectMethod == null) throw new Exception($"无法找到 ISelect.{methodExp.Method.Name}({string.Join(", ", methodExp.Arguments.Select(a => a.Type.FullName))}) 方法");

                        var selectArgs = methodExp.Arguments.Where((a, b) => b > 0).Select(a =>
                        {
                            switch (a.NodeType)
                            {
                                case ExpressionType.Lambda: return (object)a;
                                default: return Expression.Lambda(a).Compile().DynamicInvoke();
                            }
                        }).ToArray();
                        selectMethod.Invoke(_select, selectArgs);
                        break;
                }
                methodExp = methodExp.Arguments.FirstOrDefault() as MethodCallExpression;
            }
            var resultType = typeof(TResult);
            var resultTypeIsList = typeof(IList).IsAssignableFrom(resultType);
            if (resultTypeIsList) resultType = resultType.GetGenericArguments()[0];
            var ret = _select.GetType().GetMethod(resultTypeIsList ? "ToList" : "First", new Type[0])
                .MakeGenericMethod(resultType)
                .Invoke(_select, new object[0]);
            return (TResult)ret;
        }
        public object Execute(Expression expression) => throw new NotImplementedException();
    }
}
