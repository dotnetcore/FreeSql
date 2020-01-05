using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using System.Threading.Tasks;

#if net40
#else
namespace FreeSql
{
    partial class DbContext
    {
        async public virtual Task<int> SaveChangesAsync()
        {
            await ExecCommandAsync();
            return SaveChangesSuccess();
        }

        static Dictionary<Type, Dictionary<string, Func<object, object[], Task<int>>>> _dicExecCommandDbContextBatchAsync = new Dictionary<Type, Dictionary<string, Func<object, object[], Task<int>>>>();
        async internal Task ExecCommandAsync()
        {
            if (isExecCommanding) return;
            if (_actions.Any() == false) return;
            isExecCommanding = true;

            ExecCommandInfo oldinfo = null;
            var states = new List<object>();

            Func<string, Task<int>> dbContextBatch = methodName =>
            {
                if (_dicExecCommandDbContextBatchAsync.TryGetValue(oldinfo.stateType, out var trydic) == false)
                    trydic = new Dictionary<string, Func<object, object[], Task<int>>>();
                if (trydic.TryGetValue(methodName, out var tryfunc) == false)
                {
                    var arrType = oldinfo.stateType.MakeArrayType();
                    var dbsetType = oldinfo.dbSet.GetType().BaseType;
                    var dbsetTypeMethod = dbsetType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { arrType }, null);

                    var returnTarget = Expression.Label(typeof(Task<int>));
                    var parm1DbSet = Expression.Parameter(typeof(object));
                    var parm2Vals = Expression.Parameter(typeof(object[]));
                    var var1Vals = Expression.Variable(arrType);
                    tryfunc = Expression.Lambda<Func<object, object[], Task<int>>>(Expression.Block(
                        new[] { var1Vals },
                        Expression.Assign(var1Vals, Expression.Convert(global::FreeSql.Internal.Utils.GetDataReaderValueBlockExpression(arrType, parm2Vals), arrType)),
                        Expression.Return(returnTarget, Expression.Call(Expression.Convert(parm1DbSet, dbsetType), dbsetTypeMethod, var1Vals)),
                        Expression.Label(returnTarget, Expression.Default(typeof(Task<int>)))
                    ), new[] { parm1DbSet, parm2Vals }).Compile();
                    trydic.Add(methodName, tryfunc);
                }
                return tryfunc(oldinfo.dbSet, states.ToArray());
            };
            Func<Task> funcDelete = async () =>
            {
                _affrows += await dbContextBatch("DbContextBatchRemoveAsync");
                states.Clear();
            };
            Func<Task> funcInsert = async () =>
            {
                _affrows += await dbContextBatch("DbContextBatchAddAsync");
                states.Clear();
            };
            Func<bool, Task> funcUpdate = async (isLiveUpdate) =>
            {
                var affrows = 0;
                if (isLiveUpdate) affrows = await dbContextBatch("DbContextBatchUpdateNowAsync");
                else affrows = await dbContextBatch("DbContextBatchUpdateAsync");
                if (affrows == -999)
                { //最后一个元素已被删除
                    states.RemoveAt(states.Count - 1);
                    return;
                }
                if (affrows == -998 || affrows == -997)
                { //没有执行更新
                    var laststate = states[states.Count - 1];
                    states.Clear();
                    if (affrows == -997) states.Add(laststate); //保留最后一个
                }
                if (affrows > 0)
                {
                    _affrows += affrows;
                    var islastNotUpdated = states.Count != affrows;
                    var laststate = states[states.Count - 1];
                    states.Clear();
                    if (islastNotUpdated) states.Add(laststate); //保留最后一个
                }
            };

            while (_actions.Any() || states.Any())
            {
                var info = _actions.Any() ? _actions.Dequeue() : null;
                if (oldinfo == null) oldinfo = info;
                var isLiveUpdate = false;

                if (_actions.Any() == false && states.Any() ||
                    info != null && oldinfo.changeType != info.changeType ||
                    info != null && oldinfo.stateType != info.stateType ||
                    info != null && oldinfo.entityType != info.entityType)
                {

                    if (info != null && oldinfo.changeType == info.changeType && oldinfo.stateType == info.stateType && oldinfo.entityType == info.entityType)
                    {
                        //最后一个，合起来发送
                        states.Add(info.state);
                        info = null;
                    }

                    switch (oldinfo.changeType)
                    {
                        case EntityChangeType.Insert:
                            await funcInsert();
                            break;
                        case EntityChangeType.Delete:
                            await funcDelete();
                            break;
                    }
                    isLiveUpdate = true;
                }

                if (isLiveUpdate || oldinfo.changeType == EntityChangeType.Update)
                {
                    if (states.Any())
                        await funcUpdate(isLiveUpdate);
                }

                if (info != null)
                {
                    states.Add(info.state);
                    oldinfo = info;
                }
            }
            isExecCommanding = false;
        }
    }
}
#endif