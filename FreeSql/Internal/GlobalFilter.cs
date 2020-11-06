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
            internal Func<bool> Condition { get; set; }
            public LambdaExpression Where { get; internal set; }
            public bool Only { get; internal set; }
        }
        /// <summary>
        /// 创建一个过滤器<para></para>
        /// 提示：在 Lambda 中判断登陆身份，请参考资料 AsyncLocal
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="name">名字</param>
        /// <param name="where">表达式</param>
        /// <returns></returns>
        public GlobalFilter Apply<TEntity>(string name, Expression<Func<TEntity, bool>> where) => Apply(false, name, () => true, where);
        /// <summary>
        /// 创建一个动态过滤器，当 condition 返回值为 true 时才生效<para></para>
        /// 场景：当登陆身份是管理员，则过滤条件不生效<para></para>
        /// 提示：在 Lambda 中判断登陆身份，请参考资料 AsyncLocal
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="name">名字</param>
        /// <param name="condition">委托，返回值为 true 时才生效</param>
        /// <param name="where">表达式</param>
        /// <returns></returns>
        public GlobalFilter ApplyIf<TEntity>(string name, Func<bool> condition, Expression<Func<TEntity, bool>> where) => Apply(false, name, condition, where);

        /// <summary>
        /// 创建一个过滤器（实体类型 属于指定 TEntity 才会生效）<para></para>
        /// 提示：在 Lambda 中判断登陆身份，请参考资料 AsyncLocal
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="name">名字</param>
        /// <param name="where">表达式</param>
        /// <returns></returns>
        public GlobalFilter ApplyOnly<TEntity>(string name, Expression<Func<TEntity, bool>> where) => Apply(true, name, () => true, where);
        /// <summary>
        /// 创建一个过滤器（实体类型 属于指定 TEntity 才会生效）<para></para>
        /// 场景：当登陆身份是管理员，则过滤条件不生效<para></para>
        /// 提示：在 Lambda 中判断登陆身份，请参考资料 AsyncLocal
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="name">名字</param>
        /// <param name="condition">委托，返回值为 true 时才生效</param>
        /// <param name="where">表达式</param>
        /// <returns></returns>
        public GlobalFilter ApplyOnlyIf<TEntity>(string name, Func<bool> condition, Expression<Func<TEntity, bool>> where) => Apply(true, name, condition, where);

        GlobalFilter Apply<TEntity>(bool only, string name, Func<bool> condition, Expression<Func<TEntity, bool>> where)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (where == null) return this;

            _filters.TryGetValue(name, out var item);
            if (item == null) item = new Item { Id = ++_id, Name = name };

            var newParameter = Expression.Parameter(typeof(TEntity), $"gf{_id}");
            var newlambda = Expression.Lambda<Func<TEntity, bool>>(
                new CommonExpression.ReplaceParameterVisitor().Modify(where, newParameter),
                newParameter
            );
            item.Where = newlambda;
            item.Condition = condition;
            item.Only = only;
            _filters.AddOrUpdate(name, item, (_, __) => item);
            return this;
        }

        public void Remove(string name) => _filters.TryRemove(name ?? throw new ArgumentNullException(nameof(name)), out var _);

        public List<Item> GetFilters() => _filters.Values.Where(a => a.Condition?.Invoke() != false).OrderBy(a => a.Id).ToList();
    }
}
