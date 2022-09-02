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
    public partial class AggregateRootRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
    {
        readonly IBaseRepository<TEntity> _repository;
        public AggregateRootRepository(IFreeSql fsql)
        {
            if (fsql == null) throw new ArgumentNullException(nameof(fsql));
            _repository = fsql.GetRepository<TEntity>();
            _repository.DbContextOptions.EnableCascadeSave = false;
        }
        public AggregateRootRepository(IFreeSql fsql, UnitOfWorkManager uowManager) : this(uowManager?.Orm ?? fsql)
        {
            uowManager?.Binding(_repository);
        }
        public void Dispose()
        {
            DisposeChildRepositorys();
            _repository.Dispose();
            FlushState();
        }

        public IFreeSql Orm => _repository.Orm;
        public IUnitOfWork UnitOfWork { get => _repository.UnitOfWork; set => _repository.UnitOfWork = value; }
        public DbContextOptions DbContextOptions
        {
            get => _repository.DbContextOptions;
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(DbContextOptions));
                _repository.DbContextOptions = value;
                _repository.DbContextOptions.EnableCascadeSave = false;
            }
        }
        public void AsType(Type entityType) => _repository.AsType(entityType); 
        Func<string, string> _asTableRule;
        public void AsTable(Func<string, string> rule)
        {
            _repository.AsTable(rule);
            _asTableRule = rule;
        }
        public Type EntityType => _repository.EntityType;
        public IDataFilter<TEntity> DataFilter => _repository.DataFilter;

        public void Attach(TEntity entity)
        {
            var state = CreateEntityState(entity);
            if (_states.ContainsKey(state.Key)) _states[state.Key] = state;
            else _states.Add(state.Key, state);
        }
        public void Attach(IEnumerable<TEntity> entity)
        {
            foreach (var item in entity)
                Attach(item);
        }
        public IBaseRepository<TEntity> AttachOnlyPrimary(TEntity data) => _repository.AttachOnlyPrimary(data);
        public Dictionary<string, object[]> CompareState(TEntity newdata) => _repository.CompareState(newdata);
        public void FlushState()
        {
            _repository.FlushState();
            _states.Clear();
        }

        public IUpdate<TEntity> UpdateDiy => _repository.UpdateDiy;
        public ISelect<TEntity> Where(Expression<Func<TEntity, bool>> exp) => Select.Where(exp);
        public ISelect<TEntity> WhereIf(bool condition, Expression<Func<TEntity, bool>> exp) => Select.WhereIf(condition, exp);

        readonly Dictionary<Type, IBaseRepository<object>> _childRepositorys = new Dictionary<Type, IBaseRepository<object>>();
        IBaseRepository<object> GetChildRepository(Type type)
        {
            if (_childRepositorys.TryGetValue(type, out var repo) == false)
            {
                repo = Orm.GetRepository<object>();
                repo.AsType(type);
                _childRepositorys.Add(type, repo);
            }
            repo.UnitOfWork = UnitOfWork;
            repo.DbContextOptions = DbContextOptions;
            repo.DbContextOptions.EnableCascadeSave = false;
            repo.AsTable(_asTableRule);
            return repo;
        }
        void DisposeChildRepositorys()
        {
            foreach (var repo in _childRepositorys.Values)
            {
                repo.FlushState();
                repo.Dispose();
            }
            _childRepositorys.Clear();
        }

        #region State
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
            AggregateRootUtils.MapEntityValue(Orm, EntityType, data, state.Value);
            return state;
        }
        bool? ExistsInStates(object data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            var key = Orm.GetEntityKeyString(EntityType, data, false);
            if (string.IsNullOrEmpty(key)) return null;
            return _states.ContainsKey(key);
        }
        #endregion

        #region Selectoriginal
        public virtual ISelect<TEntity> Select => SelectAggregateRoot;
        protected ISelect<TEntity> SelectAggregateRoot
        {
            get
            {
                var query = _repository.Select.TrackToList(SelectAggregateRootTracking);
                SelectAggregateRootNavigateReader(query, EntityType, "", new Stack<Type>());
                return query;
            }
        }
        protected void SelectAggregateRootTracking(object list)
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
                    if (item is TEntity item2) Attach(item2);
                    else return;
                }
                return;
            }
        }
        void SelectAggregateRootNavigateReader<T1>(ISelect<T1> currentQuery, Type entityType, string navigatePath, Stack<Type> ignores)
        {
            if (ignores.Any(a => a == entityType)) return;
            ignores.Push(entityType);
            var table = Orm.CodeFirst.GetTableByEntity(entityType);
            foreach (var prop in table.Properties.Values)
            {
                var tbref = table.GetTableRef(prop.Name, false);
                if (tbref == null) continue;
                if (!string.IsNullOrWhiteSpace(navigatePath)) navigatePath = $"{navigatePath}.";
                var navigateExpression = $"{navigatePath}{prop.Name}";
                switch (tbref.RefType)
                {
                    case TableRefType.OneToOne:
                        if (ignores.Any(a => a == tbref.RefEntityType)) break;
                        currentQuery.IncludeByPropertyName(navigateExpression);
                        SelectAggregateRootNavigateReader(currentQuery, tbref.RefEntityType, navigateExpression, ignores);
                        break;
                    case TableRefType.OneToMany:
                        var ignoresCopy = new Stack<Type>(ignores.ToArray());
                        currentQuery.IncludeByPropertyName(navigateExpression, then =>
                            SelectAggregateRootNavigateReader(then, tbref.RefEntityType, "", ignoresCopy));
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
        #endregion

    }
}
