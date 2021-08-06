#if NET40
using FreeSql.DataAnnotations;
using System;

#else
using FreeSql.DataAnnotations;
using System;
using System.Threading.Tasks;

#endif

// ReSharper disable once CheckNamespace
namespace FreeSql
{
    /// <summary>
    /// 包括 CreateTime/UpdateTime/IsDeleted、CRUD 方法、以及 ID 主键定义 的实体基类
    /// <para></para>
    /// 当 TKey 为 int/long 时，Id 主键被设为自增值主键
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    [Table(DisableSyncStructure = true)]
    public abstract class BaseEntity<TEntity, TKey> : BaseEntity<TEntity> where TEntity : class
    {
        static BaseEntity()
        {
            var keyType = typeof(TKey).NullableTypeOrThis();
            if (keyType == typeof(int) || keyType == typeof(long))
                ConfigEntity(typeof(TEntity), t => t.Property("Id").IsIdentity(true));
        }

        /// <summary>
        /// 主键
        /// </summary>
        [Column(Position = 1)]
        public virtual TKey Id { get; set; }

#if !NET40
        /// <summary>
        /// 根据主键值获取数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<TEntity> FindAsync(TKey id)
        {
            var item = await Select.WhereDynamic(id).FirstAsync();
            (item as BaseEntity<TEntity>)?.Attach();
            return item;
        }
#endif

        /// <summary>
        /// 根据主键值获取数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static TEntity Find(TKey id)
        {
            var item = Select.WhereDynamic(id).First();
            (item as BaseEntity<TEntity>)?.Attach();
            return item;
        }
    }

    /// <summary>
    /// 包括 CreateTime/UpdateTime/IsDeleted、以及 CRUD 异步和同步方法的实体基类
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    [Table(DisableSyncStructure = true)]
    public abstract class BaseEntity<TEntity> : BaseEntityAsync<TEntity> where TEntity : class
    {
        bool UpdateIsDeleted(bool value)
        {
            if (Repository is null)
            {
                return Orm.Update<TEntity>(this as TEntity)
                          .WithTransaction(_resolveUow?.Invoke()?.GetOrBeginTransaction())
                          .Set(a => (a as BaseEntity).IsDeleted, IsDeleted = value).ExecuteAffrows() == 1;
            }

            IsDeleted = value;
            Repository.UnitOfWork = _resolveUow?.Invoke();
            return Repository.Update(this as TEntity) == 1;
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="physicalDelete">是否物理删除</param>
        /// <returns></returns>
        public virtual bool Delete(bool physicalDelete = false)
        {
            if (physicalDelete == false)
                return UpdateIsDeleted(true);

            if (Repository is null)
                return Orm.Delete<TEntity>(this as TEntity).ExecuteAffrows() == 1;

            Repository.UnitOfWork = _resolveUow?.Invoke();
            return Repository.Delete(this as TEntity) == 1;
        }

        /// <summary>
        /// 恢复删除的数据
        /// </summary>
        /// <returns></returns>
        public virtual bool Restore() => UpdateIsDeleted(false);

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <returns></returns>
        public virtual bool Update()
        {
            UpdateTime = DateTime.Now;
            if (Repository is null)
            {
                return Orm.Update<TEntity>()
                          .WithTransaction(_resolveUow?.Invoke()?.GetOrBeginTransaction())
                          .SetSource(this as TEntity).ExecuteAffrows() == 1;
            }

            Repository.UnitOfWork = _resolveUow?.Invoke();
            return Repository.Update(this as TEntity) == 1;
        }

        /// <summary>
        /// 插入数据
        /// </summary>
        public virtual TEntity Insert()
        {
            CreateTime = DateTime.Now;
            if (Repository is null)
                Repository = Orm.GetRepository<TEntity>();

            Repository.UnitOfWork = _resolveUow?.Invoke();
            return Repository.Insert(this as TEntity);
        }

        /// <summary>
        /// 更新或插入
        /// </summary>
        /// <returns></returns>
        public virtual TEntity Save()
        {
            UpdateTime = DateTime.Now;
            if (Repository is null)
                Repository = Orm.GetRepository<TEntity>();

            Repository.UnitOfWork = _resolveUow?.Invoke();
            return Repository.InsertOrUpdate(this as TEntity);
        }

        /// <summary>
        /// 【完整】保存导航属性，子表
        /// </summary>
        /// <param name="navigatePropertyName">导航属性名</param>
        public virtual void SaveMany(string navigatePropertyName)
        {
            if (Repository is null)
                Repository = Orm.GetRepository<TEntity>();

            Repository.UnitOfWork = _resolveUow?.Invoke();
            Repository.SaveMany(this as TEntity, navigatePropertyName);
        }
    }
}