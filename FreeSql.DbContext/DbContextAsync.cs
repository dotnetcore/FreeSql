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

		static Dictionary<Type, Dictionary<string, Func<object, object[], Task<int>>>> _dicExecCommandAsyncDbContextBetch = new Dictionary<Type, Dictionary<string, Func<object, object[], Task<int>>>>();
		async internal Task ExecCommandAsync() {
			ExecCommandInfo oldinfo = null;
			var states = new List<object>();

			Func<string, Task<int>> dbContextBetch = methodName => {
				if (_dicExecCommandAsyncDbContextBetch.TryGetValue(oldinfo.stateType, out var trydic) == false)
					trydic = new Dictionary<string, Func<object, object[], Task<int>>>();
				if (trydic.TryGetValue(methodName, out var tryfunc) == false) {
					var arrType = oldinfo.stateType.MakeArrayType();
					var dbsetType = oldinfo.dbSet.GetType().BaseType;
					var dbsetTypeMethod = dbsetType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { arrType }, null);

					var returnTarget = Expression.Label(typeof(Task<int>));
					var parm1DbSet = Expression.Parameter(typeof(object));
					var parm2Vals = Expression.Parameter(typeof(object[]));
					var var1Vals = Expression.Variable(arrType);
					tryfunc = Expression.Lambda<Func<object, object[], Task<int>>>(Expression.Block(
						new[] { var1Vals },
						Expression.Assign(var1Vals, Expression.Convert(FreeSql.Internal.Utils.GetDataReaderValueBlockExpression(arrType, parm2Vals), arrType)),
						Expression.Return(returnTarget, Expression.Call(Expression.Convert(parm1DbSet, dbsetType), dbsetTypeMethod, var1Vals)),
						Expression.Label(returnTarget, Expression.Default(typeof(Task<int>)))
					), new[] { parm1DbSet, parm2Vals }).Compile();
					trydic.Add(methodName, tryfunc);
				}
				return tryfunc(oldinfo.dbSet, states.ToArray());
			};
			Func<Task> funcDelete = async () => {
				_affrows += await dbContextBetch("DbContextBetchRemoveAsync");
				states.Clear();
			};
			Func<Task> funcInsert = async () => {
				_affrows += await dbContextBetch("DbContextBetchAddAsync");
				states.Clear();
			};
			Func<bool, Task> funcUpdate = async isLiveUpdate => {
				var affrows = 0;
				if (isLiveUpdate) affrows = await dbContextBetch("DbContextBetchUpdateNowAsync");
				else affrows = await dbContextBetch("DbContextBetchUpdateAsync");
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
