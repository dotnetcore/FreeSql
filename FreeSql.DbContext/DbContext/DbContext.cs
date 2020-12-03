using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using FreeSql.Internal.Model;

namespace FreeSql
{
    public abstract partial class DbContext : IDisposable
    {
        internal DbContextScopedFreeSql _ormScoped;
        internal IFreeSql OrmOriginal => _ormScoped?._originalFsql ?? throw new ArgumentNullException("请在 OnConfiguring 或 AddFreeDbContext 中配置 UseFreeSql");

        /// <summary>
        /// 该对象 Select/Delete/Insert/Update/InsertOrUpdate 与 DbContext 事务保持一致，可省略传递 WithTransaction
        /// </summary>
        public IFreeSql Orm => _ormScoped ?? throw new ArgumentNullException("请在 OnConfiguring 或 AddFreeDbContext 中配置 UseFreeSql");

        #region Property UnitOfWork
        internal bool _isUseUnitOfWork = true; //是否创建工作单元事务
        IUnitOfWork _uowPriv;
        public IUnitOfWork UnitOfWork
        {
            set => _uowPriv = value;
            get
            {
                if (_uowPriv != null) return _uowPriv;
                if (_isUseUnitOfWork == false) return null;
                return _uowPriv = new UnitOfWork(OrmOriginal);
            }
        }
        #endregion

        #region Property Options
        internal DbContextOptions _optionsPriv;
        public DbContextOptions Options
        {
            set => _optionsPriv = value;
            get
            {
                if (_optionsPriv == null)
                {
                    _optionsPriv = new DbContextOptions();
                    if (FreeSqlDbContextExtensions._dicSetDbContextOptions.TryGetValue(OrmOriginal.Ado.Identifier, out var opt))
                    {
                        _optionsPriv.EnableAddOrUpdateNavigateList = opt.EnableAddOrUpdateNavigateList;
                        _optionsPriv.EnableGlobalFilter = opt.EnableGlobalFilter;
                        _optionsPriv.NoneParameter = opt.NoneParameter;
                        _optionsPriv.OnEntityChange = opt.OnEntityChange;
                    }
                }
                return _optionsPriv;
            }
        }
        internal void EmitOnEntityChange(List<EntityChangeReport.ChangeInfo> report)
        {
            var oec = UnitOfWork?.EntityChangeReport?.OnChange ?? Options.OnEntityChange;
            if (oec == null || report == null || report.Any() == false) return;
            oec(report);
        }
        #endregion

        protected DbContext() : this(null, null) { }
        protected DbContext(IFreeSql fsql, DbContextOptions options)
        {
            _ormScoped = DbContextScopedFreeSql.Create(fsql, () => this, () => UnitOfWork);
            _optionsPriv = options;

            if (_ormScoped == null)
            {
                var builder = new DbContextOptionsBuilder();
                OnConfiguring(builder);
                _ormScoped = DbContextScopedFreeSql.Create(builder._fsql, () => this, () => UnitOfWork);
                _optionsPriv = builder._options;
            }
            if (_ormScoped != null) InitPropSets();
        }
        protected virtual void OnConfiguring(DbContextOptionsBuilder options) { }
        protected virtual void OnModelCreating(ICodeFirst codefirst) { }

        #region Set
        static ConcurrentDictionary<Type, NativeTuple<PropertyInfo[], bool>> _dicGetDbSetProps = new ConcurrentDictionary<Type, NativeTuple<PropertyInfo[], bool>>();
        internal void InitPropSets()
        {
            var thisType = this.GetType();
            var dicval = _dicGetDbSetProps.GetOrAdd(thisType, tp =>
                NativeTuple.Create(
                    tp.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
                        .Where(a => a.PropertyType.IsGenericType &&
                            a.PropertyType == typeof(DbSet<>).MakeGenericType(a.PropertyType.GetGenericArguments()[0])).ToArray(),
                    false));
            if (dicval.Item2 == false)
            {
                if (_dicGetDbSetProps.TryUpdate(thisType, NativeTuple.Create(dicval.Item1, true), dicval))
                    OnModelCreating(OrmOriginal.CodeFirst);
            }

            foreach (var prop in dicval.Item1)
            {
                var set = this.Set(prop.PropertyType.GetGenericArguments()[0]);

                prop.SetValue(this, set, null);
                AllSets.Add(prop.Name, set);
            }
        }

