using FreeSql.DataAnnotations;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable InconsistentlySynchronizedField
namespace FreeSql
{
    /// <summary>
    /// Entity base class, including CreateTime/UpdateTime/IsDeleted.
    /// <para></para>
    /// 包括 CreateTime/UpdateTime/IsDeleted 的实体基类
    /// </summary>
    [Table(DisableSyncStructure = true)]
    public abstract class BaseEntity
    {
        static Func<IFreeSql> _resoleOrm;
        internal static Func<IUnitOfWork> _resolveUow;

        public static IFreeSql Orm => _resoleOrm?.Invoke() ?? throw new Exception(CoreStrings.S_BaseEntity_Initialization_Error);

        public static void Initialization(IFreeSql fsql, Func<IUnitOfWork> resolveUow) => Initialization(() => fsql, resolveUow);
        public static void Initialization(Func<IFreeSql> resoleOrm, Func<IUnitOfWork> resolveUow)
        {
            _resoleOrm = resoleOrm;
            _resolveUow = resolveUow;

            if (_configEntityQueues.Any())
            {
                lock (_configEntityLock)
                {
                    while (_configEntityQueues.TryDequeue(out var cei))
                        Orm.CodeFirst.ConfigEntity(cei.EntityType, cei.Fluent);
                }
            }
        }

        class ConfigEntityInfo
        {
            public Type EntityType;
            public Action<TableFluent> Fluent;
        }

        static readonly ConcurrentQueue<ConfigEntityInfo> _configEntityQueues = new ConcurrentQueue<ConfigEntityInfo>();
        static readonly object _configEntityLock = new object();

        internal static void ConfigEntity(Type entityType, Action<TableFluent> fluent)
        {
            lock (_configEntityLock)
            {
                if (_resoleOrm?.Invoke() == null)
                    _configEntityQueues.Enqueue(new ConfigEntityInfo { EntityType = entityType, Fluent = fluent });
                else
                    Orm.CodeFirst.ConfigEntity(entityType, fluent);
            }
        }

        /// <summary>
        /// Created time <br />
        /// 创建时间
        /// </summary>
        [Column(Position = -4)]
        public virtual DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// Updated time <br />
        /// 更新时间
        /// </summary>
        [Column(Position = -3)]
        public virtual DateTime UpdateTime { get; set; }

        /// <summary>
        /// Logical Delete <br />
        /// 逻辑删除
        /// </summary>
        [Column(Position = -2)]
        public virtual bool IsDeleted { get; set; }

        /// <summary>
        /// Sort <br />
        /// 排序
        /// </summary>
        [Column(Position = -1)]
        public virtual int Sort { get; set; }
    }

    /// <summary>
    /// A readonly entity base class, including CreateTime/UpdateTime/IsDeleted.
    /// <para></para>
    /// 包括 CreateTime/UpdateTime/IsDeleted 的只读实体基类
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    [Table(DisableSyncStructure = true)]
    public abstract class BaseEntityReadOnly<TEntity> : BaseEntity where TEntity : class
    {
        /// <summary>
        /// To query data <br />
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public static ISelect<TEntity> Select
        {
            get
            {
                var select = Orm.Select<TEntity>()
                    .TrackToList(TrackToList) //自动为每个元素 Attach
                    .WithTransaction(_resolveUow?.Invoke()?.GetOrBeginTransaction(false));
                return select.WhereCascade(a => (a as BaseEntity).IsDeleted == false);
            }
        }

        static void TrackToList(object list)
        {
            if (list == null) return;

            var ls = list as IList<TEntity>;
            if (ls == null)
            {
                var ie = list as IEnumerable;
                if (ie == null) return;

                var isFirst = true;
                IBaseRepository<TEntity> baseRepo = null;

                foreach (var item in ie)
                {
                    if (item == null) return;

                    if (isFirst)
                    {
                        isFirst = false;
                        var itemType = item.GetType();
                        if (itemType == typeof(object)) return;
                        if (itemType.FullName.Contains("FreeSqlLazyEntity__")) itemType = itemType.BaseType;
                        if (Orm.CodeFirst.GetTableByEntity(itemType)?.Primarys.Any() != true) return;
                        if (itemType.GetConstructor(Type.EmptyTypes) == null) return;
                        if (item is BaseEntity<TEntity> == false) return;
                    }

                    if (item is BaseEntity<TEntity> entity)
                    {
                        if (baseRepo == null) baseRepo = Orm.GetRepository<TEntity>();
                        entity.Repository = baseRepo;
                        entity.Attach();
                    }
                }
                return;
            }

            if (ls.Any() == false) return;
            if (ls.FirstOrDefault() is BaseEntity<TEntity> == false) return;
            if (Orm.CodeFirst.GetTableByEntity(typeof(TEntity))?.Primarys.Any() != true) return;

            IBaseRepository<TEntity> repo = null;
            foreach (var item in ls)
            {
                if (item is BaseEntity<TEntity> entity)
                {
                    if (repo == null) repo = Orm.GetRepository<TEntity>();
                    entity.Repository = repo;
                    entity.Attach();
                }
            }
        }

        /// <summary>
        /// Query conditions <br />
        /// 查询条件，Where(a => a.Id> 10)
        /// <para></para>
        /// Support navigation object query <br />
        /// 支持导航对象查询，Where(a => a.Author.Email == "2881099@qq.com")
        /// </summary>
        /// <param name="exp">lambda表达式</param>
        /// <returns></returns>
        public static ISelect<TEntity> Where(Expression<Func<TEntity, bool>> exp) => Select.Where(exp);

        /// <summary>
        /// Query conditions <br />
        /// 查询条件，Where(true, a => a.Id > 10)
        /// <para></para>
        /// Support navigation object query <br />
        /// 支导航对象查询，Where(true, a => a.Author.Email == "2881099@qq.com")
        /// </summary>
        /// <param name="condition">true 时生效</param>
        /// <param name="exp">lambda表达式</param>
        /// <returns></returns>
        public static ISelect<TEntity> WhereIf(bool condition, Expression<Func<TEntity, bool>> exp) => Select.WhereIf(condition, exp);

        /// <summary>
        /// Repository object. <br />
        /// 仓储对象
        /// </summary>
        protected IBaseRepository<TEntity> Repository { get; set; }

        /// <summary>
        /// To Attach entities. When updating data, only the changed part is updated. <br />
        /// 附加实体。在更新数据时，只更新变化的部分
        /// </summary>
        public TEntity Attach()
        {
            if (Repository == null) 
                Repository = Orm.GetRepository<TEntity>();
            var item = this as TEntity;
            Repository.Attach(item);
            return item;
        }
    }
}