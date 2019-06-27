using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FreeSql
{
    public interface IReadOnlyRepository<TEntity> : IBaseRepository
        where TEntity : class
    {

        IDataFilter<TEntity> DataFilter { get; }

        ISelect<TEntity> Select { get; }

        ISelect<TEntity> Where(Expression<Func<TEntity, bool>> exp);
        ISelect<TEntity> WhereIf(bool condition, Expression<Func<TEntity, bool>> exp);
    }

    public interface IReadOnlyRepository<TEntity, TKey> : IReadOnlyRepository<TEntity>
        where TEntity : class
    {
        TEntity Get(TKey id);

        Task<TEntity> GetAsync(TKey id);

        TEntity Find(TKey id);

        Task<TEntity> FindAsync(TKey id);
    }
}
