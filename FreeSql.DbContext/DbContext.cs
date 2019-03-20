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
				AllSets.Add(prop, set);
			}
		}

		protected virtual void OnConfiguring(DbContextOptionsBuilder builder) {
			
		}

		public DbSet<TEntity> Set<TEntity>() where TEntity : class => this.Set(typeof(TEntity)) as DbSet<TEntity>;
		public object Set(Type entityType) => Activator.CreateInstance(typeof(BaseDbSet<>).MakeGenericType(entityType), this);

		protected Dictionary<PropertyInfo, object> AllSets => new Dictionary<PropertyInfo, object>();

		public long SaveChanges() {
			ExecCommand();
			Commit();
			return _affrows;
		}

		internal class ExecCommandInfo {
			public ExecCommandInfoType actionType { get; set; }
			public Type entityType { get; set; }
			public object dbSet { get; set; }
			public object state { get; set; }
		}
		internal enum ExecCommandInfoType { Insert, Update, Delete }
		Queue<ExecCommandInfo> _actions = new Queue<ExecCommandInfo>();
		internal long _affrows = 0;

		internal void EnqueueAction(ExecCommandInfoType actionType, Type entityType, object dbSet, object state) {
			_actions.Enqueue(new ExecCommandInfo { actionType = actionType, entityType = entityType, dbSet = dbSet, state = state });
		}

		static ConcurrentDictionary<Type, Func<object, object[], int>> _dicExecCommandInsert = new ConcurrentDictionary<Type, Func<object, object[], int>>();
		static ConcurrentDictionary<Type, Func<object, object[], int>> _dicExecCommandDelete = new ConcurrentDictionary<Type, Func<object, object[], int>>();
		static ConcurrentDictionary<Type, Func<object, object[], bool, int>> _dicExecCommandUpdate = new ConcurrentDictionary<Type, Func<object, object[], bool, int>>();
		internal void ExecCommand() {
			ExecCommandInfo oldinfo = null;
			var states = new List<object>();

			Action funcInsert = () => {
				var insertFunc = _dicExecCommandInsert.GetOrAdd(oldinfo.entityType, t => {
					var arrType = t.MakeArrayType();
					var dbsetType = typeof(DbSet<>).MakeGenericType(t);
					var dbsetTypeInsert = dbsetType.GetMethod("OrmInsert", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { arrType }, null);
					var insertBuilder = typeof(IInsert<>).MakeGenericType(t);
					var insertExecuteAffrows = insertBuilder.GetMethod("ExecuteAffrows", new Type[0]);

					var returnTarget = Expression.Label(typeof(int));
					var parm1DbSet = Expression.Parameter(typeof(object));
					var parm2Vals = Expression.Parameter(typeof(object[]));
					var var1Vals = Expression.Variable(arrType);
					return Expression.Lambda<Func<object, object[], int>>(Expression.Block(
						new[] { var1Vals },
						Expression.Assign(var1Vals, Expression.Convert(FreeSql.Internal.Utils.GetDataReaderValueBlockExpression(arrType, parm2Vals), arrType)),
						Expression.Return(returnTarget,
							Expression.Call(
								Expression.Call(Expression.Convert(parm1DbSet, dbsetType), dbsetTypeInsert, var1Vals),
								insertExecuteAffrows
							)
						),
						Expression.Label(returnTarget, Expression.Default(typeof(int)))
					), new[] { parm1DbSet, parm2Vals }).Compile();
				});
				_affrows += insertFunc(oldinfo.dbSet, states.ToArray());
				states.Clear();
			};
			Action funcDelete = () => {
				var deleteFunc = _dicExecCommandDelete.GetOrAdd(oldinfo.entityType, t => {
					var arrType = t.MakeArrayType();
					var dbsetType = typeof(DbSet<>).MakeGenericType(t);
					var dbsetTypeDelete = dbsetType.GetMethod("DbContextBetchRemove", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { arrType }, null);

					var returnTarget = Expression.Label(typeof(int));
					var parm1DbSet = Expression.Parameter(typeof(object));
					var parm2Vals = Expression.Parameter(typeof(object[]));
					var var1Vals = Expression.Variable(arrType);
					return Expression.Lambda<Func<object, object[], int>>(Expression.Block(
						new[] { var1Vals },
						Expression.Assign(var1Vals, Expression.Convert(FreeSql.Internal.Utils.GetDataReaderValueBlockExpression(arrType, parm2Vals), arrType)),
						Expression.Return(returnTarget, Expression.Call(Expression.Convert(parm1DbSet, dbsetType), dbsetTypeDelete, var1Vals)),
						Expression.Label(returnTarget, Expression.Default(typeof(int)))
					), new[] { parm1DbSet, parm2Vals }).Compile();
				});
				_affrows += deleteFunc(oldinfo.dbSet, states.ToArray());
				states.Clear();
			};
			Action<bool> funcUpdate = isLiveUpdate => {
				var updateFunc = _dicExecCommandUpdate.GetOrAdd(oldinfo.entityType, t => {
					var arrType = t.MakeArrayType();
					var dbsetType = typeof(DbSet<>).MakeGenericType(t);
					var dbsetTypeUpdate = dbsetType.GetMethod("DbContextBetchUpdate", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { arrType, typeof(bool) }, null);

					var returnTarget = Expression.Label(typeof(int));
					var parm1DbSet = Expression.Parameter(typeof(object));
					var parm2Vals = Expression.Parameter(typeof(object[]));
					var parm3IsLiveUpdate = Expression.Parameter(typeof(bool));
					var var1Vals = Expression.Variable(arrType);
					return Expression.Lambda<Func<object, object[], bool, int>>(Expression.Block(
						new[] { var1Vals },
						Expression.Assign(var1Vals, Expression.Convert(FreeSql.Internal.Utils.GetDataReaderValueBlockExpression(arrType, parm2Vals), arrType)),
						Expression.Return(returnTarget, Expression.Call(Expression.Convert(parm1DbSet, dbsetType), dbsetTypeUpdate, var1Vals, parm3IsLiveUpdate)),
						Expression.Label(returnTarget, Expression.Default(typeof(int)))
					), new[] { parm1DbSet, parm2Vals, parm3IsLiveUpdate }).Compile();
				});
				var affrows = updateFunc(oldinfo.dbSet, states.ToArray(), isLiveUpdate);
				if (affrows > 0) {
					_affrows += affrows;
					var islastNotUpdated = states.Count != affrows;
					states.Clear();
					if (islastNotUpdated) states.Add(oldinfo.state);
				}
			};

			while(_actions.Any() || states.Any()) {
				var info = _actions.Any() ? _actions.Dequeue() : null;
				if (oldinfo == null) oldinfo = info;
				var isLiveUpdate = false;

				if (_actions.Any() == false && states.Any() ||
					info != null && oldinfo.actionType != info.actionType ||
					info != null && oldinfo.entityType != info.entityType) {

					if (info != null && oldinfo.actionType == info.actionType && oldinfo.entityType == info.entityType) {
						//最后一个，合起来发送
						states.Add(info.state);
						info = null;
					}

					switch (oldinfo.actionType) {
						case ExecCommandInfoType.Insert:
							funcInsert();
							break;
						case ExecCommandInfoType.Delete:
							funcDelete();
							break;
					}
					isLiveUpdate = true;
				}

				if (isLiveUpdate || oldinfo.actionType == ExecCommandInfoType.Update) {
					if (states.Any())
						funcUpdate(isLiveUpdate);
				}

				if (info != null) {
					states.Add(info.state);
					oldinfo = info;
				}
			}
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
			this.Rollback();
		}
	}
}