        protected List<IDbSet> _listSet = new List<IDbSet>();
        protected Dictionary<Type, IDbSet> _dicSet = new Dictionary<Type, IDbSet>();
        internal Dictionary<Type, IDbSet> InternalDicSet => _dicSet;
        public DbSet<TEntity> Set<TEntity>() where TEntity : class => this.Set(typeof(TEntity)) as DbSet<TEntity>;
        public virtual IDbSet Set(Type entityType)
        {
            if (_dicSet.ContainsKey(entityType)) return _dicSet[entityType];
            var sd = Activator.CreateInstance(typeof(DbContextDbSet<>).MakeGenericType(entityType), this) as IDbSet;
            _listSet.Add(sd);
            if (entityType != typeof(object)) _dicSet.Add(entityType, sd);
            return sd;
        }
        protected Dictionary<string, IDbSet> AllSets { get; } = new Dictionary<string, IDbSet>();
        #endregion

        #region DbSet 快速代理
        void CheckEntityTypeOrThrow(Type entityType)
        {
            if (OrmOriginal.CodeFirst.GetTableByEntity(entityType) == null)
                throw new ArgumentException($"参数 data 类型错误 {entityType.FullName} ");
        }
        /// <summary>
        /// 添加
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="data"></param>
        public void Add<TEntity>(TEntity data) where TEntity : class
        {
            CheckEntityTypeOrThrow(typeof(TEntity));
            this.Set<TEntity>().Add(data);
        }
        public void AddRange<TEntity>(IEnumerable<TEntity> data) where TEntity : class => this.Set<TEntity>().AddRange(data);

        /// <summary>
        /// 更新
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="data"></param>
        public void Update<TEntity>(TEntity data) where TEntity : class
        {
            CheckEntityTypeOrThrow(typeof(TEntity));
            this.Set<TEntity>().Update(data);
        }
        public void UpdateRange<TEntity>(IEnumerable<TEntity> data) where TEntity : class => this.Set<TEntity>().UpdateRange(data);

        /// <summary>
        /// 删除
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="data"></param>
        public void Remove<TEntity>(TEntity data) where TEntity : class
        {
            CheckEntityTypeOrThrow(typeof(TEntity));
            this.Set<TEntity>().Remove(data);
        }
        public void RemoveRange<TEntity>(IEnumerable<TEntity> data) where TEntity : class => this.Set<TEntity>().RemoveRange(data);

        /// <summary>
        /// 添加或更新
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="data"></param>
        public void AddOrUpdate<TEntity>(TEntity data) where TEntity : class
        {
            CheckEntityTypeOrThrow(typeof(TEntity));
            this.Set<TEntity>().AddOrUpdate(data);
        }
        /// <summary>
        /// 保存实体的指定 ManyToMany/OneToMany 导航属性（完整对比）<para></para>
        /// 场景：在关闭级联保存功能之后，手工使用本方法<para></para>
        /// 例子：保存商品的 OneToMany 集合属性，SaveMany(goods, "Skus")<para></para>
        /// 当 goods.Skus 为空(非null)时，会删除表中已存在的所有数据<para></para>
        /// 当 goods.Skus 不为空(非null)时，添加/更新后，删除表中不存在 Skus 集合属性的所有记录
        /// </summary>
        /// <param name="data">实体对象</param>
        /// <param name="propertyName">属性名</param>
        public void SaveMany<TEntity>(TEntity data, string propertyName) where TEntity : class
        {
            CheckEntityTypeOrThrow(typeof(TEntity));
            this.Set<TEntity>().SaveMany(data, propertyName);
        }

