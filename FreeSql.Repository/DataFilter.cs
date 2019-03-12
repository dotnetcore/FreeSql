using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Text;
using System.Linq;

namespace FreeSql {
	public interface IDataFilter<TEntity> where TEntity : class {

		IDataFilter<TEntity> Apply(string filterName, Expression<Func<TEntity, bool>> filterAndValidateExp);

		IDataFilter<TEntity> Enable(params string[] filterName);
		IDataFilter<TEntity> EnableAll();

		IDataFilter<TEntity> Disable(params string[] filterName);
		IDataFilter<TEntity> DisableAll();

		bool IsEnabled(string filterName);
	}

	internal class DataFilter<TEntity> : IDataFilter<TEntity> where TEntity : class {

		internal class FilterItem {
			public Expression<Func<TEntity, bool>> Expression { get; set; }
			Func<TEntity, bool> _expressionDelegate;
			public Func<TEntity, bool> ExpressionDelegate => _expressionDelegate ?? (_expressionDelegate = Expression?.Compile());
			public bool IsEnabled { get; set; }
		}

		internal ConcurrentDictionary<string, FilterItem> _filters = new ConcurrentDictionary<string, FilterItem>(StringComparer.CurrentCultureIgnoreCase);
		public IDataFilter<TEntity> Apply(string filterName, Expression<Func<TEntity, bool>> filterAndValidateExp) {

			if (filterName == null)
				throw new ArgumentNullException(nameof(filterName));
			if (filterAndValidateExp == null) return this;

			var filterItem = new FilterItem { Expression = filterAndValidateExp, IsEnabled = true };
			_filters.AddOrUpdate(filterName, filterItem, (k, v) => filterItem);
			return this;
		}

		public IDataFilter<TEntity> Disable(params string[] filterName) {
			if (filterName == null || filterName.Any() == false) return this;

			foreach (var name in filterName) {
				if (_filters.TryGetValue(name, out var tryfi))
					tryfi.IsEnabled = false;
			}
			return this;
		}
		public IDataFilter<TEntity> DisableAll() {
			foreach (var val in _filters.Values.ToArray())
				val.IsEnabled = false;
			return this;
		}

		public IDataFilter<TEntity> Enable(params string[] filterName) {
			if (filterName == null || filterName.Any() == false) return this;

			foreach (var name in filterName) {
				if (_filters.TryGetValue(name, out var tryfi))
					tryfi.IsEnabled = true;
			}
			return this;
		}
		public IDataFilter<TEntity> EnableAll() {
			foreach (var val in _filters.Values.ToArray())
				val.IsEnabled = true;
			return this;
		}

		public bool IsEnabled(string filterName) {
			if (filterName == null) return false;
			return _filters.TryGetValue(filterName, out var tryfi) ? tryfi.IsEnabled : false;
		}
	}

	public class GlobalDataFilter {

		internal List<(Type type, string name, LambdaExpression exp)> _filters = new List<(Type type, string name, LambdaExpression exp)>();

		public GlobalDataFilter Apply<TEntity>(string filterName, Expression<Func<TEntity, bool>> filterAndValidateExp) where TEntity : class {
			if (filterName == null)
				throw new ArgumentNullException(nameof(filterName));
			if (filterAndValidateExp == null) return this;

			_filters.Add((typeof(TEntity), filterName, filterAndValidateExp));
			return this;
		}
	}
}
