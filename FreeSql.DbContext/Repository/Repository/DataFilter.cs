using FreeSql.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace FreeSql
{
    public class RepositoryDataFilter
    {
        internal class FilterItemByOrm
        {
            public GlobalFilter.Item Filter { get; set; }
            public bool IsEnabled { get; set; }
        }

        internal ConcurrentDictionary<string, FilterItemByOrm> _filtersByOrm = new ConcurrentDictionary<string, FilterItemByOrm>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// 开启过滤器，若使用 using 则使用完后，恢复为原有状态
        /// </summary>
        /// <param name="filterName">过滤器名称</param>
        /// <returns></returns>
        public IDisposable Disable(params string[] filterName)
        {
            if (filterName == null || filterName.Any() == false) return new UsingAny(() => { });

            List<string> restoreByOrm = new List<string>();
            foreach (var name in filterName)
            {
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
                restoreByOrm.ForEach(name =>
                {
                    if (_filtersByOrm.TryGetValue(name, out var tryfiByOrm) && tryfiByOrm.IsEnabled == false)
                        tryfiByOrm.IsEnabled = true;
                });
            });
        }
        /// <summary>
        /// 开启所有过滤器，若使用 using 则使用完后，恢复为原有状态
        /// </summary>
        /// <returns></returns>
        public IDisposable DisableAll()
        {
            List<string> restoreByOrm = new List<string>();
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

        /// <summary>
        /// 禁用过滤器，若使用 using 则使用完后，恢复为原有状态
        /// </summary>
        /// <param name="filterName"></param>
        /// <returns></returns>
        public IDisposable Enable(params string[] filterName)
        {
            if (filterName == null || filterName.Any() == false) return new UsingAny(() => { });

            List<string> restoreByOrm = new List<string>();
            foreach (var name in filterName)
            {
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
                restoreByOrm.ForEach(name =>
                {
                    if (_filtersByOrm.TryGetValue(name, out var tryfiByOrm) && tryfiByOrm.IsEnabled == true)
                        tryfiByOrm.IsEnabled = false;
                });
            });
        }
        /// <summary>
        /// 禁用所有过滤器，若使用 using 则使用完后，恢复为原有状态
        /// </summary>
        /// <returns></returns>
        public IDisposable EnableAll()
        {
            List<string> restoreByOrm = new List<string>();
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
            return _filtersByOrm.TryGetValue(filterName, out var tryfiByOrm) ? tryfiByOrm.IsEnabled : false;
        }
    }
}