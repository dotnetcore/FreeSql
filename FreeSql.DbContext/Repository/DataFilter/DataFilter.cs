using FreeSql.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace FreeSql
{
    public interface IDataFilter<TEntity> : IDisposable where TEntity : class
    {

        IDataFilter<TEntity> Apply(string filterName, Expression<Func<TEntity, bool>> filterAndValidateExp);

        /// <summary>
        /// 开启过滤器，若使用 using 则使用完后，恢复为原有状态
        /// </summary>
        /// <param name="filterName">过滤器名称</param>
        /// <returns></returns>
        IDisposable Enable(params string[] filterName);
        /// <summary>
        /// 开启所有过滤器，若使用 using 则使用完后，恢复为原有状态
        /// </summary>
        /// <returns></returns>
        IDisposable EnableAll();

        /// <summary>
        /// 禁用过滤器，若使用 using 则使用完后，恢复为原有状态
        /// </summary>
        /// <param name="filterName"></param>
        /// <returns></returns>
        IDisposable Disable(params string[] filterName);
        /// <summary>
        /// 禁用所有过滤器，若使用 using 则使用完后，恢复为原有状态
        /// </summary>
        /// <returns></returns>
        IDisposable DisableAll();

        bool IsEnabled(string filterName);
    }

    internal class DataFilter<TEntity> : IDataFilter<TEntity> where TEntity : class
    {

        internal class FilterItem
        {
            public Expression<Func<TEntity, bool>> Expression { get; set; }
            Func<TEntity, bool> _expressionDelegate;
            public Func<TEntity, bool> ExpressionDelegate => _expressionDelegate ?? (_expressionDelegate = Expression?.Compile());
            public bool IsEnabled { get; set; }
        }
        internal class FilterItemByOrm
        {
            public GlobalFilter.Item Filter { get; set; }
            public bool IsEnabled { get; set; }
        }

        internal ConcurrentDictionary<string, FilterItem> _filters = new ConcurrentDictionary<string, FilterItem>(StringComparer.CurrentCultureIgnoreCase);
        internal ConcurrentDictionary<string, FilterItemByOrm> _filtersByOrm = new ConcurrentDictionary<string, FilterItemByOrm>(StringComparer.CurrentCultureIgnoreCase);
        public IDataFilter<TEntity> Apply(string filterName, Expression<Func<TEntity, bool>> filterAndValidateExp)
        {

            if (filterName == null)
                throw new ArgumentNullException(nameof(filterName));
            if (filterAndValidateExp == null) return this;

            var filterItem = new FilterItem { Expression = filterAndValidateExp, IsEnabled = true };
            _filters.AddOrUpdate(filterName, filterItem, (k, v) => filterItem);
            return this;
        }

        public IDisposable Disable(params string[] filterName)
        {
            if (filterName == null || filterName.Any() == false) return new UsingAny(() => { });

            List<string> restore = new List<string>();
            List<string> restoreByOrm = new List<string>();
            foreach (var name in filterName)
            {
                if (_filters.TryGetValue(name, out var tryfi))
                {
                    if (tryfi.IsEnabled)
                    {
                        restore.Add(name);
                        tryfi.IsEnabled = false;
                    }
                }
                if (_filtersByOrm.TryGetValue(name, out var tryfiByOrm))
                {
                    if (tryfiByOrm.IsEnabled)
                    {
                        restoreByOrm.Add(name);
                        tryfiByOrm.IsEnabled = false;
                    }
                }
            }
            return new UsingAny(() =>
            {
                restore.ForEach(name =>
                {
                    if (_filters.TryGetValue(name, out var tryfi) && tryfi.IsEnabled == false)
                        tryfi.IsEnabled = true;
                });
                restoreByOrm.ForEach(name =>
                {
                    if (_filtersByOrm.TryGetValue(name, out var tryfiByOrm) && tryfiByOrm.IsEnabled == false)
                        tryfiByOrm.IsEnabled = true;
                });
            });
        }
        public IDisposable DisableAll()
        {
            List<string> restore = new List<string>();
            List<string> restoreByOrm = new List<string>();
            foreach (var val in _filters)
            {
                if (val.Value.IsEnabled)
                {
                    restore.Add(val.Key);
                    val.Value.IsEnabled = false;
                }
            }
            foreach (var val in _filtersByOrm)
            {
                if (val.Value.IsEnabled)
                {
                    restoreByOrm.Add(val.Key);
                    val.Value.IsEnabled = false;
                }
            }
            return new UsingAny(() =>
            {
                restore.ForEach(name =>
                {
                    if (_filters.TryGetValue(name, out var tryfi) && tryfi.IsEnabled == false)
                        tryfi.IsEnabled = true;
                });
                restoreByOrm.ForEach(name =>
                {
                    if (_filtersByOrm.TryGetValue(name, out var tryfiByOrm) && tryfiByOrm.IsEnabled == false)
                        tryfiByOrm.IsEnabled = true;
                });
            });
        }
        class UsingAny : IDisposable
        {
            Action _ondis;
            public UsingAny(Action ondis)
            {
                _ondis = ondis;
            }
            public void Dispose()
            {
                _ondis?.Invoke();
            }
        }

        public IDisposable Enable(params string[] filterName)
        {
            if (filterName == null || filterName.Any() == false) return new UsingAny(() => { });

            List<string> restore = new List<string>();
            List<string> restoreByOrm = new List<string>();
            foreach (var name in filterName)
            {
                if (_filters.TryGetValue(name, out var tryfi))
                {
                    if (tryfi.IsEnabled == false)
                    {
                        restore.Add(name);
                        tryfi.IsEnabled = true;
                    }
                }
                if (_filtersByOrm.TryGetValue(name, out var tryfiByOrm))
                {
                    if (tryfiByOrm.IsEnabled == false)
                    {
                        restoreByOrm.Add(name);
                        tryfiByOrm.IsEnabled = true;
                    }
                }
            }
            return new UsingAny(() =>
            {
                restore.ForEach(name =>
                {
                    if (_filters.TryGetValue(name, out var tryfi) && tryfi.IsEnabled == true)
                        tryfi.IsEnabled = false;
                });
                restoreByOrm.ForEach(name =>
                {
                    if (_filtersByOrm.TryGetValue(name, out var tryfiByOrm) && tryfiByOrm.IsEnabled == true)
                        tryfiByOrm.IsEnabled = false;
                });
            });
        }
        public IDisposable EnableAll()
        {
            List<string> restore = new List<string>();
            List<string> restoreByOrm = new List<string>();
            foreach (var val in _filters)
            {
                if (val.Value.IsEnabled == false)
                {
                    restore.Add(val.Key);
                    val.Value.IsEnabled = true;
                }
            }
            foreach (var val in _filtersByOrm)
            {
                if (val.Value.IsEnabled == false)
                {
                    restoreByOrm.Add(val.Key);
                    val.Value.IsEnabled = true;
                }
            }
            return new UsingAny(() =>
            {
                restore.ForEach(name =>
                {
                    if (_filters.TryGetValue(name, out var tryfi) && tryfi.IsEnabled == true)
                        tryfi.IsEnabled = false;
                });
                restoreByOrm.ForEach(name =>
                {
                    if (_filtersByOrm.TryGetValue(name, out var tryfiByOrm) && tryfiByOrm.IsEnabled == true)
                        tryfiByOrm.IsEnabled = false;
                });
            });
        }

        public bool IsEnabled(string filterName)
        {
            if (filterName == null) return false;
            return _filters.TryGetValue(filterName, out var tryfi) ? tryfi.IsEnabled :
                _filtersByOrm.TryGetValue(filterName, out var tryfiByOrm) ? tryfiByOrm.IsEnabled : false;
        }

        ~DataFilter() => this.Dispose();
        public void Dispose()
        {
            _filters.Clear();
        }
    }

    public class FluentDataFilter : IDisposable
    {
        internal class FilterInfo
        {
            public Type type { get; }
            public string name { get; }
            public LambdaExpression exp { get; }
            public FilterInfo(Type type, string name, LambdaExpression exp)
            {
                this.type = type;
                this.name = name;
                this.exp = exp;
            }
        }
        internal List<FilterInfo> _filters = new List<FilterInfo>();

        public FluentDataFilter Apply<TEntity>(string filterName, Expression<Func<TEntity, bool>> filterAndValidateExp) where TEntity : class
        {
            if (filterName == null)
                throw new ArgumentNullException(nameof(filterName));
            if (filterAndValidateExp == null) return this;

            _filters.Add(new FilterInfo(typeof(TEntity), filterName, filterAndValidateExp));
            return this;
        }

        ~FluentDataFilter() => this.Dispose();
        public void Dispose()
        {
            _filters.Clear();
        }
    }
}
