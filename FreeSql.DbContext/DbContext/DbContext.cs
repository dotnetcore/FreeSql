using SafeObjectPool;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;

namespace FreeSql {
	public abstract partial class DbContext : IDisposable {

		internal IFreeSql _orm;
		internal IFreeSql _fsql => _orm ?? throw new ArgumentNullException("请在 OnConfiguring 或 AddFreeDbContext 中配置 UseFreeSql");

		IUnitOfWork _uowPriv;
		internal IUnitOfWork _uow => _isUseUnitOfWork ? (_uowPriv ?? (_uowPriv = new UnitOfWork(_fsql))) : null;
		internal bool _isUseUnitOfWork = true; //不使用工作单元事务

		static ConcurrentDictionary<Type, PropertyInfo[]> _dicGetDbSetProps = new ConcurrentDictionary<Type, PropertyInfo[]>();
		protected DbContext() {

			var builder = new DbContextOptionsBuilder();
			OnConfiguring(builder);
			_orm = builder._fsql;

			var props = _dicGetDbSetProps.GetOrAdd(this.GetType(), tp => 
				tp.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
					.Where(a => a.PropertyType.IsGenericType &&
						a.PropertyType == typeof(DbSet<>).MakeGenericType(a.PropertyType.GenericTypeArguments[0])).ToArray());

			foreach (var prop in props) {
				var set = this.Set(prop.PropertyType.GenericTypeArguments[0]);

				prop.SetValue(this, set);
				AllSets.Add(prop.Name, set);
			}
		}

		protected virtual void OnConfiguring(DbContextOptionsBuilder builder) {
			
		}


		protected Dictionary<Type, object> _dicSet = new Dictionary<Type, object>();
		public DbSet<TEntity> Set<TEntity>() where TEntity : class => this.Set(typeof(TEntity)) as DbSet<TEntity>;
		public virtual object Set(Type entityType) {
			if (_dicSet.ContainsKey(entityType)) return _dicSet[entityType];
			var sd = Activator.CreateInstance(typeof(DbContextDbSet<>).MakeGenericType(entityType), this);
			_dicSet.Add(entityType, sd);
			return sd;
		}
		protected Dictionary<string, object> AllSets { get; } = new Dictionary<string, object>();

		internal class ExecCommandInfo {
			public ExecCommandInfoType actionType { get; set; }
			public object dbSet { get; set; }
			public Type stateType { get; set; }
			public object state { get; set; }
		}
		internal enum ExecCommandInfoType { Insert, Update, Delete }
		Queue<ExecCommandInfo> _actions = new Queue<ExecCommandInfo>();
		internal int _affrows = 0;

		internal void EnqueueAction(ExecCommandInfoType actionType, object dbSet, Type stateType, object state) {
			_actions.Enqueue(new ExecCommandInfo { actionType = actionType, dbSet = dbSet, stateType = stateType, state = state });
		}

		~DbContext() {
			this.Dispose();
		}
		bool _isdisposed = false;
		public void Dispose() {
			if (_isdisposed) return;
			try {
				_actions.Clear();
				_dicSet.Clear();
				AllSets.Clear();
				
				_uow?.Rollback();
			} finally {
				_isdisposed = true;
				GC.SuppressFinalize(this);
			}
		}
	}
}
