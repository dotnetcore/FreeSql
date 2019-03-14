using System.Threading.Tasks;

namespace FreeSql {
	public interface IReadOnlyRepository<TEntity> : IRepository
		where TEntity : class {

		IDataFilter<TEntity> DataFilter { get; }

		ISelect<TEntity> Select { get; }
	}

	public interface IReadOnlyRepository<TEntity, TKey> : IReadOnlyRepository<TEntity>
		where TEntity : class {
		TEntity Get(TKey id);

		Task<TEntity> GetAsync(TKey id);

		TEntity Find(TKey id);

		Task<TEntity> FindAsync(TKey id);
	}
}
