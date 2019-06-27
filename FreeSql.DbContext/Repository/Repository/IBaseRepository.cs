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
    }

    public interface IBaseRepository<TEntity> : IReadOnlyRepository<TEntity>, IBasicRepository<TEntity>
        where TEntity : class
    {
        int Delete(Expression<Func<TEntity, bool>> predicate);

        Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate);
    }

    public interface IBaseRepository<TEntity, TKey> : IBaseRepository<TEntity>, IReadOnlyRepository<TEntity, TKey>, IBasicRepository<TEntity, TKey>
        where TEntity : class
    {
    }
}