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
        internal static IFreeSql _ormPriv;

        private const string ErrorMessageTemplate = @"使用前请初始化：
BaseEntity.Initialization(new FreeSqlBuilder()
        .UseAutoSyncStructure(true)
        .UseConnectionString(DataType.Sqlite, ""data source=test.db;max pool size=5"")
        .Build());";

        /// <summary>
        /// Global IFreeSql ORM Object <br />
        /// 全局 IFreeSql ORM 对象
        /// </summary>
        public static IFreeSql Orm => _ormPriv ?? throw new Exception(ErrorMessageTemplate);

        internal static Func<IUnitOfWork> _resolveUow;

        /// <summary>
        /// To initialize the BaseEntity <br />
        /// 初始化 BaseEntity
        /// <para></para>
        /// BaseEntity.Initialization( <br />
        ///   new FreeSqlBuilder() <br />
        ///     .UseAutoSyncStructure(true) <br />
        ///     .UseConnectionString(DataType.Sqlite, "data source=test.db;max pool size=5") <br />
        ///     .Build());
        /// </summary>
        /// <param name="fsql">IFreeSql ORM Object</param>
        /// <param name="resolveUow">工作单元(事务)委托，如果不使用事务请传 null<para></para>解释：由于AsyncLocal平台兼容不好，所以交给外部管理</param>
        public static void Initialization(IFreeSql fsql, Func<IUnitOfWork> resolveUow)
        {
            _ormPriv = fsql;
            _ormPriv.Aop.CurdBefore += (s, e) => Trace.WriteLine($"\r\n线程{Thread.CurrentThread.ManagedThreadId}: {e.Sql}\r\n");
            if (_configEntityQueues.Any())
            {
                lock (_configEntityLock)
                {
                    while (_configEntityQueues.TryDequeue(out var cei))
                        _ormPriv.CodeFirst.ConfigEntity(cei.EntityType, cei.Fluent);
                }
            }

            _resolveUow = resolveUow;
        }

        class ConfigEntityInfo
        {
            public Type EntityType;
            public Action<TableFluent> Fluent;
        }

        static readonly ConcurrentQueue<ConfigEntityInfo> _configEntityQueues = new();
        static readonly object _configEntityLock = new();

        internal static void ConfigEntity(Type entityType, Action<TableFluent> fluent)
        {
            lock (_configEntityLock)
            {
                if (_ormPriv is null)
                    _configEntityQueues.Enqueue(new ConfigEntityInfo { EntityType = entityType, Fluent = fluent });
                else
                    _ormPriv.CodeFirst.ConfigEntity(entityType, fluent);
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
            if (list is null)
                return;

            if (list is not IList<TEntity> ls)
            {
                if (list is not IEnumerable ie)
                    return;

                var isFirst = true;
                IBaseRepository<TEntity> baseRepo = null;

                foreach (var item in ie)
                {
                    if (item is null)
                    {
                        return;
                    }

                    if (isFirst)
                    {
                        isFirst = false;
                        var itemType = item.GetType();
                        if (itemType == typeof(object)) return;
                        if (itemType.FullName!.Contains("FreeSqlLazyEntity__")) itemType = itemType.BaseType;
                        if (Orm.CodeFirst.GetTableByEntity(itemType)?.Primarys.Any() != true) return;
                        if (item is not BaseEntity<TEntity>) return;
                    }

                    if (item is BaseEntity<TEntity> entity)
                    {
                        baseRepo ??= Orm.GetRepository<TEntity>();
                        entity.Repository = baseRepo;
                        entity.Attach();
                    }
                }

                return;
            }

            if (ls.Any() == false)
                return;

            if (ls.FirstOrDefault() is not BaseEntity<TEntity>)
                return;

            if (Orm.CodeFirst.GetTableByEntity(typeof(TEntity))?.Primarys.Any() != true)
                return;

            IBaseRepository<TEntity> repo = null;

            foreach (var item in ls)
            {
                if (item is BaseEntity<TEntity> entity)
                {
                    repo ??= Orm.GetRepository<TEntity>();
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
            Repository ??= Orm.GetRepository<TEntity>();
            var item = this as TEntity;
            Repository.Attach(item);
            return item;
        }
    }
}