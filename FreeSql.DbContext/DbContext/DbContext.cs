using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace FreeSql
{
    public abstract partial class DbContext : IDisposable
    {
        internal IFreeSql _ormPriv;
        public IFreeSql Orm => _ormPriv ?? throw new ArgumentNullException("请在 OnConfiguring 或 AddFreeDbContext 中配置 UseFreeSql");

        #region Property UnitOfWork
        internal bool _isUseUnitOfWork = true; //是否使用工作单元事务
        IUnitOfWork _uowPriv;
        public IUnitOfWork UnitOfWork
        {
            set => _uowPriv = value;
            get
            {
                if (_uowPriv != null) return _uowPriv;
                if (_isUseUnitOfWork == false) return null;
                return _uowPriv = new UnitOfWork(Orm);
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
                if (_optionsPriv != null) return _optionsPriv;
                if (FreeSqlDbContextExtenssions._dicSetDbContextOptions.TryGetValue(Orm, out _optionsPriv)) return _optionsPriv;
                _optionsPriv = new DbContextOptions();
                return _optionsPriv;
            }
        }
        #endregion

        protected DbContext()
        {
            var builder = new DbContextOptionsBuilder();
            OnConfiguring(builder);
            _ormPriv = builder._fsql;
            _optionsPriv = builder._options;

            if (_ormPriv != null) InitPropSets();
        }
        protected virtual void OnConfiguring(DbContextOptionsBuilder builder) { }

        #region Set
        static ConcurrentDictionary<Type, PropertyInfo[]> _dicGetDbSetProps = new ConcurrentDictionary<Type, PropertyInfo[]>();
        internal void InitPropSets()
        {
            var props = _dicGetDbSetProps.GetOrAdd(this.GetType(), tp =>
                tp.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
                    .Where(a => a.PropertyType.IsGenericType &&
                        a.PropertyType == typeof(DbSet<>).MakeGenericType(a.PropertyType.GenericTypeArguments[0])).ToArray());

            foreach (var prop in props)
            {
                var set = this.Set(prop.PropertyType.GenericTypeArguments[0]);

                prop.SetValue(this, set);
                AllSets.Add(prop.Name, set);
            }
        }

        protected List<IDbSet> _listSet = new List<IDbSet>();
        protected Dictionary<Type, IDbSet> _dicSet = new Dictionary<Type, IDbSet>();
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
        /// <summary>
        /// 添加
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="data"></param>
        public void Add<TEntity>(TEntity data) where TEntity : class => this.Set<TEntity>().Add(data);
        public void AddRange<TEntity>(IEnumerable<TEntity> data) where TEntity : class => this.Set<TEntity>().AddRange(data);
        public Task AddAsync<TEntity>(TEntity data) where TEntity : class => this.Set<TEntity>().AddAsync(data);
        public Task AddRangeAsync<TEntity>(IEnumerable<TEntity> data) where TEntity : class => this.Set<TEntity>().AddRangeAsync(data);

        /// <summary>
        /// 更新
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="data"></param>
        public void Update<TEntity>(TEntity data) where TEntity : class => this.Set<TEntity>().Update(data);
        public void UpdateRange<TEntity>(IEnumerable<TEntity> data) where TEntity : class => this.Set<TEntity>().UpdateRange(data);
        public Task UpdateAsync<TEntity>(TEntity data) where TEntity : class => this.Set<TEntity>().UpdateAsync(data);
        public Task UpdateRangeAsync<TEntity>(IEnumerable<TEntity> data) where TEntity : class => this.Set<TEntity>().UpdateRangeAsync(data);

        /// <summary>
        /// 删除
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="data"></param>
        public void Remove<TEntity>(TEntity data) where TEntity : class => this.Set<TEntity>().Remove(data);
        public void RemoveRange<TEntity>(IEnumerable<TEntity> data) where TEntity : class => this.Set<TEntity>().RemoveRange(data);

        /// <summary>
        /// 添加或更新
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="data"></param>
        public void AddOrUpdate<TEntity>(TEntity data) where TEntity : class => this.Set<TEntity>().AddOrUpdate(data);
        public Task AddOrUpdateAsync<TEntity>(TEntity data) where TEntity : class => this.Set<TEntity>().AddOrUpdateAsync(data);

        /// <summary>
        /// 附加实体，可用于不查询就更新或删除
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="data"></param>
        public void Attach<TEntity>(TEntity data) where TEntity : class => this.Set<TEntity>().Attach(data);
        public void AttachRange<TEntity>(IEnumerable<TEntity> data) where TEntity : class => this.Set<TEntity>().AttachRange(data);
        #endregion

        #region Queue Action
        internal class ExecCommandInfo
        {
            public ExecCommandInfoType actionType { get; set; }
            public IDbSet dbSet { get; set; }
            public Type stateType { get; set; }
            public object state { get; set; }
        }
        internal enum ExecCommandInfoType { Insert, Update, Delete }
        Queue<ExecCommandInfo> _actions = new Queue<ExecCommandInfo>();
        internal int _affrows = 0;

        internal void EnqueueAction(ExecCommandInfoType actionType, IDbSet dbSet, Type stateType, object state) =>
            _actions.Enqueue(new ExecCommandInfo { actionType = actionType, dbSet = dbSet, stateType = stateType, state = state });
        #endregion

        ~DbContext() => this.Dispose();
        bool _isdisposed = false;
        public void Dispose()
        {
            if (_isdisposed) return;
            _isdisposed = true;
            try
            {
                _actions.Clear();

                foreach (var set in _listSet)
                    try
                    {
                        set.Dispose();
                    }
                    catch { }

                _listSet.Clear();
                _dicSet.Clear();
                AllSets.Clear();

                UnitOfWork?.Rollback();
            }
            finally
            {
                GC.SuppressFinalize(this);
            }
        }
    }
}
