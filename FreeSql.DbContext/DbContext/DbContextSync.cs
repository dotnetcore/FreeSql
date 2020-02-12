using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;

namespace FreeSql
{
    partial class DbContext
    {
        int SaveChangesSuccess()
        {
            UnitOfWork?.Commit();
            int ret;
            try
            {
                EmitOnEntityChange(_entityChangeReport);
            }
            finally
            {
                _entityChangeReport.Clear();
                ret = _affrows;
                _affrows = 0;
            }
            return ret;
        }
        public virtual int SaveChanges()
        {
            ExecCommand();
            return SaveChangesSuccess();
        }

        static Dictionary<Type, Dictionary<string, Func<object, object[], int>>> _dicExecCommandDbContextBatch = new Dictionary<Type, Dictionary<string, Func<object, object[], int>>>();
        bool isExecCommanding = false;
        internal void ExecCommand()
        {
            if (isExecCommanding) return;
            if (_actions.Any() == false) return;
            isExecCommanding = true;

            ExecCommandInfo oldinfo = null;
            var states = new List<object>();

            Func<string, int> dbContextBatch = methodName =>
            {
                if (_dicExecCommandDbContextBatch.TryGetValue(oldinfo.stateType, out var trydic) == false)
                    trydic = new Dictionary<string, Func<object, object[], int>>();
                if (trydic.TryGetValue(methodName, out var tryfunc) == false)
                {
                    var arrType = oldinfo.stateType.MakeArrayType();
                    var dbsetType = oldinfo.dbSet.GetType().BaseType;
                    var dbsetTypeMethod = dbsetType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { arrType }, null);

                    var returnTarget = Expression.Label(typeof(int));
                    var parm1DbSet = Expression.Parameter(typeof(object));
                    var parm2Vals = Expression.Parameter(typeof(object[]));
                    var var1Vals = Expression.Variable(arrType);
                    tryfunc = Expression.Lambda<Func<object, object[], int>>(Expression.Block(
                        new[] { var1Vals },
                        Expression.Assign(var1Vals, Expression.Convert(global::FreeSql.Internal.Utils.GetDataReaderValueBlockExpression(arrType, parm2Vals), arrType)),
                        Expression.Return(returnTarget, Expression.Call(Expression.Convert(parm1DbSet, dbsetType), dbsetTypeMethod, var1Vals)),
                        Expression.Label(returnTarget, Expression.Default(typeof(int)))
                    ), new[] { parm1DbSet, parm2Vals }).Compile();
                    trydic.Add(methodName, tryfunc);
                }
                return tryfunc(oldinfo.dbSet, states.ToArray());
            };
            Action funcDelete = () =>
            {
                _affrows += dbContextBatch("DbContextBatchRemove");
                states.Clear();
            };
            Action funcInsert = () =>
            {
                _affrows += dbContextBatch("DbContextBatchAdd");
                states.Clear();
            };
            Action<bool> funcUpdate = isLiveUpdate =>
            {
                var affrows = 0;
                if (isLiveUpdate) affrows = dbContextBatch("DbContextBatchUpdateNow");
                else affrows = dbContextBatch("DbContextBatchUpdate");
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
                            funcInsert();
                            break;
                        case EntityChangeType.Delete:
                            funcDelete();
                            break;
                    }
                    isLiveUpdate = true;
                }

                if (isLiveUpdate || oldinfo.changeType == EntityChangeType.Update)
                {
                    if (states.Any())
                        funcUpdate(isLiveUpdate);
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
