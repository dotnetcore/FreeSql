using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FreeSql.Internal
{
    public class GlobalFilter
    {
        ConcurrentDictionary<string, Item> _filters = new ConcurrentDictionary<string, Item>(StringComparer.CurrentCultureIgnoreCase);
        int _id = 0;

        public class Item
        {
            public int Id { get; internal set; }
            public string Name { get; internal set; }
            public LambdaExpression Where { get; internal set; }
        }
        /// <summary>
        /// 创建一个过滤器
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="name">名字</param>
        /// <param name="where">表达式</param>
        /// <returns></returns>
        public GlobalFilter Apply<TEntity>(string name, Expression<Func<TEntity, bool>> where)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (where == null) return this;

            _filters.TryGetValue(name, out var item);
            if (item == null) item = new Item { Id = ++_id, Name = name };

            var newParameter = Expression.Parameter(typeof(TEntity), $"gf{_id}");
            var newlambda = Expression.Lambda<Func<TEntity, bool>>(
                new CommonExpression.ReplaceVisitor().Modify(where.Body, newParameter),
                newParameter
            );
            item.Where = newlambda;
            _filters.AddOrUpdate(name, item, (_, __) => item);
            return this;
        }
        public void Remove(string name) => _filters.TryRemove(name ?? throw new ArgumentNullException(nameof(name)), out var _);

        public List<Item> GetFilters() => _filters.Values.OrderBy(a => a.Id).ToList();
    }
}
