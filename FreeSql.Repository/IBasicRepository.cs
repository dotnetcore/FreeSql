using System.Threading.Tasks;

namespace FreeSql {
	public interface IBasicRepository<TEntity> : IReadOnlyRepository<TEntity>
		where TEntity : class {
		TEntity Insert(TEntity entity);

		Task<TEntity> InsertAsync(TEntity entity);

		void Update(TEntity entity);

		Task UpdateAsync(TEntity entity);

		IUpdate<TEntity> UpdateDiy { get; }

		void Delete(TEntity entity);

		Task DeleteAsync(TEntity entity);
	}

	public interface IBasicRepository<TEntity, TKey> : IBasicRepository<TEntity>, IReadOnlyRepository<TEntity, TKey>
		where TEntity : class {
		void Delete(TKey id);

		Task DeleteAsync(TKey id);
	}
}

