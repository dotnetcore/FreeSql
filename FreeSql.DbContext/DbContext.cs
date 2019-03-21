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

		public long SaveChanges() {
			ExecCommand();
			Commit();
			return _affrows;
		}

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

		static Dictionary<Type, Dictionary<string, Func<object, object[], int>>> _dicExecCommandDbContextBetch = new Dictionary<Type, Dictionary<string, Func<object, object[], int>>>();
		internal void ExecCommand() {
			ExecCommandInfo oldinfo = null;
			var states = new List<object>();

			Func<string, int> dbContextBetch = methodName => {
				if (_dicExecCommandDbContextBetch.TryGetValue(oldinfo.stateType, out var trydic) == false)
					trydic = new Dictionary<string, Func<object, object[], int>>();
				if (trydic.TryGetValue(methodName, out var tryfunc) == false) {
					var arrType = oldinfo.stateType.MakeArrayType();
					var dbsetType = oldinfo.dbSet.GetType().BaseType;
					var dbsetTypeMethod = dbsetType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { arrType }, null);

					var returnTarget = Expression.Label(typeof(int));
					var parm1DbSet = Expression.Parameter(typeof(object));
					var parm2Vals = Expression.Parameter(typeof(object[]));
					var var1Vals = Expression.Variable(arrType);
					tryfunc = Expression.Lambda<Func<object, object[], int>>(Expression.Block(
						new[] { var1Vals },
						Expression.Assign(var1Vals, Expression.Convert(FreeSql.Internal.Utils.GetDataReaderValueBlockExpression(arrType, parm2Vals), arrType)),
						Expression.Return(returnTarget, Expression.Call(Expression.Convert(parm1DbSet, dbsetType), dbsetTypeMethod, var1Vals)),
						Expression.Label(returnTarget, Expression.Default(typeof(int)))
					), new[] { parm1DbSet, parm2Vals }).Compile();
					trydic.Add(methodName, tryfunc);
				}
				return tryfunc(oldinfo.dbSet, states.ToArray());
			};
			Action funcDelete = () => {
				_affrows += dbContextBetch("DbContextBetchRemove");
				states.Clear();
			};
			Action funcInsert = () => {
				_affrows += dbContextBetch("DbContextBetchAdd");
				states.Clear();
			};
			Action<bool> funcUpdate = isLiveUpdate => {
				var affrows = 0;
				if (isLiveUpdate) affrows = dbContextBetch("DbContextBetchUpdateNow");
				else affrows = dbContextBetch("DbContextBetchUpdate");
				if (affrows == -999) { //最后一个元素已被删除
					states.RemoveAt(states.Count - 1);
					return;
				}
				if (affrows > 0) {
					_affrows += affrows;
					var islastNotUpdated = states.Count != affrows;
					states.Clear();
					if (islastNotUpdated) states.Add(oldinfo.state);
				}
			};

			while (_actions.Any() || states.Any()) {
				var info = _actions.Any() ? _actions.Dequeue() : null;
				if (oldinfo == null) oldinfo = info;
				var isLiveUpdate = false;

				if (_actions.Any() == false && states.Any() ||
					info != null && oldinfo.actionType != info.actionType ||
					info != null && oldinfo.stateType != info.stateType) {

					if (info != null && oldinfo.actionType == info.actionType && oldinfo.stateType == info.stateType) {
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
			//_fsql.Aop.ToList -= AopToList;
			this.Rollback();
		}
	}
}
