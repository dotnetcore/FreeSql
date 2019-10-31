using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FreeSql
{

    public interface IBaseRepository : IDisposable
    {
        Type EntityType { get; }
        IUnitOfWork UnitOfWork { get; set; }
        IFreeSql Orm { get; }

        /// <summary>
        /// 动态Type，在使用 Repository&lt;object&gt; 后使用本方法，指定实体类型
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        void AsType(Type entityType);

        /// <summary>
        /// 设置 DbContext 选项
        /// </summary>
        DbContextOptions DbContextOptions { get; set; }
    }

    public interface IBaseRepository<TEntity> : IReadOnlyRepository<TEntity>, IBasicRepository<TEntity>
        where TEntity : class
    {
        int Delete(Expression<Func<TEntity, bool>> predicate);

#if net40
#else
        Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate);
#endif
    }

    public interface IBaseRepository<TEntity, TKey> : IBaseRepository<TEntity>, IReadOnlyRepository<TEntity, TKey>, IBasicRepository<TEntity, TKey>
        where TEntity : class
    {
    }
}