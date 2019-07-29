using FreeSql;
using FreeSql.DataAnnotations;
using System;
using System.Data;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// 包括 CreateTime/UpdateTime/IsDeleted、以及 CRUD 异步和同步方法的实体基类
/// </summary>
/// <typeparam name="TEntity"></typeparam>
[Table(DisableSyncStructure = true)]
public abstract class BaseEntity<TEntity> : BaseEntityAsync<TEntity> where TEntity : class
{
    bool UpdateIsDeleted(bool value)
    {
        if (this.Repository == null)
            return Orm.Update<TEntity>(this as TEntity)
                .WithTransaction(UnitOfWork.Current.Value?.GetOrBeginTransaction())
                .Set(a => (a as BaseEntity).IsDeleted, this.IsDeleted = value).ExecuteAffrows() == 1;

        this.IsDeleted = value;
        this.Repository.UnitOfWork = UnitOfWork.Current.Value;
        return this.Repository.Update(this as TEntity) == 1;
    }
    /// <summary>
    /// 删除数据
    /// </summary>
    /// <returns></returns>
    public virtual bool Delete() => this.UpdateIsDeleted(true);
    /// <summary>
    /// 恢复删除的数据
    /// </summary>
    /// <returns></returns>
    public virtual bool Restore() => this.UpdateIsDeleted(false);

    /// <summary>
    /// 更新数据
    /// </summary>
    /// <returns></returns>
    public virtual bool Update()
    {
        this.UpdateTime = DateTime.Now;
        if (this.Repository == null)
            return Orm.Update<TEntity>()
                .WithTransaction(UnitOfWork.Current.Value?.GetOrBeginTransaction())
                .SetSource(this as TEntity).ExecuteAffrows() == 1;

        this.Repository.UnitOfWork = UnitOfWork.Current.Value;
        return this.Repository.Update(this as TEntity) == 1;
    }
    /// <summary>
    /// 插入数据
    /// </summary>
    public virtual TEntity Insert()
    {
        this.CreateTime = DateTime.Now;
        if (this.Repository == null)
            this.Repository = Orm.GetRepository<TEntity>();

        this.Repository.UnitOfWork = UnitOfWork.Current.Value;
        return this.Repository.Insert(this as TEntity);
    }

    /// <summary>
    /// 更新或插入
    /// </summary>
    /// <returns></returns>
    public virtual TEntity Save()
    {
        this.UpdateTime = DateTime.Now;
        if (this.Repository == null)
            this.Repository = Orm.GetRepository<TEntity>();

        this.Repository.UnitOfWork = UnitOfWork.Current.Value;
        return this.Repository.InsertOrUpdate(this as TEntity);
    }
}
