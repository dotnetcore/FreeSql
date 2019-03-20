using SafeObjectPool;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FreeSql {
	partial class DbContext {

		async public Task<long> SaveChangesAsync() {
			await ExecCommandAsync();
			Commit();
			return _affrows;
		}

		static ConcurrentDictionary<Type, Func<object, object[], Task<int>>> _dicExecCommandAsyncInsert = new ConcurrentDictionary<Type, Func<object, object[], Task<int>>>();
		static ConcurrentDictionary<Type, Func<object, object[], Task<int>>> _dicExecCommandAsyncDelete = new ConcurrentDictionary<Type, Func<object, object[], Task<int>>>();
		static ConcurrentDictionary<Type, Func<object, object[], bool, Task<int>>> _dicExecCommandAsyncUpdate = new ConcurrentDictionary<Type, Func<object, object[], bool, Task<int>>>();
		async internal Task ExecCommandAsync() {
			ExecCommandInfo oldinfo = null;
			var states = new List<object>();

			Func<Task> funcInsert = async () => {
				var insertFunc = _dicExecCommandAsyncInsert.GetOrAdd(oldinfo.entityType, t => {
					var arrType = t.MakeArrayType();
					var dbsetType = typeof(DbSet<>).MakeGenericType(t);
					var dbsetTypeInsert = dbsetType.GetMethod("OrmInsert", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { arrType }, null);
					var insertBuilder = typeof(IInsert<>).MakeGenericType(t);
					var insertExecuteAffrows = insertBuilder.GetMethod("ExecuteAffrowsAsync", new Type[0]);

					var returnTarget = Expression.Label(typeof(Task<int>));
					var parm1DbSet = Expression.Parameter(typeof(object));
					var parm2Vals = Expression.Parameter(typeof(object[]));
					var var1Vals = Expression.Variable(arrType);
					return Expression.Lambda<Func<object, object[], Task<int>>>(Expression.Block(
						new[] { var1Vals },
						Expression.Assign(var1Vals, Expression.Convert(FreeSql.Internal.Utils.GetDataReaderValueBlockExpression(arrType, parm2Vals), arrType)),
						Expression.Return(returnTarget,
							Expression.Call(
								Expression.Call(Expression.Convert(parm1DbSet, dbsetType), dbsetTypeInsert, var1Vals),
								insertExecuteAffrows
							)
						),
						Expression.Label(returnTarget, Expression.Default(typeof(Task<int>)))
					), new[] { parm1DbSet, parm2Vals }).Compile();
				});
				_affrows += await insertFunc(oldinfo.dbSet, states.ToArray());
				states.Clear();
			};
			Func<Task> funcDelete = async () => {
				var deleteFunc = _dicExecCommandAsyncDelete.GetOrAdd(oldinfo.entityType, t => {
					var arrType = t.MakeArrayType();
					var dbsetType = typeof(DbSet<>).MakeGenericType(t);
					var dbsetTypeDelete = dbsetType.GetMethod("DbContextBetchRemoveAsync", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { arrType }, null);

					var returnTarget = Expression.Label(typeof(Task<int>));
					var parm1DbSet = Expression.Parameter(typeof(object));
					var parm2Vals = Expression.Parameter(typeof(object[]));
					var var1Vals = Expression.Variable(arrType);
					return Expression.Lambda<Func<object, object[], Task<int>>>(Expression.Block(
						new[] { var1Vals },
						Expression.Assign(var1Vals, Expression.Convert(FreeSql.Internal.Utils.GetDataReaderValueBlockExpression(arrType, parm2Vals), arrType)),
						Expression.Return(returnTarget, Expression.Call(Expression.Convert(parm1DbSet, dbsetType), dbsetTypeDelete, var1Vals)),
						Expression.Label(returnTarget, Expression.Default(typeof(Task<int>)))
					), new[] { parm1DbSet, parm2Vals }).Compile();
				});
				_affrows += await deleteFunc(oldinfo.dbSet, states.ToArray());
				states.Clear();
			};
			Func<bool, Task> funcUpdate = async (isLiveUpdate) => {
				var updateFunc = _dicExecCommandAsyncUpdate.GetOrAdd(oldinfo.entityType, t => {
					var arrType = t.MakeArrayType();
					var dbsetType = typeof(DbSet<>).MakeGenericType(t);
					var dbsetTypeUpdate = dbsetType.GetMethod("DbContextBetchUpdateAsync", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { arrType, typeof(bool) }, null);

					var returnTarget = Expression.Label(typeof(Task<int>));
					var parm1DbSet = Expression.Parameter(typeof(object));
					var parm2Vals = Expression.Parameter(typeof(object[]));
					var parm3IsLiveUpdate = Expression.Parameter(typeof(bool));
					var var1Vals = Expression.Variable(arrType);
					return Expression.Lambda<Func<object, object[], bool, Task<int>>>(Expression.Block(
						new[] { var1Vals },
						Expression.Assign(var1Vals, Expression.Convert(FreeSql.Internal.Utils.GetDataReaderValueBlockExpression(arrType, parm2Vals), arrType)),
						Expression.Return(returnTarget, Expression.Call(Expression.Convert(parm1DbSet, dbsetType), dbsetTypeUpdate, var1Vals, parm3IsLiveUpdate)),
						Expression.Label(returnTarget, Expression.Default(typeof(Task<int>)))
					), new[] { parm1DbSet, parm2Vals, parm3IsLiveUpdate }).Compile();
				});
				var affrows = await updateFunc(oldinfo.dbSet, states.ToArray(), isLiveUpdate);
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
							await funcInsert();
							break;
						case ExecCommandInfoType.Delete:
							await funcDelete();
							break;
					}
					isLiveUpdate = true;
				}

				if (isLiveUpdate || oldinfo.actionType == ExecCommandInfoType.Update) {
					if (states.Any())
						await funcUpdate(isLiveUpdate);
				}

				if (info != null) {
					states.Add(info.state);
					oldinfo = info;
				}
			}
		}
	}
}
