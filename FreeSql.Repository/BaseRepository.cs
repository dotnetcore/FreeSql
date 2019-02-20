using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FreeSql {
	public abstract class BaseRepository<TEntity> : IRepository<TEntity>
		where TEntity : class {

		protected IFreeSql _fsql;

		public BaseRepository(IFreeSql fsql) : base() {
			_fsql = fsql;
			if (_fsql == null) throw new NullReferenceException("fsql 参数不可为空");
		}

		public ISelect<TEntity> Select => _fsql.Select<TEntity>();

		public IUpdate<TEntity> UpdateDiy => _fsql.Update<TEntity>();

		public void Delete(Expression<Func<TEntity, bool>> predicate) => _fsql.Delete<TEntity>().Where(predicate).ExecuteAffrows();

		public void Delete(TEntity entity) => _fsql.Delete<TEntity>(entity).ExecuteAffrows();

		public Task DeleteAsync(Expression<Func<TEntity, bool>> predicate) => _fsql.Delete<TEntity>().Where(predicate).ExecuteAffrowsAsync();

		public Task DeleteAsync(TEntity entity) => _fsql.Delete<TEntity>(entity).ExecuteAffrowsAsync();

		public TEntity Insert(TEntity entity) => _fsql.Insert<TEntity>().AppendData(entity).ExecuteInserted().FirstOrDefault();

		async public Task<TEntity> InsertAsync(TEntity entity) => (await _fsql.Insert<TEntity>().AppendData(entity).ExecuteInsertedAsync()).FirstOrDefault();

		public void Update(TEntity entity) => _fsql.Update<TEntity>().SetSource(entity).ExecuteAffrows();

		public Task UpdateAsync(TEntity entity) => _fsql.Update<TEntity>().SetSource(entity).ExecuteAffrowsAsync();
	}

	public abstract class BaseRepository<TEntity, TKey> : BaseRepository<TEntity>, IRepository<TEntity, TKey>
		where TEntity : class {

		public BaseRepository(IFreeSql fsql) : base(fsql) {
		}

		public void Delete(TKey id) => _fsql.Delete<TEntity>(id).ExecuteAffrows();

		public Task DeleteAsync(TKey id) => _fsql.Delete<TEntity>(id).ExecuteAffrowsAsync();

		public TEntity Find(TKey id) => _fsql.Select<TEntity>(id).ToOne();

		public Task<TEntity> FindAsync(TKey id) => _fsql.Select<TEntity>(id).ToOneAsync();

		public TEntity Get(TKey id) => Find(id);

		public Task<TEntity> GetAsync(TKey id) => FindAsync(id);
	}
}
