#if netcore

using FreeSql.DataAnnotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace FreeSql
{
    /// <summary>
    /// 包括 CreateTime/UpdateTime/IsDeleted 的实体基类
    /// </summary>
    [Table(DisableSyncStructure = true)]
    public abstract class BaseEntity
    {
        static IFreeSql _ormPriv;
        /// <summary>
        /// 全局 IFreeSql orm 对象
        /// </summary>
        public static IFreeSql Orm => _ormPriv ?? throw new Exception(@"使用前请初始化 BaseEntity.Initialization(new FreeSqlBuilder()
.UseAutoSyncStructure(true)
.UseConnectionString(DataType.Sqlite, ""data source=test.db;max pool size=5"")
.Build());");

        /// <summary>
        /// 初始化BaseEntity
        /// BaseEntity.Initialization(new FreeSqlBuilder()
        /// <para></para>
        /// .UseAutoSyncStructure(true)
        /// <para></para>
        /// .UseConnectionString(DataType.Sqlite, "data source=test.db;max pool size=5")
        /// <para></para>
        /// .Build());
        /// </summary>
        /// <param name="fsql">IFreeSql orm 对象</param>
        public static void Initialization(IFreeSql fsql)
        {
            _ormPriv = fsql;
            _ormPriv.Aop.CurdBefore += (s, e) => Trace.WriteLine($"\r\n线程{Thread.CurrentThread.ManagedThreadId}: {e.Sql}\r\n");
        }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Position = -4)]
        public DateTime CreateTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 更新时间
        /// </summary>
        [Column(Position = -3)]
        public DateTime UpdateTime { get; set; }
        /// <summary>
        /// 逻辑删除
        /// </summary>
        [Column(Position = -2)]
        public bool IsDeleted { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        [Column(Position = -1)]
        public int Sort { get; set; }

        /// <summary>
        /// 开启工作单元事务
        /// </summary>
        /// <returns></returns>
        public static IUnitOfWork Begin() => Begin(null);
        /// <summary>
        /// 开启工作单元事务
        /// </summary>
        /// <param name="level">事务等级</param>
        /// <returns></returns>
        public static IUnitOfWork Begin(IsolationLevel? level)
        {
            var uow = Orm.CreateUnitOfWork();
            uow.IsolationLevel = level;
            return uow;
        }

        static readonly AsyncLocal<string> _AsyncTenantId = new AsyncLocal<string>();
        /// <summary>
        /// 获取或设置当前租户id
        /// </summary>
        public static string CurrentTenantId
        {
            get => _AsyncTenantId.Value;
            set => _AsyncTenantId.Value = value;
        }
    }

    /// <summary>
    /// 租户
    /// </summary>
    public interface ITenant
    {
        /// <summary>
        /// 租户id
        /// </summary>
        string TenantId { get; set; }
    }

    public abstract class BaseEntityReadOnly<TEntity> : BaseEntity where TEntity : class
    {
        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public static ISelect<TEntity> Select
        {
            get
            {
                var select = Orm.Select<TEntity>()
                    .TrackToList(TrackToList) //自动为每个元素 Attach
                    .WithTransaction(UnitOfWork.Current.Value?.GetOrBeginTransaction(false));
                if (string.IsNullOrEmpty(CurrentTenantId) == false)
                    select.WhereCascade(a => (a as ITenant).TenantId == CurrentTenantId);
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
                foreach (var item in ie)
                {
                    if (item == null) return;
                    if (isFirst)
                    {
                        isFirst = false;
                        var itemType = item.GetType();
                        if (itemType == typeof(object)) return;
                        if (itemType.FullName.StartsWith("Submission#")) itemType = itemType.BaseType;
                        if (Orm.CodeFirst.GetTableByEntity(itemType)?.Primarys.Any() != true) return;
                        if (item is BaseEntity<TEntity> == false) return;
                    }
                    (item as BaseEntity<TEntity>)?.Attach();
                }
                return;
            }
            if (ls.Any() == false) return;
            if (ls.FirstOrDefault() is BaseEntity<TEntity> == false) return;
            if (Orm.CodeFirst.GetTableByEntity(typeof(TEntity))?.Primarys.Any() != true) return;
            foreach (var item in ls)
                (item as BaseEntity<TEntity>)?.Attach();
        }

        /// <summary>
        /// 设置当前租户id
        /// </summary>
        protected void SetTenantId()
        {
            if (string.IsNullOrEmpty(CurrentTenantId) == false)
            {
                var ten = this as ITenant;
                if (ten != null)
                    ten.TenantId = CurrentTenantId;
            }
        }

        /// <summary>
        /// 查询条件，Where(a => a.Id > 10)，支持导航对象查询，Where(a => a.Author.Email == "2881099@qq.com")
        /// </summary>
        /// <param name="exp">lambda表达式</param>
        /// <returns></returns>
        public static ISelect<TEntity> Where(Expression<Func<TEntity, bool>> exp) => Select.Where(exp);
        /// <summary>
        /// 查询条件，Where(true, a => a.Id > 10)，支导航对象查询，Where(true, a => a.Author.Email == "2881099@qq.com")
        /// </summary>
        /// <param name="condition">true 时生效</param>
        /// <param name="exp">lambda表达式</param>
        /// <returns></returns>
        public static ISelect<TEntity> WhereIf(bool condition, Expression<Func<TEntity, bool>> exp) => Select.WhereIf(condition, exp);

        /// <summary>
        /// 仓储对象
        /// </summary>
        protected IBaseRepository<TEntity> Repository { get; set; }

        /// <summary>
        /// 附加实体，在更新数据时，只更新变化的部分
        /// </summary>
        public TEntity Attach()
        {
            if (this.Repository == null)
                this.Repository = Orm.GetRepository<TEntity>();

            var item = this as TEntity;
            this.SetTenantId();
            this.Repository.Attach(item);
            return item;
        }
    }
}

#endif