using FreeSql.Extensions.EntityUtil;
using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

        #region 状态管理
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

        #region 查询数据
        /// <summary>
        /// 默认：创建查询对象（递归包含 Include/IncludeMany 边界之内的导航属性）<para></para>
        /// 重写：使用
        /// </summary>
        public virtual ISelect<TEntity> Select => SelectAggregateRoot;
        /// <summary>
        /// 创建查询对象（纯净）<para></para>
        /// 当聚合根内关系复杂时，使用 this.SelectAggregateRootStaticCode 可以生成边界以内的 Include/IncludeMany 代码块
        /// </summary>
        protected ISelect<TEntity> SelectDiy => _repository.Select;
        /// <summary>
        /// 创建查询对象（递归包含 Include/IncludeMany 边界之内的导航属性）
        /// </summary>
        /// <returns></returns>
        protected ISelect<TEntity> SelectAggregateRoot
        {
            get
            {
                var query = _repository.Select.TrackToList(SelectAggregateRootTracking);
                SelectAggregateRootNavigateReader(query, EntityType, "", new Stack<Type>());
                return query;
            }
        }
        /// <summary>
        /// 按默认边界规则，返回 c# 静态代码<para></para>
        /// 1、聚合根内关系复杂手工编写 Include/IncludeMany 会很蛋疼<para></para>
        /// 2、返回的内容用，可用于配合重写仓储 override Select 属性<para></para>
        /// 返回内容：fsql.Select&lt;T&gt;().Include(...).IncludeMany(...) 
        /// </summary>
        protected string SelectAggregateRootStaticCode => $"//fsql.Select<{EntityType.Name}>()\r\nthis.SelectDiy{SelectAggregateRootNavigateReader(1, EntityType, "", new Stack<Type>())}";
        /// <summary>
        /// ISelect.TrackToList 委托，数据返回后自动 Attach
        /// </summary>
        /// <param name="list"></param>
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
            if (table == null) return;
            if (!string.IsNullOrWhiteSpace(navigatePath)) navigatePath = $"{navigatePath}.";
            foreach (var prop in table.Properties.Values)
            {
                var tbref = table.GetTableRef(prop.Name, false);
                if (tbref == null) continue;
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
        string SelectAggregateRootNavigateReader(int depth, Type entityType, string navigatePath, Stack<Type> ignores)
        {
            var code = new StringBuilder();
            if (ignores.Any(a => a == entityType)) return null;
            ignores.Push(entityType);
            var table = Orm.CodeFirst.GetTableByEntity(entityType);
            if (table == null) return null;
            if (!string.IsNullOrWhiteSpace(navigatePath)) navigatePath = $"{navigatePath}.";
            foreach (var prop in table.Properties.Values)
            {
                var tbref = table.GetTableRef(prop.Name, false);
                if (tbref == null) continue;
                var navigateExpression = $"{navigatePath}{prop.Name}";
                var depthTab = "".PadLeft(depth * 4);
                var lambdaAlias = (char)((byte)'a' + (depth - 1));
                var lambdaStr = $"{lambdaAlias} => {lambdaAlias}.";
                switch (tbref.RefType)
                {
                    case TableRefType.OneToOne:
                        if (ignores.Any(a => a == tbref.RefEntityType)) break;
                        code.Append("\r\n").Append(depthTab).Append(".Include(").Append(lambdaStr).Append(navigateExpression).Append(")");
                        code.Append(SelectAggregateRootNavigateReader(depth, tbref.RefEntityType, navigateExpression, ignores));
                        break;
                    case TableRefType.OneToMany:
                        code.Append("\r\n").Append(depthTab).Append(".IncludeMany(").Append(lambdaStr).Append(navigateExpression);
                        var thencode = SelectAggregateRootNavigateReader(depth + 1, tbref.RefEntityType, "", new Stack<Type>(ignores.ToArray()));
                        if (thencode.Length > 0) code.Append(", then => then").Append(thencode);
                        code.Append(")");
                        break;
                    case TableRefType.ManyToMany:
                        code.Append("\r\n").Append(depthTab).Append(".IncludeMany(").Append(lambdaStr).Append(navigateExpression).Append(")");
                        break;
                    case TableRefType.PgArrayToMany:
                        code.Append("\r\n//").Append(depthTab).Append(".IncludeMany(").Append(lambdaStr).Append(navigateExpression).Append(")");
                        break;
                    case TableRefType.ManyToOne: //不属于聚合根
                        code.Append("\r\n//").Append(depthTab).Append(".Include(").Append(lambdaStr).Append(navigateExpression).Append(")");
                        break;
                }
            }
            ignores.Pop();
            return code.ToString();
        }
        #endregion

    }
}
