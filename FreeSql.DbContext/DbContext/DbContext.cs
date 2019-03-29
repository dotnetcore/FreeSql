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

		Object<DbConnection> _conn;
		DbTransaction _tran;

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

			//_fsql.Aop.ToList += AopToList;
		}

		protected virtual void OnConfiguring(DbContextOptionsBuilder builder) {
			
		}

		public DbSet<TEntity> Set<TEntity>() where TEntity : class => this.Set(typeof(TEntity)) as DbSet<TEntity>;
		public object Set(Type entityType) => Activator.CreateInstance(typeof(BaseDbSet<>).MakeGenericType(entityType), this);

		protected Dictionary<string, object> AllSets { get; } = new Dictionary<string, object>();

		internal class ExecCommandInfo {
			public ExecCommandInfoType actionType { get; set; }
			public object dbSet { get; set; }
			public Type stateType { get; set; }
			public object state { get; set; }
		}
		internal enum ExecCommandInfoType { Insert, Update, Delete }
		Queue<ExecCommandInfo> _actions = new Queue<ExecCommandInfo>();
		internal long _affrows = 0;

		internal void EnqueueAction(ExecCommandInfoType actionType, object dbSet, Type stateType, object state) {
			_actions.Enqueue(new ExecCommandInfo { actionType = actionType, dbSet = dbSet, stateType = stateType, state = state });
		}

		void ReturnObject() {
			_fsql.Ado.MasterPool.Return(_conn);
			_tran = null;
			_conn = null;
		}
		internal DbTransaction GetOrBeginTransaction(bool isCreate = true) {

			if (_tran != null) return _tran;
			if (isCreate == false) return null;
			if (_conn != null) _fsql.Ado.MasterPool.Return(_conn);

			_conn = _fsql.Ado.MasterPool.Get();
			try {
				_tran = _conn.Value.BeginTransaction();
			} catch {
				ReturnObject();
				throw;
			}
			return _tran;
		}

		void Commit() {
			if (_tran != null) {
				try {
					_tran.Commit();
				} finally {
					ReturnObject();
				}
			}
		}
		void Rollback() {
			if (_tran != null) {
				try {
					_tran.Rollback();
				} finally {
					ReturnObject();
				}
			}
		}
		public void Dispose() {
			//_fsql.Aop.ToList -= AopToList;
			this.Rollback();
		}
	}
}
