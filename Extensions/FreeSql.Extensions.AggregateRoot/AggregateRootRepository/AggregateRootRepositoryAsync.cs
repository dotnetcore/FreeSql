#if net40
#else
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

        #region InsertAsync
        async public virtual Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default) => (await InsertAsync(new[] { entity }, cancellationToken)).FirstOrDefault();
        async public virtual Task<List<TEntity>> InsertAsync(IEnumerable<TEntity> entitys, CancellationToken cancellationToken = default)
        {
            var repos = new Dictionary<Type, object>();
            try
            {
                var ret = await InsertWithinBoundaryStaticAsync(_boundaryName, _repository, GetChildRepository, entitys, null, cancellationToken);
                Attach(ret);
                return ret;
            }
            finally
            {
                DisposeChildRepositorys();
                _repository.FlushState();
            }
        }
        async Task<List<T1>> InsertWithinBoundaryStaticAsync<T1>(string boundaryName, IBaseRepository<T1> rootRepository, Func<Type, IBaseRepository<object>> getChildRepository, IEnumerable<T1> rootEntitys, int[] affrows, CancellationToken cancellationToken) where T1 : class
        {
            Dictionary<Type, Dictionary<string, bool>> ignores = new Dictionary<Type, Dictionary<string, bool>>();
            Dictionary<Type, IBaseRepository<object>> repos = new Dictionary<Type, IBaseRepository<object>>();
            var localAffrows = 0;
            try
            {
                rootRepository.DbContextOptions.EnableCascadeSave = false;
                return await LocalInsertAsync(rootRepository, rootEntitys, true);
            }
            finally
            {
                if (affrows != null) affrows[0] = localAffrows;
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
            async Task<List<T2>> LocalInsertAsync<T2>(IBaseRepository<T2> repository, IEnumerable<T2> entitys, bool cascade) where T2 : class
            {
                var table = repository.Orm.CodeFirst.GetTableByEntity(repository.EntityType);
                if (table.Primarys.Any(col => col.Attribute.IsIdentity))
                {
                    foreach (var entity in entitys)
                        repository.Orm.ClearEntityPrimaryValueWithIdentity(repository.EntityType, entity);
                }
                var ret = await repository.InsertAsync(entitys, cancellationToken);
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
                                await LocalInsertAsync(repo, otoList, boundaryAttr?.BreakThen != true);
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
                                await LocalInsertAsync(repo, otmList, boundaryAttr?.BreakThen != true);
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
                                await LocalInsertAsync(repo, mtmMidList, false);
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

        async public virtual Task<TEntity> InsertOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var stateKey = Orm.GetEntityKeyString(EntityType, entity, false);
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var table = Orm.CodeFirst.GetTableByEntity(EntityType);
            if (table.Primarys.Any() == false) throw new Exception(DbContextStrings.CannotAdd_EntityHasNo_PrimaryKey(Orm.GetEntityString(EntityType, entity)));

            var flagExists = ExistsInStates(entity);
            if (flagExists == false)
            {
                var olddata = await Select.WhereDynamic(entity).FirstAsync(cancellationToken);
                flagExists = olddata != null;
            }
            if (flagExists == true)
            {
                var affrows = await UpdateAsync(entity, cancellationToken);
                return entity;
            }
            if (table.Primarys.Where(a => a.Attribute.IsIdentity).Count() == table.Primarys.Length)
                Orm.ClearEntityPrimaryValueWithIdentity(EntityType, entity);
            return await InsertAsync(entity, cancellationToken);
        }

        public virtual Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default) => UpdateAsync(new[] { entity }, cancellationToken);
        async public virtual Task<int> UpdateAsync(IEnumerable<TEntity> entitys, CancellationToken cancellationToken = default)
        {
            var tracking = new AggregateRootTrackingChangeInfo();
            foreach (var entity in entitys)
            {
                var stateKey = Orm.GetEntityKeyString(EntityType, entity, false);
                if (_states.TryGetValue(stateKey, out var state) == false) throw new Exception($"AggregateRootRepository 使用仓储对象查询后，才可以更新数据 {Orm.GetEntityString(EntityType, entity)}");
                AggregateRootUtils.CompareEntityValue(_boundaryName, Orm, EntityType, state.Value, entity, null, tracking);
            }
            var affrows = await SaveTrackingChangeAsync(tracking, cancellationToken);
            foreach (var entity in entitys)
                Attach(entity);
            return affrows;
        }


        public virtual Task<int> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default) => DeleteWithinBoundaryAsync(new[] { entity }, null, cancellationToken);
        public virtual Task<int> DeleteAsync(IEnumerable<TEntity> entitys, CancellationToken cancellationToken = default) => DeleteWithinBoundaryAsync(entitys, null, cancellationToken);
        async public virtual Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) => await DeleteWithinBoundaryAsync(await SelectAggregateRoot.Where(predicate).ToListAsync(cancellationToken), null, cancellationToken);
        async public virtual Task<List<object>> DeleteCascadeByDatabaseAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var deletedOutput = new List<object>();
            await DeleteWithinBoundaryAsync(await SelectAggregateRoot.Where(predicate).ToListAsync(cancellationToken), deletedOutput, cancellationToken);
            return deletedOutput;
        }
        async Task<int> DeleteWithinBoundaryAsync(IEnumerable<TEntity> entitys, List<object> deletedOutput, CancellationToken cancellationToken)
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
                affrows += await Orm.Delete<object>().AsType(tracking.DeleteLog[a].Item1).AsTable(_asTableRule)
                    .WhereDynamic(tracking.DeleteLog[a].Item2).ExecuteAffrowsAsync(cancellationToken);
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

        async public virtual Task SaveManyAsync(TEntity entity, string propertyName, CancellationToken cancellationToken = default)
        {
            var tracking = new AggregateRootTrackingChangeInfo();
            var stateKey = Orm.GetEntityKeyString(EntityType, entity, false);
            if (_states.TryGetValue(stateKey, out var state) == false) throw new Exception($"AggregateRootRepository 使用仓储对象查询后，才可以保存数据 {Orm.GetEntityString(EntityType, entity)}");
            AggregateRootUtils.CompareEntityValue(_boundaryName, Orm, EntityType, state.Value, entity, propertyName, tracking);
            await SaveTrackingChangeAsync(tracking, cancellationToken);
            Attach(entity); //应该只存储 propertyName 内容
        }


        async Task<int> SaveTrackingChangeAsync(AggregateRootTrackingChangeInfo tracking, CancellationToken cancellationToken)
        {
            var affrows = 0;
            DisposeChildRepositorys();
            var insertLogDict = tracking.InsertLog.GroupBy(a => a.Item1).ToDictionary(a => a.Key, a => tracking.InsertLog.Where(b => b.Item1 == a.Key).Select(b => b.Item2).ToArray());
            foreach (var il in insertLogDict)
            {
                var repo = GetChildRepository(il.Key);
                var affrowsOut = new int[1];
                await InsertWithinBoundaryStaticAsync(_boundaryName, repo, GetChildRepository, il.Value, affrowsOut, cancellationToken);
                affrows += affrowsOut[0];
            }

            for (var a = tracking.DeleteLog.Count - 1; a >= 0; a--)
            {
                affrows += await Orm.Delete<object>().AsType(tracking.DeleteLog[a].Item1).AsTable(_asTableRule)
                    .WhereDynamic(tracking.DeleteLog[a].Item2).ExecuteAffrowsAsync(cancellationToken);
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
            var updateLogDict2 = updateLogDict.ToDictionary(a => a.Key, a => 
                a.Value.GroupBy(b => b.UpdateColumnsString).ToDictionary(b => b.Key, b => a.Value.Where(c => c.UpdateColumnsString == b.Key).ToArray()));
            foreach (var dl in updateLogDict2)
            {
                foreach (var dl2 in dl.Value)
                {
                    affrows += await Orm.Update<object>().AsType(dl.Key).AsTable(_asTableRule)
                        .SetSource(dl2.Value.Select(a => a.AfterObject).ToArray())
                        .UpdateColumns(dl2.Value.First().UpdateColumns.ToArray())
                        .ExecuteAffrowsAsync(cancellationToken);
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
#endif