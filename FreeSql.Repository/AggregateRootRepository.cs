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
    public class AggregateRootRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
    {
        readonly IBaseRepository<TEntity> _repositoryPriv;
        readonly Dictionary<Type, IBaseRepository<object>> _repositorys = new Dictionary<Type, IBaseRepository<object>>();
        protected IBaseRepository<TEntity> MainRepository
        {
            get
            {
                _repositoryPriv.DbContextOptions.EnableCascadeSave = false;
                return _repositoryPriv;
            }
        }
        protected IBaseRepository<object> GetOrAddRepository(Type otherEntityType)
        {
            if (_repositorys.TryGetValue(otherEntityType, out var repo) == false)
            {
                repo = Orm.GetRepository<object>();
                repo.AsType(otherEntityType);
                _repositorys.Add(otherEntityType, repo);
            }
            repo.UnitOfWork = UnitOfWork;
            repo.DbContextOptions = DbContextOptions;
            repo.DbContextOptions.EnableCascadeSave = false;
            repo.AsTable(_asTableRule);
            return repo;
        }

        public AggregateRootRepository(IFreeSql fsql)
        {
            _repositoryPriv = fsql.GetRepository<TEntity>();
        }
        public AggregateRootRepository(IFreeSql fsql, UnitOfWorkManager uowManager)
        {
            _repositoryPriv = (uowManager?.Orm ?? fsql).GetRepository<TEntity>();
            uowManager?.Binding(_repositoryPriv);
        }

        protected void DisposeRepositorys()
        {
            foreach (var repo in _repositorys.Values)
            {
                repo.FlushState();
                repo.Dispose();
            }
            _repositorys.Clear();
        }
        public void Dispose()
        {
            foreach (var repo in _repositorys.Values)
            {
                repo.FlushState();
                repo.Dispose();
            }
            _repositorys.Clear();
            _repositoryPriv.FlushState();
            _repositoryPriv.Dispose();
        }

        public IUnitOfWork UnitOfWork
        {
            get => _repositoryPriv.UnitOfWork;
            set => _repositoryPriv.UnitOfWork = value;
        }
        public DbContextOptions DbContextOptions
        {
            get => _repositoryPriv.DbContextOptions;
            set => _repositoryPriv.DbContextOptions = value ?? throw new ArgumentNullException(nameof(DbContextOptions));
        }
        public void AsType(Type entityType) => _repositoryPriv.AsType(entityType);
        Func<string, string> _asTableRule;
        public void AsTable(Func<string, string> rule) => _repositoryPriv.AsTable(_asTableRule = rule);
        public Type EntityType => _repositoryPriv.EntityType;
        public IDataFilter<TEntity> DataFilter => _repositoryPriv.DataFilter;

        public void Attach(TEntity entity) => AttachCascade(entity);
        public void Attach(IEnumerable<TEntity> entity)
        {
            foreach (var item in entity)
                AttachCascade(item);
        }
        public IBaseRepository<TEntity> AttachOnlyPrimary(TEntity data) => _repositoryPriv.AttachOnlyPrimary(data);
        public Dictionary<string, object[]> CompareState(TEntity newdata) => _repositoryPriv.CompareState(newdata);
        public void FlushState() => _repositoryPriv.FlushState();

        public IFreeSql Orm => _repositoryPriv.Orm;
        public IUpdate<TEntity> UpdateDiy => _repositoryPriv.UpdateDiy;
        public ISelect<TEntity> Where(Expression<Func<TEntity, bool>> exp) => Select.Where(exp);
        public ISelect<TEntity> WhereIf(bool condition, Expression<Func<TEntity, bool>> exp) => Select.WhereIf(condition, exp);

        public TEntity Insert(TEntity entity) => Insert(new[] { entity }).FirstOrDefault();
        public List<TEntity> Insert(IEnumerable<TEntity> entitys)
        {
            var ret = InsertCascade(MainRepository, GetOrAddRepository, entitys, new Dictionary<string, object>());
            DisposeRepositorys();
            foreach (var item in ret)
                AttachCascade(item);
            return ret;
        }

        public int Update(TEntity entity) => Update(new[] { entity });
        public int Update(IEnumerable<TEntity> entitys)
        {
            var ret = UpdateCascade(MainRepository, GetOrAddRepository, entitys, new Dictionary<string, object>());
            DisposeRepositorys();
            foreach (var item in entitys)
                AttachCascade(item);
            return ret;
        }

        public int Delete(TEntity entity) => Delete(new[] { entity });

#if net40
#else
        public Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default) => _repositoryPriv.InsertAsync(entity, cancellationToken);
        public Task<List<TEntity>> InsertAsync(IEnumerable<TEntity> entitys, CancellationToken cancellationToken = default) => _repositoryPriv.InsertAsync(entitys, cancellationToken);
        public Task<TEntity> InsertOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default) => _repositoryPriv.InsertOrUpdateAsync(entity, cancellationToken);
        public Task SaveManyAsync(TEntity entity, string propertyName, CancellationToken cancellationToken = default) => _repositoryPriv.SaveManyAsync(entity, propertyName, cancellationToken);

        public Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default) => _repositoryPriv.UpdateAsync(entity, cancellationToken);
        public Task<int> UpdateAsync(IEnumerable<TEntity> entitys, CancellationToken cancellationToken = default) => _repositoryPriv.UpdateAsync(entitys, cancellationToken);

        public Task<int> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default) => _repositoryPriv.DeleteAsync(entity, cancellationToken);
        public Task<int> DeleteAsync(IEnumerable<TEntity> entitys, CancellationToken cancellationToken = default) => _repositoryPriv.DeleteAsync(entitys, cancellationToken);
        public Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) => _repositoryPriv.DeleteAsync(predicate, cancellationToken);
        public Task<List<object>> DeleteCascadeByDatabaseAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) => _repositoryPriv.DeleteCascadeByDatabaseAsync(predicate, cancellationToken);
