using FreeSql;
using FreeSql.DataAnnotations;
using System;
using System.Data;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// 包括 CreateTime/UpdateTime/IsDeleted、以及 CRUD 异步方法的实体基类
/// </summary>
/// <typeparam name="TEntity"></typeparam>
[Table(DisableSyncStructure = true)]
public abstract class BaseEntityAsync<TEntity> : BaseEntity where TEntity : class
{
    /// <summary>
    /// 查询数据
    /// </summary>
    /// <returns></returns>
    public static ISelect<TEntity> Select => Orm.Select<TEntity>()
        .WithTransaction(UnitOfWork.Current.Value?.GetOrBeginTransaction(false))
        .WhereCascade(a => (a as BaseEntity).IsDeleted == false);
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
        this.Repository.Attach(item);
        return item;
    }

    /**  async **/

    async Task<bool> UpdateIsDeletedAsync(bool value)
    {
        if (this.Repository == null)
            return await Orm.Update<TEntity>(this as TEntity)
                .WithTransaction(UnitOfWork.Current.Value?.GetOrBeginTransaction())
                .Set(a => (a as BaseEntity).IsDeleted, this.IsDeleted = value).ExecuteAffrowsAsync() == 1;

        this.IsDeleted = value;
        this.Repository.UnitOfWork = UnitOfWork.Current.Value;
        return await this.Repository.UpdateAsync(this as TEntity) == 1;
    }
    /// <summary>
    /// 删除数据
    /// </summary>
    /// <returns></returns>
    public virtual Task<bool> DeleteAsync() => this.UpdateIsDeletedAsync(true);
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
                .WithTransaction(UnitOfWork.Current.Value?.GetOrBeginTransaction())
                .SetSource(this as TEntity).ExecuteAffrowsAsync() == 1;

        this.Repository.UnitOfWork = UnitOfWork.Current.Value;
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

        this.Repository.UnitOfWork = UnitOfWork.Current.Value;
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

        this.Repository.UnitOfWork = UnitOfWork.Current.Value;
        return this.Repository.InsertOrUpdateAsync(this as TEntity);
    }
}
