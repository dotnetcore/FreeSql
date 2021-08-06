#if NET40
using FreeSql.DataAnnotations;

#else
using FreeSql.DataAnnotations;
using System;
using System.Threading.Tasks;

#endif

// ReSharper disable once CheckNamespace
namespace FreeSql
{
    /// <summary>
    /// Entity base class, including CreateTime/UpdateTime/IsDeleted, the async CRUD methods, and ID primary key definition.
    /// <para></para>
    /// 包括 CreateTime/UpdateTime/IsDeleted、CRUD 异步方法、以及 ID 主键定义 的实体基类
    /// <para></para>
    /// When TKey is int/long, the Id is set to be an auto-incremented primary key
    /// <para></para>
    /// 当 TKey 为 int/long 时，Id 主键被设为自增值主键
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    [Table(DisableSyncStructure = true)]
    public abstract class BaseEntityAsync<TEntity, TKey> : BaseEntityAsync<TEntity> where TEntity : class
    {
        static BaseEntityAsync()
        {
            var keyType = typeof(TKey).NullableTypeOrThis();
            if (keyType == typeof(int) || keyType == typeof(long))
                ConfigEntity(typeof(TEntity), t => t.Property("Id").IsIdentity(true));
        }

        /// <summary>
        /// Primary key <br />
        /// 主键
        /// </summary>
        [Column(Position = 1)]
        public virtual TKey Id { get; set; }

#if !NET40
        /// <summary>
        /// Get data based on the value of the primary key <br />
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
    }

    /// <summary>
    /// Entity base class, including CreateTime/UpdateTime/IsDeleted, and async CRUD methods.
    /// <para></para>
    /// 包括 CreateTime/UpdateTime/IsDeleted、以及 CRUD 异步方法的实体基类
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    [Table(DisableSyncStructure = true)]
    public abstract class BaseEntityAsync<TEntity> : BaseEntityReadOnly<TEntity> where TEntity : class
    {
#if !NET40
        async Task<bool> UpdateIsDeletedAsync(bool value)
        {
            if (Repository is null)
            {
                return await Orm.Update<TEntity>(this as TEntity)
                                .WithTransaction(_resolveUow?.Invoke()?.GetOrBeginTransaction())
                                .Set(a => (a as BaseEntity).IsDeleted, IsDeleted = value).ExecuteAffrowsAsync() == 1;
            }

            IsDeleted = value;
            Repository.UnitOfWork = _resolveUow?.Invoke();
            return await Repository.UpdateAsync(this as TEntity) == 1;
        }

        /// <summary>
        /// To delete data <br />
        /// 删除数据
        /// </summary>
        /// <param name="physicalDelete">To flag whether to delete the physical level of the data</param>
        /// <returns></returns>
        public virtual async Task<bool> DeleteAsync(bool physicalDelete = false)
        {
            if (physicalDelete == false)
                return await UpdateIsDeletedAsync(true);

            if (Repository is null)
                return await Orm.Delete<TEntity>(this as TEntity).ExecuteAffrowsAsync() == 1;

            Repository.UnitOfWork = _resolveUow?.Invoke();
            return await Repository.DeleteAsync(this as TEntity) == 1;
        }

        /// <summary>
        /// To recover deleted data <br />
        /// 恢复删除的数据
        /// </summary>
        /// <returns></returns>
        public virtual Task<bool> RestoreAsync() => UpdateIsDeletedAsync(false);

        /// <summary>
        /// To update data <br />
        /// 更新数据
        /// </summary>
        /// <returns></returns>
        public virtual async Task<bool> UpdateAsync()
        {
            UpdateTime = DateTime.Now;
            if (Repository is null)
            {
                return await Orm.Update<TEntity>()
                                .WithTransaction(_resolveUow?.Invoke()?.GetOrBeginTransaction())
                                .SetSource(this as TEntity).ExecuteAffrowsAsync() == 1;
            }

            Repository.UnitOfWork = _resolveUow?.Invoke();
            return await Repository.UpdateAsync(this as TEntity) == 1;
        }

        /// <summary>
        /// To insert data <br />
        /// 插入数据
        /// </summary>
        public virtual Task<TEntity> InsertAsync()
        {
            CreateTime = DateTime.Now;
            Repository ??= Orm.GetRepository<TEntity>();
            Repository.UnitOfWork = _resolveUow?.Invoke();
            return Repository.InsertAsync(this as TEntity);
        }

        /// <summary>
        /// To insert or update data <br />
        /// 更新或插入
        /// </summary>
        /// <returns></returns>
        public virtual Task<TEntity> SaveAsync()
        {
            UpdateTime = DateTime.Now;
            Repository ??= Orm.GetRepository<TEntity>();
            Repository.UnitOfWork = _resolveUow?.Invoke();
            return Repository.InsertOrUpdateAsync(this as TEntity);
        }

        /// <summary>
        /// To completely save the navigation properties of the entity in the form of sub-tables. <br />
        /// 【完整】保存导航属性，子表
        /// </summary>
        /// <param name="navigatePropertyName">Navigation property name</param>
        public virtual Task SaveManyAsync(string navigatePropertyName)
        {
            Repository ??= Orm.GetRepository<TEntity>();
            Repository.UnitOfWork = _resolveUow?.Invoke();
            return Repository.SaveManyAsync(this as TEntity, navigatePropertyName);
        }
#endif
    }
}