using FreeSql.Extensions.EntityUtil;
using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql
{
    partial class AggregateRootRepository<TEntity>
    {

        #region BeginEdit/EndEdit
        List<TEntity> _dataEditing;
        ConcurrentDictionary<string, EntityState> _statesEditing = new ConcurrentDictionary<string, EntityState>();
        public virtual void BeginEdit(List<TEntity> data)
        {
            if (data == null) return;
            var table = Orm.CodeFirst.GetTableByEntity(EntityType);
            if (table.Primarys.Any() == false) throw new Exception(DbContextStrings.CannotEdit_EntityHasNo_PrimaryKey(Orm.GetEntityString(EntityType, data.First())));
            _statesEditing.Clear();
            _dataEditing = data;
            foreach (var item in data)
            {
                var key = Orm.GetEntityKeyString(EntityType, item, false);
                if (string.IsNullOrEmpty(key)) continue;

                _statesEditing.AddOrUpdate(key, k => CreateEntityState(item), (k, ov) =>
                {
                    AggregateRootUtils.MapEntityValue(_boundaryName, Orm, EntityType, item, ov.Value);
                    ov.Time = DateTime.Now;
                    return ov;
                });
            }
        }
        public virtual int EndEdit(List<TEntity> data = null)
        {
            if (data == null) data = _dataEditing;
            if (data == null) return 0;
            var tracking = new AggregateRootTrackingChangeInfo();
            try
            {
                var addList = new List<TEntity>();
                var ediList = new List<TEntity>();
                foreach (var item in data)
                {
                    var key = Orm.GetEntityKeyString(EntityType, item, false);
                    if (_statesEditing.TryRemove(key, out var state) == false)
                    {
                        tracking.InsertLog.Add(NativeTuple.Create(EntityType, (object)item));
                        continue;
                    }
                    _states[key] = state;
                    AggregateRootUtils.CompareEntityValue(_boundaryName, Orm, EntityType, state.Value, item, null, tracking);
                }
                foreach (var item in _statesEditing.Values.OrderBy(a => a.Time))
                    AggregateRootUtils.CompareEntityValue(_boundaryName, Orm, EntityType, item, null, null, tracking);

                return SaveTrackingChange(tracking);
            }
            finally
            {
                _dataEditing = null;
                _statesEditing.Clear();
            }
        }
        #endregion

        #region Insert
        public virtual TEntity Insert(TEntity entity) => Insert(new[] { entity }).FirstOrDefault();
        public virtual List<TEntity> Insert(IEnumerable<TEntity> entitys)
        {
            var repos = new Dictionary<Type, object>();
            try
            {
                var ret = InsertWithinBoundaryStatic(_boundaryName, _repository, GetChildRepository, entitys, out var affrows);
                Attach(ret);
                return ret;
            }
            finally
            {
                DisposeChildRepositorys();
                _repository.FlushState();
            }
        }
        static List<T1> InsertWithinBoundaryStatic<T1>(string boundaryName, IBaseRepository<T1> rootRepository, Func<Type, IBaseRepository<object>> getChildRepository, IEnumerable<T1> rootEntitys, out int affrows) where T1 : class {
            Dictionary<Type, Dictionary<string, bool>> ignores = new Dictionary<Type, Dictionary<string, bool>>();
            Dictionary<Type, IBaseRepository<object>> repos = new Dictionary<Type, IBaseRepository<object>>();
            var localAffrows = 0;
            try
            {
                return LocalInsert(rootRepository, rootEntitys, true);
            }
            finally
            {
                affrows = localAffrows;
            }

            bool LocalCanInsert(Type entityType, object entity, bool isadd)
            {
                var stateKey = rootRepository.Orm.GetEntityKeyString(entityType, entity, false);
                if (stateKey == null) return true;
                if (ignores.TryGetValue(entityType, out var stateKeys) == false)
                {
                    if (isadd)
                    {
                        ignores.Add(entityType, stateKeys = new Dictionary<string, bool>());
                        stateKeys.Add(stateKey, true);
                    }
                    return true;
                }
                if (stateKeys.ContainsKey(stateKey) == false)
                {
                    if (isadd) stateKeys.Add(stateKey, true);
                    return true;
                }
                return false;
            }
            List<T2> LocalInsert<T2>(IBaseRepository<T2> repository, IEnumerable<T2> entitys, bool cascade) where T2 : class
            {
                var table = repository.Orm.CodeFirst.GetTableByEntity(repository.EntityType);
                if (table.Primarys.Any(col => col.Attribute.IsIdentity))
                {
                    foreach (var entity in entitys) 
                        repository.Orm.ClearEntityPrimaryValueWithIdentity(repository.EntityType, entity);
                }
                var ret = repository.Insert(entitys);
                localAffrows += ret.Count;
                foreach (var entity in entitys) LocalCanInsert(repository.EntityType, entity, true);
                if (cascade == false) return ret;

                foreach (var tr in table.GetAllTableRef().OrderBy(a => a.Value.RefType).ThenBy(a => a.Key))
                {
                    var tbref = tr.Value;
                    if (tbref.Exception != null) continue;
                    if (table.Properties.TryGetValue(tr.Key, out var prop) == false) continue;
                    var boundaryAttr = AggregateRootUtils.GetPropertyBoundaryAttribute(prop, boundaryName);
                    if (boundaryAttr?.Break == true) continue;
                    switch (tbref.RefType)
                    {
                        case TableRefType.OneToOne:
                            var otoList = ret.Select(entity =>
                            {
                                var otoItem = table.GetPropertyValue(entity, prop.Name);
                                if (LocalCanInsert(tbref.RefEntityType, otoItem, false) == false) return null;
                                AggregateRootUtils.SetNavigateRelationshipValue(repository.Orm, tbref, table.Type, entity, otoItem);
                                return otoItem;
                            }).Where(entity => entity != null).ToArray();
                            if (otoList.Any())
                            {
                                var repo = getChildRepository(tbref.RefEntityType);
                                LocalInsert(repo, otoList, boundaryAttr?.BreakThen != true);
                            }
                            break;
                        case TableRefType.OneToMany:
                            var otmList = ret.Select(entity =>
                            {
                                var otmEach = table.GetPropertyValue(entity, prop.Name) as IEnumerable;
                                if (otmEach == null) return null;
                                var otmItems = new List<object>();
                                foreach (var otmItem in otmEach)
                                {
                                    if (LocalCanInsert(tbref.RefEntityType, otmItem, false) == false) continue;
                                    otmItems.Add(otmItem);
                                }
                                AggregateRootUtils.SetNavigateRelationshipValue(repository.Orm, tbref, table.Type, entity, otmItems);
                                return otmItems;
                            }).Where(entity => entity != null).SelectMany(entity => entity).ToArray();
                            if (otmList.Any())
                            {
                                var repo = getChildRepository(tbref.RefEntityType);
                                LocalInsert(repo, otmList, boundaryAttr?.BreakThen != true);
                            }
                            break;
                        case TableRefType.ManyToMany:
                            var mtmMidList = new List<object>();
                            ret.ForEach(entity =>
                            {
                                var mids = AggregateRootUtils.GetManyToManyObjects(repository.Orm, table, tbref, entity, prop);
                                if (mids != null) mtmMidList.AddRange(mids);
                            });
                            if (mtmMidList.Any())
                            {
                                var repo = getChildRepository(tbref.RefMiddleEntityType);
                                LocalInsert(repo, mtmMidList, false);
                            }
                            break;
                        case TableRefType.PgArrayToMany:
                        case TableRefType.ManyToOne: //ManyToOne、ManyToMany外部表、PgArrayToMany 不属于聚合根成员，可以查询，不能增删改
                            break;
                    }
                }
                return ret;
            }
        }
        #endregion

        public virtual TEntity InsertOrUpdate(TEntity entity)
        {
            var stateKey = Orm.GetEntityKeyString(EntityType, entity, false);
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var table = Orm.CodeFirst.GetTableByEntity(EntityType);
            if (table.Primarys.Any() == false) throw new Exception(DbContextStrings.CannotAdd_EntityHasNo_PrimaryKey(Orm.GetEntityString(EntityType, entity)));

            var flagExists = ExistsInStates(entity);
            if (flagExists == false)
            {
                var olddata = Select.WhereDynamic(entity).First();
                flagExists = olddata != null;
            }
            if (flagExists == true)
            {
                var affrows = Update(entity);
                if (affrows > 0) return entity;
            }
            if (table.Primarys.Where(a => a.Attribute.IsIdentity).Count() == table.Primarys.Length)
            {
                Orm.ClearEntityPrimaryValueWithIdentity(EntityType, entity);
                return Insert(entity);
            }
            throw new Exception(DbContextStrings.CannotAdd_PrimaryKey_NotSet(Orm.GetEntityString(EntityType, entity)));
        }

        public virtual int Update(TEntity entity) => Update(new[] { entity });
        public virtual int Update(IEnumerable<TEntity> entitys)
        {
            var tracking = new AggregateRootTrackingChangeInfo();
            foreach(var entity in entitys)
            {
                var stateKey = Orm.GetEntityKeyString(EntityType, entity, false);
                if (_states.TryGetValue(stateKey, out var state) == false) throw new Exception($"AggregateRootRepository 使用仓储对象查询后，才可以更新数据 {Orm.GetEntityString(EntityType, entity)}");
                AggregateRootUtils.CompareEntityValue(_boundaryName, Orm, EntityType, state.Value, entity, null, tracking);
            }
            foreach (var entity in entitys)
                Attach(entity);

            return SaveTrackingChange(tracking);
        }

        public virtual int Delete(TEntity entity) => DeleteWithinBoundary(new[] { entity }, null);
        public virtual int Delete(IEnumerable<TEntity> entitys) => DeleteWithinBoundary(entitys, null);
        public virtual int Delete(Expression<Func<TEntity, bool>> predicate) => DeleteWithinBoundary(SelectAggregateRoot.Where(predicate).ToList(), null);
        public virtual List<object> DeleteCascadeByDatabase(Expression<Func<TEntity, bool>> predicate)
        {
            var deletedOutput = new List<object>();
            DeleteWithinBoundary(SelectAggregateRoot.Where(predicate).ToList(), deletedOutput);
            return deletedOutput;
        }
        int DeleteWithinBoundary(IEnumerable<TEntity> entitys, List<object> deletedOutput)
        {
            var tracking = new AggregateRootTrackingChangeInfo();
            foreach (var entity in entitys)
            {
                var stateKey = Orm.GetEntityKeyString(EntityType, entity, false);
                AggregateRootUtils.CompareEntityValue(_boundaryName, Orm, EntityType, entity, null, null, tracking);
                _states.Remove(stateKey);
            }
            var affrows = 0;
            for (var a = tracking.DeleteLog.Count - 1; a >= 0; a--)
            {
                affrows += Orm.Delete<object>().AsType(tracking.DeleteLog[a].Item1).AsTable(_asTableRule)
                    .WhereDynamic(tracking.DeleteLog[a].Item2).ExecuteAffrows();
                if (deletedOutput != null) deletedOutput.AddRange(tracking.DeleteLog[a].Item2);
                UnitOfWork?.EntityChangeReport?.Report.AddRange(tracking.DeleteLog[a].Item2.Select(x =>
                    new DbContext.EntityChangeReport.ChangeInfo
                    {
                        Type = DbContext.EntityChangeType.Delete,
                        EntityType = tracking.DeleteLog[a].Item1,
                        Object = x
                    }));
            }
            return affrows;
        }

        public virtual void SaveMany(TEntity entity, string propertyName)
        {
            var tracking = new AggregateRootTrackingChangeInfo();
            var stateKey = Orm.GetEntityKeyString(EntityType, entity, false);
            if (_states.TryGetValue(stateKey, out var state) == false) throw new Exception($"AggregateRootRepository 使用仓储对象查询后，才可以保存数据 {Orm.GetEntityString(EntityType, entity)}");
            AggregateRootUtils.CompareEntityValue(_boundaryName, Orm, EntityType, state.Value, entity, propertyName, tracking);
            Attach(entity); //应该只存储 propertyName 内容
            SaveTrackingChange(tracking);
        }


        int SaveTrackingChange(AggregateRootTrackingChangeInfo tracking)
        {
            var affrows = 0;
            DisposeChildRepositorys();
            var insertLogDict = tracking.InsertLog.GroupBy(a => a.Item1).ToDictionary(a => a.Key, a => tracking.InsertLog.Where(b => b.Item1 == a.Key).Select(b => b.Item2).ToArray());
            foreach (var il in insertLogDict)
            {
                var repo = GetChildRepository(il.Key);
                InsertWithinBoundaryStatic(_boundaryName, repo, GetChildRepository, il.Value, out var affrowsOut);
                affrows += affrowsOut;
            }

            for (var a = tracking.DeleteLog.Count - 1; a >= 0; a--)
            {
                affrows += Orm.Delete<object>().AsType(tracking.DeleteLog[a].Item1).AsTable(_asTableRule)
                    .WhereDynamic(tracking.DeleteLog[a].Item2).ExecuteAffrows();
                UnitOfWork?.EntityChangeReport?.Report.AddRange(tracking.DeleteLog[a].Item2.Select(x =>
                    new DbContext.EntityChangeReport.ChangeInfo
                    {
                        Type = DbContext.EntityChangeType.Delete,
                        EntityType = tracking.DeleteLog[a].Item1,
                        Object = x
                    }));
            }

            var updateLogDict = tracking.UpdateLog.GroupBy(a => a.Item1).ToDictionary(a => a.Key, a => tracking.UpdateLog.Where(b => b.Item1 == a.Key).Select(b => new
            {
                BeforeObject = b.Item2,
                AfterObject = b.Item3,
                UpdateColumns = b.Item4,
                UpdateColumnsString = string.Join(",", b.Item4.OrderBy(c => c))
            }).ToArray());
            var updateLogDict2 = updateLogDict.ToDictionary(a => a.Key, a => a.Value.ToDictionary(b => b.UpdateColumnsString, b => a.Value.Where(c => c.UpdateColumnsString == b.UpdateColumnsString).ToArray()));
            foreach (var dl in updateLogDict2)
            {
                foreach (var dl2 in dl.Value)
                {
                    affrows += Orm.Update<object>().AsType(dl.Key).AsTable(_asTableRule)
                        .SetSource(dl2.Value.Select(a => a.AfterObject).ToArray())
                        .UpdateColumns(dl2.Value.First().UpdateColumns.ToArray())
                        .ExecuteAffrows();
                    UnitOfWork?.EntityChangeReport?.Report.AddRange(dl2.Value.Select(x =>
                        new DbContext.EntityChangeReport.ChangeInfo
                        {
                            Type = DbContext.EntityChangeType.Update,
                            EntityType = dl.Key,
                            Object = x.AfterObject,
                            BeforeObject = x.BeforeObject
                        }));
                }
            }
            DisposeChildRepositorys();
            return affrows;
        }
    }
}
