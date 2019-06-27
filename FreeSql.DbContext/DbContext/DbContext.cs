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

        internal IFreeSql _orm;
        internal IFreeSql _fsql => _orm ?? throw new ArgumentNullException("请在 OnConfiguring 或 AddFreeDbContext 中配置 UseFreeSql");

        public IFreeSql Orm => _fsql;

        protected IUnitOfWork _uowPriv;
        internal IUnitOfWork _uow => _isUseUnitOfWork ? (_uowPriv ?? (_uowPriv = new UnitOfWork(_fsql))) : null;
        internal bool _isUseUnitOfWork = true; //不使用工作单元事务

        public IUnitOfWork UnitOfWork => _uow;

        DbContextOptions _options;
        internal DbContextOptions Options
        {
            get
            {
                if (_options != null) return _options;
                if (FreeSqlDbContextExtenssions._dicSetDbContextOptions.TryGetValue(Orm, out _options)) return _options;
                _options = new DbContextOptions();
                return _options;
            }
        }

        static ConcurrentDictionary<Type, PropertyInfo[]> _dicGetDbSetProps = new ConcurrentDictionary<Type, PropertyInfo[]>();
        protected DbContext()
        {

            var builder = new DbContextOptionsBuilder();
            OnConfiguring(builder);
            _orm = builder._fsql;

            if (_orm != null) InitPropSets();
        }

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

        protected virtual void OnConfiguring(DbContextOptionsBuilder builder)
        {

        }

        protected Dictionary<Type, IDbSet> _dicSet = new Dictionary<Type, IDbSet>();
        public DbSet<TEntity> Set<TEntity>() where TEntity : class => this.Set(typeof(TEntity)) as DbSet<TEntity>;
        public virtual IDbSet Set(Type entityType)
        {
            if (_dicSet.ContainsKey(entityType)) return _dicSet[entityType];
            var sd = Activator.CreateInstance(typeof(DbContextDbSet<>).MakeGenericType(entityType), this) as IDbSet;
            if (entityType != typeof(object)) _dicSet.Add(entityType, sd);
            return sd;
        }
        protected Dictionary<string, IDbSet> AllSets { get; } = new Dictionary<string, IDbSet>();

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

        internal void EnqueueAction(ExecCommandInfoType actionType, IDbSet dbSet, Type stateType, object state)
        {
            _actions.Enqueue(new ExecCommandInfo { actionType = actionType, dbSet = dbSet, stateType = stateType, state = state });
        }

        ~DbContext()
        {
            this.Dispose();
        }
        bool _isdisposed = false;
        public void Dispose()
        {
            if (_isdisposed) return;
            try
            {
                _actions.Clear();

                foreach (var set in _dicSet)
                    try
                    {
                        set.Value.Dispose();
                    }
                    catch { }

                _dicSet.Clear();
                AllSets.Clear();

                _uow?.Rollback();
            }
            finally
            {
                _isdisposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
}