        /// <summary>
        /// 附加实体，可用于不查询就更新或删除
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="data"></param>
        public void Attach<TEntity>(TEntity data) where TEntity : class
        {
            CheckEntityTypeOrThrow(typeof(TEntity));
            this.Set<TEntity>().Attach(data);
        }
        public void AttachRange<TEntity>(IEnumerable<TEntity> data) where TEntity : class => this.Set<TEntity>().AttachRange(data);

        /// <summary>
        /// 附加实体，并且只附加主键值，可用于不更新属性值为null或默认值的字段
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="data"></param>
        public DbContext AttachOnlyPrimary<TEntity>(TEntity data) where TEntity : class
        {
            CheckEntityTypeOrThrow(typeof(TEntity));
            this.Set<TEntity>().AttachOnlyPrimary(data);
            return this;
        }
#if net40
#else
        public Task AddAsync<TEntity>(TEntity data) where TEntity : class
        {
            CheckEntityTypeOrThrow(typeof(TEntity));
            return this.Set<TEntity>().AddAsync(data);
        }
        public Task AddRangeAsync<TEntity>(IEnumerable<TEntity> data) where TEntity : class => this.Set<TEntity>().AddRangeAsync(data);

        public Task UpdateAsync<TEntity>(TEntity data) where TEntity : class
        {
            CheckEntityTypeOrThrow(typeof(TEntity));
            return this.Set<TEntity>().UpdateAsync(data);
        }
        public Task UpdateRangeAsync<TEntity>(IEnumerable<TEntity> data) where TEntity : class => this.Set<TEntity>().UpdateRangeAsync(data);

        public Task AddOrUpdateAsync<TEntity>(TEntity data) where TEntity : class
        {
            CheckEntityTypeOrThrow(typeof(TEntity));
            return this.Set<TEntity>().AddOrUpdateAsync(data);
        }
        public Task SaveManyAsync<TEntity>(TEntity data, string propertyName) where TEntity : class
        {
            CheckEntityTypeOrThrow(typeof(TEntity));
            return this.Set<TEntity>().SaveManyAsync(data, propertyName);
        }
#endif
        #endregion

        #region Queue PreCommand
        public class EntityChangeReport
        {
            public class ChangeInfo
            {
                public object Object { get; set; }
                /// <summary>
                /// Type = Update 的时候，获取更新之前的对象
                /// </summary>
                public object BeforeObject { get; set; }
                public EntityChangeType Type { get; set; }
            }
            /// <summary>
            /// 实体变化记录
            /// </summary>
            public List<ChangeInfo> Report { get; } = new List<ChangeInfo>();
            /// <summary>
            /// 实体变化事件
            /// </summary>
            public Action<List<ChangeInfo>> OnChange { get; set; }
        }
        internal List<EntityChangeReport.ChangeInfo> _entityChangeReport = new List<EntityChangeReport.ChangeInfo>();
        public enum EntityChangeType { Insert, Update, Delete, SqlRaw }
        internal class PrevCommandInfo
        {
            public EntityChangeType changeType { get; set; }
            public IDbSet dbSet { get; set; }
            public Type stateType { get; set; }
            public Type entityType { get; set; }
            public object state { get; set; }
        }
        Queue<PrevCommandInfo> _prevCommands = new Queue<PrevCommandInfo>();
        internal int _affrows = 0;

        internal void EnqueuePreCommand(EntityChangeType changeType, IDbSet dbSet, Type stateType, Type entityType, object state) =>
            _prevCommands.Enqueue(new PrevCommandInfo { changeType = changeType, dbSet = dbSet, stateType = stateType, entityType = entityType, state = state });
        #endregion

        ~DbContext() => this.Dispose();
        int _disposeCounter;
        public void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCounter) != 1) return;
            try
            {
                _prevCommands.Clear();

                foreach (var set in _listSet)
                    try
                    {
                        set.Dispose();
                    }
                    catch { }

                _listSet.Clear();
                _dicSet.Clear();
                AllSets.Clear();

                if (_isUseUnitOfWork)
                    UnitOfWork?.Dispose();
            }
            finally
            {
                GC.SuppressFinalize(this);
            }
        }
    }
}
