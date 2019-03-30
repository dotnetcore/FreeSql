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
		internal UnitOfWork _uow;
		RepositoryDbSet<TEntity> _setPriv;
		internal RepositoryDbSet<TEntity> _set => _setPriv ?? (_setPriv = new RepositoryDbSet<TEntity>(this));
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
		}

		public Type EntityType => _set._entityTypeInternal;
		public IUpdate<TEntity> UpdateDiy => _set.OrmUpdateInternal(null);


		public ISelect<TEntity> Select => _set.OrmSelectInternal(null);
		public ISelect<TEntity> Where(Expression<Func<TEntity, bool>> exp) => _set.OrmSelectInternal(null).Where(exp);
		public ISelect<TEntity> WhereIf(bool condition, Expression<Func<TEntity, bool>> exp) => _set.OrmSelectInternal(null).WhereIf(condition, exp);

		public int Delete(Expression<Func<TEntity, bool>> predicate) => _set.OrmDeleteInternal(null).Where(predicate).ExecuteAffrows();
		public Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate) => _set.OrmDeleteInternal(null).Where(predicate).ExecuteAffrowsAsync();

		public int Delete(TEntity entity) => _set.RemoveAffrows(entity);
		public Task<int> DeleteAsync(TEntity entity) => _set.RemoveAffrowsAsync(entity);
		public int Delete(IEnumerable<TEntity> entitys) => _set.RemoveRangeAffrows(entitys);
		public Task<int> DeleteAsync(IEnumerable<TEntity> entitys) => _set.RemoveRangeAffrowsAsync(entitys);

		public virtual TEntity Insert(TEntity entity) {
			_set.Add(entity);
			return entity;
		}
		public virtual List<TEntity> Insert(IEnumerable<TEntity> entitys) {
			_set.AddRange(entitys);
			return entitys.ToList();
		}
		async public virtual Task<TEntity> InsertAsync(TEntity entity) {
			await _set.AddAsync(entity);
			return entity;
		}
		async public virtual Task<List<TEntity>> InsertAsync(IEnumerable<TEntity> entitys) {
			await _set.AddRangeAsync(entitys);
			return entitys.ToList();
		}

		public int Update(TEntity entity) => _set.UpdateAffrows(entity);
		public Task<int> UpdateAsync(TEntity entity) => _set.UpdateAffrowsAsync(entity);
		public int Update(IEnumerable<TEntity> entitys) => _set.UpdateRangeAffrows(entitys);
		public Task<int> UpdateAsync(IEnumerable<TEntity> entitys) => _set.UpdateRangeAffrowsAsync(entitys);
	}

	public abstract class BaseRepository<TEntity, TKey> : BaseRepository<TEntity>, IRepository<TEntity, TKey>
		where TEntity : class {

		public BaseRepository(IFreeSql fsql, Expression<Func<TEntity, bool>> filter, Func<string, string> asTable = null) : base(fsql, filter, asTable) {
		}

		public int Delete(TKey id) {
			var stateKey = string.Concat(id);
			if (_set._statesInternal.ContainsKey(stateKey)) _set._statesInternal.Remove(stateKey);
			return _set.OrmDeleteInternal(id).ExecuteAffrows();
		}
		public Task<int> DeleteAsync(TKey id) {
			var stateKey = string.Concat(id);
			if (_set._statesInternal.ContainsKey(stateKey)) _set._statesInternal.Remove(stateKey);
			return _set.OrmDeleteInternal(id).ExecuteAffrowsAsync();
		}

		public TEntity Find(TKey id) => _set.OrmSelectInternal(id).ToOne();
		public Task<TEntity> FindAsync(TKey id) => _set.OrmSelectInternal(id).ToOneAsync();

		public TEntity Get(TKey id) => Find(id);
		public Task<TEntity> GetAsync(TKey id) => FindAsync(id);
	}
}
