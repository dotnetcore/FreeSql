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
                var ret = await InsertWithinBoundaryStaticAsync(_repository, GetChildRepository, entitys, out var affrows, cancellationToken);
                Attach(ret);
                return ret;
            }
            finally
            {
                DisposeChildRepositorys();
                _repository.FlushState();
            }
        }
        Task<List<T1>> InsertWithinBoundaryStaticAsync<T1>(IBaseRepository<T1> rootRepository, Func<Type, IBaseRepository<object>> getChildRepository, IEnumerable<T1> rootEntitys, out int affrows, CancellationToken cancellationToken) where T1 : class
        {
            Dictionary<Type, Dictionary<string, bool>> ignores = new Dictionary<Type, Dictionary<string, bool>>();
            Dictionary<Type, IBaseRepository<object>> repos = new Dictionary<Type, IBaseRepository<object>>();
            var localAffrows = 0;
            try
            {
                return LocalInsertAsync(rootRepository, rootEntitys);
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
            async Task<List<T2>> LocalInsertAsync<T2>(IBaseRepository<T2> repository, IEnumerable<T2> entitys) where T2 : class
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

                foreach (var tr in table.GetAllTableRef().OrderBy(a => a.Value.RefType).ThenBy(a => a.Key))
                {
                    var tbref = tr.Value;
                    if (tbref.Exception != null) continue;
                    if (table.Properties.TryGetValue(tr.Key, out var prop) == false) continue;
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
                                await LocalInsertAsync(repo, otoList);
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
                                await LocalInsertAsync(repo, otmList);
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
                                await LocalInsertAsync(repo, mtmMidList);
                            }
                            break;
                        case TableRefType.PgArrayToMany:
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
                if (affrows > 0) return entity;
            }
            if (table.Primarys.Where(a => a.Attribute.IsIdentity).Count() == table.Primarys.Length)
            {
                Orm.ClearEntityPrimaryValueWithIdentity(EntityType, entity);
                return await InsertAsync(entity, cancellationToken);
            }
            throw new Exception(DbContextStrings.CannotAdd_PrimaryKey_NotSet(Orm.GetEntityString(EntityType, entity)));
        }

        public virtual Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default) => UpdateAsync(new[] { entity }, cancellationToken);
        public virtual Task<int> UpdateAsync(IEnumerable<TEntity> entitys, CancellationToken cancellationToken = default)
        {
            var tracking = new AggregateRootTrackingChangeInfo();
            foreach (var entity in entitys)
            {
                var stateKey = Orm.GetEntityKeyString(EntityType, entity, false);
                if (_states.TryGetValue(stateKey, out var state) == false) throw new Exception($"AggregateRootRepository 使用仓储对象查询后，才可以更新数据 {Orm.GetEntityString(EntityType, entity)}");
                AggregateRootUtils.CompareEntityValue(Orm, EntityType, state.Value, entity, null, tracking);
            }
            foreach (var entity in entitys)
                Attach(entity);

            return SaveTrackingChangeAsync(tracking, cancellationToken);
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
                AggregateRootUtils.CompareEntityValue(Orm, EntityType, entity, null, null, tracking);
                _states.Remove(stateKey);
            }
            var affrows = 0;
            for (var a = tracking.DeleteLog.Count - 1; a >= 0; a--)
            {
                affrows += await Orm.Delete<object>().AsType(tracking.DeleteLog[a].Item1).AsTable(_asTableRule)
                    .WhereDynamic(tracking.DeleteLog[a].Item2).ExecuteAffrowsAsync(cancellationToken);
                if (deletedOutput != null) deletedOutput.AddRange(tracking.DeleteLog[a].Item2);
            }
            return affrows;
        }

        async public virtual Task SaveManyAsync(TEntity entity, string propertyName, CancellationToken cancellationToken = default)
        {
            var tracking = new AggregateRootTrackingChangeInfo();
            var stateKey = Orm.GetEntityKeyString(EntityType, entity, false);
            if (_states.TryGetValue(stateKey, out var state) == false) throw new Exception($"AggregateRootRepository 使用仓储对象查询后，才可以保存数据 {Orm.GetEntityString(EntityType, entity)}");
            AggregateRootUtils.CompareEntityValue(Orm, EntityType, state.Value, entity, propertyName, tracking);
            Attach(entity); //应该只存储 propertyName 内容
            await SaveTrackingChangeAsync(tracking, cancellationToken);
        }


        async Task<int> SaveTrackingChangeAsync(AggregateRootTrackingChangeInfo tracking, CancellationToken cancellationToken)
        {
            var affrows = 0;
            DisposeChildRepositorys();
            var insertLogDict = tracking.InsertLog.GroupBy(a => a.Item1).ToDictionary(a => a.Key, a => tracking.InsertLog.Where(b => b.Item1 == a.Key).Select(b => b.Item2).ToArray());
            foreach (var il in insertLogDict)
            {
                var repo = GetChildRepository(il.Key);
                await InsertWithinBoundaryStaticAsync(repo, GetChildRepository, il.Value, out var affrowsOut, cancellationToken);
                affrows += affrowsOut;
            }

            for (var a = tracking.DeleteLog.Count - 1; a >= 0; a--)
                affrows += await Orm.Delete<object>().AsType(tracking.DeleteLog[a].Item1).AsTable(_asTableRule)
                    .WhereDynamic(tracking.DeleteLog[a].Item2).ExecuteAffrowsAsync(cancellationToken);

            var updateLogDict = tracking.UpdateLog.GroupBy(a => a.Item1).ToDictionary(a => a.Key, a => tracking.UpdateLog.Where(b => b.Item1 == a.Key).Select(b =>
                NativeTuple.Create(b.Item2, b.Item3, string.Join(",", b.Item4.OrderBy(c => c)), b.Item4)).ToArray());
            var updateLogDict2 = updateLogDict.ToDictionary(a => a.Key, a => a.Value.ToDictionary(b => b.Item3, b => a.Value.Where(c => c.Item3 == b.Item3).ToArray()));
            foreach (var dl in updateLogDict2)
            {
                foreach (var dl2 in dl.Value)
                {
                    affrows += await Orm.Update<object>().AsType(dl.Key).AsTable(_asTableRule)
                        .SetSource(dl2.Value.Select(a => a.Item2).ToArray())
                        .UpdateColumns(dl2.Value.First().Item4.ToArray())
                        .ExecuteAffrowsAsync(cancellationToken);
                }
            }
            DisposeChildRepositorys();
            return affrows;
        }
    }
}
#endif