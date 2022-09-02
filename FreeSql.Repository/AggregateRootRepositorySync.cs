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
        public TEntity Insert(TEntity entity) => InsertCascade(new[] { entity }).FirstOrDefault();
        public List<TEntity> Insert(IEnumerable<TEntity> entitys) => InsertCascade(entitys);
        public TEntity InsertOrUpdate(TEntity entity) => InsertOrUpdateCascade(entity);
        public int Update(TEntity entity) => UpdateCascade(new[] { entity });
        public int Update(IEnumerable<TEntity> entitys) => UpdateCascade(entitys);
        public int Delete(TEntity entity) => DeleteCascade(new[] { entity });
        public int Delete(IEnumerable<TEntity> entitys) => DeleteCascade(entitys);
        public int Delete(Expression<Func<TEntity, bool>> predicate) => DeleteCascade(Where(predicate).ToList());
        public List<object> DeleteCascadeByDatabase(Expression<Func<TEntity, bool>> predicate)
        {
            var deletedOutput = new List<object>();
            DeleteCascade(Where(predicate).ToList(), deletedOutput);
            return deletedOutput;
        }
        public void SaveMany(TEntity entity, string propertyName) => SaveManyCascade(entity, propertyName);

        protected virtual List<TEntity> InsertCascade(IEnumerable<TEntity> entitys)
        {
            var repos = new Dictionary<Type, object>();
            try
            {
                return InsertCascadeStatic(_repository, GetChildRepository, entitys);
            }
            finally
            {
                DisposeChildRepositorys();
                _repository.FlushState();
            }
        }
        protected static List<TEntity> InsertCascadeStatic(IBaseRepository<TEntity> rootRepository, Func<Type, IBaseRepository<object>> getChildRepository, IEnumerable<TEntity> rootEntitys) {
            Dictionary<Type, Dictionary<string, bool>> ignores = new Dictionary<Type, Dictionary<string, bool>>();
            Dictionary<Type, IBaseRepository<object>> repos = new Dictionary<Type, IBaseRepository<object>>();
            return LocalInsertCascade(rootRepository, rootEntitys);

            bool LocalCanCascade(Type entityType, object entity, bool isadd)
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
            List<T1> LocalInsertCascade<T1>(IBaseRepository<T1> repository, IEnumerable<T1> entitys) where T1 : class
            {
                var ret = repository.Insert(entitys);
                foreach (var entity in entitys) LocalCanCascade(repository.EntityType, entity, true);

                var table = repository.Orm.CodeFirst.GetTableByEntity(repository.EntityType);
                foreach (var prop in table.Properties.Values)
                {
                    var tbref = table.GetTableRef(prop.Name, false);
                    if (tbref == null) continue;
                    switch (tbref.RefType)
                    {
                        case TableRefType.OneToOne:
                            var otoList = ret.Select(entity =>
                            {
                                var otoItem = table.GetPropertyValue(entity, prop.Name);
                                if (LocalCanCascade(tbref.RefEntityType, otoItem, false) == false) return null;
                                AggregateRootUtils.SetNavigateRelationshipValue(repository.Orm, tbref, table.Type, entity, otoItem);
                                return otoItem;
                            }).Where(entity => entity != null).ToArray();
                            if (otoList.Any())
                            {
                                var repo = getChildRepository(tbref.RefEntityType);
                                LocalInsertCascade(repo, otoList);
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
                                    if (LocalCanCascade(tbref.RefEntityType, otmItem, false) == false) continue;
                                    otmItems.Add(otmItem);
                                }
                                AggregateRootUtils.SetNavigateRelationshipValue(repository.Orm, tbref, table.Type, entity, otmItems);
                                return otmItems;
                            }).Where(entity => entity != null).SelectMany(entity => entity).ToArray();
                            if (otmList.Any())
                            {
                                var repo = getChildRepository(tbref.RefEntityType);
                                LocalInsertCascade(repo, otmList);
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
                                LocalInsertCascade(repo, mtmMidList);
                            }
                            break;
                        case TableRefType.PgArrayToMany:
                            break;
                    }
                }
                return ret;
            }
        }
        
        protected virtual TEntity InsertOrUpdateCascade(TEntity entity)
        {
            var stateKey = Orm.GetEntityKeyString(EntityType, entity, false);
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (_table.Primarys.Any() == false) throw new Exception(DbContextStrings.CannotAdd_EntityHasNo_PrimaryKey(Orm.GetEntityString(EntityType, entity)));

            var flagExists = ExistsInStates(entity);
            if (flagExists == false)
            {
                var olddata = Select.WhereDynamic(entity).First();
                flagExists = olddata != null;
            }
            if (flagExists == true)
            {
                var affrows = UpdateCascade(new[] { entity });
                if (affrows > 0) return entity;
            }
            Orm.ClearEntityPrimaryValueWithIdentity(EntityType, entity);
            return InsertCascade(new[] { entity }).FirstOrDefault();
        }
        protected virtual int UpdateCascade(IEnumerable<TEntity> entitys)
        {
            List<NativeTuple<Type, object>> insertLog = new List<NativeTuple<Type, object>>();
            List<NativeTuple<Type, object, object, List<string>>> updateLog = new List<NativeTuple<Type, object, object, List<string>>>();
            List<NativeTuple<Type, object>> deleteLog = new List<NativeTuple<Type, object>>();
            foreach(var entity in entitys)
            {
                var stateKey = Orm.GetEntityKeyString(EntityType, entity, false);
                if (_states.TryGetValue(stateKey, out var state) == false) throw new Exception($"AggregateRootRepository 使用仓储对象查询后，才可以更新数据 {Orm.GetEntityString(EntityType, entity)}");
                AggregateRootUtils.CompareEntityValueCascade(Orm, EntityType, state.Value, entity, null, insertLog, updateLog, deleteLog);
                AttachCascade(entity);
            }
            return insertLog.Count + updateLog.Count + deleteLog.Count;
        }
        protected virtual int DeleteCascade(IEnumerable<TEntity> entitys, List<object> deletedOutput = null)
        {
            List<NativeTuple<Type, object>> insertLog = new List<NativeTuple<Type, object>>();
            List<NativeTuple<Type, object, object, List<string>>> updateLog = new List<NativeTuple<Type, object, object, List<string>>>();
            List<NativeTuple<Type, object>> deleteLog = new List<NativeTuple<Type, object>>();
            foreach (var entity in entitys)
            {
                var stateKey = Orm.GetEntityKeyString(EntityType, entity, false);
                AggregateRootUtils.CompareEntityValueCascade(Orm, EntityType, entity, null, null, insertLog, updateLog, deleteLog);
                _states.Remove(stateKey);
            }
            if (deletedOutput != null) deletedOutput.AddRange(deleteLog.Select(a => a.Item2));
            return deleteLog.Count;
        }

        protected virtual void SaveManyCascade(TEntity entity, string propertyName)
        {
            List<NativeTuple<Type, object>> insertLog = new List<NativeTuple<Type, object>>();
            List<NativeTuple<Type, object, object, List<string>>> updateLog = new List<NativeTuple<Type, object, object, List<string>>>();
            List<NativeTuple<Type, object>> deleteLog = new List<NativeTuple<Type, object>>();
            var stateKey = Orm.GetEntityKeyString(EntityType, entity, false);
            if (_states.TryGetValue(stateKey, out var state) == false) throw new Exception($"AggregateRootRepository 使用仓储对象查询后，才可以保存数据 {Orm.GetEntityString(EntityType, entity)}");
            AggregateRootUtils.CompareEntityValueCascade(Orm, EntityType, state.Value, entity, propertyName, insertLog, updateLog, deleteLog);
            AttachCascade(entity);
        }

        protected List<TEntity> _dataEditing;
        protected ConcurrentDictionary<string, EntityState> _statesEditing = new ConcurrentDictionary<string, EntityState>();
        public void BeginEdit(List<TEntity> data)
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
                    AggregateRootUtils.MapEntityValueCascade(Orm, EntityType, item, ov.Value);
                    ov.Time = DateTime.Now;
                    return ov;
                });
            }
        }
        public int EndEdit(List<TEntity> data = null)
        {
            if (data == null) data = _dataEditing;
            if (data == null) return 0;
            List<NativeTuple<Type, object>> insertLog = new List<NativeTuple<Type, object>>();
            List<NativeTuple<Type, object, object, List<string>>> updateLog = new List<NativeTuple<Type, object, object, List<string>>>();
            List<NativeTuple<Type, object>> deleteLog = new List<NativeTuple<Type, object>>();
            try
            {
                var addList = new List<TEntity>();
                var ediList = new List<TEntity>();
                foreach (var item in data)
                {
                    var key = Orm.GetEntityKeyString(EntityType, item, false);
                    if (_statesEditing.TryRemove(key, out var state) == false)
                    {
                        insertLog.Add(NativeTuple.Create(EntityType, (object)item));
                        continue;
                    }
                    _states[key] = state;
                    AggregateRootUtils.CompareEntityValueCascade(Orm, EntityType, state.Value, item, null, insertLog, updateLog, deleteLog);
                }
                foreach (var item in _statesEditing.Values.OrderBy(a => a.Time))
                {
                    AggregateRootUtils.CompareEntityValueCascade(Orm, EntityType, item, null, null, insertLog, updateLog, deleteLog);
                }
            }
            finally
            {
                _dataEditing = null;
                _statesEditing.Clear();
            }
            return insertLog.Count + updateLog.Count + deleteLog.Count;
        }

    }
}
