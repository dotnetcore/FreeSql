using FreeSql.Extensions.EntityUtil;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FreeSql {
	public abstract class BaseRepository<TEntity> : IRepository<TEntity>
		where TEntity : class {

		internal IFreeSql _fsql;
		internal RepositoryDbContext<TEntity> _db;
		public IDataFilter<TEntity> DataFilter { get; } = new DataFilter<TEntity>();
		Func<string, string> _asTableVal;
		protected Func<string, string> AsTable {
			get => _asTableVal;
			set {
				_asTableVal = value;
				AsTableSelect = value == null ? null : new Func<Type, string, string>((a, b) => a == EntityType ? value(b) : null);
			}
		}
		internal Func<string, string> AsTableInternal => AsTable;
		protected Func<Type, string, string> AsTableSelect { get; private set; }
		internal Func<Type, string, string> AsTableSelectInternal => AsTableSelect;

		protected BaseRepository(IFreeSql fsql, Expression<Func<TEntity, bool>> filter, Func<string, string> asTable = null) {
			_fsql = fsql;
			DataFilterUtil.SetRepositoryDataFilter(this, null);
			DataFilter.Apply("", filter);
			AsTable = asTable;
			_db = new RepositoryDbContext<TEntity>(_fsql, this);
		}

		~BaseRepository() {
			this.Dispose();
		}
		bool _isdisposed = false;
		public void Dispose() {
			if (_isdisposed) return;
			try {
				_db.Dispose();
				this.DataFilter.Dispose();
			} finally {
				_isdisposed = true;
				GC.SuppressFinalize(this);
			}
		}

		public IUnitOfWork UnitOfWork { get; set; }
		public Type EntityType => _db.DbSet._entityTypeInternal;
		public IUpdate<TEntity> UpdateDiy => _db.DbSet.OrmUpdateInternal(null);

		public ISelect<TEntity> Select => _db.DbSet.OrmSelectInternal(null);
		public ISelect<TEntity> Where(Expression<Func<TEntity, bool>> exp) => _db.DbSet.OrmSelectInternal(null).Where(exp);
		public ISelect<TEntity> WhereIf(bool condition, Expression<Func<TEntity, bool>> exp) => _db.DbSet.OrmSelectInternal(null).WhereIf(condition, exp);

		public int Delete(Expression<Func<TEntity, bool>> predicate) => _db.DbSet.OrmDeleteInternal(null).Where(predicate).ExecuteAffrows();
		public Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate) => _db.DbSet.OrmDeleteInternal(null).Where(predicate).ExecuteAffrowsAsync();

		public int Delete(TEntity entity) {
			_db.DbSet.Remove(entity);
			return _db.SaveChanges();
		}
		public Task<int> DeleteAsync(TEntity entity) {
			_db.DbSet.Remove(entity);
			return _db.SaveChangesAsync();
		}
		public int Delete(IEnumerable<TEntity> entitys) {
			_db.DbSet.RemoveRange(entitys);
			return _db.SaveChanges();
		}
		public Task<int> DeleteAsync(IEnumerable<TEntity> entitys) {
			_db.DbSet.RemoveRange(entitys);
			return _db.SaveChangesAsync();
		}

		public virtual TEntity Insert(TEntity entity) {
			_db.DbSet.Add(entity);
			_db.SaveChanges();
			return entity;
		}
		async public virtual Task<TEntity> InsertAsync(TEntity entity) {
			await _db.DbSet.AddAsync(entity);
			_db.SaveChanges();
			return entity;
		}
		public virtual List<TEntity> Insert(IEnumerable<TEntity> entitys) {
			_db.DbSet.AddRange(entitys);
			_db.SaveChanges();
			return entitys.ToList();
		}
		async public virtual Task<List<TEntity>> InsertAsync(IEnumerable<TEntity> entitys) {
			await _db.DbSet.AddRangeAsync(entitys);
			await _db.SaveChangesAsync();
			return entitys.ToList();
		}

		public int Update(TEntity entity) {
			_db.DbSet.Update(entity);
			return _db.SaveChanges();
		}
		public Task<int> UpdateAsync(TEntity entity) {
			_db.DbSet.Update(entity);
			return _db.SaveChangesAsync();
		}
		public int Update(IEnumerable<TEntity> entitys) {
			_db.DbSet.UpdateRange(entitys);
			return _db.SaveChanges();
		}
		public Task<int> UpdateAsync(IEnumerable<TEntity> entitys) {
			_db.DbSet.UpdateRange(entitys);
			return _db.SaveChangesAsync();
		}
	}

	public abstract class BaseRepository<TEntity, TKey> : BaseRepository<TEntity>, IRepository<TEntity, TKey>
		where TEntity : class {

		public BaseRepository(IFreeSql fsql, Expression<Func<TEntity, bool>> filter, Func<string, string> asTable = null) : base(fsql, filter, asTable) {
		}

		public int Delete(TKey id) {
			var stateKey = string.Concat(id);
			if (_db.DbSet._statesInternal.ContainsKey(stateKey))
				_db.DbSet._statesInternal.Remove(stateKey);
			return _db.DbSet.OrmDeleteInternal(id).ExecuteAffrows();
		}
		public Task<int> DeleteAsync(TKey id) {
			var stateKey = string.Concat(id);
			if (_db.DbSet._statesInternal.ContainsKey(stateKey))
				_db.DbSet._statesInternal.Remove(stateKey);
			return _db.DbSet.OrmDeleteInternal(id).ExecuteAffrowsAsync();
		}

		public TEntity Find(TKey id) => _db.DbSet.OrmSelectInternal(id).ToOne();
		public Task<TEntity> FindAsync(TKey id) => _db.DbSet.OrmSelectInternal(id).ToOneAsync();

		public TEntity Get(TKey id) => Find(id);
		public Task<TEntity> GetAsync(TKey id) => FindAsync(id);
	}
}
