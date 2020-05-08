
using FreeSql;
using FreeSql.DataAnnotations;
using System;
using System.Threading.Tasks;

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
            var tkeyType = typeof(TKey)?.NullableTypeOrThis();
            if (tkeyType == typeof(int) || tkeyType == typeof(long))
                BaseEntity.ConfigEntity(typeof(TEntity), t => t.Property("Id").IsIdentity(true));
        }

        /// <summary>
        /// 主键
        /// </summary>
        [Column(Position = 1)] 
        public virtual TKey Id { get; set; }

#if net40
#else
        /// <summary>
        /// 根据主键值获取数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        async public static Task<TEntity> FindAsync(TKey id)
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
#if net40
#else
        async Task<bool> UpdateIsDeletedAsync(bool value)
        {
            if (this.Repository == null)
                return await Orm.Update<TEntity>(this as TEntity)
                    .WithTransaction(_resolveUow?.Invoke()?.GetOrBeginTransaction())
                    .Set(a => (a as BaseEntity).IsDeleted, this.IsDeleted = value).ExecuteAffrowsAsync() == 1;

            this.IsDeleted = value;
            this.Repository.UnitOfWork = _resolveUow?.Invoke();
            return await this.Repository.UpdateAsync(this as TEntity) == 1;
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="physicalDelete">是否物理删除</param>
        /// <returns></returns>
        async public virtual Task<bool> DeleteAsync(bool physicalDelete = false)
        {
            if (physicalDelete == false) return await this.UpdateIsDeletedAsync(true);
            if (this.Repository == null)
                return await Orm.Delete<TEntity>(this as TEntity).ExecuteAffrowsAsync() == 1;

            this.Repository.UnitOfWork = _resolveUow?.Invoke();
            return await this.Repository.DeleteAsync(this as TEntity) == 1;
        }
        /// <summary>
        /// 恢复删除的数据
        /// </summary>
        /// <returns></returns>
        public virtual Task<bool> RestoreAsync() => this.UpdateIsDeletedAsync(false);

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <returns></returns>
        async public virtual Task<bool> UpdateAsync()
        {
            this.UpdateTime = DateTime.Now;
            if (this.Repository == null)
                return await Orm.Update<TEntity>()
                    .WithTransaction(_resolveUow?.Invoke()?.GetOrBeginTransaction())
                    .SetSource(this as TEntity).ExecuteAffrowsAsync() == 1;

            this.Repository.UnitOfWork = _resolveUow?.Invoke();
            return await this.Repository.UpdateAsync(this as TEntity) == 1;
        }
        /// <summary>
        /// 插入数据
        /// </summary>
        public virtual Task<TEntity> InsertAsync()
        {
            this.CreateTime = DateTime.Now;
            if (this.Repository == null)
                this.Repository = Orm.GetRepository<TEntity>();

            this.Repository.UnitOfWork = _resolveUow?.Invoke();
            return this.Repository.InsertAsync(this as TEntity);
        }

        /// <summary>
        /// 更新或插入
        /// </summary>
        /// <returns></returns>
        public virtual Task<TEntity> SaveAsync()
        {
            this.UpdateTime = DateTime.Now;
            if (this.Repository == null)
                this.Repository = Orm.GetRepository<TEntity>();

            this.Repository.UnitOfWork = _resolveUow?.Invoke();
            return this.Repository.InsertOrUpdateAsync(this as TEntity);
        }

        /// <summary>
        /// 【完整】保存导航属性，子表
        /// </summary>
        /// <param name="navigatePropertyName">导航属性名</param>
        public virtual Task SaveManyAsync(string navigatePropertyName)
        {
            if (this.Repository == null)
                this.Repository = Orm.GetRepository<TEntity>();

            this.Repository.UnitOfWork = _resolveUow?.Invoke();
            return this.Repository.SaveManyAsync(this as TEntity, navigatePropertyName);
        }
#endif
    }
}
