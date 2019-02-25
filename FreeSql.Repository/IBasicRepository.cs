using System.Collections.Generic;
using System.Threading.Tasks;

namespace FreeSql {
	public interface IBasicRepository<TEntity> : IReadOnlyRepository<TEntity>
		where TEntity : class {
		TEntity Insert(TEntity entity);

		List<TEntity> Insert(List<TEntity> entity);

		Task<TEntity> InsertAsync(TEntity entity);

		Task<List<TEntity>> InsertAsync(List<TEntity> entity);

		int Update(TEntity entity);

		Task<int> UpdateAsync(TEntity entity);

		IUpdate<TEntity> UpdateDiy { get; }

		int Delete(TEntity entity);

		Task<int> DeleteAsync(TEntity entity);
	}

	public interface IBasicRepository<TEntity, TKey> : IBasicRepository<TEntity>, IReadOnlyRepository<TEntity, TKey>
		where TEntity : class {
		int Delete(TKey id);

		Task<int> DeleteAsync(TKey id);
	}
}

