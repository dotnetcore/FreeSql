#if netcore
#else

using FreeSql;
using FreeSql.DataAnnotations;
using System;
using System.Data;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

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
            var tkeyType = typeof(TKey)?.NullableTypeOrThis();
            if (tkeyType == typeof(int) || tkeyType == typeof(long))
                Orm.CodeFirst.ConfigEntity(typeof(TEntity),
                    t => t.Property("Id").IsIdentity(true));
        }

        /// <summary>
        /// 主键
        /// </summary>
        [Column(Position = 1)]
        public virtual TKey Id { get; set; }

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
    public abstract class BaseEntity<TEntity> : BaseEntityReadOnly<TEntity> where TEntity : class
    {
        bool DeletedPrivate(bool value)
        {
            if (this.Repository == null)
                return Orm.Delete<TEntity>(this as TEntity)
                    .ExecuteAffrows() == 1;

            return this.Repository.Delete(this as TEntity) == 1;
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <returns></returns>
        public virtual bool Delete() => this.DeletedPrivate(true);

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <returns></returns>
        public virtual bool Update()
        {
            if (this.Repository == null)
                return Orm.Update<TEntity>()
                    .SetSource(this as TEntity).ExecuteAffrows() == 1;

            return this.Repository.Update(this as TEntity) == 1;
        }
        /// <summary>
        /// 插入数据
        /// </summary>
        public virtual TEntity Insert()
        {
            if (this.Repository == null)
                this.Repository = Orm.GetRepository<TEntity>();

            return this.Repository.Insert(this as TEntity);
        }

        /// <summary>
        /// 更新或插入
        /// </summary>
        /// <returns></returns>
        public virtual TEntity Save()
        {
            if (this.Repository == null)
                this.Repository = Orm.GetRepository<TEntity>();

            return this.Repository.InsertOrUpdate(this as TEntity);
        }
    }
}

#endif