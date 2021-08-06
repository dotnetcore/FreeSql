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
    /// 包括 CreateTime/UpdateTime/IsDeleted、CRUD 异步方法、以及 ID 主键定义 的实体基类
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
    }

    /// <summary>
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
        /// 删除数据
        /// </summary>
        /// <param name="physicalDelete">是否物理删除</param>
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
        /// 恢复删除的数据
        /// </summary>
        /// <returns></returns>
        public virtual Task<bool> RestoreAsync() => UpdateIsDeletedAsync(false);

        /// <summary>
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
        /// 插入数据
        /// </summary>
        public virtual Task<TEntity> InsertAsync()
        {
            CreateTime = DateTime.Now;
            if (Repository is null)
                Repository = Orm.GetRepository<TEntity>();

            Repository.UnitOfWork = _resolveUow?.Invoke();
            return Repository.InsertAsync(this as TEntity);
        }

        /// <summary>
        /// 更新或插入
        /// </summary>
        /// <returns></returns>
        public virtual Task<TEntity> SaveAsync()
        {
            UpdateTime = DateTime.Now;
            if (Repository is null)
                Repository = Orm.GetRepository<TEntity>();

            Repository.UnitOfWork = _resolveUow?.Invoke();
            return Repository.InsertOrUpdateAsync(this as TEntity);
        }

        /// <summary>
        /// 【完整】保存导航属性，子表
        /// </summary>
        /// <param name="navigatePropertyName">导航属性名</param>
        public virtual Task SaveManyAsync(string navigatePropertyName)
        {
            if (Repository is null)
                Repository = Orm.GetRepository<TEntity>();

            Repository.UnitOfWork = _resolveUow?.Invoke();
            return Repository.SaveManyAsync(this as TEntity, navigatePropertyName);
        }
#endif
    }
}