using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FreeSql {

	public interface IRepository : IDisposable {
		Type EntityType { get; }
		IUnitOfWork UnitOfWork { get; set; }
	}

	public interface IRepository<TEntity> : IReadOnlyRepository<TEntity>, IBasicRepository<TEntity>
		where TEntity : class {
		int Delete(Expression<Func<TEntity, bool>> predicate);

		Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate);
	}

	public interface IRepository<TEntity, TKey> : IRepository<TEntity>, IReadOnlyRepository<TEntity, TKey>, IBasicRepository<TEntity, TKey>
		where TEntity : class {
	}
}