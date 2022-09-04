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
                var ret = await InsertWithinBoundaryStaticAsync(_boundaryName, _repository, GetChildRepository, entitys, out var affrows, cancellationToken);
                Attach(ret);
                return ret;
            }
            finally
            {
                DisposeChildRepositorys();
                _repository.FlushState();
            }
        }
        Task<List<T1>> InsertWithinBoundaryStaticAsync<T1>(string boundaryName, IBaseRepository<T1> rootRepository, Func<Type, IBaseRepository<object>> getChildRepository, IEnumerable<T1> rootEntitys, out int affrows, CancellationToken cancellationToken) where T1 : class
        {
            return Task.FromResult(InsertWithinBoundaryStatic(boundaryName, rootRepository, getChildRepository, rootEntitys, out affrows));
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
                AggregateRootUtils.CompareEntityValue(_boundaryName, Orm, EntityType, state.Value, entity, null, tracking);
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
                await InsertWithinBoundaryStaticAsync(_boundaryName, repo, GetChildRepository, il.Value, out var affrowsOut, cancellationToken);
                affrows += affrowsOut;
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
            var updateLogDict2 = updateLogDict.ToDictionary(a => a.Key, a => a.Value.ToDictionary(b => b.UpdateColumnsString, b => a.Value.Where(c => c.UpdateColumnsString == b.UpdateColumnsString).ToArray()));
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