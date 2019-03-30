using System.Collections.Generic;
using System.Threading.Tasks;

namespace FreeSql {
	public interface IBasicRepository<TEntity> : IReadOnlyRepository<TEntity>
		where TEntity : class {
		TEntity Insert(TEntity entity);
		List<TEntity> Insert(IEnumerable<TEntity> entitys);
		Task<TEntity> InsertAsync(TEntity entity);
		Task<List<TEntity>> InsertAsync(IEnumerable<TEntity> entitys);

		int Update(TEntity entity);
		int Update(IEnumerable<TEntity> entitys);
		Task<int> UpdateAsync(TEntity entity);
		Task<int> UpdateAsync(IEnumerable<TEntity> entitys);

		IUpdate<TEntity> UpdateDiy { get; }

		int Delete(TEntity entity);
		int Delete(IEnumerable<TEntity> entitys);
		Task<int> DeleteAsync(TEntity entity);
		Task<int> DeleteAsync(IEnumerable<TEntity> entitys);
	}

	public interface IBasicRepository<TEntity, TKey> : IBasicRepository<TEntity>, IReadOnlyRepository<TEntity, TKey>
		where TEntity : class {
		int Delete(TKey id);

		Task<int> DeleteAsync(TKey id);
	}
}