#endif

        protected Dictionary<string, EntityState> _states = new Dictionary<string, EntityState>();
        protected class EntityState
        {
            public EntityState(TEntity value, string key)
            {
                this.Value = value;
                this.Key = key;
                this.Time = DateTime.Now;
            }
            public TEntity OldValue { get; set; }
            public TEntity Value { get; set; }
            public string Key { get; set; }
            public DateTime Time { get; set; }
        }
        EntityState CreateEntityState(TEntity data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            var key = Orm.GetEntityKeyString(EntityType, data, false);
            var state = new EntityState((TEntity)EntityType.CreateInstanceGetDefaultValue(), key);
            AggregateRootUtils.MapEntityValueCascade(Orm, EntityType, data, state.Value);
            return state;
        }
        bool? ExistsInStates(object data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            var key = Orm.GetEntityKeyString(EntityType, data, false);
            if (string.IsNullOrEmpty(key)) return null;
            return _states.ContainsKey(key);
        }
        void AttachCascade(TEntity entity)
        {
            var state = CreateEntityState(entity);
            if (_states.ContainsKey(state.Key)) _states[state.Key] = state;
            else _states.Add(state.Key, state);
        }

        #region Select
        public ISelect<TEntity> Select
        {
            get
            {
                var query = MainRepository.Select.TrackToList(SelectTrackingAggregateRootNavigate);
                SelectFetchAggregateRootNavigate(query, EntityType, "", new Stack<Type>());
                return query;
            }
        }
        void SelectFetchAggregateRootNavigate<T1>(ISelect<T1> currentQuery, Type entityType, string navigatePath, Stack<Type> ignores)
        {
            if (ignores.Any(a => a == entityType)) return;
            ignores.Push(entityType);
            var tb = Orm.CodeFirst.GetTableByEntity(entityType);
            foreach (var prop in tb.Properties.Values)
            {
                var tbref = tb.GetTableRef(prop.Name, false);
                if (tbref == null) continue;
                if (!string.IsNullOrWhiteSpace(navigatePath)) navigatePath = $"{navigatePath}.";
                var navigateExpression = $"{navigatePath}{prop.Name}";
                switch (tbref.RefType)
                {
                    case TableRefType.OneToOne:
                        if (ignores.Any(a => a == tbref.RefEntityType)) break;
                        currentQuery.IncludeByPropertyName(navigateExpression);
                        SelectFetchAggregateRootNavigate(currentQuery, tbref.RefEntityType, navigateExpression, ignores);
                        break;
                    case TableRefType.OneToMany:
                        var ignoresCopy = new Stack<Type>(ignores.ToArray());
                        currentQuery.IncludeByPropertyName(navigateExpression, then =>
                            SelectFetchAggregateRootNavigate(then, tbref.RefEntityType, "", ignoresCopy));
                        break;
                    case TableRefType.ManyToMany:
                        currentQuery.IncludeByPropertyName(navigateExpression);
                        break;
                    case TableRefType.PgArrayToMany:
                    case TableRefType.ManyToOne: //不属于聚合根
                        break;
                }
            }
            ignores.Pop();
        }
        void SelectTrackingAggregateRootNavigate(object list)
        {
            if (list == null) return;
            var ls = list as IEnumerable<TEntity>;
            if (ls == null)
            {
                var ie = list as IEnumerable;
                if (ie == null) return;
                var isfirst = true;
                foreach (var item in ie)
                {
                    if (item == null) continue;
                    if (isfirst)
                    {
                        isfirst = false;
                        var itemType = item.GetType();
                        if (itemType == typeof(object)) return;
                        if (itemType.FullName.Contains("FreeSqlLazyEntity__")) itemType = itemType.BaseType;
                        if (Orm.CodeFirst.GetTableByEntity(itemType)?.Primarys.Any() != true) return;
                        if (itemType.GetConstructor(Type.EmptyTypes) == null) return;
                    }
                    AttachCascade(item as TEntity);
                }
                return;
            }
        }
        #endregion

        protected static List<T1> InsertCascade<T1>(IBaseRepository<T1> repository, Func<Type, IBaseRepository<object>> getOrAddRepository, IEnumerable<T1> entitys, Dictionary<string, object> states) where T1 : class
        {
            var ret = repository.Insert(entitys);
            foreach (var entity in entitys) IsCascade(repository.EntityType, entity, true);

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
                            if (IsCascade(tbref.RefEntityType, otoItem, false) == false) return null;
                            AggregateRootUtils.SetNavigateRelationshipValue(repository.Orm, tbref, table.Type, entity, otoItem);
                            return otoItem;
                        }).Where(entity => entity != null).ToArray();
                        if (otoList.Any())
                        {
                            var repo = getOrAddRepository(tbref.RefEntityType);
                            InsertCascade(repo, getOrAddRepository, otoList, states);
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
                                if (IsCascade(tbref.RefEntityType, otmItem, false) == false) continue;
                                otmItems.Add(otmItem);
                            }
                            AggregateRootUtils.SetNavigateRelationshipValue(repository.Orm, tbref, table.Type, entity, otmItems);
                            return otmItems;
                        }).Where(entity => entity != null).SelectMany(entity => entity).ToArray();
                        if (otmList.Any())
                        {
                            var repo = getOrAddRepository(tbref.RefEntityType);
                            InsertCascade(repo, getOrAddRepository, otmList, states);
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
                            var repo = getOrAddRepository(tbref.RefMiddleEntityType);
                            InsertCascade(repo, getOrAddRepository, mtmMidList, states);
                        }
                        break;
                    case TableRefType.PgArrayToMany:
                        break;
                }
            }
            return ret;

            bool IsCascade(Type entityType, object entity, bool isadd)
            {
                var stateKey = repository.Orm.GetEntityKeyString(entityType, entity, false);
                if (stateKey == null) return true;
                stateKey = $"{stateKey}*|_,[,_|*{entityType.DisplayCsharp()}";
                if (states.ContainsKey(stateKey)) return false;
                if (isadd) states.Add(stateKey, entity);
                return true;
            }
        }
        public TEntity InsertOrUpdate(TEntity entity) => MainRepository.InsertOrUpdate(entity);
        public int UpdateCascade<T1>(IBaseRepository<T1> repository, Func<Type, IBaseRepository<object>> getOrAddRepository, IEnumerable<T1> entitys, Dictionary<string, object> states) where T1 : class
        {
            return 0;
        }
        public int Delete(IEnumerable<TEntity> entitys) => MainRepository.Delete(entitys);
        public int Delete(Expression<Func<TEntity, bool>> predicate) => MainRepository.Delete(predicate);
        public List<object> DeleteCascadeByDatabase(Expression<Func<TEntity, bool>> predicate) => MainRepository.DeleteCascadeByDatabase(predicate);

        public void SaveMany(TEntity entity, string propertyName) => MainRepository.SaveMany(entity, propertyName);

        public void BeginEdit(List<TEntity> data) => MainRepository.BeginEdit(data);
        public int EndEdit(List<TEntity> data = null) => MainRepository.EndEdit(data);

        protected static void FetchAggregateRootNavigate<T1>(IFreeSql orm, Type entityType, Func<Type, TableRef, T1> callback, Dictionary<Type, bool> ignores)
        {
            if (ignores.ContainsKey(entityType)) return;
            ignores.Add(entityType, true);
            var tb = orm.CodeFirst.GetTableByEntity(entityType);
            foreach (var prop in tb.Properties.Values)
            {
                var tbref = tb.GetTableRef(prop.Name, false);
                if (tbref == null) continue;
                switch (tbref.RefType)
                {
                    case TableRefType.OneToOne:
                        callback(tb.Type, tbref);
                        FetchAggregateRootNavigate(orm, tbref.RefEntityType, callback, ignores);
                        break;
                    case TableRefType.OneToMany:
                        callback(tb.Type, tbref);
                        FetchAggregateRootNavigate(orm, tbref.RefEntityType, callback, ignores);
                        break;
                    case TableRefType.ManyToMany:
                        callback(tb.Type, tbref);
                        FetchAggregateRootNavigate(orm, tbref.RefMiddleEntityType, callback, ignores);
                        break;
                    case TableRefType.PgArrayToMany:
                        break;
                }
            }
        }

        
    }
}
